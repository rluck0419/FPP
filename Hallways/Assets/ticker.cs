using UnityEngine;
using System.Collections;

public class ticker : MonoBehaviour {

	public float angle;
	public float period;

	private float time;

// Update is called once per frame
	void Update () {
    	if(GetComponent<Rigidbody>().isKinematic == false) {
    		time = time + Time.deltaTime;
    		float phase = Mathf.Sin(time / period);
    		transform.localRotation = Quaternion.Euler(new Vector3(0, 0, phase * angle));
		}
	}
}