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
        zoneCols[0] = new Color(0, 0.3f, 0, .6f);
        zoneCols[1] = new Color(0.3f, 0.3f, 0, .6f);
        zoneCols[2] = new Color(0.3f, 0, 0, .6f);
        render = GetComponent<Renderer> ();
		render.material.color = color;
		sc = transform.parent.GetComponent<SphereManager> ();
		GetComponent<MeshRenderer> ().enabled = false;
		GetComponent<ParticleSystem> ().startColor = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
	}

    void OnEnable()
    {
        AkSoundEngine.SetSwitch("Cycles", transform.parent.parent.GetComponent<PuzzleManager>().CycleSwitch, gameObject);
        AkSoundEngine.SetSwitch("Elements", "Zone" + (transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode() + 1), gameObject);
    }
    void OnDisable()
    {
        AkSoundEngine.PostEvent("Puzzles_Stop", gameObject);
    }
    // Update is called once per frame
    void Update () {
        AkSoundEngine.SetRTPCValue("DistZoneSphere", Vector3.Distance(transform.parent.GetChild(0).position, transform.position));
       

        if (!sc.IsLocked()){
			if(Vector3.Distance(transform.parent.GetChild(0).position, transform.position) < 1f){
				if (!active){
					GetComponent<ParticleSystem> ().startColor = GetComponent<ParticleSystem> ().startColor * 3f;
					var shape = GetComponent<ParticleSystem> ().shape;
					shape.radius = 0.3f;
				}
				active = true;
				
			}
			else{
				if(active){
					GetComponent<ParticleSystem> ().startColor = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
					var shape = GetComponent<ParticleSystem> ().shape;
					shape.radius = 0.1f;
				}
				active = false;
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
