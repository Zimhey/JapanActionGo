using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class VRPlayerController : MonoBehaviour
{
    public bool IsWalking;
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpSpeed;
    public float GravityMultiplier;

    public bool SnapRotationToHead;
    public GameObject Head;

    private CharacterController controller;
    private float yVelocity;

	void Start()
    {
        controller = GetComponent<CharacterController>();
    }
	
	void FixedUpdate()
    {
        UpdateRotation();
        // get input
        Vector2 input = getAnalog();
        Vector3 move = transform.forward * input.y + transform.right * input.x;

        // check for walls
      /*  RaycastHit hitInfo;
        Physics.SphereCast(transform.position, controller.radius, Vector3.down, out hitInfo,
            controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        move = Vector3.ProjectOnPlane(move, hitInfo.normal).normalized;
        */
        // scale to speed
        move *= IsWalking ? WalkSpeed : RunSpeed;

        // on ground?
        if(controller.isGrounded)
        {
            yVelocity = -10;

            if(CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                yVelocity = JumpSpeed;
            }
        }
        else
        {
            yVelocity += Physics.gravity.y * GravityMultiplier * Time.fixedDeltaTime;
        }

        move.y = yVelocity;
        controller.Move(move * Time.fixedDeltaTime);

    }

    private void UpdateRotation()
    {
        if(SnapRotationToHead)
        {
            float yRot = Head.transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, yRot, 0);
        }
    }

    private Vector2 getAnalog()
    {
        Vector2 move = Vector2.zero;

        move.x = CrossPlatformInputManager.GetAxis("Horizontal");
        move.y = CrossPlatformInputManager.GetAxis("Vertical");
        if(move.sqrMagnitude > 1)
            move.Normalize();

        IsWalking = true; // TODO VR Button

        return move;
    }
}
