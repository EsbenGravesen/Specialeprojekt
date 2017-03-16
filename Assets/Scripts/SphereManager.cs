using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SphereManager : MonoBehaviour {
    public Vector2 CycleTempo;
    public static int ZoneAmount;
    public enum ColorType { Green, Yellow, Red};
    private float _tempo;
	int isLocked;
    [SerializeField]
    [Range(1, 8)]
    private int _switchCycle = 1;
    public string switchCycle
    {
        get
        {
            return "Cycle" + _switchCycle;
        }
    }
    [System.Serializable]
    public struct point
    {
        public ColorType ZoneColor;
        public Vector2 position;
    }
    public List<point> Zones;
    public float tempo {
        set {
            _tempo = value;
        }
        get {
			if (CycleTempo.y == 0 || CycleTempo.x == 0)
                return 1;
			else{
				float t = transform.parent.GetComponent<PuzzleManager>().baseTempo * CycleTempo.x / CycleTempo.y;
				return t == 0 ? 1 : t;
			}
        }
    }

	void Start () {
        transform.GetChild(0).GetComponent<OrbitManager>().initRot(tempo);
        ZoneManager[] zones = GetComponentsInChildren<ZoneManager>();
        for (int x = 0; x < zones.Length; ++x)
            zones[x].initialize();
        transform.GetChild(0).GetComponent<OrbitManager>().initRot(tempo);
		isLocked = 0;
		GetComponent<MeshRenderer> ().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public bool IsLocked(){
		return isLocked != 0;
	}

	public void SetLocked(int locked){
		isLocked = locked;
	}

	public GameObject GetLocked()
	{
		return transform.GetChild (isLocked).gameObject;
	}

    public void AddZone()
    {
        if (Zones == null)
            Zones = new List<point>();
        GameObject go = Instantiate(Resources.Load<GameObject>("Zone"));
		go.transform.SetParent(transform);
		go.transform.position = transform.position + transform.up * (transform.parent.GetComponent<PuzzleManager>().RotationDiameter / 2f);
        go.transform.localRotation = Quaternion.identity;
        Zones.Add(new point());
    }
}
