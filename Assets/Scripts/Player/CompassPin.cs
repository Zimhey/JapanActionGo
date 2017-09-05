using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassPin : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Quaternion quat = Quaternion.FromToRotation(transform.parent.rotation.eulerAngles, Vector3.forward);
        transform.rotation = quat;
    }
	
	// Update is called once per frame
	void Update () {
        Quaternion quat = Quaternion.FromToRotation(transform.parent.rotation.eulerAngles, Vector3.forward);
        transform.rotation = quat;
	}
}
