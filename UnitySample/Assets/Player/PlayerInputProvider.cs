using UnityEngine;

public class PlayerInputProvider: MonoBehaviour, PRN.InputProvider<PlayerInput> {

	private PlayerInput input;
	public bool pendingJump = false;

	private void Update() {
		input.forward = (Input.GetKey(KeyCode.Z) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
		input.right = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0);
		pendingJump |= Input.GetKeyDown(KeyCode.Space);
	}

	// You need to implement this method
	public PlayerInput GetInput() {
		input.jump = pendingJump;
		pendingJump = false;
		return input;
	}

}