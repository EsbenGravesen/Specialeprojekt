﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stationary : MonoBehaviour {

	public Color color;
	public Color activationColor;
	public int type;
    private float distanceToActive = 2f; 
	private bool active, locked = false;
	private Renderer render;
	private SphereManager sc;
    public bool amILinked = false;
   
    Color[] zoneCols = new Color[3];


    // Use this for initialization
    void Start () {
        amILinked = false;
        zoneCols[0] = new Color(0, 0.3f, 0, .6f);
        zoneCols[1] = new Color(0.3f, 0.3f, 0, .6f);
        zoneCols[2] = new Color(0.3f, 0, 0, .6f);
        render = GetComponent<Renderer> ();
		render.material.color = color;
		sc = transform.parent.GetComponent<SphereManager> ();
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<ParticleSystem> ().startColor = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];

        //AkSoundEngine.SetSwitch("Cycles", transform.parent.GetComponent<SphereManager>().switchCycle, gameObject);
        //Debug.Log("Switch: Cycles: " + transform.parent.GetComponent<SphereManager>().switchCycle, gameObject);
        //AkSoundEngine.SetSwitch("CycleObjects", "Zone" + (transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode() + 1), gameObject);
        //Debug.Log("Switch: CycleObjects: Zone" + (transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode() + 1));
    }

    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        AkSoundEngine.PostEvent("Puzzles_Stop", gameObject);
    }
    // Update is called once per frame
    void Update () {
        float dist = amILinked ? 0 : Vector3.Distance(transform.parent.GetChild(0).position, transform.position);
        AkSoundEngine.SetRTPCValue("DistZoneSphere", dist, gameObject);
        //Debug.Log("RTPC: DistZoneSphere: " + dist + " ID: " + gameObject.GetInstanceID());
       

        if (!sc.IsLocked()){
			if(Vector3.Distance(transform.parent.GetChild(0).position, transform.position) < distanceToActive)
            {
				if (!active)
                {
					GetComponent<ParticleSystem> ().startColor = GetComponent<ParticleSystem> ().startColor * 3f;
					var shape = GetComponent<ParticleSystem> ().shape;
					shape.radius = 0.3f;
				}
				active = true;
				
			}
			else{
				if(active)
                {
					GetComponent<ParticleSystem> ().startColor = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
					var shape = GetComponent<ParticleSystem> ().shape;
					shape.radius = 0.1f;
				}
				active = false;
			}
		}
	}

	public bool Activate()
    {
		if(sc.IsLocked())
        {
			transform.parent.parent.GetComponent<PuzzleManager> ().UnLink (transform.parent.gameObject);
            locked = false;
			return true;
		}
		if(!active)
        {
            locked = true;
			return false;
		}
		transform.parent.parent.GetComponent<PuzzleManager> ().Activated (transform.parent.GetSiblingIndex(), transform.GetSiblingIndex ());
       // locked = true;
		return true;
	}

	public bool IsActive(){
		return active;
	}

	public int GetOrbType(){
		return type;
	}
}
