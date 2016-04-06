using UnityEngine;
using System.Collections;

public class Ladder : MonoBehaviour {
	private GameObject player;

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider collider) {
		if (collider.gameObject == player) {
			player.GetComponent<PlayerCont>().climbing = true;
			player.GetComponent<PlayerCont>().gravity = 0f;
		}
	}

	void OnTriggerExit (Collider collider) {
		if (collider.gameObject == player) {
			player.GetComponent<PlayerCont>().climbing = false;
			player.GetComponent<PlayerCont>().gravity = 17.0f;
		}
	}
}
