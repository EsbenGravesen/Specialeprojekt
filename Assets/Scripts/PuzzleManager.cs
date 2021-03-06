﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour {

	private List<List<GameObject>> linked;
	private List<List<GameObject>> lines;

	public GameObject LinePrefab;
    private GameManager gm;
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
    private bool puzzleCompleted = false;
    private Transform Player;
    // Use this for initialization
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        linked = new List<List<GameObject>>();
        lines = new List<List<GameObject>>();
        Player = GameObject.Find("SpaceShuttle").transform;
        StartCoroutine(WaitForAFrame());
    }

    private IEnumerator WaitForAFrame() {
        yield return 0;
        AkSoundEngine.SetState("PuzzleCount", gameObject.name);
        Debug.Log("State: PuzzleCount: " + gameObject.name);
    }
        
    

    public string CycleSwitch {
        get
        {
            return "Cycle" + _CycleSwitch;
        }
    }
    void Update()
    {
        if (puzzleCompleted)
            return;
        if(Vector3.Distance(Player.position, transform.position) > AreaDiameter / 2.0f)
        {
            Player.GetComponent<TankControl>().redAlert(transform);
        }
        
    }

    private void OnEnable()
    {
        if (puzzleCompleted)
            return;
        
    }

    public void Activated(int ringIndex, int orbIndex)
	{
        if (puzzleCompleted)
            return;
        bool OtherActive = false;
		List<GameObject> newLink = new List<GameObject> ();
        int linkCount = 0;
		for(int i = 0; i < transform.childCount; i++)
        {
			if (i != ringIndex && !transform.GetChild(i).GetComponent<SphereManager>().IsLocked())
            {
				for(int j = 1; j < transform.GetChild(i).childCount; j++)
                {
					if(transform.GetChild(i).GetChild(j).GetComponent<Stationary>().IsActive() && 
						transform.GetChild(ringIndex).GetChild(orbIndex).GetComponent<Stationary>().GetOrbType() == 
						transform.GetChild(i).GetChild(j).GetComponent<Stationary>().GetOrbType())
                    {
						OtherActive = true;
						transform.GetChild (i).GetChild (0).GetComponent<OrbitManager> ().Visible (false);
						transform.GetChild (i).GetComponent<SphereManager> ().SetLocked (j);
                        AkSoundEngine.SetSwitch("ZoneState", "Linked", transform.GetChild(i).GetChild(j).gameObject);
                        Debug.Log("Set Switch: " + "ZoneState: Linked " + transform.GetChild(i).GetChild(j).gameObject);
                       

                        transform.GetChild(i).GetChild(j).GetComponent<Stationary>().amILinked = true;
						newLink.Add (transform.GetChild (i).gameObject);
					}
				}
			}
		}
		if(OtherActive){
			transform.GetChild (ringIndex).GetChild (0).GetComponent<OrbitManager> ().Visible (false);
			transform.GetChild (ringIndex).GetComponent<SphereManager> ().SetLocked (orbIndex);
			newLink.Add (transform.GetChild (ringIndex).gameObject);
            transform.GetChild(ringIndex).GetChild(orbIndex).GetComponent<Stationary>().amILinked = true;
            AkSoundEngine.SetSwitch("ZoneState", "Linked", transform.GetChild(ringIndex).GetChild(orbIndex).gameObject);
            Debug.Log("Set Switch: " + "ZoneState: Linked " + transform.GetChild(ringIndex).GetChild(orbIndex).gameObject);
            lines.Add(LineDrawer (newLink));
			linked.Add (newLink);
		}
        for(int x = 0; x<transform.childCount; ++x)
        {
            for (int i = 1; i < transform.GetChild(x).childCount; i++)
            {
                if (transform.GetChild(x).GetChild(i).GetComponent<Stationary>().amILinked)
                {
                    linkCount++;
                    break;
                }
            }
        }
        if (linkCount == transform.childCount)
            gm.ActivateNextPuzzle();
	}

	public void UnLink(GameObject ring){
        if (puzzleCompleted)
            return;
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
                for(int x = 1; x<linked[index][i].transform.childCount; ++x)
                {
                    linked[index][i].transform.GetChild(x).GetComponent<Stationary>().amILinked = false;
                    AkSoundEngine.SetSwitch("ZoneState", "Unlinked", linked[index][i].transform.GetChild(x).gameObject);
                }
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
		GameObject go = Instantiate(Resources.Load<GameObject>("Center of Rotation(Clone)"));
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
        
        Color lineColor = pos[0].transform.GetChild(1).GetComponent<ParticleSystem>().startColor;
        int linesNUM = 0;
		for (int i = 0; i < pos.Count; i++)
		{
			linesNUM += i;
		}
		GameObject[] lineObj = new GameObject[linesNUM];
		int xLine = 0;
		for (int i = 0; i < pos.Count; i++)
		{
			for(int k = 0; k < i; k++)
			{
				lineObj[xLine] = Instantiate(LinePrefab);
				lineObj[xLine].GetComponent<LineRenderer>().SetPosition(0,pos[i].GetComponent<SphereManager>().GetLocked().transform.position);
				lineObj[xLine].GetComponent<LineRenderer>().SetPosition(1, pos[k].GetComponent<SphereManager>().GetLocked().transform.position);
                lineObj[xLine].GetComponent<LineRenderer>().startColor = lineColor;
                lineObj[xLine].GetComponent<LineRenderer>().endColor = lineColor;
                ListOfGO.Add(lineObj[xLine]);
				xLine++;
			}
		}
		return ListOfGO;
	}

    public void ActivateHeleLortet()
    {
    }
    public void DeactivateHeleLortet()
    {
        puzzleCompleted = true;
        for(int x = 0; x<lines.Count; ++x)
        {
            for(int y = 0; y<lines[x].Count; ++y)
            {
                StartCoroutine(fadeLines(lines[x][y].GetComponent<LineRenderer>()));
            }
        }
        for(int x = 0; x<transform.childCount; ++x)
        {
            for(int y = 1; y<transform.GetChild(x).childCount; ++y) //skip sphere
            {
                StartCoroutine(fadeParticleSystems(transform.GetChild(x).GetChild(y).GetComponent<ParticleSystem>()));
            }
        }
    }

    private IEnumerator fadeLines(LineRenderer line)
    {
        Color start, end;
        start = line.startColor;
        end = start * gm.disabledGreyscale;
        end.a = 1;

        float t = 0;
        while (t < 2)
        {
            t += Time.deltaTime;
            line.startColor = Color.Lerp(start, Color.white, t / 2f);
            line.endColor = Color.Lerp(start, Color.white, t / 2f);
            yield return null;
        }
        t = 0;
        while (t < 4)
        {
            t += Time.deltaTime;
            line.startColor = Color.Lerp(Color.white, end, t / 4f);
            line.endColor = Color.Lerp(Color.white, end, t / 4f);
            yield return null;
        }
        yield break;
    }

    private IEnumerator fadeParticleSystems(ParticleSystem ps)
    {
        ps.gameObject.layer = LayerMask.NameToLayer("Default");
        Color start, end;
        start = ps.startColor;
        end = start * gm.disabledGreyscale;
        end.a = 1;
        float t = 0;
        while (t < 2)
        {
            t += Time.deltaTime;
            ps.startColor = Color.Lerp(start, Color.white, t / 2f);
            yield return null;
        }
        t = 0;
        while (t < 4)
        {
            t += Time.deltaTime;
            ps.startColor = Color.Lerp(Color.white, end, t / 4f);
            yield return null;
        }
        yield break;
    }
}
