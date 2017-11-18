using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class PlayerActions : MonoBehaviour
{
    private LayerMask drawingLayerMask;
    private int levelLayer;
    private int dynamicObjectLayer;

    public Inventory PlayerInventory;
    
    public float DrawingDistance;
    public float DistanceDrawn;
    private GameObject chalkMarksParent;

    private bool drawing;
    private GameObject chalkPrefab;
    private LineRenderer currentMark;
    private Vector3 chalkFaceNormal;
    private Vector3 lastDrawnPoint;
    private GameObject lastObjectDrawnOn;

    // Analytics
    public float TimeBetweenDrawings;
    private float lastDrawTime;
    private bool isDrawingToPush;
    private List<LineRenderer> linesToPush;
    int drawingEventID;
    
    private bool isUsingCompass;
    public bool IsUsingCompass
    {
        get
        {
            return isUsingCompass;
        }
        set
        {
            if(!isUsingCompass && value) // if wasn't using compass and now using compass, report to analytics
                GameManager.Instance.UsedItem(ItemType.Compass);
            isUsingCompass = value;
            Compass.SetActive(value);
        }
    }

    private bool isUsingMirror;
    public bool IsUsingMirror
    {
        get
        {
            return isUsingMirror;
        }
        set
        {
            if (!isUsingMirror && value) // if wasn't using mirror and now using mirror, report to analytics
                GameManager.Instance.UsedItem(ItemType.Mirror);
            isUsingMirror = value;
            Mirror.SetActive(value);
        }
    }

    // Ofuda
    private GameObject ofudaPrefab;
    private GameObject thrownOfudaParent;
    private bool thrown;

	public GameObject DrawingHand;
	private SteamVR_TrackedController drawingController;
	public GameObject ThrowingHand;
	private SteamVR_TrackedController throwingController;

    public GameObject Compass;
    public GameObject Mirror;

    private Camera cam;

    // Use this for initialization
    void Start()
    {
        chalkPrefab = Actors.Prefabs[ActorType.Chalk_Mark];
        ofudaPrefab = Actors.Prefabs[ActorType.Ofuda_Projectile];
        cam = Camera.main;
        chalkMarksParent = new GameObject("Chalk Marks");
        chalkMarksParent.transform.parent = GameManager.Instance.GameParent.transform;
        thrownOfudaParent = new GameObject("Thrown Ofuda");
        thrownOfudaParent.transform.parent = GameManager.Instance.GameParent.transform;
        levelLayer = LayerMask.NameToLayer("Level");
        dynamicObjectLayer = LayerMask.NameToLayer("DynamicObject");
        drawingLayerMask =  1 << levelLayer | 1 << dynamicObjectLayer;

        linesToPush = new List<LineRenderer>();

        if (Compass != null)
            Compass.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        TryDraw();
        TryThrow();
        TryCompass();
        TryMirror();
    }

    public void TryDraw()
    {
        bool attemptDraw;
        if (!UnityEngine.XR.XRDevice.isPresent)
            attemptDraw = (Input.GetKey(KeyBindingScript.buttons["Draw"]) || Input.GetKey(KeyBindingScript.controller["C_Draw"])); // TODO move this all into KeyBindScript with one function to call
        else
            if (KeyBindingScript.LeftController != null && KeyBindingScript.RightController != null) // TODO move this into KeyBindScript
                attemptDraw = KeyBindingScript.DrawPressedVR();
            else
                attemptDraw = false;

        if (attemptDraw && PlayerInventory.CanUse(ItemType.Chalk))
            DrawChalk();
        else
            drawing = false;

        // analytics
        if (drawing)
        {
            if (!isDrawingToPush)
            {
                isDrawingToPush = true;
                drawingEventID = GameManager.Instance.UsedItem(ItemType.Chalk);
            }

            lastDrawTime = Time.time;
        }
        else if (isDrawingToPush && Time.time > lastDrawTime + TimeBetweenDrawings)
        {
            GameManager.Instance.MarkDrawn(drawingEventID, linesToPush);
            linesToPush.Clear();
            isDrawingToPush = false;
        }
    }

    public void TryThrow()
    {
        bool attemptThrow;
        if (!UnityEngine.XR.XRDevice.isPresent)
            attemptThrow = (Input.GetKeyDown(KeyBindingScript.buttons["Throw"]) || Input.GetKey(KeyBindingScript.controller["C_Throw"]));
        else
        {
            if (KeyBindingScript.LeftController != null && KeyBindingScript.RightController != null)
            {
                attemptThrow = KeyBindingScript.ThrowPressedVR();
            }
            else
            {
                attemptThrow = false;
            }
        }

        if (attemptThrow && PlayerInventory.CanUse(ItemType.Ofuda))
            ThrowOfuda();
        else
            thrown = false;
    }

    public void TryMirror()
    {
        if (!PlayerInventory.CanUse(ItemType.Mirror))
            return; // can't use mirror
        if(GameManager.Instance.PlayersVRType == VirtualRealityType.None)
        {
            if (Input.GetKey(KeyCode.E))
            {
                IsUsingMirror = true;
                Mirror.transform.localRotation = Quaternion.Euler(-90, -45, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                IsUsingMirror = true;
                Mirror.transform.localRotation = Quaternion.Euler(-90, 45, 0);
            }
            else if (Input.GetKey(KeyCode.R))
            {
                IsUsingMirror = true;
                Mirror.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }
            else
                IsUsingMirror = false;
        }
    }

    public void TryCompass()
    {
        if (!PlayerInventory.CanUse(ItemType.Compass))
            return; // can't use compass
        if (GameManager.Instance.PlayersVRType == VirtualRealityType.None)
        {
            if (Input.GetButtonDown("Use_Compass") && Compass != null)
            {
                IsUsingCompass = true;
            }

            if (Input.GetButtonUp("Use_Compass") && Compass != null)
            {
                IsUsingCompass = false;
            }
        }
    }

    public GameObject StartNewMark()
    {
        GameObject chalkMark = Instantiate(chalkPrefab);
        chalkMark.transform.parent = chalkMarksParent.transform;
        currentMark = chalkMark.GetComponentInChildren<LineRenderer>();
        linesToPush.Add(currentMark);
        return chalkMark;
    }

    void DrawChalk()
    {
        Transform pointer;

        if (GameManager.Instance.PlayersVRType == VirtualRealityType.None)
            pointer = cam.transform;
        else
            pointer = DrawingHand.transform;

		Vector3 dir = pointer.transform.rotation * Vector3.forward; 
		Ray ray = new Ray(pointer.transform.position, dir);	         
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, DrawingDistance, drawingLayerMask)) // if object to draw on
        {

            // if first segment or different face normals or moved onto a dynamic object
            if (!drawing || rayHit.normal != chalkFaceNormal || (lastObjectDrawnOn != rayHit.collider.gameObject && rayHit.collider.gameObject.layer == dynamicObjectLayer))
            {
                drawing = true;
                StartNewMark();
                currentMark.transform.rotation = Quaternion.LookRotation(-rayHit.normal);
                chalkFaceNormal = rayHit.normal;
                lastObjectDrawnOn = rayHit.collider.gameObject;
                if (rayHit.collider.gameObject.layer == dynamicObjectLayer)
                {
                    AttachTo a = currentMark.gameObject.AddComponent<AttachTo>();
                    a.To = rayHit.collider.gameObject;
                }
            }

            // prevent z fighting
            Vector3 offset = rayHit.normal * 0.002f;
            Vector3 position = (Vector3)(currentMark.transform.worldToLocalMatrix * (rayHit.point + offset)) - currentMark.transform.position;
            
            // Add point to the line render
            currentMark.positionCount++;
            currentMark.SetPosition(currentMark.positionCount - 1, position);

            // Track distance drawn
            if (currentMark.positionCount > 1)
                DistanceDrawn += Vector3.Distance(lastDrawnPoint, rayHit.point);
            lastDrawnPoint = rayHit.point;

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
			Transform pointer;
            if (GameManager.Instance.PlayersVRType == VirtualRealityType.None)
                pointer = cam.transform;
            else
                pointer = ThrowingHand.transform;
            GameObject ofuda = Instantiate(ofudaPrefab, pointer.position + pointer.forward, pointer.rotation);
            ofuda.transform.parent = thrownOfudaParent.transform;
            PlayerInventory.Used(ItemType.Ofuda);
            thrown = true;
        }
    }


    void Die()
    {
        GameManager.Instance.GameOver();
    }
    

    void SafeZoneCollision()
    {
        
    }

}
