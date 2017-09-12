using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachTo : MonoBehaviour {

    private Vector3 startPosition;
    private Vector3 offset;

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
            offset = transform.position - to.transform.position;
        }
    }

    public bool UseWorldSpace;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (To != null)
            if (UseWorldSpace)
                gameObject.transform.position = To.transform.position + offset;
            else
                gameObject.transform.position = To.transform.position - startPosition;
	}
}
