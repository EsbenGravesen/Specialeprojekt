using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour {

	private List<List<GameObject>> linked;
	private List<List<GameObject>> lines;

	public GameObject LinePrefab;

	public float baseTempo;
	public float RotationDiameter;
	public float SphereDiameter;
    public float ZoneDiameter;
	public float AreaDiameter;
	Mesh mesh;
	bool meshCreated = false;
    [SerializeField]
    [Range(1, 8)]
    int _CycleSwitch = 1;

	// Use this for initialization
	void Start () {
		linked = new List<List<GameObject>> ();
		lines = new List<List<GameObject>> ();
	}

    public string CycleSwitch {
        get
        {
            return "Cycle" + _CycleSwitch;
        }
    }
    private void OnEnable()
    {
        AkSoundEngine.SetState("PuzzleCount", gameObject.name);
    }

    public void Activated(int ringIndex, int orbIndex)
	{
		bool OtherActive = false;
		List<GameObject> newLink = new List<GameObject> ();
		for(int i = 0; i < transform.childCount; i++){
			if (i != ringIndex && !transform.GetChild(i).GetComponent<SphereManager>().IsLocked()){
				for(int j = 1; j < transform.GetChild(i).childCount; j++){
					if(transform.GetChild(i).GetChild(j).GetComponent<Stationary>().IsActive() && 
						transform.GetChild(ringIndex).GetChild(orbIndex).GetComponent<Stationary>().GetOrbType() == 
						transform.GetChild(i).GetChild(j).GetComponent<Stationary>().GetOrbType()){
						OtherActive = true;
						transform.GetChild (i).GetChild (0).GetComponent<OrbitManager> ().Visible (false);
						transform.GetChild (i).GetComponent<SphereManager> ().SetLocked (j);
						newLink.Add (transform.GetChild (i).gameObject);
					}
				}
			}
		}
		if(OtherActive){
			transform.GetChild (ringIndex).GetChild (0).GetComponent<OrbitManager> ().Visible (false);
			transform.GetChild (ringIndex).GetComponent<SphereManager> ().SetLocked (orbIndex);
			newLink.Add (transform.GetChild (ringIndex).gameObject);

			lines.Add(LineDrawer (newLink));
			linked.Add (newLink);
		}
	}

	public void UnLink(GameObject ring){
		int index = 0;
		bool found = false;
		for(int i = 0; i < linked.Count; i++){
			for(int j = 0; j < linked[i].Count; j++){
				if(linked[i][j] == ring){
					index = i;
					found = true;
					goto searching;
				}
			}
		}
		searching:
		if(found){
			for(int i = 0; i < linked[index].Count; i++){
				linked [index][i].GetComponent<SphereManager>().SetLocked(0);
				linked [index] [i].transform.GetChild (0).gameObject.GetComponent<OrbitManager> ().Visible (true);
			}
			for(int i = 0; i < lines[index].Count; i++){
				Destroy (lines [index] [i]);
			}
			linked.RemoveAt (index);
			lines.RemoveAt (index);
		}
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
		go.transform.GetChild (0).localPosition = Vector3.zero + go.transform.up * radius;
		go.transform.GetChild(0).localScale = Vector3.one * (SphereDiameter / 2.0f);

	}

	public List<GameObject> LineDrawer(List<GameObject> pos)
	{
		List<GameObject> ListOfGO = new List<GameObject>();
		int linesNUM = 0;
		for (int i = 0; i < pos.Count; i++)
		{
			linesNUM += i;
		}
		print("positions: " + pos.Count + " Lines: " + linesNUM);
		GameObject[] lineObj = new GameObject[linesNUM];
		int xLine = 0;
		for (int i = 0; i < pos.Count; i++)
		{
			for(int k = 0; k < i; k++)
			{
				lineObj[xLine] = Instantiate(LinePrefab);
				lineObj[xLine].GetComponent<LineRenderer>().SetPosition(0,pos[i].GetComponent<SphereManager>().GetLocked().transform.position);
				lineObj[xLine].GetComponent<LineRenderer>().SetPosition(1, pos[k].GetComponent<SphereManager>().GetLocked().transform.position);
				ListOfGO.Add(lineObj[xLine]);
				xLine++;
			}
		}
		return ListOfGO;
	}
}
