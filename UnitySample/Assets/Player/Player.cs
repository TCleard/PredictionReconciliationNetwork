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

	private Ticker ticker;
	private NetworkHandler<PlayerInput, PlayerState> networkHandler;

	public override void OnNetworkSpawn() {
		base.OnNetworkSpawn();
		ticker = new Ticker(TimeSpan.FromSeconds(1 / 60f));
		NetworkRole role;
		if (IsServer) {
			role = IsOwner ? NetworkRole.HOST : NetworkRole.SERVER;
		} else {
			role = IsOwner ? NetworkRole.OWNER : NetworkRole.GUEST;
		}
		networkHandler = new NetworkHandler<PlayerInput, PlayerState>(
			role: role,
			ticker: ticker,
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
		ticker.OnTimePassed(TimeSpan.FromSeconds(Time.fixedDeltaTime));
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

    public override void OnDestroy() {
		base.OnDestroy();
		networkHandler.Dispose();
    }

}
