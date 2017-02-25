using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControl : MonoBehaviour {


	public float speed = 2;
	public float rotateSpeed = 2;
    public GameObject LeftMotor;
    public GameObject RightMotor;
    float pEmission;
    float pDir;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
			transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * speed;
			transform.Rotate(-Vector3.up * -Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed);

        pEmission = 10 * Input.GetAxis("Vertical");

        LeftMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;
        LeftMotor.GetComponent<ParticleSystem>().startSpeed = Input.GetAxis("Horizontal");
        RightMotor.GetComponent<ParticleSystem>().startSpeed = -Input.GetAxis("Horizontal");
        RightMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;
    }
}
