using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	#region Singleton

	public static GameController instance;

	void Awake () {
		instance = this;
	}

	#endregion

	public GameObject gameOverPanel;
	public Text gameEndText;

	[HideInInspector]
	public bool gameEnded;

	void Start () {
		gameEnded = false;
	}

	void Update () {
		if (gameEnded) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				SceneManager.LoadScene ("SampleScene");
			}
		}
	}

	public void OnGameWon () {
		gameOverPanel.SetActive (true);
		gameEndText.text = "You passed unseen";
		gameEnded = true;
	}

	public void OnGameOver () {
		gameOverPanel.SetActive (true);
		gameEndText.text = "You were caught";
		gameEnded = true;
	}
}
