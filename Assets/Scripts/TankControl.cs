using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControl : MonoBehaviour {


	public float speed = 2;
	public float rotateSpeed = 2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("w")){
			transform.position += transform.forward * Time.deltaTime * speed;
		}
		if(Input.GetKey("s")){
			transform.position -= transform.forward * Time.deltaTime * speed;
		}
		if(Input.GetKey("a")){
			transform.Rotate(-Vector3.up * Time.deltaTime * rotateSpeed);
		}
		if(Input.GetKey("d")){
			transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);
		}
	}
}
