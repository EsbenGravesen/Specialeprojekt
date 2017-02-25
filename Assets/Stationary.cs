using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stationary : MonoBehaviour {

	public Color color;
	public Color activationColor;
	public int type;
	private bool active;
	private Renderer render;
	private SphereControl sc;


	// Use this for initialization
	void Start () {
		render = GetComponent<Renderer> ();
		render.material.color = color;
		sc = transform.parent.GetComponent<SphereControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(!sc.IsLocked()){
			if(Vector3.Distance(transform.parent.GetChild(0).position, transform.position) < 1f){
				active = true;
				render.material.color = activationColor;
				
			}
			else{
				active = false;
				render.material.color = color;
			}
		}
	}

	public bool Activate(){
		if(sc.IsLocked() && active){
			transform.parent.parent.GetComponent<PuzzleManager> ().UnLink (transform.parent.gameObject);
			return true;
		}
		if(!active){
			return false;
		}
		transform.parent.parent.GetComponent<PuzzleManager> ().Activated (transform.parent.GetSiblingIndex(), transform.GetSiblingIndex ());
		return true;
	}

	public bool IsActive(){
		return active;
	}

	public int GetOrbType(){
		return type;
	}
}
