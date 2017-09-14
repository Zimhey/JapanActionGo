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
    public float DegreeSegment;
    public float RotationSensitivity;
    public GameObject Head;
    public GameObject Rig;

    private CharacterController controller;
    private float yVelocity;
    private float yRotation;
    private bool wasGrounded;
    private bool jumpPushed;
    public bool IsJumping;

    private Vector3 cameraPosition;
    private Vector3 rigPosition;
    private Vector3 cameraOffset;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        IsJumping = false;
    }

    void Update()
    {
        if(!jumpPushed)
            jumpPushed = CrossPlatformInputManager.GetButtonDown("Jump");

        if(!wasGrounded && controller.isGrounded)
        {
            yVelocity = 0f;
            IsJumping = false;
        }
        if(!controller.isGrounded && !IsJumping && wasGrounded)
            yVelocity = 0f;

        wasGrounded = controller.isGrounded;
    }

    void FixedUpdate()
    {
        UpdateRotation();
        // get input
        Vector2 input = getAnalog();
        Vector3 move = transform.forward * input.y + transform.right * input.x;

        // check for walls
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, controller.radius, Vector3.down, out hitInfo,
            controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        move = Vector3.ProjectOnPlane(move, hitInfo.normal).normalized;
        
        // scale to speed
        move *= IsWalking ? WalkSpeed : RunSpeed;

        // on ground?
        if (controller.isGrounded)
        {
            yVelocity = -10;

            if (jumpPushed)
            {
                yVelocity = JumpSpeed;
                jumpPushed = false;
                IsJumping = true;
            }
        }
        else
        {
            yVelocity += Physics.gravity.y * GravityMultiplier * Time.fixedDeltaTime;
        }

        move.y = yVelocity;
        controller.Move(move * Time.fixedDeltaTime + UpdateCamera());

    }

    private Vector3 UpdateCamera()
    {
        cameraPosition = Head.transform.position;
        rigPosition = Rig.transform.position;
        cameraPosition.y = 0f;
        rigPosition.y = 0f;

        Vector3 deltaOffset = cameraOffset - (rigPosition - cameraPosition);

        cameraOffset = rigPosition - cameraPosition;
        Rig.transform.position = transform.position + cameraOffset - new Vector3(0, controller.height / 2F);
        return deltaOffset;
    }

    private void UpdateRotation()
    {
        float y = CrossPlatformInputManager.GetAxis("Mouse X") * RotationSensitivity;

        // Q and E
        if (Input.GetKeyDown(KeyCode.Q))
            yRotation -= DegreeSegment;
        if (Input.GetKeyDown(KeyCode.E))
            yRotation += DegreeSegment;

        // Mouse X
        yRotation += CrossPlatformInputManager.GetAxis("Mouse X") * RotationSensitivity;

        if (yRotation < 0)
            yRotation += 360;

        float finalRot = (((int)yRotation) / (int)DegreeSegment) * DegreeSegment; // min max stuff

        transform.rotation = Quaternion.Euler(0, finalRot, 0);
        Rig.transform.rotation = Quaternion.Euler(0, finalRot, 0);
		
        if (SnapRotationToHead)
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
