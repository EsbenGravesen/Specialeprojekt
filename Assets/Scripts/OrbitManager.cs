using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitManager : MonoBehaviour {
    private bool rot = false;
    private float rotSpeed;
	private bool isActive;
	// Use this for initialization
	void Start () {
		
	}
    void OnEnable()
    {
        AkSoundEngine.SetSwitch("Elements", "Sphere", gameObject);
        AkSoundEngine.PostEvent("Puzzles_Play", gameObject);
    }
    // Update is called once per frame
    void Update () {
        if(rot)
            transform.RotateAround(transform.parent.position, transform.parent.forward, 360f / rotSpeed * Time.deltaTime);
	}
   
    public void initRot(float speed)
    {
        rotSpeed = speed;
		if (speed == 0)
			rotSpeed = 1f;
        rot = true;
    }

	public void Visible(bool visible){
		isActive = visible;
		GetComponent<MeshRenderer> ().enabled = visible;
	}

	public bool IsActive(){
		return isActive;
	}
}
