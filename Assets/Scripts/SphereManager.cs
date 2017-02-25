using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour {
    public Vector2 CycleTempo;
    public static int ZoneAmount;
    public float _tempo;
	bool isLocked;
    public enum ColorType { Green, Yellow, Red};
    [System.Serializable]
    public struct point
    {
        public ColorType ZoneColor;
        public Vector2 position;
    }
    public List<point> Zones = new List<point>();
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
        for (int x = 0; 0 < zones.Length; ++x)
            zones[x].initialize();
		isLocked = false;
		GetComponent<MeshRenderer> ().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public bool IsLocked(){
		return isLocked;
	}

	public void SetLocked(bool locked){
		isLocked = locked;
	}

    public void AddPoint()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Point"));
		go.transform.SetParent(transform);
		go.transform.position = transform.position + transform.up * (transform.parent.GetComponent<PuzzleManager>().RotationDiameter / 2f);
        go.transform.localRotation = Quaternion.identity;
        Zones.Add(new point());
    }
}
