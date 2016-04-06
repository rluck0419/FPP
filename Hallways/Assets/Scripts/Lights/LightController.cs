using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour
{
	// public float timeOn = 0.1f;
	public float timeOff = 0.1f;
	private float changeTime = 0f;
	
	void Update() 
	{
		if (Time.time > changeTime) {
			GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
			if (GetComponent<Light>().enabled) {
				changeTime = Time.time + Random.Range(0.3f, 1f);
			} else {
				changeTime = Time.time + timeOff;
			}
		}
	}
}