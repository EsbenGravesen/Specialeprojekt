using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LAVLINJERDOTDK : MonoBehaviour {

    public Vector3[] pos;
    List<Vector3> list;
	// Use this for initialization
	void Start () {
        list = new List<Vector3>();
        for (int i = 0; i < pos.Length; i++)
        {
            list.Add(pos[i]);
        }
        GetComponent<LineSpawnerTEst>().LineDrawer(list);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
