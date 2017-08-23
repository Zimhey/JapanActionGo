using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

//state machine for taka AI
public enum TakaState
{
    Idle, // taka currently doing nothing
    Patrol, // taka has not seen player, wandering maze
    Search, // taka has seen player, cannot see player or footprints, is looking for player in maze
    Chase, // taka sees player, is moving towards player to taunt
    Taunt, // taka is next to player and taunts so player will look up
    Flee, // taka has encountered safe zone, is returning to home position
    Dead, // taka has encountered trap, no longer active
    Follow, // taka does not see player, taka sees footprints, is moving towards footprints
    Stun // taka has been hit by ofuda and is stunned
}

public class TakaController : YokaiController
{
    //player game object
    public GameObject PlayerObject;
    //starting node for patrol
    public GameObject StartingNode;
    //taka movement speed, set in unity
    public float Speed;
    //layermask to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;

    //taka physics body
    private Rigidbody rb;
    //taka starting point
    private Vector3 home;
    //taka starting rotation
    private Quaternion startingRotation;
    //current taka state
    private TakaState state;
    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //array of locations the taka has been
    private ArrayList previousLocations = new ArrayList();
    private int lessEnough = 5;
    //current node for patrol
    private GameObject currentNode;
    //countdown until no longer stunned
    private int stunTimer;
    private Camera cam;
    private float distanceToFloor = 2.5F;
    private GameObject footprintPrefab;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = TakaState.Idle;
        awake = false;
        currentNode = StartingNode;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        footprintPrefab = Actors.Prefabs[ActorType.Oni_Footprint];
        cam = PlayerObject.GetComponentInChildren<Camera>();
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        if (state != TakaState.Idle)
            print("State" + state);
        switch (state)
        {
            case TakaState.Idle:
                idle();
                break;
            case TakaState.Patrol:
                patrol();
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

        PlaceFootprints(previousLocations, lessEnough, footprintPrefab, rb, distanceToFloor);

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (FleeInu(agent, PlayerObject))
        {
            state = TakaState.Flee;
        }
    }

    void idle()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            awake = true;
            state = TakaState.Chase;
        }
        else if (SeeFootprint(PlayerObject) && awake == true)
        {
            state = TakaState.Follow;
        }
        else if (awake == true && StartingNode != null)
        {
            state = TakaState.Patrol;
        }
    }

    void patrol()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            state = TakaState.Chase;
        }
        else if (SeeFootprint(PlayerObject) && awake == true)
        {
            state = TakaState.Follow;
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
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // get taka's navigation agent
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
        if (!seen)
        {
            if (SeeFootprint(PlayerObject))
            {
                state = TakaState.Follow;
            }
            else
            {
                state = TakaState.Idle;
            }
        }

        //by using a Raycast you make sure an enemy does not see you
        //if there is a building separating you from his view, for example
        //the enemy only sees you if it has you in open view
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        ExecuteChase(agent, PlayerObject, PlayerMask);

        Vector3 dest = PlayerObject.transform.position;
        agent.destination = dest;
        if (rb.transform.position.x < dest.x + 5 && rb.transform.position.x > dest.x - 5)
        {
            if (rb.transform.position.y < dest.y + 5 && rb.transform.position.y > dest.y - 5)
            {
                if (rb.transform.position.z < dest.z + 5 && rb.transform.position.z > dest.z - 5)
                {
                    state = TakaState.Taunt;
                    agent.destination = rb.transform.position;
                    gameObject.transform.rotation = startingRotation;
                }
            }
        }
    }

    void taunt()
    {
        int maxDistance = 7;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = PlayerObject.transform.localPosition - (transform.localPosition - new Vector3(0,distanceToFloor,0));
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;
        if (!playerCloseToEnemy)
        {
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                state = TakaState.Chase;
            }
            else if (SeeFootprint(PlayerObject))
            {
                state = TakaState.Follow;
            }
            else if(StartingNode != null)
            {
                state = TakaState.Patrol;
            }
            else 
            {
                state = TakaState.Idle;
            }
        }

        //play taunt sounds
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean noWallfound = NoWall(PlayerObject, LevelMask);

        if (gameObject.transform.localScale.y < 10)
        {
            gameObject.transform.localScale += new Vector3(0, 0.01F, 0);
            gameObject.transform.position += new Vector3(0, 0.005F, 0);
            distanceToFloor += 0.005F;
        }
        else if (gameObject.transform.localScale.y >= 10)
        {
            state = TakaState.Flee;
        }
        if (playerInFrontOfEnemy)
        {
            if (noWallfound)
            {
                if (playerLookingUp())
                {
                    string curlevel = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene(curlevel);
                }
            }
        }
    }

    void flee()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        agent.destination = home;
        if (gameObject.transform.localScale.y > 5)
        {
            gameObject.transform.localScale -= new Vector3(0, 0.01F, 0);
            gameObject.transform.position -= new Vector3(0, 0.005F, 0);
            distanceToFloor -= 0.005F;
        }
        if (rb.transform.position.x < home.x + 1 && rb.transform.position.x > home.x - 1)
        {
            if (rb.transform.position.y < home.y + 1 && rb.transform.position.y > home.y - 1)
            {
                if (rb.transform.position.z < home.z + 1 && rb.transform.position.z > home.z - 1)
                {
                    state = TakaState.Idle;
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
        if (seen)
        {
            state = TakaState.Chase;
        }
        else if (!SeeFootprint(PlayerObject))
        {
            state = TakaState.Idle;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        ExecuteFollow(agent, PlayerObject);
    }

    void stun()
    {
        stunTimer--;
        if (stunTimer <= 0)
        {
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                state = TakaState.Chase;
            }
            else if (SeeFootprint(PlayerObject) && awake == true)
            {
                state = TakaState.Follow;
            }
            else
            {
                state = TakaState.Idle;
            }
        }
    }

    void Stun()
    {
        state = TakaState.Stun;
        stunTimer = 120;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = rb.position;
    }

    void SafeZoneCollision()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        state = TakaState.Flee;
    }

    bool playerLookingUp()
    {
        Vector3 dir = cam.transform.rotation * Vector3.up;
        Vector3 enemyDirection = PlayerObject.transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(dir, enemyDirection);
        System.Boolean playerlookup = angleDot < -0.5;
        if (playerlookup)
        {
            return true;
        }
        else
        {
            return false;
        }
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
        }
    }
}