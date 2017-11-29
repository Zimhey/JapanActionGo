using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintList : MonoBehaviour {
    public FootprintList PreviousFootprint;
    public FootprintList NextFootprint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setPrevious(FootprintList footprint)
    {
        PreviousFootprint = footprint;
    }

    public void setNext(FootprintList footprint)
    {
        NextFootprint = footprint;
    }

    public FootprintList getPrevious()
    {
        return PreviousFootprint;
    }

    public FootprintList getNext()
    {
        return NextFootprint;
    }
}
