using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankControl : MonoBehaviour {


	public float speed = 2;
	public float rotateSpeed = 2;
    public GameObject LeftMotor;
    public GameObject RightMotor;
    public GameObject LeftMotorDown;
    public GameObject RightMotorDown;
    float pEmission;
    float pDir;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
			transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * speed;
			transform.Rotate(-Vector3.up * -Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed);

        
        pEmission = 100 * Mathf.Max(Mathf.Abs(Input.GetAxis("Vertical")), Mathf.Abs(Input.GetAxis("Horizontal")));

        LeftMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;
        RightMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;

        LeftMotor.GetComponent<ParticleSystem>().startSpeed = 3* Input.GetAxis("Vertical") + Input.GetAxis("Horizontal");
        RightMotor.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Vertical") -Input.GetAxis("Horizontal");

        LeftMotorDown.GetComponent<ParticleSystem>().emissionRate = pEmission;
        RightMotorDown.GetComponent<ParticleSystem>().emissionRate = pEmission;

        LeftMotorDown.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Vertical") + Input.GetAxis("Horizontal");
        RightMotorDown.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Vertical") - Input.GetAxis("Horizontal");

    }
}
