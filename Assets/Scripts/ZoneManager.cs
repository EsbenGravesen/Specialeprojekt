using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ZoneManager : MonoBehaviour {
    Material mat;
    bool inPlay = false;
    const float _2pi = 2.0f * Mathf.PI;
    Vector3 p;
    Color[] zoneCols;
    Vector3 zeroVec;
	// Use this for initialization
    void Start () {
        if (zoneCols == null)
            zoneCols = new Color[3];
        zoneCols[0] = new Color(0, 1, 0, .6f);
        zoneCols[1] = new Color(1, 1, 0, .6f);
        zoneCols[2] = new Color(1, 0, 0, .6f);
        Material tempMaterial = new Material(GetComponent<MeshRenderer>().sharedMaterial);
        tempMaterial.color = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
        GetComponent<MeshRenderer>().sharedMaterial = tempMaterial;
        transform.localScale = Vector3.one * transform.parent.parent.GetComponent<PuzzleManager>().ZoneDiameter / 2.0f;
    }
	
	// Update is called once per frame
	void Update () {
        if (!inPlay)
        {
            float r1, cos, sin;
            GetComponent<MeshRenderer>().sharedMaterial.color = zoneCols[transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].ZoneColor.GetHashCode()];
            p = new Vector3(0, transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, 0f);
            float zRot;
            if (transform.parent.rotation.eulerAngles.x > 90f && transform.parent.rotation.eulerAngles.x < 270f)
                zRot = transform.parent.rotation.eulerAngles.z + 90f;
            else
                zRot = transform.parent.rotation.eulerAngles.z;
            if (transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.y == 0)
            {
                r1 = zRot * Mathf.Deg2Rad;
                cos = -Mathf.Sin(r1);
                sin = Mathf.Cos(r1);
                zeroVec = new Vector3(cos * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, sin * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, 0f);
            }
            else
            {
                r1 = transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.x /
                    transform.parent.GetComponent<SphereManager>().Zones[transform.GetSiblingIndex() - 1].position.y * _2pi + zRot * Mathf.Deg2Rad;
                cos = -Mathf.Sin(r1);
                sin = Mathf.Cos(r1);
                zeroVec = new Vector3(cos * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, sin * transform.parent.parent.GetComponent<PuzzleManager>().RotationDiameter / 2.0f, 0f);
            }
            //zeroVec = transform.InverseTransformPoint(zeroVec);
            //zeroVec.z = 0;
            //zeroVec = transform.TransformPoint(zeroVec);
            transform.localPosition = zeroVec;
            transform.localScale = Vector3.one * transform.parent.parent.GetComponent<PuzzleManager>().ZoneDiameter / 2.0f;
            
        }
    }

    void OnDestroy()
    {
        try { transform.parent.GetComponent<SphereManager>().Zones.RemoveAt(transform.GetSiblingIndex() - 1); }
        catch { }
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
