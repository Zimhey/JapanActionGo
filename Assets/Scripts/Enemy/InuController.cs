﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.VR;

//state machine for inu AI
public enum InuState
{
    Idle, // inu currently doing nothing
    Patrol, // inu has not seen player, wandering maze
    Search, // inu has seen player, cannot see player or footprints, is looking for player in maze
    Chase, // inu sees player, is moving towards player to stalk
    Stalk, // inu is following player until they trip, then they attack
    Cornered, // inu is backed up against a wall;
    Flee, // inu has encountered safe zone, is returning to home position
    Dead, // inu has encountered trap, no longer active
    Follow, // inu does not see player, inu sees footprints, is moving towards footprints
    Stun // inu has been hit by ofuda and is stunned
}

//state machine for inu animations
public enum InuAnim
{
    Idle, // inu doing nothing
    Walk, // inu walking around patrolling
    Run, // inu killing player
    Creep, //inu stalking player
    Attack, // inu attacking player
    Stunned, // inu stunned by player
    Sit // inu sitting down
}

public class InuController : YokaiController
{
    //player game object
    public GameObject PlayerObject;
    //starting node for patrol
    public MazeNode StartingNode;
    //inu movement speed, set in unity
    public float Speed;
    //layermask to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;
    public int StalkDistance;
    public int StartCorneredDistance;
    public int StayCorneredDistance;
    public int CorneredChaseDistance;
    public int KillDistance;

    //inu physics body
    private Rigidbody rb;
    //inu starting point
    private Vector3 home;
    //inu starting rotation
    private Quaternion startingRotation;

    //current inu state
    private InuState state;
    //current anim state
    private InuAnim animState;
    private Actor actorID;
    private bool retreating;

    public InuState State
    {
        set
        {
            state = value;
            GameManager.Instance.ActorStateChange(actorID, (int)state);
            if(state == InuState.Flee)
            {
                fleeTimer = 30;
            }
        }
    }

    public InuAnim AnimState
    {
        set
        {
            animState = value;

            anim.SetInteger("State", (int) animState);
        }
        get
        {
            return animState;
        }
    }

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
    //has player been too close
    private System.Boolean beenTooClose;
    //private float distanceToFloor = 0.8F;
    private Vector3 oldPosition;
    private Vector3 oldPosition2;
    private Vector3 newPosition;
    private int posTimer;
    private int posTimer2;
    private GameObject nextFootprint;
    private NavMeshAgent agent;
    private int fleeTimer;

    private Animator anim;
    private Vector3 oldSitPosition;
    private Vector3 newSitPosition;
    private int sitTimer;

    private Transform playerTransform;
    private CharacterController controller;

