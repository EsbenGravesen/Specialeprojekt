using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ZoneManager : MonoBehaviour {
    Material mat;
	// Use this for initialization
	void Start () {
        Material tempMaterial = new Material(GetComponent<MeshRenderer>().sharedMaterial);
        tempMaterial.color = Color.red;
        GetComponent<MeshRenderer>().sharedMaterial = tempMaterial;
    }
	
	// Update is called once per frame
	void Update () {
        GetComponent<MeshRenderer>().sharedMaterial.color = transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].zoneColor;
        transform.position = transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position;
    }
    void OnDestroy()
    {
        transform.parent.GetComponent<SphereManager>().Zones.RemoveAt(transform.GetSiblingIndex() - 1);
    }
}
