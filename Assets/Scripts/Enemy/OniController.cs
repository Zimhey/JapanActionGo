using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

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

    private Animator anim;

    private OniState state;

    private Transform playerTransform;
    private CharacterController controller;

    public OniState State
    {
        set
        {
            state = value;
            GameManager.Instance.ActorStateChange(actorID, (int) state);
            //print("OniState " + state);
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

        playerTransform = PlayerObject.transform;
        

        if (nextFootprint != null)
        {
            //print(nextFootprint.transform.position);
        }

        /*if (newPosition != null)
        {
            if (oldPosition2 != null)
            {*/
                if (state != OniState.Idle)
                {
                    Vector3 difference = newPosition - oldPosition;
                    float difMag = difference.magnitude;
                    if (difMag < .25)
                    {
                        Vector3 difference2 = oldPosition - oldPosition2;
                        float difMag2 = difference2.magnitude;
                        if (difMag < .25)
                        {
                            print("resetting path");
                            agent.ResetPath();
                            previous2 = previous;
                            previous = currentNode;
                            currentNode = null;
                            State = OniState.Idle;
                        }
                    }
                }
            /*}
        }*/

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
        
        if(FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
        }

        posTimer--;
        if(posTimer <= 0)
        {
            posTimer = 90;
            //if(newPosition != null)
            //{
                oldPosition = newPosition;
                //print("oldpos" + oldPosition);
            //}
            newPosition = transform.position;
            //print("newpos" + newPosition);
        }
        posTimer2--;
        if (posTimer2 <= 0)
        {
            posTimer = 77;
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
        seen = false;
        //print("idling");
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            //print("idle to chase");
            awake = true;
            State = OniState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            //print("idle to follow");
            State = OniState.Follow;
            return;
        }
        if (root != null)//awake == true && 
        {
            //print("idle to patrol");
            State = OniState.Patrol;
            return;
        }
        //print("noothing to interact with");
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
            //print("patrolling");
            List<MazeNode> nodes = MazeGenerator.GetIntersectionNodes(root);

            Vector3 currentNodePosition;

            if (currentNode == null)
            {
                MazeNode closest = null;
                closest = setClosest(closest, homeNode, nodes, rb);
                currentNode = closest;
            }

            if (currentNode != null)
            {
                currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);

                if (transform.position.x < currentNodePosition.x + 2 && transform.position.x > currentNodePosition.x - 2)
                {
                    if (transform.position.z < currentNodePosition.z + 2 && transform.position.z > currentNodePosition.z - 2)
                    {
                        MazeNode closest = null;
                        closest = updateClosest(closest, nodes, currentNode, previous, previous2, rb);
                        previous2 = previous;
                        previous = currentNode;
                        currentNode = closest;
                    }
                }

                currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
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
            }
            else
            {
                State = OniState.Idle;
            }
        }

        Vector3 rayDirection = playerTransform.localPosition - transform.localPosition;
        rayDirection.y = 0;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < KillDistance;
        if (playerCloseToEnemy)
        {
            GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            State = OniState.GameOver;
            GameManager.Instance.GameOver();
            print("GameOver");
        }

        agent.SetDestination(playerTransform.position);
    }

    void flee()
    {
        agent.ResetPath();
        agent.SetDestination(home);
        if (transform.position.x < home.x + 2 && transform.position.x > home.x - 2)
        {
            if (transform.position.z < home.z + 2 && transform.position.z > home.z - 2)
            {
                State = OniState.Idle;
                gameObject.transform.rotation = startingRotation;
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
            }
            else if (foundFootprint != null && awake == true)
            {
                State = OniState.Follow;
            }
            else
            {
                State = OniState.Idle;
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
        // if attacking player
        anim.SetInteger("State", 3);
    }

    void animStunned()
    {
        anim.SetInteger("State", 4);
    }

    void animLook()
    {
        // if looking around
        anim.SetInteger("State", 5);
    }
}

