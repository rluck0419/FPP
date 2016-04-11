using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCont : MonoBehaviour 

{
	// States
	public bool controllable = false;
	private bool carrying = false;
	private bool crouching = false;
	private bool flashlighting = false;	
	private bool sprinting = false;
	private bool canLight = false;
	private bool canFreeze = false;
	public bool freeze = false;
	public bool climbing = false;

	// Main Game Objects / Components
	private GameObject playerObject;
	private GameObject mainCamera;
	private CharacterController controller;
	private GameObject flashlight;
	private GameObject clockKey;
	private GameObject unlockDoor;
	private Light light;

	// Telekinesis / Pick-Up Objects
	private GameObject inRangeObject;
	private GameObject carriedObject;
	private GameObject thrownObject;

	// Interactable Objects (Unique) & Related Variables
	private GameObject crawlSpace;
	private GameObject drawer;
	private GameObject hutchdoor;
	private GameObject guitar;
	

	// Audio (relates to interactables)
	public AudioClip guitarNote;
	public float noteVolume = 0.25f;
	private AudioSource audio;
	private AudioClip[] music;


	// Flashlight Variables
	// private float nextLightAllowed = 0;
	// public float lightCooldownTime = 5.0f;

	// Freeze-Time Variables
	private GameObject[] timeObjects;
	private Hashtable ObjectsVelocities = new Hashtable();
	private float nextUseAllowed = 0f;

	// Movement
	private float currentSpeed;
	public float speed = 4.0f;
	public float sprintSpeed = 6.0f;
	public float crouchSpeed = 1.0f;
	public float climbSpeed = 16.0f;
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
	private CrawlSpace cs;
	private Transform t;
	private Hourglass h;
	private Cassette c;
	private CassettePlayer cp;
	private FlashLight fl;
	private int cassetteCount = 0;
	private ParticleSystem.EmissionModule pse;
	private int lastNote;
	private bool firstNote = false;
	private bool secondNote = false;
	private bool noteCombo = false;

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
		playerObject = GameObject.Find("Player");
		controller = GetComponent<CharacterController>();
		mainCamera = GameObject.FindWithTag("MainCamera");
		timeObjects = GameObject.FindGameObjectsWithTag("Time");
		guitar = GameObject.Find("Guitar");
		flashlight = GameObject.Find("Flashlight");
		clockKey = GameObject.Find("ClockKey");
		unlockDoor = GameObject.Find("unlockDoor");
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
				flashlight.transform.position = flashlight.transform.position + (flashlight.transform.up * cameraCrouch);
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
				flashlight.transform.position = flashlight.transform.position - (flashlight.transform.up * cameraCrouch);
				currentSpeed = crouchSpeed;
			} else {
				controller.height = standingHeight;
				mainCamera.transform.position = mainCamera.transform.position + (mainCamera.transform.up * cameraCrouch);
				flashlight.transform.position = flashlight.transform.position + (flashlight.transform.up * cameraCrouch);				
				currentSpeed = speed;
			}
		}

		// Flashlight on / off
		if (canLight == true) {
			light = flashlight.GetComponent<Light>();
			if (Input.GetKeyDown(KeyCode.F)) {
				flashlighting = !flashlighting;
				if (flashlighting == true) {
					light.enabled = true;
				} else {
					light.enabled = false;
				}
			}
		}

		// actions dependent on the states (booleans) - carrying, sprinting, crouching
		if (carrying == true) {
			if (sprinting == true) {
				crouching = false;
				currentSpeed = sprintSpeed;
			} else if (crouching == true) {
				currentSpeed = crouchSpeed;
			} else {
				currentSpeed = speed;
			}
			if (carriedObject != null) {
				Carry(carriedObject);
				if (carriedObject.GetComponent<Key>() != null) {
					CheckUnlock();
				} else if (carriedObject.GetComponent<Guitar>() != null) {
					PlayGuitar();					
				}
				CheckThrow();
				CheckDrop();
			} else if (carriedObject != null) {
				// precautionary measure - possibly unnecessary ("if (carriedObject) != null)" above, "carrying = false" below)
				carrying = false;
			}
		} else if (carrying == false) {
			if (sprinting == true) {
				crouching = false;				
				currentSpeed = sprintSpeed;
			} else if (crouching == true) {
				currentSpeed = crouchSpeed;
			} else {
				currentSpeed = speed;
			}
			Pickup();
			Open();
			PlayGuitar();

		} 

		// Walking / Looking around
		if (climbing == false) {
			if (controller.isGrounded == true && controllable == true) {
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
		} else {
			if (Input.GetKey(KeyCode.W)) {
				playerObject.transform.position += Vector3.up / climbSpeed;
			} else if (Input.GetKey(KeyCode.D)) {
				playerObject.transform.position -= Vector3.up / climbSpeed;
			}
		}


		// Freeze Time
		if(canFreeze == true) {
			if(Input.GetKeyDown(KeyCode.T) && Time.time > nextUseAllowed) {
				foreach (GameObject i in timeObjects) {

					if(carrying==false||thrownObject!=null) {
						ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;

						// ValueType thisValue = (ValueType)myHashtable[theKey];    // retrieve a value for the given key
						// int howBig = myHashtable.Count;                          // get the number of items in the Hashtable
						// myHashtable.Remove(theKey);                              // remove the key & value pair from the Hashtable for the given key
			
						i.GetComponent<Rigidbody>().isKinematic = true;
					}
					else if (carrying==true  && carriedObject!=i) {
						ObjectsVelocities[i.name] = i.GetComponent<Rigidbody>().velocity;
						i.GetComponent<Rigidbody>().isKinematic = true;
					}
				}
				StartCoroutine(Cooldown(timeObjects, cooldownTime, clockKey, unlockDoor));
				nextUseAllowed = Time.time + cooldownTime;
			}
		}

		// if(canLight == true) {
		// 	if(Input.GetKeyDown(KeyCode.F) && Time.time > nextLightAllowed) {
		// 		light = flashlight.GetComponent<Light>();
		// 		light.enabled = true;
		// 		StartCoroutine(FlashlightCooldown(light, lightCooldownTime));
		// 		nextLightAllowed = Time.time + lightCooldownTime;
		// 	}
		// }
	}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public void Open () {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				hd = hit.collider.GetComponent<hutchDoor>();
				cs = hit.collider.GetComponent<CrawlSpace>();
				d = hit.collider.GetComponent<Drawer>();

				if(d != null && d.open==false) {
					drawer = d.gameObject;
                    StartCoroutine("OpenObj", d.transform);
					// drawer.transform.position = Vector3.Lerp (
					// 	drawer.transform.position,
					// 	drawer.transform.position - (drawer.transform.forward * 5),
					// 	Time.deltaTime * 5
					// );
					//drawer.transform.position = drawer.transform.position - (drawer.transform.forward * 0.45f);
					d.open = true;
				}
				else if (d != null && d.open==true) {
					drawer = d.gameObject;
                    StartCoroutine("CloseObj", d.transform);
                    // drawer.transform.position = Vector3.Lerp (
                    // 	drawer.transform.position,
                    // 	drawer.transform.position + (drawer.transform.forward * 5),
                    // 	Time.deltaTime * 5
                    // );
                    //drawer.transform.position = drawer.transform.position + (drawer.transform.forward * 0.45f);
					d.open = false;
				}

				if (cs != null) {
					crawlSpace = cs.gameObject;
					crawlSpace.transform.position = crawlSpace.transform.position + Vector3.right;
				}

				if (hd != null) {
					hutchdoor = hd.gameObject;
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

    private IEnumerator OpenObj(Transform Obj, float time)
    {
        float ctime = 0f;
        while (ctime < time) {
            Obj.position = Vector3.Lerp(
                             Obj.position,
                             Obj.position - (Obj.forward * 5),
                         	Time.deltaTime * 5);
            ctime += Time.deltaTime;
            yield return null; //Lets go fo thread to run other things
        }
        yield break;
    }

    private IEnumerator CloseObj(Transform Obj, float time)
    {
        float ctime = 0f;
        while (ctime < time) {
            Obj.position = Vector3.Lerp(
                             Obj.position,
                             Obj.position + (Obj.forward * 5),
                         	Time.deltaTime * 5);
            ctime += Time.deltaTime;
            yield return null;
        }
        yield break;
    }

    public void PlayGuitar () {
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
			cp = hit.collider.GetComponent<CassettePlayer>();

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
					if (i.gameObject == c.gameObject) {
						ps = hit.collider.GetComponent<ParticleSystem>();
						pse = ps.emission;
						pse.enabled = true;
					}
				}
			}

			if (cp != null) {
				hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
				foreach (Collider i in hitColliders) {
					if (i.gameObject == cp.gameObject) {
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
				cp = hit.collider.GetComponent<CassettePlayer>();
				fl = hit.collider.GetComponent<FlashLight>();

				if (h != null) {
					carrying = true;
					carriedObject = h.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime =1.5f;
					canFreeze = true;
					Debug.Log("...Hourglass collected? Press 'T' to play with time.");
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				} else if (c != null) {
					carrying = true;
					carriedObject = c.gameObject;
					Debug.Log("Cassette tape collected. Press 'P' to play.");
					cassetteCount++;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime = 1.5f;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				} else if (p != null) {
					carrying = true;
					carriedObject = p.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
				} else if (cp != null) {
					Debug.Log("Cassette player collected. Press 'M' to change the music.");
					carrying = true;
					carriedObject = cp.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime = 1.5f;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				} else if (fl != null) {
					carrying = true;
					canLight = true;
					Debug.Log("Flashlight collected. Press 'F' to turn it on and off.");
					carriedObject = fl.gameObject;
					r = carriedObject.GetComponent<Rigidbody>();
					r.useGravity = false;
					disappearTime = 1.5f;
					StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
				}
			}
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			if (Physics.Raycast(ray, out hit)) {
				p = hit.collider.GetComponent<Pickupable>();
				c = hit.collider.GetComponent<Cassette>();
				h = hit.collider.GetComponent<Hourglass>();
				cp = hit.collider.GetComponent<CassettePlayer>();
				fl = hit.collider.GetComponent<FlashLight>();
				
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
									Debug.Log("...Hourglass collected? Press 'T' to play with time.");
									StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
								}
							}
						}
					}
				} else if (c != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == c.gameObject) {
							carrying = true;
							carriedObject = c.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							cassetteCount++;
							Debug.Log("Cassette tape collected. Press 'P' to play.");
							disappearTime = 1.0f;
							StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
						}
					}
				} else if (cp != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == cp.gameObject) {
							carrying = true;
							carriedObject = cp.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							disappearTime = 1.0f;
							Debug.Log("Cassette player collected. Press 'M' to change the music.");
							StartCoroutine(WaitForSeconds(disappearTime, carriedObject));
						}
					}
				} else if (fl != null) {
					hitColliders = Physics.OverlapSphere(mainCamera.transform.position, pickupRadius);
					foreach (Collider i in hitColliders) {
						if (i.gameObject == fl.gameObject) {
							canLight = true;
							carrying = true;
							carriedObject = fl.gameObject;
							r = carriedObject.GetComponent<Rigidbody>();
							r.useGravity = false;
							disappearTime = 1.0f;
							Debug.Log("Flashlight collected. Press 'F' to turn it on... Something seems to be wrong with the wiring though?");
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
				mainCamera.transform.position + (mainCamera.transform.forward * distance),
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
					ps = carriedObject.GetComponent<ParticleSystem>();
					pse = ps.emission;
					pse.enabled = false;
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
		if (freeze == true) {
			carriedObject.GetComponent<Rigidbody>().isKinematic = true;

			throwCooldownTime = nextUseAllowed - Time.time;

			StartCoroutine("ThrowCooldown", throwCooldownTime);
			carrying = false;
			carriedObject.GetComponent<Rigidbody>().useGravity = true;
			carriedObject = null;
		} else {
			carrying = false;
			carriedObject.GetComponent<Rigidbody>().useGravity = true;
			carriedObject = null;
		}
	}

	public void CheckThrow () {
		if(carrying == true && Input.GetMouseButtonDown(0)) {
			carriedObject.GetComponent<Rigidbody>().isKinematic = false;
			ps = carriedObject.GetComponent<ParticleSystem>();
			pse = ps.emission;
			pse.enabled = false;
			ThrowObject();
		}
	}

	public void ThrowObject () {
		carrying = false;
		thrownObject = carriedObject;
		carriedObject = null;

		if (thrownObject.GetComponent<Lockable>() != null) {
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

	IEnumerator Cooldown (GameObject[] objects, int cooldownTime, GameObject clockKey, GameObject unlockDoor) {
		freeze = true;
		yield return new WaitForSeconds(cooldownTime);
		freeze = false;
		if (clockKey.GetComponent<MeshRenderer>().enabled == true) {
			// random number  used, should be adjusted later
			unlockDoor.transform.position = unlockDoor.transform.position + (Vector3.forward * 4);
		}
		objects = GameObject.FindGameObjectsWithTag("Time");

		foreach (GameObject i in objects) {
			Vector3 vel = (Vector3)ObjectsVelocities[i.name];
			i.GetComponent<Rigidbody>().velocity = vel;
			i.GetComponent<Rigidbody>().isKinematic = false;
		}
		// if (thrownObject != null) {
		// 	thrownObject.GetComponent<Rigidbody>().isKinematic = false;
		// 	thrownObject.GetComponent<Rigidbody>().useGravity = true;
		// 	thrownObject.GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
		// 	carriedObject = null;
		// }
		// if (carriedObject != null) {
		// 	carriedObject = null;
		// }
	}

	IEnumerator ThrowCooldown (float throwCooldownTime) {
		int seconds = (int) throwCooldownTime;
		yield return new WaitForSeconds(seconds);
	}

	// IEnumerator FlashlightCooldown (Light light, float lightCooldownTime) {
	// 	yield return new WaitForSeconds(lightCooldownTime * 2 / 3);
	// 	light.enabled = false;
	// 	Debug.Log("Flashlight: Dead! Recharging...");
	// 	yield return new WaitForSeconds(lightCooldownTime / 3);
	// 	Debug.Log("Flashlight: Recharged! Let there be light...");
	// }

	IEnumerator WaitForSeconds (float disappearTime, GameObject carriedObject) {
		yield return new WaitForSeconds(disappearTime);
		Destroy(carriedObject);
		carriedObject = null;
		// objects = GameObject.FindGameObjectsWithTag("Time");
		carrying = false;
	}
}
