using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintList : MonoBehaviour {
    public GameObject PreviousFootprint;
    public GameObject NextFootprint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setPrevious(GameObject footprint)
    {
        PreviousFootprint = footprint;
    }

    public void setNext(GameObject footprint)
    {
        NextFootprint = footprint;
    }

    public GameObject getPrevious()
    {
        return PreviousFootprint;
    }

    public GameObject getNext()
    {
        return NextFootprint;
    }
}
