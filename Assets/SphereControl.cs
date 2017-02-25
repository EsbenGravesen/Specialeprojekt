using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereControl : MonoBehaviour {

	bool isLocked;

	// Use this for initialization
	void Start () {
		isLocked = false;
	}

	public bool IsLocked(){
		return isLocked;
	}

	public void SetLocked(bool locked){
		isLocked = locked;
	}
}
