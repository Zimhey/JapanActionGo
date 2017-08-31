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
    private MazeNode currentNode;
    private MazeNode root;
    private MazeNode previous;
    //countdown until no longer stunned
    private int stunTimer;
    private Camera cam;
    private float distanceToFloor = 2.5F;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private int posTimer;

    public TakaState State
    {
        set
        {
            state = value;

            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorStateChange(actorID, (int)state);
        }
    }

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = TakaState.Idle;
        animState = TakaAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        cam = PlayerObject.GetComponentInChildren<Camera>();
        oldPosition = home;
        posTimer = 60;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        currentNode = StartingNode;
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

        //PlaceFootprints(previousLocations, lessEnough, footprintPrefab, rb, distanceToFloor);

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }
        
        if (FleeInu(LevelMask))
        {
            state = TakaState.Flee;
        }

        posTimer--;
        if (posTimer <= 0)
        {
            posTimer = 60;
            if (newPosition != null)
            {
                oldPosition = newPosition;
                print("oldpos" + oldPosition);
            }
            newPosition = rb.transform.position;
            print("newpos" + newPosition);
        }
        if (newPosition != null)
        {
            Vector3 difference = newPosition - oldPosition;
            float difMag = difference.magnitude;
            if (difMag < .25)
            {
                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                agent.ResetPath();
                currentNode = null;
                print("reseting path");
            }
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
            state = TakaState.Chase;
        }
        else if (foundFootprint != null && awake == true)
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
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            awake = true;
            state = TakaState.Chase;
        }
        else if (foundFootprint != null && awake == true)
        {
            state = TakaState.Follow;
        }
        List<MazeNode> nodes = MazeGenerator.GetIntersectionNodes(root);
        Vector3 currentNodePosition = new Vector3(0, 0, 0);

        if (currentNode == null)
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
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (seen)
            {
                state = TakaState.Chase;
            }
            else if (foundFootprint != null)
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
                    actorID = GetComponent<Actor>();
                    GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
                    GameManager.Instance.GameOver();
                    PlayerObject.SetActive(false);
                    print("GameOver");
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
        if (rb.transform.position.x < home.x + 2 && rb.transform.position.x > home.x - 2)
        {
            if (rb.transform.position.y < home.y + 1 && rb.transform.position.y > home.y - 1)
            {
                if (rb.transform.position.z < home.z + 2 && rb.transform.position.z > home.z - 2)
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
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (seen)
        {
            state = TakaState.Chase;
        }
        else if (foundFootprint == null)
        {
            state = TakaState.Idle;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (foundFootprint != null)
        {
            GameObject goal = foundFootprint;
            agent.destination = goal.transform.position;
        }
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
                state = TakaState.Chase;
            }
            else if (foundFootprint != null && awake == true)
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

    void animIdle()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude < 0.5)
        {
            //anim.SetInteger("State", 0);
        }
        else
            animState = TakaAnim.Walk;
    }

    void animWalk()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 0.5)
        {
            //anim.SetInteger("State", 1);
        }
        else
            animState = TakaAnim.Idle;
    }

    void animRun()
    {
        UnityEngine.AI.NavMeshAgent agent0 = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent0.velocity.magnitude > 5.5)
        {
            //anim.SetInteger("State", 2);
        }
        else
            animState = TakaAnim.Walk;
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

    void animTaunt()
    {
        // if looking around
        //anim.SetInteger("State", 5);
    }

    void animGlare()
    {
        // if looking around
        //anim.SetInteger("State", 6);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Trap"))
        {
            dead();
        }
        if (col.gameObject == PlayerObject)
        {
            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            print("GameOver");
        }
    }
}