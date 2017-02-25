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
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    public void AddSphere()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Center of Rotation"));
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.SetParent(transform);
        mesh = new Mesh();
        mesh.name = "" + go.GetInstanceID();
        mesh.Clear();
        int nbSides = 24;
        float radius = RotationDiameter / 2.0f;
        int nbVerticesCap = nbSides + 1;
        Vector3[] vertices = new Vector3[nbVerticesCap+1];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;
        int sideCounter = 0;
        vertices[sideCounter] = Vector3.zero;
        for (vert = 1; vert < nbVerticesCap; vert++)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * radius, sin * radius, 0f);
        }
        vertices[nbVerticesCap] = vertices[1];
        Vector3[] normales = new Vector3[vertices.Length];
        for (vert = 0; vert < vertices.Length; vert++)
        {
            normales[vert] = Vector3.forward;
        }
        Vector2[] uvs = new Vector2[vertices.Length];
        sideCounter = 0;
        for (vert = 0; vert < nbVerticesCap; vert++)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert] = new Vector2(0f, t);
            uvs[vert] = new Vector2(1f, t);
        }
        int[] triangles = new int[(vertices.Length-1) * 3];
        sideCounter = 1;
        for(vert = 0; vert<triangles.Length-3; vert+=3)
        {
            triangles[vert] = 0;
            triangles[vert + 1] = sideCounter;
            triangles[vert + 2] = sideCounter + 1;
            sideCounter += 1;
        }
        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.GetChild(0).localPosition = go.transform.position + Vector3.up * radius;
        go.transform.GetChild(0).localScale = Vector3.one * (SphereDiameter / 2.0f);
        go.GetComponent<SphereManager>().tempo = baseTempo;
    }
}
