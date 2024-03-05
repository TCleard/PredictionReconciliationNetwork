using UnityEngine;

public class NetworkPlayerStateConsistencyChecker: MonoBehaviour, PRN.IStateConsistencyChecker<NetworkPlayerState> {

	// You need to implement this method
	// serverState is the one sent back by the server to the client
	// ownerState is the corresponding state the client predicted (they have the same tick value)
	public bool IsConsistent(NetworkPlayerState serverState, NetworkPlayerState ownerState) =>
		Vector3.Distance(serverState.position, ownerState.position) <= .01f
			&& Vector3.Distance(serverState.movement, ownerState.movement) <= .01f
			&& Vector3.Distance(serverState.gravity, ownerState.gravity) <= .01f;

}
