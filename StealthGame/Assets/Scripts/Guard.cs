using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public Transform pathHolder;
	public Light spotLight;
	public LayerMask obstacleLayerMask;

	public float guardPatrolSpeed = 5f;
	public float wayPointWaitTime = 1f;
	public float guardRotationSpeed = 15f;
	public float viewDistance;

	Transform player;

	float viewAngle = 90f;

	Vector3 right;
	Vector3 left;

	Color originalSpotLightColour;
	Color originalGuardColor;

	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		viewAngle = spotLight.spotAngle;
		originalSpotLightColour = spotLight.color;
		originalGuardColor = GetComponent<Renderer> ().material.color;

		Vector3[] wayPoints = new Vector3[pathHolder.childCount];
		for (int i = 0; i < wayPoints.Length; i++) {
			wayPoints [i] = pathHolder.GetChild (i).position;
			pathHolder.GetChild (i).position = new Vector3 (pathHolder.GetChild (i).transform.position.x, transform.position.y, pathHolder.GetChild (i).transform.position.z);
		}

		transform.LookAt (pathHolder.GetChild (1));

		StartCoroutine (Patrol (wayPoints));
	}

	void Update () {
		/*foreach (Collider collider in hitColliders) {
			if (collider.CompareTag ("Player")) {
				if (Vector3.Distance (collider.transform.position, transform.position) <= viewDistance && AngleBetweenPlayerAndGuard (collider.transform.position)) {
					RaycastHit hit;
					if (Physics.Raycast (transform.position, (collider.transform.position - transform.position).normalized, out hit, viewDistance)) {
						if (hit.collider.CompareTag ("Player")) {
							spotLight.color = Color.red;
						} else {
							spotLight.color = originalSpotLightColour;
						}
					}
				}
			}
		}*/

		if (CanSeePlayer () && !GameController.instance.gameEnded) {
			StartCoroutine ("CatchPlayer");
		} else {
			StopCoroutine ("CatchPlayer");
			spotLight.color = originalSpotLightColour;
		}

		if (Vector3.Distance (player.position, transform.position) > 4.5) {
			OutSideOfPlayerRange ();
		}
	}

	/*bool AngleBetweenPlayerAndGuard (Vector3 position) {
		float angle = Vector3.Angle (position, transform.forward);
		if (angle <= viewAngle / 2) {
			return true;
		} else
			return false;
	}*/

	bool CanSeePlayer () {
		if (Vector3.Distance (transform.position, player.position) <= viewDistance) {
			Vector3 directionToPlayer = (player.position - transform.position).normalized;
			float angle = Vector3.Angle (transform.forward, directionToPlayer);
			if (angle <= viewAngle / 2) {
				if (!Physics.Linecast (transform.position, player.position, obstacleLayerMask)) {
					return true;
				}
			}
		}

		return false;
	}

	public bool CanSeePoint (Vector3 point) {
		if (Vector3.Distance (transform.position, point) <= viewDistance) {
			Vector3 directionToPlayer = (point - transform.position).normalized;
			float angle = Vector3.Angle (transform.forward, directionToPlayer);
			if (angle <= viewAngle / 2) {
				if (!Physics.Linecast (transform.position, point, obstacleLayerMask)) {
					return true;
				}
			}
		}

		return false;
	}

	IEnumerator Patrol (Vector3[] wayPoints) {
		transform.position = wayPoints [0];
		int targetWayPointIndex = 1;
		Vector3 targetWayPoint = wayPoints [targetWayPointIndex];

		while (true) {
			transform.position = Vector3.MoveTowards (transform.position, targetWayPoint, guardPatrolSpeed * Time.deltaTime);
			if (transform.position == targetWayPoint) {
				targetWayPointIndex = (targetWayPointIndex + 1) % wayPoints.Length;
				targetWayPoint = wayPoints [targetWayPointIndex];
				yield return new WaitForSeconds (wayPointWaitTime);
				yield return StartCoroutine (LookTowardsNextWayPoint (targetWayPoint));
			}
			yield return null;
		}
	}

	IEnumerator LookTowardsNextWayPoint (Vector3 wayPointToLookTowards) {
		Vector3 toDirection = (wayPointToLookTowards - transform.position).normalized;
		float turnAngle = 90 - Mathf.Atan2 (toDirection.z, toDirection.x) * Mathf.Rad2Deg;

		while (Mathf.Abs (Mathf.DeltaAngle (transform.eulerAngles.y, turnAngle)) > .05f) {
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle (transform.eulerAngles.y, turnAngle, guardRotationSpeed * Time.deltaTime);
			yield return null;
		}

	}

	IEnumerator CatchPlayer () {
		float t = 0f;

		while (t < 1) {
			spotLight.color = Color.Lerp (originalSpotLightColour, Color.red, t);
			t += 2 * Time.deltaTime;
			yield return null;
		}

		GameController.instance.OnGameOver ();
	}

	void CalculateAngles () {
		float currentAngle = 90f - transform.eulerAngles.y;
		float leftAngle = currentAngle + viewAngle / 2f;
		float rightAngle = currentAngle - viewAngle / 2f;

		float xLeft = Mathf.Cos (leftAngle * Mathf.Deg2Rad);
		float yLeft = Mathf.Sin (leftAngle * Mathf.Deg2Rad);

		float xRight = Mathf.Cos (rightAngle * Mathf.Deg2Rad);
		float yRight = Mathf.Sin (rightAngle * Mathf.Deg2Rad);

		left = new Vector3 (xLeft, 0f, yLeft);
		right = new Vector3 (xRight, 0f, yRight);
	}

	public void WithinPlayerRange () {
		GetComponent<Renderer> ().material.color = Color.red;
	}

	public void OutSideOfPlayerRange () {
		GetComponent<Renderer> ().material.color = originalGuardColor;
	}

	void OnDrawGizmos () {
		Vector3 startPosition = pathHolder.GetChild (pathHolder.childCount - 1).position;
		Vector3 previousPosition = startPosition;

		foreach (Transform wayPoint in pathHolder) {
			Gizmos.DrawSphere (wayPoint.position, .3f);
			Vector3 currentPosition = wayPoint.position;
			Gizmos.DrawLine (previousPosition, currentPosition);
			previousPosition = currentPosition;
		}

		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position, transform.forward * viewDistance);

		CalculateAngles ();

		Gizmos.DrawRay (transform.position, left * viewDistance);
		Gizmos.DrawRay (transform.position, right * viewDistance);
	}
}
