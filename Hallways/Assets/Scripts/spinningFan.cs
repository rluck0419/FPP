using UnityEngine;
using System.Collections;

public class spinningFan : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (GetComponent<Rigidbody>().isKinematic == false) {
			transform.Rotate(Vector3.up * Time.deltaTime * 100);
		}
	}
}
