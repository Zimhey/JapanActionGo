using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour {

    public float UseRadius;
    public List<GameObject> Marks;
    public float DrawingDistance;
    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer recent;
    private Camera cam;


	// Use this for initialization
	void Start () {
        chalkPrefab = Resources.Load("Prefabs/ChalkMark") as GameObject;
        cam = gameObject.GetComponentInChildren<Camera>();
    }
	
	// Update is called once per frame
	void Update () {

        // Use Lever
        if (Input.GetButtonDown("Use"))
        {

            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.tag == "Lever" && Vector3.Distance(transform.position, obj.transform.position) < UseRadius)
                    obj.SendMessage("Pull");
            }
        }

        // Chalk Drawing
        if (Input.GetButton("Fire1"))
        {
            if (!drawing) // just started drawing
            {
                drawing = true;
                GameObject chalkMark = Instantiate(chalkPrefab);
                Marks.Add(chalkMark);
                recent = chalkMark.GetComponentInChildren<LineRenderer>();
            }

            Vector3 dir = cam.transform.rotation * new Vector3(0, 0, 1);
            
            Ray ray = new Ray(cam.transform.position, dir);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, DrawingDistance))
            {
                recent.positionCount++;
                recent.SetPosition(recent.positionCount - 1, rayHit.point);
            }
            else
                drawing = false;
        }
        else
            drawing = false;

    }
}
