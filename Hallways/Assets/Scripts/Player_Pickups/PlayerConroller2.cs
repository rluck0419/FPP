using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController2 : MonoBehaviour 

{
	// States
	public bool controllable = false;
	private bool carrying = false;
	private bool crouching = false;
	private bool sprinting = false;
	private bool canFreeze = false;
	private bool freeze = false;
	private bool climbing = false;


	// Main Game Objects / Components
	private GameObject mainCamera;
	private CharacterController controller;

	// Telekinesis / Pick-Up Objects
	private GameObject inRangeObject;
	private GameObject carriedObject;
	private GameObject thrownObject;

	// Interactable Objects (Unique) & Related Variables
	private GameObject drawer;
	private GameObject hutchdoor;
	private GameObject guitar;
	

	// Audio (relates to interactables)
	public AudioClip guitarNote;
	public float noteVolume = 0.25f;
	private AudioSource audio;
	private AudioClip[] music;

	// Freeze-Time Variables
	private GameObject[] timeObjects;
	private Hashtable ObjectsVelocities = new Hashtable();
	private float nextUseAllowed = 0f;

	// Movement
	private float currentSpeed;
	public float speed = 4.0f;
	public float sprintSpeed = 6.0f;
	public float crouchSpeed = 1.0f;
	public float jumpSpeed = 8.0f;
	public float gravity = 17.0f;
	public float rotateSpeed = 8.0f;
	public int cooldownTime = 4;
	private float throwCooldownTime;
	private Vector3 rotateDirection = Vector3.zero;
	private Vector3 moveDirection = Vector3.zero;

	// Crouching
	public float standingHeight = 0.75f;
	public float crouchHeight = 0.1f;
	public float cameraCrouch = 1.0f;

	// Pickups - Components
	private Collider[] hitColliders;
	private Rigidbody r;
	private Pickupable p;
	private ParticleSystem ps;
	private Guitar g;
	private Drawer d;
	private hutchDoor hd;
	private Transform t;
	private Hourglass h;
	private Cassette c;
	private int cassetteCount = 0;
	private ParticleSystem.EmissionModule pse;

	// Pickups - Carrying / Throwing
	private float mouseX = 0.0f;
	private float mouseY = 0.0f;
	public float disappearTime = 1.0f;
	public float distance = 1.0f;
	public float smooth = 7.0f;
	public float thrust = 512.0f;
	public float rotation = 2.0f;
	public float pickupRadius = 1.0f;
	public float pullRadius = 2.0f;
	public float pullForce = 1.0f;

	
	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<CharacterController>();
		mainCamera = GameObject.FindWithTag("MainCamera");
		timeObjects = GameObject.FindGameObjectsWithTag("Time");
		guitar = GameObject.FindWithTag("Guitar");
		currentSpeed = speed;
	}

	// Update is called once per frame
	void Update ()
	{
		// Basic Inputs
		// Sprinting 
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
			if (crouching == true) {
				crouching = !crouching;
				mainCamera.transform.position = mainCamera.transform.position + (mainCamera.transform.up * cameraCrouch);
				currentSpeed = speed;
			}
			crouching = false;
			sprinting = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) {
			sprinting = false;
		}

		// Crouching
		if (Input.GetKeyDown(KeyCode.C)) {
			crouching = !crouching;
			if (crouching == true) {				
				controller.height = crouchHeight;
				mainCamera.transform.position = mainCamera.transform.position - (mainCamera.transform.up * cameraCrouch);
				currentSpeed = crouchSpeed;
			} else {
				controller.height = standingHeight;
				mainCamera.transform.position = mainCamera.transform.position + (mainCamera.transform.up * cameraCrouch);
				currentSpeed = speed;
			}
		}

		// actions dependent on the states (booleans) - carrying, sprinting, crouching
		if (carrying == true) {
			if (sprinting == true) {
				crouching = false;
				currentSpeed = sprintSpeed;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				if (carriedObject != null) {
					Carry(carriedObject);
					if (carriedObject.GetComponent<Key>() != null) {
						CheckUnlock();
					}
					if (carriedObject.GetComponent<Guitar>() != null) {
						PlayGuitar();					
					}
					CheckThrow();
					CheckDrop();
				}
			} else if (crouching == true) {
				currentSpeed = crouchSpeed;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				if (carriedObject != null) {
					Carry(carriedObject);
					if (carriedObject.GetComponent<Key>() != null) {
						CheckUnlock();
					}
					if (carriedObject.GetComponent<Guitar>() != null) {
						PlayGuitar();
					}
					CheckThrow();
					CheckDrop();
				}
			} else {
				currentSpeed = speed;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				if (carriedObject != null) {
					Carry(carriedObject);
					if (carriedObject.GetComponent<Key>() != null) {
						CheckUnlock();
					}
					if (carriedObject.GetComponent<Guitar>() != null) {
						PlayGuitar();
					}
					CheckThrow();
					CheckDrop();
				}
			}
		} else {
			if (sprinting == true) {
				currentSpeed = sprintSpeed;
				crouching = false;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				Pickup();
				OpenDrawer();
				OpenHutch();
				PlayGuitar();
			} else if (crouching == true) {
				currentSpeed = crouchSpeed;
				sprinting = false;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				Pickup();
				OpenDrawer();
				OpenHutch();
				PlayGuitar();
			} else {
				currentSpeed = speed;
				Walking(controller, controllable, moveDirection, currentSpeed, jumpSpeed, gravity, mouseX, mouseY, rotateSpeed, rotateDirection);
				Pickup();
				OpenDrawer();
				OpenHutch();
				PlayGuitar();
			}
		} 


		// // Freeze Time
		// if(canFreeze == true) {
		// 	if(Input.GetKeyDown(KeyCode.T) && Time.time > nextUseAllowed) {
		// 		foreach (GameObject i in objects) {

		// 			if(carrying==false||thrownObject!=null) {
		// 				ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;

		// 				// ValueType thisValue = (ValueType)myHashtable[theKey];    // retrieve a value for the given key
		// 				// int howBig = myHashtable.Count;                          // get the number of items in the Hashtable
		// 				// myHashtable.Remove(theKey);                              // remove the key & value pair from the Hashtable for the given key
			
		// 				i.GetComponent<Rigidbody>().isKinematic = true;
		// 			}
		// 			else if (carrying==true  && carriedObject!=i) {
		// 				ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;
		// 				i.GetComponent<Rigidbody>().isKinematic = true;
		// 			}
		// 		}
		// 		StartCoroutine(Cooldown(objects, cooldownTime));
		// 		nextUseAllowed = Time.time + cooldownTime;
		// 	}
		// }
	}

	public void Walking (CharacterController controller, bool controllable, Vector3 moveDirection, float currentSpeed, float jumpSpeed, float gravity, float mouseX, float mouseY, float rotateSpeed, Vector3 rotateDirection) {
		if (controller.isGrounded && controllable == true) {
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= currentSpeed;
			if (Input.GetButton("Jump")) {
				moveDirection.y = jumpSpeed;
			}
		}
		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move(moveDirection * Time.deltaTime);

		mouseX += rotateSpeed * Input.GetAxis("Mouse X");
		mouseY += rotateSpeed * Input.GetAxis("Mouse Y");

		mouseY = Mathf.Clamp(mouseY, -90f, 90f);

		while (mouseX < 0f) {
			mouseX += 360f;
		}

		while (mouseX >= 360f) {
			mouseX -= 360f;
		}

		rotateDirection = new Vector3(-mouseY, mouseX, 0f);

		controller.transform.eulerAngles = rotateDirection;
	}

	public void OpenDrawer () {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				d = hit.collider.GetComponent<Drawer>();
				if(d != null && d.open==false) {
					drawer = d.gameObject;
					// drawer.transform.position = Vector3.Lerp (
					// 	drawer.transform.position,
					// 	drawer.transform.position - (drawer.transform.forward * 5),
					// 	Time.deltaTime * 5
					// );
					drawer.transform.position = drawer.transform.position - (drawer.transform.forward * 0.45f);
					d.open = true;
				}
				else if (d != null && d.open==true) {
					drawer = d.gameObject;
					// drawer.transform.position = Vector3.Lerp (
					// 	drawer.transform.position,
					// 	drawer.transform.position + (drawer.transform.forward * 5),
					// 	Time.deltaTime * 5
					// );
					drawer.transform.position = drawer.transform.position + (drawer.transform.forward * 0.45f);
					d.open = false;
				}
			}
		}
	}

	public void OpenHutch() {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				hd = hit.collider.GetComponent<hutchDoor>();
				

				if (hd != null) {
					hutchdoor = h.gameObject;
					if (hd.open == true) {
						if (hd.right == true) {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 135, 0);
						}
						else {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 315, 0);
						}
						hd.open = false;
					}
					else {
						if (hd.right == true) {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 30, 0);
						}
						else {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 60, 0);
						}
						hd.open = true;
					}
				}
			}
		}
	}

	public void PlayGuitar() {
		audio = guitar.GetComponent<AudioSource>();
		if(Input.GetKeyDown(KeyCode.Alpha1)||Input.GetKeyDown(KeyCode.Keypad1)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)||Input.GetKeyDown(KeyCode.Keypad2)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.12f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha3)||Input.GetKeyDown(KeyCode.Keypad3)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.26f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha4)||Input.GetKeyDown(KeyCode.Keypad4)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.33f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}	
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha5)||Input.GetKeyDown(KeyCode.Keypad5)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.5f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha6)||Input.GetKeyDown(KeyCode.Keypad6)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.68f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha7)||Input.GetKeyDown(KeyCode.Keypad7)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 1.89f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha8)||Input.GetKeyDown(KeyCode.Keypad8)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 2f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha9)||Input.GetKeyDown(KeyCode.Keypad9)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 2.245f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha0)||Input.GetKeyDown(KeyCode.Keypad0)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				g = hit.collider.GetComponent<Guitar>();
				if(g != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == guitar) {
							audio.pitch = 2.52f;
							audio.PlayOneShot(guitarNote, noteVolume);	
						}
					}
				}
			}
		}
	}

	public void Pickup () {
		int x = Screen.width / 2;
		int y = Screen.height / 2;
		
		Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			
			p = hit.collider.GetComponent<Pickupable>();
			c = hit.collider.GetComponent<Cassette>();
			
			if (p != null) {
				hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
				foreach (Collider i in hitColliders) {
					if (i.gameObject == p.gameObject) {
						ps = hit.collider.GetComponent<ParticleSystem>();
						pse = ps.emission;
						if (ps != null && pse.enabled == false) {
							inRangeObject = p.gameObject;
							pse.enabled = true;
						}
					}	
				}
			}

			if (c != null) {
				hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
				foreach (Collider i in hitColliders) {
					if (i.gameObject == guitar) {
						ps = hit.collider.GetComponent<ParticleSystem>();
						pse = ps.emission;
						pse.enabled = true;
					}
				}
			}
		}

		if (Input.GetMouseButtonDown(0)) {
			if (Physics.Raycast(ray, out hit)) {
				p = hit.collider.GetComponent<Pickupable>();
				c = hit.collider.GetComponent<Cassette>();
				h = hit.collider.GetComponent<Hourglass>();

				if (h != null) {
					carrying = true;
					carriedObject = h.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime =1.5f;
					canFreeze = true;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				}
				else if (c != null) {
					carrying = true;
					carriedObject = c.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime = 1.5f;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				}
				else if (p != null) {
					carrying = true;
					carriedObject = p.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			if (Physics.Raycast(ray, out hit)) {
				p = hit.collider.GetComponent<Pickupable>();
				c = hit.collider.GetComponent<Cassette>();
				h = hit.collider.GetComponent<Hourglass>();

				if (p != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == p.gameObject) {
							carrying = true;
							carriedObject = p.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							if (canFreeze == false) {
								if (h != null) {
									canFreeze = true;
									disappearTime = 1.0f;
									StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
								}
							}
						}
					}
				}

				if (c != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == c.gameObject) {
							carrying = true;
							carriedObject = c.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							cassetteCount++;
							Debug.Log(cassetteCount, carriedObject);
							disappearTime = 1.0f;
							StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
						}
					}
				}
			}
		}
	}

	// Check & continue carrying object after pickup
	public void Carry (GameObject o) {
		if (carrying==true && carriedObject!=null) {
			o.transform.position = Vector3.Lerp (
				o.transform.position,
				mainCamera.transform.position + mainCamera.transform.forward * distance,
				Time.deltaTime * smooth
				);
			o.transform.Rotate(Vector3.right * rotation);
		}
	}

	public void CheckUnlock () {
		if (carriedObject!=null && carriedObject.GetComponent<Key>()!=null) {
			if (carriedObject.GetComponent<Key>().inLock == true) {
				DropObject();
			}
		}
	}

	// Check if item should be dropped after pickup
	public void CheckDrop () {
		if (carriedObject != null) {
			c = carriedObject.GetComponent<Cassette>();
			h = carriedObject.GetComponent<Hourglass>();		
			if(c == null && h == null) {
				if(Input.GetKeyDown (KeyCode.E)) {
					DropObject();
				}
			}
			else {
				if(Input.GetKeyDown (KeyCode.E)) {
					if (h !=null) {
						canFreeze = true;
					}
					if (c != null) {
						cassetteCount++;
						Debug.Log(cassetteCount, carriedObject);
					}
					Destroy(carriedObject);
					carrying = false;
				}
			}
		}
	}

	// Drop objects that have been picked up
	public void DropObject () {
		carrying = false;
		r.useGravity = true;
		carriedObject = null;
	}

	public void CheckThrow () {
		if(carrying && Input.GetMouseButtonDown(0)) {
			carriedObject.GetComponent<Rigidbody>().isKinematic = false;
			ThrowObject();
		}
	}

	public void ThrowObject () {
		carrying = false;
		thrownObject = carriedObject;
		carriedObject = null;

		if (freeze == true) {
			thrownObject.GetComponent<Rigidbody>().isKinematic = true;
			
			throwCooldownTime = nextUseAllowed - Time.time;

			StartCoroutine("ThrowCooldown", throwCooldownTime);
			thrownObject = null;
		}
		else {
			thrownObject.GetComponent<Rigidbody>().useGravity = true;
			thrownObject.GetComponent<Rigidbody>().AddForce(transform.forward * thrust);

			thrownObject = null;
		}
	}

	// public void PlaySongs () {
        // GetComponent<AudioSource>().clip = music[UnityEngine.Random.Range(0,music.length)];
        // GetComponent<AudioSource>().Play();
        // Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
    // }

	IEnumerator Cooldown (GameObject[] objects, int cooldownTime) {
		freeze = true;
		yield return new WaitForSeconds(cooldownTime);
		freeze = false;
		objects = GameObject.FindGameObjectsWithTag("Time");

		foreach (GameObject i in objects) {
			if (i.GetComponent<Key>() == false) {
				Vector3 vel = (Vector3)ObjectsVelocities[i.name];
				i.GetComponent<Rigidbody>().velocity = vel;
				i.GetComponent<Rigidbody>().isKinematic = false;
			}
		}
		if (thrownObject != null) {
			thrownObject.GetComponent<Rigidbody>().isKinematic = false;
			thrownObject.GetComponent<Rigidbody>().useGravity = true;
			thrownObject.GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
			carriedObject = null;
		}
		else if (carriedObject != null) {
			carriedObject = null;
		}
	}

	IEnumerator ThrowCooldown (float throwCooldownTime) {
		int seconds = (int) throwCooldownTime;
		yield return new WaitForSeconds(seconds);
	}

	IEnumerator WaitForSeconds (float disappearTime, GameObject carriedObject) {
		yield return new WaitForSeconds(disappearTime);
		Destroy(carriedObject);
		carriedObject = null;
		// objects = GameObject.FindGameObjectsWithTag("Time");
		carrying = false;
	}
}