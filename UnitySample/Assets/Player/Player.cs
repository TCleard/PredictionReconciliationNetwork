using System;
using PRN;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

	[SerializeField]
	private Material ownerMaterial;
	[SerializeField]
	private Material clientMaterial;

	[SerializeField]
	private PlayerProcessor processor;
	[SerializeField]
	private PlayerInputProvider inputProvider;
	[SerializeField]
	private PlayerStateConsistencyChecker consistencyChecker;

	private Looper looper;
	private NetworkHandler<PlayerInput, PlayerState> networkHandler;

	public override void OnNetworkSpawn() {
		base.OnNetworkSpawn();
		looper = new Looper(TimeSpan.FromSeconds(1 / 60f));
		NetworkRole role;
		if (IsServer) {
			role = IsOwner ? NetworkRole.HOST : NetworkRole.SERVER;
		} else {
			role = IsOwner ? NetworkRole.OWNER : NetworkRole.CLIENT;
		}
		networkHandler = new NetworkHandler<PlayerInput, PlayerState>(
			role: role,
			looper: looper,
			processor: processor,
			inputProvider: inputProvider,
			consistencyChecker: consistencyChecker
		);
		networkHandler.onSendInputToServer += SendInputServerRpc;
		networkHandler.onSendStateToClient += SendStateClientRpc;
		networkHandler.onState += OnState;
		GetComponent<Renderer>().material = IsOwner ? ownerMaterial : clientMaterial;
	}

	private void FixedUpdate() {
		looper.Tick(TimeSpan.FromSeconds(Time.fixedDeltaTime));
	}

	[ServerRpc]
	private void SendInputServerRpc(PlayerInput input) {
		networkHandler.OnOwnerInputReceived(input);
	}

	[ClientRpc]
	private void SendStateClientRpc(PlayerState state) {
		networkHandler.OnServerStateReceived(state);
	}

	private void OnState(PlayerState state) {
		// Do whatever you need
		// This method is called on the server or the host when they generate a state
		// on the owner when it predicts a state and during its reconciliation
		// on the client when it receives a state from the server
	}

}
