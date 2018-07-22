using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
	[HideInInspector]
	public bool Dead = false;
	[HideInInspector]
	public float Speed = 0f;
	[HideInInspector]
	public float steerLeft = 0f;
	[HideInInspector]
	public float steerRight = 0f;
	[HideInInspector]
	public float[] distances = new float[3];
	[HideInInspector]
	public GameObject CheckpointsGO;
	[HideInInspector]
	public int score = 0;

	public Material aliveMat;
	public Material deadMat;

	float speedRate = 12.3f;
	float steerRate = 13f;
	float maxDistance = 10f;
	
	RaycastHit[] raycastHit = new RaycastHit[3];
	Vector3[] origins = new Vector3[3];
	Vector3[] directions = new Vector3[3];
	Vector3 basePos;

	Rigidbody rb;
	Renderer rend;
	float beginTime;

	GameObject[] Checkpoint;
	int checkpointCount = 0;

	void Start(){
		basePos = transform.position;
		beginTime = Time.time;

		rb = gameObject.GetComponent<Rigidbody>();
		rend = gameObject.GetComponent<Renderer>();
		rend.material = aliveMat;

		checkpointCount = CheckpointsGO.transform.childCount;
		Checkpoint = new GameObject[checkpointCount];

		for(int c=0; c<checkpointCount; c++){
			Checkpoint[c] = null;
		}
	}

	void FixedUpdate(){
		// If car is alive, move it with variables that comes from Neural Network's output layer.
		if(!Dead){
			// Acceleration.
			// rb.AddForce(transform.forward * Speed * speedRate, ForceMode.Acceleration);

			rb.AddForce(transform.forward * Speed * speedRate, ForceMode.Acceleration);

			// Steer turn & left.
			transform.Rotate(-transform.up * steerLeft * steerRate);
			transform.Rotate(transform.up * steerRight * steerRate);
		}
	}

	void Update(){
		if(!Dead){
			// Recalculate raycast position and direction.
			origins[0] = transform.position + transform.forward + (-transform.right/2) + (-transform.up/5);
			origins[1] = transform.position + transform.forward + (-transform.up/5);
			origins[2] = transform.position + transform.forward + (transform.right/2) + (-transform.up/5);
			directions[0] = transform.forward + -transform.right;
			directions[1] = transform.forward;
			directions[2] = transform.forward + transform.right;

			// Check raycasts, if intersects, store them in array. And draw them.
			for(int r=0; r<3; r++){
				if(Physics.Raycast(origins[r], directions[r], out raycastHit[r], maxDistance) && raycastHit[r].collider.tag == "Obstacle"){
					Debug.DrawRay(origins[r], directions[r] * raycastHit[r].distance, Color.blue);
					distances[r] = raycastHit[r].distance / maxDistance;
				}else{
					Debug.DrawRay(origins[r], directions[r] * maxDistance, Color.gray);
					distances[r] = maxDistance / maxDistance;
				}
			}

			// If 10 seconds passed, and car didnt move that much, it should die.
			if((Time.time - beginTime) > 5f && (Vector3.Distance(basePos, transform.position) < 3f)){
				killCar();
			}

			// If car slows down, kill it.
			if(Speed < 0.1f){
				killCar();
			}
		}
	}

	// When the car collide with something.
	void OnCollisionEnter(Collision collision){
		// If it collide with an obstacle, it should die.
		if(collision.collider.tag == "Obstacle"){
			killCar();
		}
	}

	// When the car passes from any checkpoint.
	void OnTriggerEnter(Collider collider){
		if(collider.tag == "Checkpoint"){
			bool exists = false;

			for(int c=0; c<checkpointCount; c++){
				if(Checkpoint[c] == collider.gameObject){
					exists = true;
					break;
				}
			}

			if(!exists){
				for(int c=0; c<checkpointCount; c++){
					if(Checkpoint[c] == null){
						Checkpoint[c] = collider.gameObject;
						break;
					}
				}

				score += 10;
			}
		}
	}

	public void killCar(){
		Dead = true;
		rend.material = deadMat;
	}

	public void resetCar(){
		Dead = false;
		rend.material = aliveMat;
		transform.position = basePos;
		transform.rotation = Quaternion.identity;
		beginTime = Time.time;
		score = 0;

		for(int c=0; c<checkpointCount; c++){
			Checkpoint[c] = null;
		}
	}
}
