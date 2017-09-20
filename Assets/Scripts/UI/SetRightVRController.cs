using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRightVRController : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        KeyBindingScript.RightController = GetComponent<SteamVR_TrackedController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
