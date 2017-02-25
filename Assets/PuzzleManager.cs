using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour {

	private List<List<GameObject>> linked;

	// Use this for initialization
	void Start () {
		linked = new List<List<GameObject>> ();
	}

	public void Activated(int ringIndex, int orbIndex)
	{
		bool OtherActive = false;
		List<GameObject> newLink = new List<GameObject> ();
		for(int i = 0; i < transform.childCount; i++){
			if (i != ringIndex && !transform.GetChild(i).GetComponent<SphereControl>().IsLocked()){
				for(int j = 1; j < transform.GetChild(i).childCount; j++){
					if(transform.GetChild(i).GetChild(j).GetComponent<Stationary>().IsActive() && 
						transform.GetChild(ringIndex).GetChild(orbIndex).GetComponent<Stationary>().GetOrbType() == 
						transform.GetChild(i).GetChild(j).GetComponent<Stationary>().GetOrbType()){
						OtherActive = true;
						transform.GetChild (i).GetChild (0).GetComponent<Orbit> ().Visible (false);
						transform.GetChild (i).GetComponent<SphereControl> ().SetLocked (true);
						newLink.Add (transform.GetChild (i).gameObject);
					}
				}
			}
		}
		if(OtherActive){
			transform.GetChild (ringIndex).GetChild (0).GetComponent<Orbit> ().Visible (false);
			transform.GetChild (ringIndex).GetComponent<SphereControl> ().SetLocked (true);
			newLink.Add (transform.GetChild (ringIndex).gameObject);
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
				linked [index][i].GetComponent<SphereControl>().SetLocked(false);
				linked [index] [i].transform.GetChild (0).gameObject.GetComponent<Orbit> ().Visible (true);
			}
			linked.RemoveAt (index);
		}
	}
}
