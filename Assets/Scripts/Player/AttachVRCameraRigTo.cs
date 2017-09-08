using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachVRCameraRigTo : MonoBehaviour {

    public GameObject CameraRig;
    public GameObject HMD;

    private Vector3 cameraPosition;
    private Vector3 rigPosition;
    private Vector3 cameraOffset;

    private CharacterController controller;

	// Use this for initialization
	void Start ()
    {
        controller = GetComponent<CharacterController>();
        
    }
	
	// Update is called once per frame
	void Update () {
        cameraPosition = HMD.transform.position;
        rigPosition = CameraRig.transform.position;
        cameraPosition.y = 0;
        rigPosition.y = 0;

        cameraOffset = rigPosition - cameraPosition;
        CameraRig.transform.position = transform.position + cameraOffset - new Vector3(0, controller.height / 2F);


    }
}
