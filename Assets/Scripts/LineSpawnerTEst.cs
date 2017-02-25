using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpawnerTEst : MonoBehaviour {

    public GameObject LinePrefab;
    public List<GameObject> ListOfGO;

	public List<GameObject> LineDrawer(List<Vector3> pos)
    {
        ListOfGO = new List<GameObject>();
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
                lineObj[xLine].transform.SetParent(transform);
                lineObj[xLine].GetComponent<LineRenderer>().SetPosition(0,pos[i]);
                lineObj[xLine].GetComponent<LineRenderer>().SetPosition(1, pos[k]);
                ListOfGO.Add(lineObj[xLine]);
                xLine++;
            }
        }
		return ListOfGO;
    }
}
