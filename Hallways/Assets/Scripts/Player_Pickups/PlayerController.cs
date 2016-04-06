using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour 
{
	private GameObject mainCamera;
	public bool onLadder = false;
	private bool canFreeze = false;
	private bool carrying = false;
	public float heightFactor = 1.0f;
	private GameObject inRangeObject;
	private GameObject carriedObject;
	private GameObject thrownObject;
	private GameObject guitar;
	private Guitar g;
	private Rigidbody r;
	private Pickupable p;
	private ParticleSystem ps;
	private Drawer d;
	private hutchDoor h;
	private Transform t;
	private Cassette cassette;
	private int cassetteCount=0;
	private Hourglass hourglass;
	private ParticleSystem.EmissionModule pse;
	public float disappearTime = 1.0f;
	public float distance = 1.0f;
	public float smooth = 7.0f;
	public float thrust = 512.0f;
	public float rotation = 2.0f;
	public bool freeze = false;
	private Collider[] hitColliders;

	public float pickupRadius = 1.0f;
	public float pullRadius = 2.0f;
	public float pullForce = 1.0f;

	public bool controllable = false;

	public float speed = 4.0f;
	public float sprintSpeed = 6.0f;
	public float crouchSpeed = 1.0f;
	public float jumpSpeed = 8.0f;
	public float gravity = 17.0f;
	public float rotateSpeed = 8.0f;
	public int cooldownTime = 4;
	private float throwCooldownTime;

	private float s;
	private GameObject[] objects;
	// private Vector3[] velocities = new Vector3[0];
	private float mouseX = 0f;
	private float mouseY = 0f;
	private Vector3 rotateDirection = Vector3.zero;
	private Vector3 moveDirection = Vector3.zero;
	private CharacterController controller;
	private Hashtable ObjectsVelocities = new Hashtable();
	private float nextUseAllowed = 0f;
	// private string currentObject;
	public float standingHeight = .75f;
	public float crouchHeight = 0.1f;
	public float cameraCrouch = 1.0f;
	private bool crouching = false;
	private GameObject drawer;
	private GameObject hutchdoor;
	public AudioClip guitarNote;
	public float noteVolume = 0.25f;
	private AudioSource audio;

	private AudioClip[] music;

	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<CharacterController>();
		mainCamera = GameObject.FindWithTag("MainCamera");
		objects = GameObject.FindGameObjectsWithTag("Time");
		guitar = GameObject.FindWithTag("Guitar");
		s = speed;
		// PlaySongs();
	}

	// Update is called once per frame
	void Update()
	{
		// Sprinting 
		if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && crouching != true) {
			s = sprintSpeed;
		}

		if((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) && crouching != true) {
			s = speed;
		}
		if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && crouching == true) {
				crouching = !crouching;
				mainCamera.transform.position = mainCamera.transform.position + (mainCamera.transform.up * cameraCrouch);
				s = speed;
		}

		// Crouching (Toggle System)
		if(Input.GetKeyDown(KeyCode.C)) {
			crouching = !crouching;
			if (crouching) {				
				controller.height = crouchHeight;
				mainCamera.transform.position = mainCamera.transform.position - (mainCamera.transform.up * cameraCrouch);
				s = crouchSpeed;
			}
			else {
				controller.height = standingHeight;
				mainCamera.transform.position = mainCamera.transform.position + (mainCamera.transform.up * cameraCrouch);
				s = speed;
			}
		}

		if (onLadder == true) {
			// controllable = false;
			// if (Input.GetKeyDown(KeyCode.W)) {
				// transform.position += Vector3.up * heightFactor;
			// }
			// else if (Input.GetKeyDown(KeyCode.D)) {
				// transform.position -= Vector3.up * heightFactor;
			// }
		}

		// Basic Interaction Functions
		if (carrying) {
			if (carriedObject!= null) {
				carry(carriedObject);
				checkKey();
				checkThrow();
				checkDrop();
				if (carriedObject!=null) {
					if (carriedObject.GetComponent<Guitar>() != null) {
						playGuitar();
					}
				}
			}
		} else {
			pickup();
			openDrawer();
			openHutch();
			playGuitar();
		}


		// Freeze Time
		if(canFreeze == true) {
			if(Input.GetKeyDown(KeyCode.T) && Time.time > nextUseAllowed) {
				foreach (GameObject i in objects) {

					if(carrying==false||thrownObject!=null) {
						ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;

						// ValueType thisValue = (ValueType)myHashtable[theKey];    // retrieve a value for the given key
						// int howBig = myHashtable.Count;                          // get the number of items in the Hashtable
						// myHashtable.Remove(theKey);                              // remove the key & value pair from the Hashtable for the given key
			
						i.GetComponent<Rigidbody>().isKinematic = true;
					}
					else if (carrying==true && carriedObject!=i) {
						ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;
						i.GetComponent<Rigidbody>().isKinematic = true;
					}
				}
				StartCoroutine(Cooldown(objects, cooldownTime));
				nextUseAllowed = Time.time + cooldownTime;
			}
		}
	
		// Basic Movement
		if (controller.isGrounded && controllable)
		{
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= s;

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

		while (mouseX >=360f) {
			mouseX -= 360f;
		}

		rotateDirection = new Vector3(-mouseY, mouseX, 0f);

		controller.transform.eulerAngles = rotateDirection;
	}

	public void openDrawer() {
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

	public void openHutch() {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				h = hit.collider.GetComponent<hutchDoor>();
				

				if (h != null) {
					hutchdoor = h.gameObject;
					if (h.open == true) {
						if (h.right == true) {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 135, 0);
						}
						else {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 315, 0);
						}
						h.open = false;
					}
					else {
						if (h.right == true) {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 30, 0);
						}
						else {
							hutchdoor.transform.rotation = Quaternion.Euler(0, 60, 0);
						}
						h.open = true;
					}
				}
			}
		}
	}

	public void playGuitar() {
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

	public void pickup () {
		int x = Screen.width / 2;
		int y = Screen.height / 2;
		
		Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			
			p = hit.collider.GetComponent<Pickupable>();
			cassette = hit.collider.GetComponent<Cassette>();
			
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

			if (cassette != null) {
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
				cassette = hit.collider.GetComponent<Cassette>();
				hourglass = hit.collider.GetComponent<Hourglass>();

				if (hourglass != null) {
					carrying = true;
					carriedObject = hourglass.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime =1.5f;
					canFreeze = true;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				}
				else if (cassette != null) {
					carrying = true;
					carriedObject = cassette.gameObject;
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
				cassette = hit.collider.GetComponent<Cassette>();
				hourglass = hit.collider.GetComponent<Hourglass>();

				if (p != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == p.gameObject) {
							carrying = true;
							carriedObject = p.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							if (canFreeze == false) {
								if (hourglass != null) {
									canFreeze = true;
									disappearTime = 1.0f;
									StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
								}
							}
						}
					}
				}

				if (cassette != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == cassette.gameObject) {
							carrying = true;
							carriedObject = cassette.gameObject;
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
	public void carry (GameObject o) {
		if (carrying==true && carriedObject!=null) {
			o.transform.position = Vector3.Lerp (
				o.transform.position,
				mainCamera.transform.position + mainCamera.transform.forward * distance,
				Time.deltaTime * smooth
				);
			o.transform.Rotate(Vector3.right * rotation);
		}
	}

	public void checkKey () {
		if (carriedObject!=null && carriedObject.GetComponent<Key>()!=null) {
			if (carriedObject.GetComponent<Key>().inLock == true) {
				dropObject();
			}
		}
	}

	// Check if item should be dropped after pickup
	public void checkDrop () {
		if (carriedObject!=null) {
			cassette = carriedObject.GetComponent<Cassette>();
			hourglass = carriedObject.GetComponent<Hourglass>();		
			if(cassette==null && hourglass==null) {
				if(Input.GetKeyDown (KeyCode.E)) {
					dropObject();
				}
			}
			else {
				if(Input.GetKeyDown (KeyCode.E)) {
					if (hourglass !=null) {
						canFreeze = true;
					}
					if (cassette != null) {
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
	public void dropObject () {
		carrying = false;
		r.useGravity = true;
		carriedObject = null;
	}

	public void checkThrow () {
		if(carrying && Input.GetMouseButtonDown(0)) {
			carriedObject.GetComponent<Rigidbody>().isKinematic = false;
			throwObject();
		}
	}

	public void throwObject () {
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

	public void PlaySongs () {
        // GetComponent<AudioSource>().clip = music[UnityEngine.Random.Range(0,music.length)];
        // GetComponent<AudioSource>().Play();
        // Invoke("PlayNextSong", GetComponent<AudioSource>().clip.length);
    }

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