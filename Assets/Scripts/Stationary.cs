using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stationary : MonoBehaviour {

	public Color color;
	public Color activationColor;
	public int type;
	private bool active;
	private Renderer render;
	private SphereManager sc;
    Color[] zoneCols = new Color[3];


    // Use this for initialization
    void Start () {
        zoneCols[0] = new Color(0, 1, 0, .6f);
        zoneCols[1] = new Color(1, 1, 0, .6f);
        zoneCols[2] = new Color(1, 0, 0, .6f);
        render = GetComponent<Renderer> ();
		render.material.color = color;
		sc = transform.parent.GetComponent<SphereManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(!sc.IsLocked()){
			if(Vector3.Distance(transform.parent.GetChild(0).position, transform.position) < 1f){
				active = true;
				render.material.color = render.material.color * 0.9f;
				
			}
			else{
				active = false;
                print("resetting");
				render.material.color = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
			}
		}
	}

	public bool Activate(){
		if(sc.IsLocked()){
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
