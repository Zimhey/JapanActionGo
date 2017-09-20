using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLeftVRController : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        KeyBindingScript.LeftController = GetComponent<SteamVR_TrackedController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
