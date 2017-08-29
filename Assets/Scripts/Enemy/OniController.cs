using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

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
    Stun // oni has been hit by ofuda and is stunned
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
    //countdown until no longer stunned
    private int stunTimer;
    //private float distanceToFloor = 0.0F;

    private GameObject footprintPrefab;
    private Animator anim;

    private OniState state;

    public OniState State
    {
        set
        {
            state = value;

            actorID = GetComponent<Actor>();
            ActorStateChange(actorID, (int) state);
        }
    }

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = OniState.Patrol;
        animState = OniAnim.Idle;
        awake = false;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        currentNode = StartingNode;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        footprintPrefab = Actors.Prefabs[ActorType.Oni_Footprint];
        anim = GetComponentInChildren<Animator>();
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        print(state);
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

        //PlaceFootprints(previousLocations, lessEnough, footprintPrefab, rb, distanceToFloor);

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }
        
        if(FleeInu(LevelMask))
        {
            state = OniState.Flee;
        }
    }

    void idle()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            awake = true;
            state = OniState.Chase;
        }
        else if (foundFootprint != null && awake == true)
        {
            state = OniState.Follow;
        }
        else if (awake == true && root != null)//awake == true && 
        {
            state = OniState.Patrol;
        }
    }
    
    void patrol()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            awake = true;
            state = OniState.Chase;
        }
        else if (foundFootprint != null && awake == true)
        {
            state = OniState.Follow;
        }
        List<MazeNode> nodes = MazeGenerator.GetIntersectionNodes(root);
        Vector3 currentNodePosition = new Vector3(0,0,0);

        if(currentNode == null)
        {
            MazeNode closest = null;
            closest = setClosest(closest, nodes, rb);
            currentNode = closest;
        }
        else
        {
            currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
        }

        if (rb.transform.position.x < currentNodePosition.x + 2 && rb.transform.position.x > currentNodePosition.x - 2)
        {
            if (rb.transform.position.z < currentNodePosition.z + 2 && rb.transform.position.z > currentNodePosition.z - 2)
            {
                MazeNode closest = null;
                closest = updateClosest(closest, nodes, currentNode, previous, rb);
                previous = currentNode;
                currentNode = closest;
            }
        }
        else // not yet at current node's location
        {
            Vector3 goal = currentNodePosition; // set current node location as desired location
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // get oni's navigation agent
            agent.destination = goal; // set destination to current node's location
        }
    }

    void search()
    {
        Vector3 newdir = transform.forward * 10;
        Vector3 goal = transform.position - newdir;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
        agent.destination = goal; 
    }

    void chase()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (!seen)
        {
            if (foundFootprint != null)
            {
                print("following");
                state = OniState.Follow;
            }
            else
            {
                print("idling");
                state = OniState.Idle;
            }
        }

        //by using a Raycast you make sure an enemy does not see you
        //if there is a building separating you from his view, for example
        //the enemy only sees you if it has you in open view
        Transform goal = PlayerObject.transform; // set current player location as desired location
        agent.destination = goal.position; // set destination to player's current location
    }

    void flee()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        //print("OriDest" + agent.destination);
        agent.destination = home;
        //print("NewDest" + agent.destination);
        //print("Curpos" + rb.transform.position);
        if (rb.transform.position.x < home.x + 2 && rb.transform.position.x > home.x - 2)
        {
            if (rb.transform.position.y < home.y + 2 && rb.transform.position.y > home.y - 2)
            {
                if (rb.transform.position.z < home.z + 2 && rb.transform.position.z > home.z - 2)
                {
                    state = OniState.Idle;
                    gameObject.transform.rotation = startingRotation;
                }
            }
        }
    }

    void dead()
    {
        Die();
    }

    void follow()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            state = OniState.Chase;
        }
        else if (foundFootprint == null)
        {
            state = OniState.Idle;
        }
        if (foundFootprint != null)
        {
            GameObject goal = foundFootprint;
            agent.destination = goal.transform.position;
        }
    }

    void stun()
    {
        stunTimer--;
        if(stunTimer <= 0)
        {
            animState = OniAnim.Idle;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (seen)
            {
                state = OniState.Chase;
            }
            else if (foundFootprint != null && awake == true)
            {
                state = OniState.Follow;
            }
            else
            {
                state = OniState.Idle;
            }
        }
    }

    void Stun()
    {
        state = OniState.Stun;
        animState = OniAnim.Stunned;
        stunTimer = 300;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = rb.position;
    }

    void SafeZoneCollision()
    {
        state = OniState.Flee;
    }

    void animIdle()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude < 0.5)
            anim.SetInteger("State", 0);
        else
            animState = OniAnim.Walk;
    }

    void animWalk()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 0.5)
            anim.SetInteger("State", 1);
        else
            animState = OniAnim.Idle;
    }

    void animRun()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 5.5)
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

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Trap"))
        {
            dead();
        }
        if (col.gameObject == PlayerObject)
        {
            string curlevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(curlevel);
            actorID = GetComponent<Actor>();
            ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
        }
    }
}

