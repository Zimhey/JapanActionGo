using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassPin : MonoBehaviour {

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        transform.localRotation = Quaternion.Euler(0, 360 - transform.parent.rotation.eulerAngles.y, 0);
    }
}
