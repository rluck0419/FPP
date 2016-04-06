using UnityEngine;
using System.Collections;

public class Keyhole : MonoBehaviour {
	private GameObject player;
	private Vector3 position;
	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		if (other.gameObject != null) {
			if (player.GetComponent<PlayerCont>().freeze == true) {
				if (other.GetComponent<Key>()!=null) {
					other.GetComponent<Key>().inLock = true;		
				}
			}
		}
	}
}
