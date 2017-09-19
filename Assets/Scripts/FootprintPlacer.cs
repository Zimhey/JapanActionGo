using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootprintPlacer : MonoBehaviour {

    private LayerMask mask;
    private int levelLayer;
    private int dynamicObjectLayer;

    public GameObject[] LeftPrefabs;
    public GameObject[] RightPrefabs;
    public float Stride;
    public float Radius;
    public float FootprintLifetime;

    private CharacterController controller;
    private GameObject previousFootprint;
    private GameObject currentFootprint;
    private GameObject footPrintParent;

    private Vector3 lastLocation;
    private float distanceTraveled;
    private bool wasGrounded;

    private bool rightFootLast;
    private System.Random rand;

    public bool PlaceFootprintOnLanding;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if(controller == null) // something is wrong
        {
            Debug.LogWarning("FootprintPlacer on " + gameObject.name + " without a CharacterController. Removing FootprintPlacer from " + gameObject.name + ".");
            Destroy(this);
            return;
        }

        if(LeftPrefabs.Length == 0 || RightPrefabs.Length == 0)
        {
            Debug.LogWarning("FootprintPlacer on " + gameObject.name + " is missing footprint prefabs. Removing FootprintPlacer from " + gameObject.name + ".");
            Destroy(this);
            return;
        }

        footPrintParent = new GameObject(gameObject.tag + " Footprints");
        footPrintParent.transform.parent = GameManager.Instance.GameParent.transform;

        lastLocation = transform.position;

        levelLayer = LayerMask.NameToLayer("Level");
        dynamicObjectLayer = LayerMask.NameToLayer("DynamicObject");
        mask = 1 << levelLayer | 1 << dynamicObjectLayer;

        rand = new System.Random();
    }

    private void Update()
    {
        if (!wasGrounded && controller.isGrounded && PlaceFootprintOnLanding) // just landed
            PlaceFootprint();
        else if(controller.isGrounded)
        {
            distanceTraveled += Vector3.Distance(lastLocation, transform.position);
            lastLocation = transform.position;
            if (distanceTraveled > Stride)
                PlaceFootprint();
        }

        wasGrounded = controller.isGrounded;
    }

    private void PlaceFootprint()
    {
        Vector3 footPosition = transform.position + (rightFootLast? transform.right* Radius / 2f : -transform.right * Radius / 2f);
        Ray ray = new Ray(footPosition, Vector3.down);
        RaycastHit rayHit;

        if(Physics.Raycast(ray, out rayHit, controller.height, mask))
        {
            Vector3 position = rayHit.point + rayHit.normal * 0.001f;
            GameObject prefab = NextPrefab();

            previousFootprint = currentFootprint;

            // Spawn
            currentFootprint = Instantiate(prefab);
            currentFootprint.transform.position = position;
            currentFootprint.transform.rotation = transform.rotation;
            currentFootprint.transform.parent = footPrintParent.transform;
            currentFootprint.GetComponent<FootprintDecay>().SetLifeTime(FootprintLifetime);

            if (rayHit.collider.gameObject.layer == dynamicObjectLayer)
            {
                // attach footprint to dynamic objects
                AttachTo a = currentFootprint.gameObject.AddComponent<AttachTo>();
                a.To = rayHit.collider.gameObject;
                a.UseWorldSpace = true;
            }

            // Set Linked List
            if (previousFootprint != null)
                previousFootprint.GetComponent<FootprintList>().setNext(currentFootprint);
            currentFootprint.GetComponent<FootprintList>().setPrevious(previousFootprint);

            // Prep for next footprint
            distanceTraveled = 0f;
            rightFootLast = !rightFootLast;
        }
    }

    private GameObject NextPrefab()
    {
        GameObject[] array = rightFootLast ? RightPrefabs : LeftPrefabs;
        return array[rand.Next() % array.Length];
    }
    
}
