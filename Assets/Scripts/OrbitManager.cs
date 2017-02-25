using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitManager : MonoBehaviour {
    private bool rot = false;
    private float rotSpeed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(rot)
            transform.RotateAround(transform.parent.position, transform.parent.forward, 360f / rotSpeed * Time.deltaTime);
		
	}
    public void initRot(float speed)
    {
        rotSpeed = speed;
        rot = true;
    }
}
