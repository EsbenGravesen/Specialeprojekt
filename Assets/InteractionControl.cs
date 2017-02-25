using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Fire1"))
        {
            Debug.Log("SHOOT");
            RaycastHit hitted;
            if (Physics.Raycast(transform.position, transform.forward, out hitted, 100f))
            {
                if (hitted.transform.gameObject.GetComponent<Stationary>() != null)
                {
                    Debug.Log(hitted.transform.gameObject.GetComponent<Stationary>().Activate());
                }
            }
        }
    }
}
