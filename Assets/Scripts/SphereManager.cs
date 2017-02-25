using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour {
    public Vector2 CycleTempo;
    public static int ZoneAmount;
<<<<<<< HEAD
    public float _tempo;
	bool isLocked;
    public enum ColorType { Green, Yellow, Red};
=======
    private float _tempo;
	int isLocked;

>>>>>>> c58c8ebb4458847d28a92e8bedfa3c5dcead1b59
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
<<<<<<< HEAD
        transform.GetChild(0).GetComponent<OrbitManager>().initRot(tempo);
        ZoneManager[] zones = GetComponentsInChildren<ZoneManager>();
        for (int x = 0; 0 < zones.Length; ++x)
            zones[x].initialize();
		isLocked = false;
=======
        print(tempo);
        transform.GetChild(0).GetComponent<OrbitManager>().initRot(tempo);
		isLocked = 0;
>>>>>>> c58c8ebb4458847d28a92e8bedfa3c5dcead1b59
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

    public void AddPoint()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Point"));
		go.transform.SetParent(transform);
		go.transform.position = transform.position + transform.up * (transform.parent.GetComponent<PuzzleManager>().RotationDiameter / 2f);
        go.transform.localRotation = Quaternion.identity;
        Zones.Add(new point());
    }
}
