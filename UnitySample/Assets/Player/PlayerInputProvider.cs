using UnityEngine;

public class PlayerInputProvider: MonoBehaviour, PRN.InputProvider<PlayerInput> {

	private PlayerInput input;

	public float deltaLookY = 0f;
	public bool pendingJump = false;

	private void Update() {
		input.forward = (Input.GetKey(KeyCode.Z) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
		input.right = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0);
		deltaLookY += Input.GetAxis("Mouse X");
		pendingJump |= Input.GetKeyDown(KeyCode.Space);
	}

	// You need to implement this method
	public PlayerInput GetInput() {
		input.deltaLookY = deltaLookY;
		input.jump = pendingJump;
		deltaLookY = 0f;
		pendingJump = false;
		return input;
	}

}