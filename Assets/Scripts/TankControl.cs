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
    public bool alarmed = false;
    Rigidbody rig;
	// Use this for initialization
	void Start () {
        rig = GetComponent<Rigidbody>();
	}
    private void Update()
    {
        AkSoundEngine.SetRTPCValue("PlayerAcc", Mathf.Abs(Input.GetAxis("Gas")));
    }
    // Update is called once per frame
    void FixedUpdate () {
        //if(Input.GetKey(KeyCode.Space))
        //	rig.velocity += transform.forward * Time.deltaTime * speed;
        //if(Input.GetKey(KeyCode.LeftControl))
        //	rig.velocity -= transform.forward * Time.deltaTime * speed;


        rig.velocity -= transform.forward * Input.GetAxis("Gas") * Time.deltaTime * speed;
        //transform.position += transform.forward * Input.GetAxis("Gas") * Time.deltaTime * speed;
		transform.Rotate(-Vector3.up * -Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed);
        transform.Rotate(-Vector3.left * -Input.GetAxis("Vertical") * Time.deltaTime * rotateSpeed);


        pEmission = 100 * Mathf.Max(Mathf.Abs(Input.GetAxis("Gas")), Mathf.Abs(Input.GetAxis("Horizontal")));

        LeftMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;
        RightMotor.GetComponent<ParticleSystem>().emissionRate = pEmission;

        LeftMotor.GetComponent<ParticleSystem>().startSpeed = 3* Input.GetAxis("Gas") + Input.GetAxis("Horizontal");
        RightMotor.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Gas") -Input.GetAxis("Horizontal");

        LeftMotorDown.GetComponent<ParticleSystem>().emissionRate = pEmission;
        RightMotorDown.GetComponent<ParticleSystem>().emissionRate = pEmission;

        LeftMotorDown.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Gas") + Input.GetAxis("Horizontal");
        RightMotorDown.GetComponent<ParticleSystem>().startSpeed = 3 * Input.GetAxis("Gas") - Input.GetAxis("Horizontal");

    }

    public void redAlert(Transform pos)
    {
        
        if(!alarmed)
        {
            alarmed = true;
            StartCoroutine("turn180",pos);
        }
        
    }
    IEnumerator turn180(Transform pos)
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_TintColor",Color.red);
        yield return new WaitForSeconds(1);
        transform.position += 10 * (( pos.position - transform.position).normalized);
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_TintColor", Color.white);
        transform.LookAt(pos);
        rig.velocity = transform.forward * 10f;
        alarmed = false;
        yield return null;
    }
}
