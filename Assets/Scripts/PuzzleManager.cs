using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour {
    public float baseTempo;
    public float RotationDiameter;
    public float SphereDiameter;
    public float ZoneDiameter;
    Mesh mesh;
    bool meshCreated = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void AddSphere()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Center of Rotation"));
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.SetParent(transform);
    }
}
