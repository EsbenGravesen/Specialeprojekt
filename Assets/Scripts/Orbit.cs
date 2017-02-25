using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {

	public Color color;
	public float speed = 1;
	private bool isActive;
	private Renderer render;

	// Use this for initialization
	void Start () {
		render = GetComponent<Renderer> ();
		render.material.color = color;
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (transform.parent.position, Vector3.forward, 360 * Time.deltaTime * 1 / speed);
	}

	public void Visible(bool visible){
		isActive = visible;
		GetComponent<MeshRenderer> ().enabled = visible;
	}

	public bool IsActive(){
		return isActive;
	}
}
