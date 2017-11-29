﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

//state machine for taka AI
public enum TakaState
{
    Idle, // taka currently doing nothing
    Patrol, // taka has not seen player, wandering maze
    LookAround,
    Search, // taka has seen player, cannot see player or footprints, is looking for player in maze
    Chase, // taka sees player, is moving towards player to taunt
    Taunt, // taka is next to player and taunts so player will look up
    Flee, // taka has encountered safe zone, is returning to home position
    Dead, // taka has encountered trap, no longer active
    Follow, // taka does not see player, taka sees footprints, is moving towards footprints
    Stun // taka has been hit by ofuda and is stunned
}

//state machine for taka animations
public enum TakaAnim
{
    Idle, // taka doing nothing
    Walk, // taka walking around patrolling
    Run, // taka chasing player
    Attack, // taka attacking player
    Stunned, // taka stunned by player
    Taunt, // taka taunting player
    Glare // taka glaring
}

public class TakaController : YokaiController
{
    //player game object
    public GameObject PlayerObject;
    //starting node for patrol
    public MazeNode StartingNode;
    //taka movement speed, set in unity
    public float Speed;
    //layermask to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;
    //distance at which the taka continues to taunt the player
    public int TauntDistance;
    //distance at which the taka can kill the player
    public int KillDistance;
    public bool TestDebug;

    //taka physics body
    private Rigidbody rb;
    //taka starting point
    private Vector3 home;
    //taka starting rotation
    private Quaternion startingRotation;

    //current taka state
    private TakaState state;
    //current anim state
    private TakaAnim animState;
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
    private float lookTimer;
    //countdown until no longer stunned
    private float stunTimer;
    private Camera cam;
    private float distanceToFloor = 2.5F;
    private Vector3 oldPosition;
    private Vector3 oldPosition2;
    private Vector3 newPosition;
    private float posTimer;
    private float posTimer2;
    private FootprintList nextFootprint;
    private NavMeshAgent agent;
    private float fleeTimer;
    private bool fleeingInu;

    private Transform playerTransform;
    //mesh renderer to change the look of the taka
    //private MeshRenderer mr;
    private Transform mr;
    private float growthTimer;
    private float shrinkTimer;
    private CharacterController controller;
    private MazeNode fleeTarget = null;
    LinkedList<MazeNode> fleePath = new LinkedList<MazeNode>();

    private FootprintList foundFootprint;
    private Vector3 currentNodePosition;
    private MazeNode closest;
    private Vector3 rayDirection;
    private System.Boolean playerCloseToEnemy;
    private Vector3 targetPos;
    private MazeNode presentNode;
    private bool obstacle;
    private int column;
    private int row;
    private LinkedList<MazeNode> possiblePath;
    private MazeNode prevCheckNode;
    private Actor player;
    private Vector3 dest;

    public TakaState State
    {
        set
        {
            if (state == TakaState.Patrol)
            {
                ClearPaths();
                if (currentNode != null)
                    currentNode.EnemyPathNode = true;
            }

            if (state == TakaState.Taunt)
            {
                if (shrinkTimer <= 0)
                {
                    shrinkTimer = 2.5f - growthTimer;
                }
            }
            state = value;
            GameManager.Instance.ActorStateChange(actorID, (int)state);
            if(state == TakaState.Flee)
            {
                fleeTimer = 30;
            }
            if (state == TakaState.Taunt)
            {
                if (growthTimer <= 0)
                {
                    growthTimer = 2.5f;
                }
            }
        }
    }

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        distanceToFloor = home.y + 0.1F;
        startingRotation = gameObject.transform.rotation;
        actorID = GetComponent<Actor>();
        State = TakaState.Idle;
        animState = TakaAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        cam = Camera.main;
        oldPosition = home;
        posTimer = 60;
        posTimer2 = 27;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        if (root != null)
        {
            nodes = MazeGenerator.GetIntersectionNodes(root);
        }
        currentNode = StartingNode;
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;
        agent.nextPosition = transform.position;
        //transform.position = agent.nextPosition;
        agent.Warp(transform.position);
        fleeingInu = false;

