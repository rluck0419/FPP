using UnityEngine;
using System.Collections;

public class Key : MonoBehaviour {

	public bool inLock = false;
	private GameObject key;
	
	// Use this for initialization
	void Start () {
		key = GameObject.Find("ClockKey");
	}
	
	// Update is called once per frame
	void Update () {
		if (inLock == true) {
			key.GetComponent<MeshRenderer>().enabled = true;
			Destroy(gameObject);
		}
	}
}
