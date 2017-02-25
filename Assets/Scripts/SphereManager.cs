using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour {
    public Vector2 CycleTempo;
    public static int ZoneAmount;
    private float _tempo;

    [System.Serializable]
    public struct point
    {
        public Color zoneColor;
        public Vector2 position;
        public int type;
    }
    public List<point> Zones = new List<point>();
    public float tempo {
        set {
            _tempo = value;
        }
        get {
            if (CycleTempo.y == 0)
                return 1;
            return _tempo * CycleTempo.x / CycleTempo.y;
        }
    }

	void Start () {
        print(tempo);
        transform.GetChild(0).GetComponent<OrbitManager>().initRot(tempo);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddPoint()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Point"));
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.SetParent(transform);
        Zones.Add(new point());
    }
}
