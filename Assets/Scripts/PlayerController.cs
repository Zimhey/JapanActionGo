using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public float speed;

    public CameraController cam;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        Vector3 curmovement = Quaternion.Euler(0, cam.rotation, 0) * movement;

        rb.AddForce(curmovement * speed);

        if (!Input.GetKey(KeyCode.DownArrow))
        {
            if (!Input.GetKey(KeyCode.UpArrow))
            {
                if (!Input.GetKey(KeyCode.LeftArrow))
                {
                    if (!Input.GetKey(KeyCode.RightArrow))
                    {
                        rb.velocity = new Vector3(0,0,0);
                    }
                }
            }
        }
    }
}