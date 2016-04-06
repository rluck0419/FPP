using UnityEngine;
using System.Collections;

public class Spikeable : MonoBehaviour {
	GameObject spikedObject;
	Rigidbody r;
	RigidbodyConstraints cons;


	// Use this for initialization
	void Start () {
		spikedObject = GameObject.FindWithTag("Spiked");
		r = spikedObject.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void OnCollisionEnter (Collision c) {	
		// Lock the spike ball's position & rotation when the ball collides with any surface
		// cons = spikedObject.GetComponent<RigidbodyConstraints> ();
		// cons.FreezePosition;
	}
}
