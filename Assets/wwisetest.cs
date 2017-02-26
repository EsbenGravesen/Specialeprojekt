using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wwisetest : MonoBehaviour  {
    bool test = false;
    int ctrl = 0;
    // Use this for initialization
    void Start()
    {
        AkSoundEngine.SetSwitch("")
    }
	// Update is called once per frame
	void Update () {
        if (test)
            wtest();
	}
    private void LateUpdate()
    {
        test = true;
    }
    void wtest()
    {
        if(test && ctrl == 0)
            AkSoundEngine.SetSwitch("Elements", "Sphere", gameObject);
        ctrl++;
    }
}
