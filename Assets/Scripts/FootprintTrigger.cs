﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oni")){
            Destroy(gameObject);
        }
        if (other.CompareTag("Inu"))
        {
            Destroy(gameObject);
        }
    }
}
