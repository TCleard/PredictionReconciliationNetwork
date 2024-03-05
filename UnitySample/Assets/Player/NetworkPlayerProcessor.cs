using System;
using UnityEngine;

public class NetworkPlayerProcessor: MonoBehaviour, PRN.IProcessor<NetworkPlayerInput, NetworkPlayerState> {

	private CharacterController controller;

	[SerializeField]
	private float movementSpeed = 8f;
	[SerializeField]
	private float jumpHeight = 2.5f;

	[SerializeField]
	private float gravityForce = -9.81f;

	public Vector3 movement = Vector3.zero;
	public Vector3 gravity = Vector3.zero;

	private void Awake() {
		controller = GetComponent<CharacterController>();
	}

	// You need to implement this method
	// Your player logic happens here
	public NetworkPlayerState Process(NetworkPlayerInput input, TimeSpan deltaTime) {
		movement = (Vector3.forward * input.forward + Vector3.right * input.right).normalized * movementSpeed * (float) deltaTime.TotalSeconds;
		if (controller.isGrounded) {
			gravity = Vector3.zero;
			if (input.jump) {
				gravity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * -gravityForce) * (float) deltaTime.TotalSeconds;
			}
        }
		if (gravity.y > 0) {
			gravity += Vector3.up * gravityForce * Mathf.Pow((float) deltaTime.TotalSeconds, 2);
		} else {
			gravity += Vector3.up * gravityForce * Mathf.Pow((float) deltaTime.TotalSeconds, 2) * 1.3f;
		}
		controller.Move(movement + gravity);
		return new NetworkPlayerState() {
			position = transform.position,
			movement = movement,
			gravity = gravity
		};
	}

	// You need to implement this method
	// Called when an inconsistency occures
	public void Rewind(NetworkPlayerState state) {
		controller.enabled = false;
		transform.position = state.position;
		movement = state.movement;
		gravity = state.gravity;
		controller.enabled = true;
	}

}