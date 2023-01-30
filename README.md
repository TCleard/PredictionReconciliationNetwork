

# PredictionReconciliationNetwork

## Summary

1. [Purpose](#purpose)
2. [Import](#import)
3. [Usage](#usage)<br/>a. [Processor, Input and State](#processor-input-and-state)<br/>b. [StateConsistencyChecker](#stateconsistencychecker)<br/>c. [InputProvider](#inputprovider)<br/>d. [NetworkHandler](#networkhandler)
4. [What's next](#whats-next)
5. [Conclusion](#conclusion)

## Purpose

This C# project goal is to simplify online multiplayer games with Client-side prediction and Server reconciliation.
It can be adapted in any languages as it is not bound to any engine (even though I've written it to work with Unity)

## Import

Go to the [Release](https://github.com/TCleard/PredictionReconciliationNetwork/releases) page, download the latest .dll and import it into your project (for Unity, under the Assets folder).
You can also pull this repository directly into your project.

## Usage

### Processor, Input and State

A **processor** holds the core logic of the object you need to sync over the network. It takes an Input as a parameter, and returns a State. It will also reconcile your client if there's inconsistency between the server and the client.
An **input** is simply the instruction your processor needs to perform an action. It needs a **tick** value to inform the server *when* it's created.
A **state** is the result of this processing, and needs the input's **tick** value so the client knows how far the reconciliation has to go in case of unconsistency between the server and the client.
Those **tick* values are set by the lib.

Example (for Unity) :

```C#
// Your Input
public struct PlayerInput: PRN.Input {
	
	public int tick;
	public int forward;
	public int right;

	// You need to implement those 2 methods
	public void SetTick(int tick) => this.tick = tick;
	public int GetTick() => tick;

}

// Your State
public struct PlayerState: PRN.State {

	public int tick;
	public Vector3 position;

	// You need to implement those 2 methods
	public void SetTick(int tick) => this.tick = tick;
	public int GetTick() => tick;
	
}

// Your Processor
public class PlayerProcessor: MonoBehaviour, PRN.Processor<PlayerInput, PlayerState> {

	private CharacterController controller;
	
	private void Awake() {
		base.Awake();
		controller = GetComponent<CharacterController>();
	}
	
	// You need to implement this method
	// Your player logic happens here
	public PlayerState Process(PlayerInput input, TimeSpan deltaTime) {
        controller.Move((Vector3.forward * input.forward + Vector3.right * input.right).normalized * .01f * deltaTime.Milliseconds);
        return new PlayerState() {
			position = transform.position // or controller.transform.position
		};
	}

	// You need to implement this method
	// Called when an inconsistency occures
	public void Rewind(PlayerState state) {
		controller.enabled = false;
		transform.position = state.position;
		controller.enabled = true;
	}

}
```

### StateConsistencyChecker

When an **input** is processed by a client, it generates a **state** that updates directly the client. It is required to also send this input to the server, so it's processed server-side. The server will so generates its own state, and will send it back to the client. The client then needs to know if he has correctly predicted the state.

```C#
public class PlayerStateConsistencyChecker: PRN.StateConsistencyChecker<PlayerState> {

	// You need to implement this method
	// serverState is the one sent back by the server to the client
	// ownerState is the corresponding state the client predicted (they have the same tick value)
	public bool IsConsistent(PlayerState serverState, PlayerState ownerState) {
		return Vector3.Distance(serverState.position, ownerState.position) <= .01f;
	}
	
}
```
If this method return false, then there's an inconsistency. The lib will automatically called the Processor.**Rewind** method with the server state to restore, syncing the server and the client, and then all the client inputs that has been processed since this state.tick will be reapplied.

### InputProvider
To provide an **input** to the processor, you'll need an **InputProvider**.

```C#
public class PlayerInputProvider: MonoBehaviour, PRN.InputProvider<PlayerInput> {

    private PlayerInput input;

	private void Update() {
		base.Update();
        input.forward = (UnityEngine.Input.GetKey(KeyCode.Z) ? 1 : 0) - (UnityEngine.Input.GetKey(KeyCode.S) ? 1 : 0);
        input.right = (UnityEngine.Input.GetKey(KeyCode.D) ? 1 : 0) - (UnityEngine.Input.GetKey(KeyCode.Q) ? 1 : 0);
	}

	// You need to implement this method
	public PlayerInput GetInput() {
		return input;
	}

}
```
Note : You don't need to take care of the Input.**tick** value, as it is automatically set by the lib.

### NetworkHandler
The NetworkHandler is where the magic happens (almost).
It has multiple roles :
* Server : 
	* receives an input from a client
	* generates the corresponding state
	* sends the state to the clients
* Owner : 
	* your local player
	* predicts a state based on his own input
	* sends his input to the server
	* reconciles its state if there's an inconsistency with the server
* Host :
	* your local player, but also acts as a server
	* no reconciliation on this side
* Client :
	* just receives states

It needs a **Looper** to know the state update frequency, and to ensure every connected players and the server run at the same speed.
```C#
PRN.Looper looper = new PRN.Looper(TimeSpan.FromSeconds(1 / 30f));
```
To make it *loop*, you need to make it *tick*
```C#
public class Player: MonoBehaviour {

	private PRN.Looper looper;
	
	private void Start() {
		base.Start();
		looper = new PRN.Looper(TimeSpan.FromSeconds(1 / 30f));
	}

	private void FixedUpdate() {
		base.FixedUpdate();
		looper.Tick(TimeSpan.FromSeconds(Time.fixedDeltaTime));
	}

}
```

Now, you have everything to create your NetworkHandler.
I'll show you how to use it with [Netcode For GameObjects](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)
```C#
public class Player: NetworkBehaviour {

	[SerializeField]
	private PlayerProcessor processor;
	[SerializeField]
	private PlayerStateConsistencyChecker consistencyChecker;
	[SerializeField]
	private PlayerInputProvider inputProvider;
	
	private Looper looper;
	
	protected override void OnNetworkSpawn() {
		base.OnNetworkSpawn();
		looper = new PRN.Looper(TimeSpan.FromSeconds(1 / 30f));
		PRN.NetworkRole role;
        if (IsServer) {
            role = IsOwner? PRN.NetworkRole.HOST: PRN.NetworkRole.SERVER;
        } else {
            role = IsOwner ? PRN.NetworkRole.OWNER: PRN.NetworkRole.CLIENT;
        }
		networkHandler = new PRN.NetworkHandler<PlayerInput, PlayerState>(
			role: role,
			looper: looper,
			processor: processor,
			inputProvider: inputProvider,
			consistencyChecker: consistencyChecker
		);
	}

	[...]

}
```

You are almost done ! 

You now need to synchronise all the inputs and state :

```C#
public class Player: NetworkBehaviour {

	[...]
	
	protected override void OnNetworkSpawn() {
		[...]
		networkHandler.onSendInputToServer += SendInputServerRpc;
		networkHandler.onSendStateToClient += SendStateClientRpc;
	}
	
    [ServerRpc]
    private void SendInputServerRpc(PlayerInput input)
    {
        networkHandler.OnOwnerInputReceived(input);
    }

    [ClientRpc]
    private void SendStateClientRpc(PlayerState state)
    {
        networkHandler.OnServerStateReceived(state);
    }

	[...]
}
```

In order to send inputs and states throught Netcode RPC calls, your inputs and states need to implements Unity.Netcode.INetworkSerializable :


```C#
public struct PlayerInput: PRN.Input, Unity.Netcode.INetworkSerializable {
	
	[...]

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref forward);
        serializer.SerializeValue(ref right);
    }

}

public struct PlayerState: PRN.State, Unity.Netcode.INetworkSerializable {

	[...]

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);
    }
	
}

```

Now that your data is synced between the server and the clients, your player should now move on the server and other clients.

If you need to perform specific tasks when a state is processed, you can subscribe to the **onState** action on the network handler.

```C#
public class Player: NetworkBehaviour {

	[...]
	
	protected override void OnNetworkSpawn() {
		[...]
		networkHandler.onState += OnState;
	}

	private void OnState(PlayerState state) {
		// Do whatever you need
		// This method is called on the server or the host when they generate a state
		// on the owner when it predicts a state and during its reconciliation
		// on the client when it receives a state from the server
	}

	[...]
}
```

## What's next

Knowing that every one is playing in the past of other clients (you are live, but you see old states of others clients, even the server is behind), my next goal is to create a way to extrapolate a client's state on the owner / host / server side. Right now, if you create a FPS game with this lib, if you shoot at someone, you're in fact shooting on it's past position, the server might not register any hit on your target. With an extrapolation on a client state, you can predict where it would be, so the owner would see a predicted futur position of the client he is aiming at.

## Conclusion

I hope I didn't forget anything, and that it's clear enough. Feel free to open a PR if you have any question, modification request, or whatever :D

A little wink and a big thank you to [Ajackster](https://www.youtube.com/@Ajackster) to help me understanding Prediction Reconciliation with [this video](https://www.youtube.com/watch?v=TFLD9HWOc2k&t=14s&ab_channel=Ajackster) (which I highly recommend you to watch), who greatly inspired this project (<s>some</s> most of my code is in fact his)
Also, thanks to Unity for providing Netcode for GameObjects ([Documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/index.html), [GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects)), it motivated me to try and develop online games.