    void Start()
    {
        //intialize variables
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        actorID = GetComponent<Actor>();
        State = InuState.Idle;
        AnimState = InuAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        beenTooClose = false;
        oldPosition = home;
        posTimer = 60;
        posTimer2 = 27;
        stunTimer = 0;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        if (root != null)
        {
            nodes = MazeGenerator.GetIntersectionNodes(root);
        }
        currentNode = StartingNode;
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;
        retreating = false;

        int column = (int)((home.x - 8) / 6);
        int row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;
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

        //if (newPosition != null)
        //{
        //if (oldPosition2 != null)
        //{
        if (state != InuState.Idle && state != InuState.Stalk && state != InuState.Cornered && state != InuState.Flee)
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
                    State = InuState.Flee;
                    return;
                }
            }
        }
        //}
        //}

        if (stunTimer > 0)
        {
            state = InuState.Stun;
        }

        switch (state)
        {
            case InuState.Idle:
                idle();
                break;
            case InuState.Patrol:
                patrol();
                break;
            case InuState.Search:
                search();
                break;
            case InuState.Chase:
                chase();
                break;
            case InuState.Stalk:
                stalk();
                break;
            case InuState.Cornered:
                cornered();
                break;
            case InuState.Flee:
                flee();
                break;
            case InuState.Dead:
                dead();
                break;
            case InuState.Follow:
                follow();
                break;
            case InuState.Stun:
                stun();
                break;
        }

        switch (animState)
        {
            case InuAnim.Idle:
                //print("doing idle");
                animIdle();
                break;
            case InuAnim.Walk:
                //print("doing walk");
                animWalk();
                break;
            case InuAnim.Creep:
                //print("doing creep");
                animCreep();
                break;
            case InuAnim.Run:
                //print("doing run");
                animRun();
                break;
            case InuAnim.Attack:
                //print("doing attack");
                animAttack();
                break;
            case InuAnim.Stunned:
                //print("doing stunned");
                animStunned();
                break;
            case InuAnim.Sit:
                //print("doing sit");
                animSit();
                break;
        }

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        posTimer--;
        if (posTimer <= 0)
        {
            posTimer = 90;
            //if (newPosition != null)
            //{
                oldPosition = newPosition;
            //}
            newPosition = transform.position;
        }
        posTimer2--;
        if (posTimer2 <= 0)
        {
            posTimer2 = 77;
            //if (oldPosition != null)
            //{
                oldPosition2 = oldPosition;
            //}
            oldPosition = transform.position;
        }
        
        MoveYokai(controller, agent);
    }

    void idle()
    {
        if (PlayerObject != null)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                awake = true;
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
            }
            else if (root != null)
            {
                State = InuState.Patrol;
            }
        }
    }

    void patrol()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = InuState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask, home);

        if (foundFootprint != null)
        {
            State = InuState.Follow;
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

    }

    void chase()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (!seen)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
            }
            else
            {
                State = InuState.Idle;
            }
        }
        
        Vector3 dest = playerTransform.position;
        agent.SetDestination(dest);

        if (transform.position.x < dest.x + 5 && transform.position.x > dest.x - 5)
        {
            if (transform.position.z < dest.z + 5 && transform.position.z > dest.z - 5)
            {
                State = InuState.Stalk;
                agent.SetDestination(transform.position);
            }
        }
    }

    void stalk()
    {
        AnimState = InuAnim.Creep;
        Vector3 rayDirection = playerTransform.position - transform.position;
        rayDirection.y = 0;
        System.Boolean playerCloseToEnemy;
        /*if (VRDevice.isPresent) {
            playerCloseToEnemy = Vector3.Magnitude(rayDirection) < StalkDistance * StalkDistance;
        }
        else
        {*/
            playerCloseToEnemy = rayDirection.sqrMagnitude < StalkDistance;
        //}
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
                return;
            }
            else if (StartingNode != null)
            {
                State = InuState.Patrol;
                return;
            }
            else
            {
                State = InuState.Idle;
                return;
            }
        }
        System.Boolean playerTooCloseToEnemy;
        /*if (VRDevice.isPresent)
        {
            playerTooCloseToEnemy = Vector3.Magnitude(rayDirection) < StartCorneredDistance * StartCorneredDistance;
        }
        else
        {*/
            playerTooCloseToEnemy = rayDirection.sqrMagnitude < StartCorneredDistance;
        //}
        if (playerTooCloseToEnemy)
        {
            //signify the player is too close to the inu
            //print("too close");
            beenTooClose = true;
            //get the distance from player to inu
            Vector3 newdir = transform.position - playerTransform.position;
            MazeNode destinationNode = null;
            MazeNode secondDestNode = null;
            MazeNode tertDestNode = null;
            Vector3 currentLocation = new Vector3(transform.position.x, home.y + 1.5F, transform.position.z);
            MazeNode fromNode = MazeGenerator.getNodeBasedOnLocation(currentLocation);
            //print("current location " + new Vector3(transform.position.x, home.y + 1.5F, transform.position.z));
            //print("from node " + new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
            MazeNode playerNode = MazeGenerator.getNodeBasedOnLocation(playerTransform.position);

            if (Math.Abs(newdir.x) > Math.Abs(newdir.z))
            {
                if(newdir.x > 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1)  * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                    if (newdir.z > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                    }

                    if (newdir.z < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                    }
                }

                if(newdir.x < 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                    if (newdir.z > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                    }

                    if (newdir.z < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                    }
                }
            }

            if (Math.Abs(newdir.x) == Math.Abs(newdir.z))
            {
                int rand = UnityEngine.Random.Range(0, 1);

                if (rand == 0)
                {
                    if (newdir.x > 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                        if (newdir.z > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                        }

                        if (newdir.z < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                        }
                    }

                    if (newdir.x < 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                        if (newdir.z > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                        }

                        if (newdir.z < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                        }
                    }
                }

                if (rand == 1)
                {
                    if (newdir.z > 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));

                        if (newdir.x > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }

                        if (newdir.x < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }
                    }

                    if (newdir.z < 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));


                        if (newdir.x > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }

                        if (newdir.x < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }
                    }
                }
            }
            if (Math.Abs(newdir.x) < Math.Abs(newdir.z))
            {

                if (newdir.z > 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));

                    if (newdir.x > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }

                    if (newdir.x < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }
                }

                if (newdir.z < 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));

                    if (newdir.x > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }

                    if (newdir.x < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }
                }
            }

            List<MazeNode> adjacent = fromNode.GetAdjacentNodes();
            for(int iter = 0; iter < adjacent.Count; iter++)
            {
                if(adjacent[iter] != destinationNode && adjacent[iter] != secondDestNode && adjacent[iter] != playerNode)
                {
                    tertDestNode = adjacent[iter];
                    Vector3 inuToPlayer = playerTransform.position - transform.position;
                    Vector3 inuToTert = new Vector3(tertDestNode.Col * 6 + 8, tertDestNode.Floor * 30, tertDestNode.Row * 6 + 8) - transform.position;
                    if(inuToPlayer.x > 0 && inuToTert.x > 0)
                    {
                        if(inuToPlayer.z > 0 && inuToTert.z > 0)
                        {
                            tertDestNode = null;
                        }

                        if (inuToPlayer.z < 0 && inuToTert.z < 0)
                        {
                            tertDestNode = null;
                        }
                    }
                    if (inuToPlayer.x < 0 && inuToTert.x < 0)
                    {
                        if (inuToPlayer.z > 0 && inuToTert.z > 0)
                        {
                            tertDestNode = null;
                        }

                        if (inuToPlayer.z < 0 && inuToTert.z < 0)
                        {
                            tertDestNode = null;
                        }
                    }
                }
            }

            if(adjacent.Contains(destinationNode) == false)
            {
                if (destinationNode != null)
                {
                    //print("primary not adjacent " + new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                    destinationNode = null;
                }
                else
                {
                    //print("no primary dest found");
                }
            }
            if (adjacent.Contains(secondDestNode) == false)
            {
                if (secondDestNode != null)
                {
                    //print("secondary not adjacent " + new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    secondDestNode = null;
                }
                else
                {
                    //print("no secondary dest found");
                }
            }



            if(destinationNode == null)
            {
                //print("primary not valid");
                if(secondDestNode == null)
                {
                    //print("secondary not valid");
                    if(tertDestNode == null)
                    {
                        //print("tertiary not valid");
                        newdir.y = 0;
                        //normalize to get direction only
                        newdir.Normalize();
                        //create a scalar
                        float scalar = (float)Math.Sqrt(15);
                        //scale direction vector to set distace to go
                        newdir.Scale(new Vector3(scalar, 1, scalar));
                        //set inu to go from current direction to scalar distance in normalized direction

                        Vector3 goal = playerTransform.position + newdir;
                        float wallDistance = newdir.magnitude;
                        Ray ray = new Ray(playerTransform.position, newdir);
                        RaycastHit rayHit;

                        if (Physics.Raycast(ray, out rayHit, wallDistance, LevelMask))
                        {
                            State = InuState.Cornered;
                            return;
                        }
                        else
                        {
                            agent.ResetPath();
                            agent.SetDestination(goal);
                            retreating = true;
                            return;
                        }
                    }

                    else
                    {
                        print("trying to go to tert " + new Vector3(tertDestNode.Col * 6 + 8, fromNode.Floor * 30, tertDestNode.Row * 6 + 8));
                        agent.ResetPath();
                        agent.SetDestination(new Vector3(tertDestNode.Col * 6 + 8, fromNode.Floor * 30, tertDestNode.Row * 6 + 8));
                        retreating = true;
                        return;
                    }
                }
                else
                {
                    print("trying to go to second " + new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    agent.ResetPath();
                    agent.SetDestination(new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    retreating = true;
                    return;
                }
            }
            else
            {
                print("trying to go to primary " + new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                agent.ResetPath();
                agent.SetDestination(new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                retreating = true;
                return;
            }

        }

        if(!playerTooCloseToEnemy && beenTooClose == true)
        {
            retreating = false;
            beenTooClose = false;
        }

        if (retreating != true)
        {
            agent.ResetPath();
            Vector3 dest = playerTransform.position;

            if (transform.position.x < dest.x + 3 && transform.position.x > dest.x - 3)
            {
                if (transform.position.z < dest.z + 3 && transform.position.z > dest.z - 3)
                {
                    agent.ResetPath();
                    agent.SetDestination(transform.position);
                }
            }
            else
            {
                //print("stalking towards player");
                agent.SetDestination(dest);
            }
        }

        if (hasPlayerTripped())
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
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            print("GameOver");
        }
    }

    void cornered()
    {
        Vector3 rayDirection = playerTransform.position - transform.position;
        rayDirection.y = 0;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < StayCorneredDistance;

        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
                return;
            }
            else if (StartingNode != null)
            {
                State = InuState.Patrol;
                return;
            }
            else
            {
                State = InuState.Idle;
                return;
            }
        }

        //play growl

        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < CorneredChaseDistance;
        if (playerTooCloseToEnemy && beenTooClose == true)
        {
            Vector3 goal = playerTransform.position;
            agent.ResetPath();
            agent.SetDestination(goal);
        }

        System.Boolean playerKillDistance = rayDirection.sqrMagnitude < KillDistance;
        if (playerKillDistance && beenTooClose == true)
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
            GameManager.Instance.GameOver();
        }


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
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
                return;
            }
        }
        agent.ResetPath();
        agent.SetDestination(home);
        if (transform.position.x < home.x + 2 && transform.position.x > home.x - 2)
        {
            if (transform.position.z < home.z + 2 && transform.position.z > home.z - 2)
            {
                State = InuState.Idle;
                gameObject.transform.rotation = startingRotation;
            }
        }
    }

    void dead()
    {
        Die();
    }

    void follow()
    {
        agent.ResetPath();
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            State = InuState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint == null)
            {
                State = InuState.Idle;
            }
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                agent.SetDestination(foundFootprint.transform.position);
            }
        }
        else
        {
            if (transform.position.x < nextFootprint.transform.position.x + 1 && transform.position.x > nextFootprint.transform.position.x - 1)
            {
                if (transform.position.z < nextFootprint.transform.position.z + 1 && transform.position.z > nextFootprint.transform.position.z - 1)
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
        if (stunTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
            }
            else if (foundFootprint != null && awake == true)
            {
                State = InuState.Follow;
            }
            else
            {
                State = InuState.Idle;
            }
        }
    }

    void Stun()
    {
        State = InuState.Stun;
        AnimState = InuAnim.Stunned;
        stunTimer = 480;
        agent.SetDestination(transform.position);
    }

    void SafeZoneCollision()
    {
        State = InuState.Flee;
    }

    bool hasPlayerTripped()
    {
        return false;
    }

    void animIdle()
    {
        if (agent.velocity.magnitude < 0.5)
        {
        }
        else
        {
            AnimState = InuAnim.Walk;
        }
    }

    void animWalk()
    {
        if (agent.velocity.magnitude > 0.5)
        {
        }
        if (agent.velocity.magnitude > 2.5)
        {
            AnimState = InuAnim.Run;
        }
        else
        {
            AnimState = InuAnim.Idle;
        }
    }

    void animRun()
    {
        if (agent.velocity.magnitude > 2.5)
        {
        }
        else
        {
            AnimState = InuAnim.Walk;
        }
    }
    
    void animCreep()
    {
        if(state != InuState.Stalk)
        {
            if(state != InuState.Cornered)
            {
                AnimState = InuAnim.Idle;
            }
        }

        if (agent.velocity.magnitude > 0.5)
        {
        }
        else
        {
            AnimState = InuAnim.Sit;
        }
    }

    void animAttack()
    {
    }

    void animStunned()
    {
    }

    void animSit()
    {
        if (agent.velocity.magnitude < 0.5)
        {
        }
        else
        {
            AnimState = InuAnim.Creep;
        }
    }
}