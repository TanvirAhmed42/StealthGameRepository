using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {

	public float speed = 7f;
	public float smoothDampSpeed = .12f;
	public float smoothDampAngle = .2f;

	Rigidbody myRigidbody;

	Vector3 inputDirection;

	float currentAngle;
	float currentSpeed;
	float currentSmoothDampAngle;
	float currentSmoothDampSpeed;

	void Start () {
		myRigidbody = GetComponent<Rigidbody> ();
	}

	void Update () {
		inputDirection = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0f, Input.GetAxisRaw ("Vertical")).normalized;
	}

	void FixedUpdate () {
		if (GameController.instance.gameEnded)
			return;

		if (inputDirection != Vector3.zero) {
			currentSpeed = Mathf.SmoothDamp (currentSpeed, speed, ref currentSmoothDampSpeed, smoothDampSpeed);
			myRigidbody.MovePosition (transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);

			float targetangle = 90 - Mathf.Atan2 (inputDirection.z, inputDirection.x) * Mathf.Rad2Deg;
			currentAngle = Mathf.SmoothDampAngle (currentAngle, targetangle, ref currentSmoothDampAngle, smoothDampAngle);
			myRigidbody.MoveRotation (Quaternion.Euler (Vector3.up * currentAngle));
		}
	}

	void OnTriggerEnter (Collider other) {
		if (other.CompareTag ("Destination")) {
			GameController.instance.OnGameWon ();
		}
	}
}
