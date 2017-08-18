/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    //player the camera is following, set in unity
    public GameObject player;

    //offset to keep camera a set distance from player
    private Vector3 offset;

    //timer to stop continous movement in stutter mode
    private int block = 0;

    //variable to track how much movement occurs so player movement can be adjusted
    public int rotation;

    //variable to hold which mode the camera is in, 0 = continous turn, 1 = stutter turn
    private int mode = 0;

    void Start()
    {
        //set offset
        offset = transform.position - player.transform.position;
    }

    private void Update()
    {
        //set turning speed of camera
        float horizontalSpeed;
        horizontalSpeed = 15.0F;
        // If Right Button is clicked Camera will turn right, left button is clicked camera will turn left.
        //decrements timer
        if (block > 0)
        {
            block--;
        }
        //adjusts mode if space is hit
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(mode > 0)
            {
                mode = 0;
            }
            else if (mode < 1)
            {
                mode = 1;
            }
        }

        //if continous set timer to 0
        if(mode == 0)
        {
            block = 0;
        }

        //if timer greater than 2 don't allow turn in stutter mode
        if (block < 2)
        {
            //right click, right turn
            if (Input.GetButton("Fire2"))
            {
                float h = horizontalSpeed;
                //if continous turn less rapidly
                if(mode == 0)
                {
                    h = h / 3;
                }
                //turn
                transform.Rotate(0, h, 0);
                //update timer
                block = 30;
                //update rotation tracker
                rotation += (int) h;
            }
            //left click, left turn
            if (Input.GetButton("Fire1"))
            {
                float h = horizontalSpeed;
                if (mode == 0)
                {
                    h = h / 3;
                }
                transform.Rotate(0, -h, 0);
                block = 30;
                rotation -= (int)h;
            }
        }
    }

    void LateUpdate()
    {
        //move camera to player's position
        transform.position = player.transform.position + offset;
    }
}
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[AddComponentMenu("Camera-Control/Smooth Mouse Look")]
public class CameraController : MonoBehaviour
{
    //player the camera is following, set in unity
    public GameObject player;
    //offset to keep camera a set distance from player
    private Vector3 offset;
    //variable to track how much movement occurs so player movement can be adjusted
    public Quaternion rotation;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0F;
    float rotationY = 0F;
    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;
    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;
    public float frameCounter = 20;
    Quaternion originalRotation;
    void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            //Resets the average rotation
            rotAverageY = 0f;
            rotAverageX = 0f;

            //Gets rotational input from the mouse
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            //Adds the rotation values to their relative array
            rotArrayY.Add(rotationY);
            rotArrayX.Add(rotationX);

            //If the arrays length is bigger or equal to the value of frameCounter remove the first value in the array
            if (rotArrayY.Count >= frameCounter)
            {
                rotArrayY.RemoveAt(0);
            }
            if (rotArrayX.Count >= frameCounter)
            {
                rotArrayX.RemoveAt(0);
            }

            //Adding up all the rotational input values from each array
            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }

            //Standard maths to find the average
            rotAverageY /= rotArrayY.Count;
            rotAverageX /= rotArrayX.Count;

            //Clamp the rotation average to be within a specific value range
            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            //Get the rotation you will be at next as a Quaternion
            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

            //Rotate
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            rotation = originalRotation * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotAverageX = 0f;
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotArrayX.Add(rotationX);
            if (rotArrayX.Count >= frameCounter)
            {
                rotArrayX.RemoveAt(0);
            }
            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }
            rotAverageX /= rotArrayX.Count;
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            rotAverageY = 0f;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotArrayY.Add(rotationY);
            if (rotArrayY.Count >= frameCounter)
            {
                rotArrayY.RemoveAt(0);
            }
            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;
            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }
    }
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        //set offset
        offset = transform.position - player.transform.position;
        if (rb)
        {
            rb.freezeRotation = true;
        }
        originalRotation = transform.localRotation;
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    void LateUpdate()
    {
        //move camera to player's position
        transform.position = player.transform.position + offset;
    }
}