﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitManager : MonoBehaviour {
    private bool rot = false;
    private float rotSpeed;
	private bool isActive;
	// Use this for initialization
	void Start () {
		GetComponent<MeshRenderer> ().enabled = false;
	}
    void OnEnable()
    {
        AkSoundEngine.SetSwitch("Cycles", transform.parent.parent.GetComponent<PuzzleManager>().CycleSwitch, gameObject);
    }
    // Update is called once per frame
    void Update () {
        if(rot)
            transform.RotateAround(transform.parent.position, transform.parent.forward, 360f / rotSpeed * Time.deltaTime);
	}
   
    public void initRot(float speed)
    {
        rotSpeed = speed;
		if (speed == 0)
			rotSpeed = 1f;
        rot = true;
    }

	public void Visible(bool visible){
		isActive = visible;
		GetComponent<MeshRenderer> ().enabled = visible;
		var emi = GetComponent<ParticleSystem> ().emission;
		emi.enabled = visible;
	}

	public bool IsActive(){
		return isActive;
	}
}
