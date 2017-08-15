﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public LayerMask levelLayer;
    public Inventory PlayerInventory;

    public float PullLeverRadius;
    
    public float DrawingDistance;
    public bool CanDrawAcrossObjects;
    public float DistanceDrawn;
    public List<GameObject> Marks;

    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer recent;
    private GameObject lastDrawnOn;

    private GameObject ofudaPrefab;
    private bool thrown;

    private Camera cam;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Resources.Load("Prefabs/ChalkMark") as GameObject;
        ofudaPrefab = Resources.Load("Prefabs/OfudaProjectile") as GameObject;
        cam = gameObject.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        // Use Lever
        if (Input.GetButtonDown("Use"))
        {
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.tag == "Lever" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
                    obj.SendMessage("Pull");
                else if(obj.tag == "Ofuda" && Vector3.Distance(transform.position, obj.transform.position) < PullLeverRadius)
                {
                    Destroy(obj);
                    PlayerInventory.Found(ItemType.Ofuda);
                }
            }
        }

        // Chalk Drawing
        if (Input.GetButton("Fire1") && PlayerInventory.CanUse(ItemType.Chalk))
            DrawChalk();
        else
            drawing = false;

        if (Input.GetButton("Fire2") && PlayerInventory.CanUse(ItemType.Ofuda))
            ThrowOfuda();
        else
            thrown = false;

    }

    void DrawChalk()
    {
        if (!drawing) // just started drawing
        {
            drawing = true;
            GameObject chalkMark = Instantiate(chalkPrefab);
            Marks.Add(chalkMark);
            recent = chalkMark.GetComponentInChildren<LineRenderer>();
        }

        Vector3 dir = cam.transform.rotation * Vector3.forward;

        Ray ray = new Ray(cam.transform.position, dir);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, DrawingDistance, levelLayer)) // if object to draw on
        {
            // If can't draw across objects, start a new line when objects change
            if (!CanDrawAcrossObjects && lastDrawnOn != rayHit.collider.gameObject)
            {
                GameObject chalkMark = Instantiate(chalkPrefab);
                Marks.Add(chalkMark);
                recent = chalkMark.GetComponentInChildren<LineRenderer>();
                lastDrawnOn = rayHit.collider.gameObject;
            }

            // Add point to the line render
            recent.positionCount++;
            recent.SetPosition(recent.positionCount - 1, rayHit.point);

            // Track distance drawn
            if (recent.positionCount > 1)
                DistanceDrawn += Vector3.Distance(recent.GetPosition(recent.positionCount - 2), rayHit.point);

            // Update Inventory
            if (DistanceDrawn > PlayerInventory.DistancePerCharge)
            {
                PlayerInventory.Used(ItemType.Chalk);
                DistanceDrawn -= PlayerInventory.DistancePerCharge;
            }

        }
        else
            drawing = false;
    }

    void ThrowOfuda()
    {
        if(!thrown)
        {
            Debug.Log("Throwing Ofuda");
            Instantiate(ofudaPrefab, cam.transform.position + cam.transform.forward, cam.transform.rotation);
            PlayerInventory.Used(ItemType.Ofuda);
            thrown = true;
        }
    }


    void Die()
    {
        Debug.Log("I've been killed!");
    }


}
