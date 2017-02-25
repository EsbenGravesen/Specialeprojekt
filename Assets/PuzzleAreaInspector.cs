using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PuzzleAreaInspector : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<SphereCollider>().radius = GetComponent<PuzzleManager>().AreaDiameter / 2.0f;
		
	}
}
