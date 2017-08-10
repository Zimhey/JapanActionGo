using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintDecay : MonoBehaviour {

    private int timer;

	// Use this for initialization
	void Start () {
        timer = 3000;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        timer--;
        print(timer);

        if(timer <= 0)
        {
            Destroy(gameObject);
            print("Destroy");
        }
	}
}
