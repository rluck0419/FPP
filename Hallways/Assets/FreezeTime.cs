using UnityEngine;
using System.Collections;

public class FreezeTime : MonoBehaviour {
	float pauseEndTime;

	// Use this for initialization
	void Start () {
		slowTime();
	}

	void slowTime () {
		if(Input.GetKeyDown (KeyCode.R)) {
			StartCoroutine("slow");	
		}
	}

	IEnumerator slow(float pauseEndTime){
		Time.timeScale = 0.1f;
    	pauseEndTime = Time.realtimeSinceStartup + 10;
   		while (Time.realtimeSinceStartup < pauseEndTime) 
   		{
   			yield return 0;
    	}
	    Time.timeScale = 1;
	}
}
