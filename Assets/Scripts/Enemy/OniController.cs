﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

//state machine for oni AI
public enum OniState
{
    Idle, // oni currently doing nothing
    Patrol, // oni has not seen player, wandering maze
    Search, // oni has seen player, cannot see player or footprints, is looking for player in maze
    Chase, // oni sees player, is moving towards player to attack
    Flee, // oni has encountered safe zone, is returning to home position
    Dead, // oni has encountered trap, no longer active
    Follow, // oni does not see player, oni sees footprints, is moving towards footprints
    Stun, // oni has been hit by ofuda and is stunned
    GameOver // game has ended
}

//state machine for oni animations
public enum OniAnim
{
    Idle, // oni doing nothing
    Walk, // oni walking around patrolling
    Run, // oni chasing player
    Attack, // oni attacking player
    Stunned, // oni stunned by player
    Look // oni looking around itself
}

public class OniController : YokaiController
{
    //player game object
    public GameObject PlayerObject;
    //starting node for patrol
    public MazeNode StartingNode;
    //oni movement speed, set in unity
    public float Speed;
    //layermask to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;
    public int KillDistance;

    //oni physics body
    private Rigidbody rb;
    //oni starting point
    private Vector3 home;
    //oni starting rotation
    private Quaternion startingRotation;
    
    //current anim state
    private OniAnim animState;
    private Actor actorID;

    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //current node for patrol
    private List<MazeNode> nodes;
    private MazeNode currentNode;
    private MazeNode root;
    private MazeNode previous;
    private MazeNode previous2;
    private MazeNode homeNode;
    //countdown until no longer stunned
    private int stunTimer;
    private Vector3 oldPosition;
    private Vector3 oldPosition2;
    private Vector3 newPosition;
    private int posTimer;
    private int posTimer2;
    private GameObject nextFootprint;
    private NavMeshAgent agent;
    private int fleeTimer;

    private Animator anim;

    private OniState state;

    private Transform playerTransform;
    private CharacterController controller;
    private bool fleeingInu;

    public OniState State
    {
        set
        {
            state = value;
            GameManager.Instance.ActorStateChange(actorID, (int) state);
            if(state == OniState.Flee)
            {
                fleeTimer = 30;
                //print(fleeTimer);
            }
        }
    }

    void Start()
    {
        //intialize variables
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        actorID = GetComponent<Actor>();
        State = OniState.Idle;
        animState = OniAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        oldPosition = home;
        posTimer = 60;
        posTimer2 = 27;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        if(root != null)
        {
            nodes = MazeGenerator.GetIntersectionNodes(root);
        }
        fleeTimer = 30;
        fleeingInu = false;

        currentNode = StartingNode;
        int column = (int)((home.x - 8) / 6);
        int row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;
    }

