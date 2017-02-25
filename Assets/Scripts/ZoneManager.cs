using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ZoneManager : MonoBehaviour {
    Material mat;
    bool inPlay = false;
    const float _2pi = 2.0f * Mathf.PI;
    Color[] zoneCols = new Color[3];
	// Use this for initialization
	void Start () {
            zoneCols[0] = new Color(0, 1, 0, .6f);
            zoneCols[1] = new Color(1, 1, 0, .6f);
            zoneCols[2] = new Color(1, 0, 0, .6f);
            Material tempMaterial = new Material(GetComponent<MeshRenderer>().sharedMaterial);
            tempMaterial.color = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
            GetComponent<MeshRenderer>().sharedMaterial = tempMaterial;
    }
	
	// Update is called once per frame
	void Update () {
        if (!inPlay)
        {
            GetComponent<MeshRenderer>().sharedMaterial.color = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
            if (transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.y * _2pi == 0)
                return;
            float r1 = transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.x /
                transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.y * _2pi;
            float cos = -Mathf.Sin(r1);
            float sin = Mathf.Cos(r1);
            transform.position = transform.parent.position + new Vector3(cos * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, sin * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, 0f);
        }
    }
    void OnDestroy()
    {
        transform.parent.GetComponent<SphereManager>().Zones.RemoveAt(transform.GetSiblingIndex() - 1);
    }
    public void initialize()
    {
        inPlay = true;
    }
    void OnApplicationQuit()
    {
        inPlay = false;
    }
}
