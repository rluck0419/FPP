using UnityEngine;
using System.Collections;

public class PickupObject : MonoBehaviour {
	GameObject mainCamera;
	bool carrying;
	GameObject carriedObject;
	Rigidbody r;
	Pickupable p;
	Transform t;
	public float distance;
	public float smooth;
	public float thrust;
	public float rotation;

	// Use this for initialization
	void Start () {
		mainCamera = GameObject.FindWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		if(carrying) {
			carry(carriedObject);
			checkThrow();
			checkDrop();
		} else {
			pickup();
		}
	}

	// Pick up objects that have "Pickupable" script
	void pickup () {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				p = hit.collider.GetComponent<Pickupable>();
				if(p != null) {
					carrying = true;
					carriedObject = p.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
				}
			}
		}
	}

	// Check & continue carrying object after pickup
	void carry (GameObject o) {
		o.transform.position = Vector3.Lerp (
			o.transform.position,
			mainCamera.transform.position + mainCamera.transform.forward * distance,
			Time.deltaTime * smooth
			);
		o.transform.Rotate(Vector3.right * rotation);
	}

	// Check if item should be dropped after pickup
	void checkDrop () {
		if(Input.GetKeyDown (KeyCode.E)) {
			dropObject();
		}
	}

	// Drop objects that have been picked up
	void dropObject () {
		carrying = false;
		r.useGravity = true;
		carriedObject = null;
	}

	void checkThrow () {
		if(Input.GetMouseButtonDown(0)) {
			throwObject();
		}
	}

	void throwObject () {
		carrying = false;
		r.useGravity = true;
		r.AddForce(transform.forward * thrust);
		carriedObject = null;
	}
}
