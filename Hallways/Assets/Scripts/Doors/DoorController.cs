using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
	void OnTriggerEnter(Collider Player)
	{
		if (Player.transform.tag == "Player")
		{
			GetComponent<Animation>().Play("Open");
		}
	}

	void OnTriggerExit(Collider Player)
	{
		if (Player.transform.tag == "Player")
		{
			GetComponent<Animation>().Play("Close");
		}
	}
}