using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

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
    Run, // inu chasing player
    Attack, // inu attacking player
    Stunned, // inu stunned by player
    Sit, // inu sitting down
    Stand, // inu standing up
    Growl // inu growling
}

public class InuController : YokaiController
{
    //player game object
    public GameObject PlayerObject;
    //starting node for patrol
    public GameObject StartingNode;
    //inu movement speed, set in unity
    public float Speed;
    //layermask to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;

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

    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //array of locations the inu has been
    private ArrayList previousLocations = new ArrayList();
    private int lessEnough = 5;
    //current node for patrol
    private GameObject currentNode;
    //countdown until no longer stunned
    private int stunTimer;
    //has player been too close
    private System.Boolean beenTooClose;
    private float distanceToFloor = 0.8F;

    private GameObject footprintPrefab;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = InuState.Idle;
        animState = InuAnim.Idle;
        awake = false;
        currentNode = StartingNode;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        footprintPrefab = Actors.Prefabs[ActorType.Okuri_Inu_Footprint];
        beenTooClose = false;
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        if (state != InuState.Idle)
            print("State" + state);
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
                animIdle();
                break;
            case InuAnim.Walk:
                animWalk();
                break;
            case InuAnim.Run:
                animRun();
                break;
            case InuAnim.Attack:
                animAttack();
                break;
            case InuAnim.Stunned:
                animStunned();
                break;
            case InuAnim.Sit:
                animSit();
                break;
            case InuAnim.Stand:
                animStand();
                break;
            case InuAnim.Growl:
                animGrowl();
                break;
        }

        PlaceFootprints(previousLocations, lessEnough, footprintPrefab, rb, distanceToFloor);

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

    }

    void idle()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            state = InuState.Chase;
        }
        else if (foundFootprint != null && awake == true)
        {
            state = InuState.Follow;
        }
        else if (awake == true && StartingNode != null)//awake == true && 
        {
            state = InuState.Patrol;
        }
    }

    void patrol()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            state = InuState.Chase;
        }
        else if (foundFootprint != null && awake == true)
        {
            state = InuState.Follow;
        }
        if (rb.transform.position.x < currentNode.transform.position.x + 1 && rb.transform.position.x > currentNode.transform.position.x - 1)
        {
            if (rb.transform.position.z < currentNode.transform.position.z + 1 && rb.transform.position.z > currentNode.transform.position.z - 1)
            {
                currentNode = currentNode.GetComponent<NodeScript>().nextNode;
            }
        }
        else // not yet at current node's location
        {
            Transform goal = currentNode.transform; // set current node location as desired location
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // get inu's navigation agent
            agent.destination = goal.position; // set destination to current node's location
        }
    }

    void search()
    {

    }

    void chase()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (!seen)
        {
            if (foundFootprint != null)
            {
                state = InuState.Follow;
            }
            else
            {
                state = InuState.Idle;
            }
        }

        //by using a Raycast you make sure an enemy does not see you
        //if there is a building separating you from his view, for example
        //the enemy only sees you if it has you in open view
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Transform goal = PlayerObject.transform; // set current player location as desired location
        agent.destination = goal.position; // set destination to player's current location

        Vector3 dest = PlayerObject.transform.position;
        agent.destination = dest;
        if (rb.transform.position.x < dest.x + 5 && rb.transform.position.x > dest.x - 5)
        {
            if (rb.transform.position.y < dest.y + 5 && rb.transform.position.y > dest.y - 5)
            {
                if (rb.transform.position.z < dest.z + 5 && rb.transform.position.z > dest.z - 5)
                {
                    state = InuState.Stalk;
                    agent.destination = rb.transform.position;
                    gameObject.transform.rotation = startingRotation;
                }
            }
        }
    }

    void stalk()
    {
        int maxDistance = 10;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = PlayerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (seen)
            {
                state = InuState.Chase;
            }
            else if (foundFootprint != null)
            {
                state = InuState.Follow;
            }
            else if (StartingNode != null)
            {
                state = InuState.Patrol;
            }
            else
            {
                state = InuState.Idle;
            }
        }
        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < maxDistance;
        if (playerTooCloseToEnemy && beenTooClose == false)
        {
            beenTooClose = true;
            print("too close");
            Vector3 newdir = PlayerObject.transform.localPosition - transform.localPosition;
            newdir.Normalize();
            float scalar = (float)Math.Sqrt(100 / 3);
            newdir.Scale(new Vector3(scalar, scalar, scalar));
            Vector3 goal = PlayerObject.transform.localPosition - newdir;

            float wallDistance = 25;
            wallDistance = rayDirection.magnitude;
            //rayDirection = Vector3.MoveTowards
            rayDirection.Normalize();
            Ray ray = new Ray(gameObject.transform.position, rayDirection);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, maxDistance, LevelMask))
            {
                state = InuState.Cornered;
            }
            else
            {
                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                agent.ResetPath();
                agent.destination = goal;
            }
        }
        else if (!playerTooCloseToEnemy)
        {
            beenTooClose = false;
        }

        if (hasPlayerTripped())
        {
            string curlevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(curlevel);
        }
    }

    void cornered()
    {
        print("cornered");
    }

    void flee()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        agent.destination = home;
        if (rb.transform.position.x < home.x + 1 && rb.transform.position.x > home.x - 1)
        {
            if (rb.transform.position.y < home.y + 1 && rb.transform.position.y > home.y - 1)
            {
                if (rb.transform.position.z < home.z + 1 && rb.transform.position.z > home.z - 1)
                {
                    state = InuState.Idle;
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
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            state = InuState.Chase;
        }
        else if (foundFootprint != null)
        {
            state = InuState.Idle;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        GameObject goal = foundFootprint;
        agent.destination = goal.transform.position;
    }

    void stun()
    {
        stunTimer--;
        if (stunTimer <= 0)
        {
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (seen)
            {
                state = InuState.Chase;
            }
            else if (foundFootprint != null && awake == true)
            {
                state = InuState.Follow;
            }
            else
            {
                state = InuState.Idle;
            }
        }
    }

    void Stun()
    {
        state = InuState.Stun;
        stunTimer = 480;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = rb.position;
    }

    void SafeZoneCollision()
    {
        state = InuState.Flee;
    }

    bool hasPlayerTripped()
    {
        return false;
    }

    void animIdle()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude < 0.5)
        {
            //anim.SetInteger("State", 0);
        }
        else
            animState = InuAnim.Walk;
    }

    void animWalk()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 0.5)
        {
            //anim.SetInteger("State", 1);
        }
        else
            animState = InuAnim.Idle;
    }

    void animRun()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 5.5)
        {
            //anim.SetInteger("State", 2);
        }
        else
            animState = InuAnim.Walk;
    }

    void animAttack()
    {
        // if attacking player
        //anim.SetInteger("State", 3);
    }

    void animStunned()
    {
        //anim.SetInteger("State", 4);
    }

    void animSit()
    {
        // if looking around
        //anim.SetInteger("State", 5);
    }

    void animStand()
    {
        // if looking around
        //anim.SetInteger("State", 6);
    }

    void animGrowl()
    {
        // if looking around
        //anim.SetInteger("State", 7);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Trap"))
        {
            dead();
        }
        if (col.gameObject == PlayerObject)
        {
            //string curlevel = SceneManager.GetActiveScene().name;
            //SceneManager.LoadScene(curlevel);
        }
    }
}