    void LateUpdate()
    {
        if (actorID == null)
        {
            actorID = GetComponent<Actor>();
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (PlayerObject != null)
            playerTransform = PlayerObject.transform;
        else
            playerTransform = null;

        /*if (newPosition != null)
        {
            if (oldPosition2 != null)
            {*/
        if (state != OniState.Idle && state != OniState.Flee)
        {
            //print("checking if stuck");
            Vector3 difference = newPosition - oldPosition;
            difference.y = 0;
            float difMag = difference.magnitude;
            //print("dif1 " + difMag);
            if (difMag < .05)
            {
                Vector3 difference2 = oldPosition - oldPosition2;
                difference2.y = 0;
                float difMag2 = difference2.magnitude;
                //print("dif2 " + difMag2);
                if (difMag < .05)
                {
                    posTimer = 0;
                    posTimer = 5;
                    print("resetting path");
                    agent.ResetPath();
                    previous2 = previous;
                    previous = currentNode;
                    currentNode = null;
                    State = OniState.Flee;
                    return;
                }
            }
        }
        /*}
    }*/


        if (state != OniState.Flee)
        {
            if (FleeInu(LevelMask, home))
            {
                State = OniState.Flee;
                fleeingInu = true;
                return;
            }
        }

        if(state == OniState.Flee)
        {
            if (!FleeInu(LevelMask, home))
            {
                fleeingInu = false;
            }
        }

        if (fleeingInu == false)
        {
            if(stunTimer > 0)
            {
                state = OniState.Stun;
            }
            switch (state)
            {
                case OniState.Idle:
                    idle();
                    break;
                case OniState.Patrol:
                    patrol();
                    break;
                case OniState.Search:
                    search();
                    break;
                case OniState.Chase:
                    chase();
                    break;
                case OniState.Flee:
                    flee();
                    break;
                case OniState.Dead:
                    dead();
                    break;
                case OniState.Follow:
                    follow();
                    break;
                case OniState.Stun:
                    stun();
                    break;
                case OniState.GameOver:
                    gameOver();
                    break;
            }
        }
        switch (animState)
        {
            case OniAnim.Idle:
                animIdle();
                break;
            case OniAnim.Walk:
                animWalk();
                break;
            case OniAnim.Run:
                animRun();
                break;
            case OniAnim.Attack:
                animAttack();
                break;
            case OniAnim.Stunned:
                animStunned();
                break;
            case OniAnim.Look:
                animLook();
                break;
        }

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        posTimer--;
        if(posTimer <= 0)
        {
            posTimer = 90;
            //if(newPosition != null)
            //{
                oldPosition = newPosition;
                //print("oldpos " + oldPosition);
            //}
            newPosition = transform.position;
            //print("newpos " + newPosition);
        }
        posTimer2--;
        if (posTimer2 <= 0)
        {
            posTimer2 = 77;
            //if (oldPosition != null)
            //{
                oldPosition2 = oldPosition;
                //print("oldpos2 " + oldPosition2);
            //}
            oldPosition = transform.position;
            //print("oldpos " + oldPosition);
        }

        if (fleeingInu == true)
        {
            agent.SetDestination(home);
        }

        MoveYokai(controller, agent);
    }

    void idle()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = OniState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            State = OniState.Follow;
            return;
        }
        if (root != null)
        {
            State = OniState.Patrol;
            return;
        }
    }
    
    void patrol()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = OniState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            State = OniState.Follow;
            return;
        }

        if (root != null)
        {
            Vector3 currentNodePosition;
            bool setCurrent = false;

            if (currentNode == null)
            {
                MazeNode closest = null;
                closest = SetClosest(closest, homeNode, nodes, rb);
                currentNode = closest;
                if (previous == null)
                {
                    previous = currentNode;
                    previous2 = previous;
                }
                setCurrent = true;
            }

            if (currentNode != null)
            {
                currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);

                if (setCurrent == false)
                {
                    if (transform.position.x < currentNodePosition.x + 2 && transform.position.x > currentNodePosition.x - 2)
                    {
                        if (transform.position.z < currentNodePosition.z + 2 && transform.position.z > currentNodePosition.z - 2)
                        {
                            MazeNode closest = null;
                            closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                            if (closest != null)
                            {
                                previous2 = previous;
                                previous = currentNode;
                                currentNode = closest;
                            }
                        }
                    }

                    currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
                }
                agent.SetDestination(currentNodePosition);
            }
        }
    }

    void search()
    {
        Vector3 newdir = transform.forward * 10;
        Vector3 goal = transform.position - newdir;
        agent.destination = goal; 
    }

    void chase()
    {
        agent.ResetPath();
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (!seen)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = OniState.Follow;
                return;
            }
            else
            {
                State = OniState.Idle;
                return;
            }
        }

        Vector3 rayDirection = playerTransform.position - transform.position;
        rayDirection.y = 0;
        System.Boolean playerCloseToEnemy;
        /*if (VRDevice.isPresent)
        {
            playerCloseToEnemy = Vector3.Magnitude(rayDirection) < KillDistance * KillDistance;
        }
        else
        {*/
            playerCloseToEnemy = rayDirection.sqrMagnitude < KillDistance;
        //}
        if (playerCloseToEnemy)
        {
            if (VRDevice.isPresent)
            {
                Actor player = PlayerObject.GetComponentInParent<Actor>();
                GameManager.Instance.ActorKilled(actorID, player);
            }
            else
            {
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            }
            State = OniState.GameOver;
            GameManager.Instance.GameOver();
            print("GameOver");
        }

        agent.SetDestination(playerTransform.position);
    }

    void flee()
    {
        fleeTimer--;
        if (fleeTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                awake = true;
                State = OniState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = OniState.Follow;
                return;
            }
        }
        agent.ResetPath();
        agent.SetDestination(home);
        if (transform.position.x < home.x + 2 && transform.position.x > home.x - 2)
        {
            if (transform.position.z < home.z + 2 && transform.position.z > home.z - 2)
            {
                State = OniState.Idle;
                gameObject.transform.rotation = startingRotation;
                return;
            }
        }
    }

    void dead()
    {
        Die();
        print("oni has died");
    }

    void follow()
    {
        agent.ResetPath();
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            State = OniState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint == null)
            {
                State = OniState.Idle;
                return;
            }
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                agent.SetDestination(foundFootprint.transform.position);
            }
        }
        else
        {
            if (transform.position.x < nextFootprint.transform.position.x + 2 && transform.position.x > nextFootprint.transform.position.x - 2)
            {
                if (transform.position.z < nextFootprint.transform.position.z + 2 && transform.position.z > nextFootprint.transform.position.z - 2)
                {
                    nextFootprint = nextFootprint.GetComponent<FootprintList>().getNext();
                }
            }
            agent.SetDestination(nextFootprint.transform.position);
        }
    }

    void stun()
    {
        stunTimer--;
        if(stunTimer <= 0)
        {
            animState = OniAnim.Idle;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (seen)
            {
                State = OniState.Chase;
                return;
            }
            else if (foundFootprint != null && awake == true)
            {
                State = OniState.Follow;
                return;
            }
            else
            {
                State = OniState.Idle;
                return;
            }
        }
    }

    void Stun()
    {
        State = OniState.Stun;
        animState = OniAnim.Stunned;
        stunTimer = 300;
        agent.SetDestination(transform.position);
    }

    void gameOver()
    {

    }

    void SafeZoneCollision()
    {
        State = OniState.Flee;
    }

    void animIdle()
    {
        if (agent.velocity.magnitude < 0.5)
            anim.SetInteger("State", 0);
        else
            animState = OniAnim.Walk;
    }

    void animWalk()
    {
        if (agent.velocity.magnitude > 0.5)
            anim.SetInteger("State", 1);
        else
            animState = OniAnim.Idle;
    }

    void animRun()
    {
        if (agent.velocity.magnitude > 5.5)
            anim.SetInteger("State", 2);
        else
            animState = OniAnim.Walk;
    }

    void animAttack()
    {
        anim.SetInteger("State", 3);
    }

    void animStunned()
    {
        anim.SetInteger("State", 4);
    }

    void animLook()
    {
        anim.SetInteger("State", 5);
    }
}

