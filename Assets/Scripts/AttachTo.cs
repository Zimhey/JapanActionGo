﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachTo : MonoBehaviour {

    private Vector3 startPosition;
    private GameObject to;
    public GameObject To
    {
        get
        {
            return to;
        }
        set
        {
            to = value;
            startPosition = to.transform.position;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(To != null)
            gameObject.transform.position = To.transform.position - startPosition;
	}
}