        column = (int)((home.x - 8) / 6);
        row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;
    }

    void LateUpdate()
    {
        if (TestDebug)
        {
            print("Taka state " + state);
        }
        if (actorID == null)
        {
            actorID = GetComponent<Actor>();
        }

        if(controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (PlayerObject != null)
            playerTransform = PlayerObject.transform;
        else
            playerTransform = null;

        //manage state machine each update, call functions based on state
        
        if (stunTimer > 0)
        {
            if (state != TakaState.Stun)
            {
                state = TakaState.Stun;
            }
        }
        switch (state)
        {
            case TakaState.Idle:
                idle();
                break;
            case TakaState.Patrol:
                patrol();
                break;
            case TakaState.LookAround:
                look();
                break;
            case TakaState.Search:
                search();
                break;
            case TakaState.Chase:
                chase();
                break;
            case TakaState.Taunt:
                taunt();
                break;
            case TakaState.Flee:
                flee();
                break;
            case TakaState.Dead:
                dead();
                break;
            case TakaState.Follow:
                follow();
                break;
            case TakaState.Stun:
                stun();
                break;
        }

        switch (animState)
        {
            case TakaAnim.Idle:
                animIdle();
                break;
            case TakaAnim.Walk:
                animWalk();
                break;
            case TakaAnim.Run:
                animRun();
                break;
            case TakaAnim.Attack:
                animAttack();
                break;
            case TakaAnim.Stunned:
                animStunned();
                break;
            case TakaAnim.Taunt:
                animTaunt();
                break;
            case TakaAnim.Glare:
                animGlare();
                break;
        }

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        posTimer -= Time.deltaTime;
        if (posTimer <= 0)
        {
            posTimer = 90;
            oldPosition = newPosition;
            newPosition = transform.position;
        }
        posTimer2 -= Time.deltaTime;
        if (posTimer2 <= 0)
        {
            posTimer2 = 77;
            oldPosition2 = oldPosition;
            oldPosition = transform.position;
        }

        if (distanceToFloor < 0.0F)
        {
            distanceToFloor = 0.0F;
        }

        MoveYokai(controller, agent);
    }

    void idle()
    {
        posTimer = 90;
        posTimer2 = 77;
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = TakaState.Chase;
            return;
        }
        foundFootprint = SeeFootprint(nodes, LevelMask, home);
        if (foundFootprint != null)
        {
            State = TakaState.Follow;
        }
        else if (root != null)
        {
            State = TakaState.Patrol;
        }
    }

    void patrol()
    {
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = TakaState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = TakaState.Chase;
            return;
        }

        foundFootprint = SeeFootprint(nodes, LevelMask, home);

        if (foundFootprint != null)
        {
            State = TakaState.Follow;
            return;
        }

        if (root != null)
        {
            bool setCurrent = false;

            if (currentNode == null)
            {
                closest = null;
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
                    if (Vector3.Distance(transform.position, currentNodePosition) < 2)
                    {
                            lookTimer = 4;
                            agent.SetDestination(transform.position);
                            state = TakaState.LookAround;
                        /*MazeNode closest = null;
                        closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                        if (closest != null)
                        {
                            previous2 = previous;
                            previous = currentNode;
                            currentNode = closest;
                        }*/
                        //lookTimer = 6;
                        //agent.SetDestination(transform.position);
                        //state = TakaState.LookAround;
                    }

                    if(currentNode != null)
                        currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
                }
                agent.SetDestination(currentNodePosition);
            }
        }
    }

    void look()
    {
        posTimer = 90;
        posTimer2 = 77;
        lookTimer -= Time.deltaTime;
        if (TestDebug)
        {
            print(lookTimer);
        }
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            //if player has been seen chase
            awake = true;
            State = TakaState.Chase;
            return;
        }
        foundFootprint = SeeFootprint(nodes, LevelMask, home);
        if (foundFootprint != null)
        {
            //if footprints found follow
            State = TakaState.Follow;
            return;
        }
        transform.Rotate(Vector3.up * (360 * Time.deltaTime));
        if (lookTimer <= 0)
        {
            if (root != null)
            {
                //old destination reached, update patrol path
                closest = null;
                closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                if (closest != null)
                {
                    previous2 = previous;
                    previous = currentNode;
                    currentNode = closest;
                }

                State = TakaState.Patrol;
                return;
            }
        }
    }

    void search()
    {

    }

    void chase()
    {
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = TakaState.Flee;
                return;
            }
        }
        agent.ResetPath();
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (!seen)
        {
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (foundFootprint != null)
            {
                State = TakaState.Follow;
            }
            else
            {
                State = TakaState.Idle;
            }
        }
        
        agent.SetDestination(PlayerObject.transform.position);

        dest = PlayerObject.transform.position;

        if (Vector3.Distance(transform.position, dest) < 5)
        {
                //if taka close enough to player taunt them
                State = TakaState.Taunt;
                agent.SetDestination(transform.position);
                //gameObject.transform.rotation = startingRotation;
        }
        
    }

    //function to execute in taunt state
    void taunt()
    {
        posTimer = 90;
        posTimer2 = 77;
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        rayDirection = playerTransform.position - transform.position;
        rayDirection.y = 0;
        playerCloseToEnemy = rayDirection.sqrMagnitude < TauntDistance;
        if (!playerCloseToEnemy)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = TakaState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (foundFootprint != null)
            {
                State = TakaState.Follow;
                return;
            }
            else if(StartingNode != null)
            {
                State = TakaState.Patrol;
                return;
            }
            else 
            {
                State = TakaState.Idle;
                return;
            }
        }

        //play taunt sounds
        if(mr == null)
        {
            //mr = gameObject.GetComponentInChildren<MeshRenderer>();
            mr = gameObject.GetComponentInChildren<Transform>();
        }
        //make the taka appear to grow taller
        if (growthTimer > 0)
        {
            growthTimer -= Time.deltaTime;
            mr.transform.localScale += new Vector3(0, 0.002F, 0);
            mr.transform.position += new Vector3(0, 0.001F, 0);
            distanceToFloor += 0.01F;
        }
        //if player has avoided looking up for a full taunt cycle the taka backs off
        else if (growthTimer <= 0)
        {
            State = TakaState.Flee;
            return;
        }

        //if player looks up while taunted, kill it
        if (playerLookingUp())
        {
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                player = PlayerObject.GetComponentInParent<Actor>();
                GameManager.Instance.ActorKilled(actorID, player);
            }
            else
            {
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            }
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            if (TestDebug)
            {
                print("GameOver");
            }
        }
    }

    void flee()
    {
        posTimer = 90;
        posTimer2 = 77;
        fleeTimer -= Time.deltaTime;
        if (fleeTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                awake = true;
                State = TakaState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (foundFootprint != null)
            {
                State = TakaState.Follow;
                return;
            }
        }

        agent.ResetPath();

        if (mr == null)
        {
            //mr = gameObject.GetComponentInChildren<MeshRenderer>();
            mr = gameObject.GetComponentInChildren<Transform>();
        }
        //if taka has grown, shrink it
        if (shrinkTimer > 0)
        {
            shrinkTimer -= Time.deltaTime;
            mr.transform.localScale -= new Vector3(0, 0.002F, 0);
            mr.transform.position -= new Vector3(0, 0.001F, 0);
            distanceToFloor -= 0.01F;
        }

        targetPos = new Vector3();
        if (fleeTarget == null)
        {
            presentNode = new MazeNode();
            obstacle = false;
            column = homeNode.Col;
            row = homeNode.Row;

            foreach (MazeNode n in MazeGenerator.nodesInSection(root))
                if (n.Col == column && n.Row == row)
                    presentNode = n;

            possiblePath = MazeGenerator.GetPath2(presentNode, homeNode);
            prevCheckNode = presentNode;

            foreach (MazeNode n in possiblePath)
            {
                if (n.EnemyPathNode || GameManager.trapNode(n))
                {
                    fleeTarget = prevCheckNode;
                    obstacle = true;
                    break;
                }
                prevCheckNode = n;
            }
            if (!obstacle)
                fleeTarget = homeNode;

            fleePath = MazeGenerator.GetPath2(presentNode, fleeTarget);
            foreach (MazeNode n in fleePath)
                n.EnemyPathNode = true;
        }

        targetPos = new Vector3(fleeTarget.Col * 6 + 8, fleeTarget.Floor * 30, fleeTarget.Row * 6 + 8);

        if (Vector3.Distance(transform.position, targetPos) < 2)
        {
                State = TakaState.Idle;
                gameObject.transform.rotation = startingRotation;
        }

        agent.SetDestination(targetPos);
        
        /*
        if (transform.position.x < home.x + 2 && transform.position.x > home.x - 2)
        {
            if (transform.position.z < home.z + 2 && transform.position.z > home.z - 2)
            {
                State = TakaState.Idle;
                gameObject.transform.rotation = startingRotation;
            }
        }

        agent.SetDestination(home);
        */
    }

    void dead()
    {
        Die();
    }

    void follow()
    {
        //agent.ResetPath();
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
            return;
        }
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = TakaState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            State = TakaState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (foundFootprint == null)
            {
                State = TakaState.Idle;
            }
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                agent.SetDestination(foundFootprint.transform.position);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, nextFootprint.transform.position) < 1)
            {
                    nextFootprint = nextFootprint.getNext();
            }

            agent.SetDestination(nextFootprint.transform.position);
        }
    }

    void stun()
    {
        posTimer = 90;
        posTimer2 = 77;
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (seen)
            {
                State = TakaState.Chase;
            }
            else if (foundFootprint != null && awake == true)
            {
                State = TakaState.Follow;
            }
            else
            {
                State = TakaState.Idle;
            }
        }
    }

    void Stun()
    {
        State = TakaState.Stun;
        stunTimer = 120;
        agent.SetDestination(transform.position);
        fleeingInu = false;
    }

    void safeZoneCollision()
    {
        State = TakaState.Flee;
    }

    //function to check if the player is looking upwards
    bool playerLookingUp()
    {
        //get player's camera orientation
        Vector3 dir = cam.transform.rotation * Vector3.up;
        //get taka's forward direction
        Vector3 enemyDirection = PlayerObject.transform.TransformDirection(Vector3.forward);
        //normalize vectors
        dir.Normalize();
        enemyDirection.Normalize();
        //perform dot product on vectors
        float angleDot = Vector3.Dot(dir, enemyDirection);
        //if value is less than -0.3 the player is looking suffieciently updwards to be killed
        System.Boolean playerlookup = angleDot < -0.3;
        if (playerlookup)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void animIdle()
    {
        if (agent.velocity.magnitude < 0.5)
        {
        }
        else
            animState = TakaAnim.Walk;
    }

    void animWalk()
    {
        if (agent.velocity.magnitude > 0.5)
        {
        }
        else
            animState = TakaAnim.Idle;
    }

    void animRun()
    {
        if (agent.velocity.magnitude > 5.5)
        {
        }
        else
            animState = TakaAnim.Walk;
    }

    void animAttack()
    {
    }

    void animStunned()
    {
    }

    void animTaunt()
    {
    }

    void animGlare()
    {
    }
}