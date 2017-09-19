using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

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
    private MazeNode previous2;
    private MazeNode homeNode;
    //countdown until no longer stunned
    private int stunTimer;
    private Camera cam;
    private float distanceToFloor = 2.5F;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private int posTimer;
    private GameObject nextFootprint;
    private NavMeshAgent agent;

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
        distanceToFloor = home.y;
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
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;

        int column = (int)((home.x - 8) / 6);
        int floor = (int)(home.y / 30);
        int row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        if (state != TakaState.Idle)
        {
            //print("TakaState " + state);
        }
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
            }
            newPosition = rb.transform.position;
        }

        if (newPosition != null)
        {
            Vector3 difference = newPosition - oldPosition;
            float difMag = difference.magnitude;
            if (state != TakaState.Taunt)
            {
                if (difMag < .25)
                {
                    agent.ResetPath();
                    previous2 = previous;
                    previous = currentNode;
                    currentNode = null;
                }
            }
        }

        //gameObject.transform.position.Set(gameObject.transform.position.x, distanceToFloor, gameObject.transform.position.z);
        MoveYokai();
    }

    void idle()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            awake = true;
            state = TakaState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask);
        if (foundFootprint != null)
        {
            state = TakaState.Follow;
        }
        else if (root != null)//awake == true && 
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
            awake = true;
            state = TakaState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask);

        if (foundFootprint != null)
        {
            state = TakaState.Follow;
            return;
        }

        if (root != null)
        {
            List<MazeNode> nodes = MazeGenerator.GetIntersectionNodes(root);
            Vector3 currentNodePosition;

            if (currentNode == null)
            {
                MazeNode closest = null;
                closest = setClosest(closest, homeNode, nodes, rb);
                currentNode = closest;
            }
            
            currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);

            if (rb.transform.position.x < currentNodePosition.x + 2 && rb.transform.position.x > currentNodePosition.x - 2)
            {
                if (rb.transform.position.z < currentNodePosition.z + 2 && rb.transform.position.z > currentNodePosition.z - 2)
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

    void search()
    {

    }

    void chase()
    {
        agent.ResetPath();
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (!seen)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = TakaState.Follow;
            }
            else
            {
                state = TakaState.Idle;
            }
        }
        
        agent.SetDestination(PlayerObject.transform.position);

        Vector3 dest = PlayerObject.transform.position;
        //agent.destination = dest;
        //print(dest);

        if (rb.transform.position.x < dest.x + 5 && rb.transform.position.x > dest.x - 5)
        {
            if (rb.transform.position.y < dest.y + 5 && rb.transform.position.y > dest.y - 5)
            {
                if (rb.transform.position.z < dest.z + 5 && rb.transform.position.z > dest.z - 5)
                {
                    state = TakaState.Taunt;
                    agent.SetDestination(rb.transform.position);
                    gameObject.transform.rotation = startingRotation;
                }
            }
        }
        
    }

    void taunt() //getting shoved into ground
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
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = TakaState.Follow;
                return;
            }
            else if(StartingNode != null)
            {
                state = TakaState.Patrol;
                return;
            }
            else 
            {
                state = TakaState.Idle;
                return;
            }
        }

        //play taunt sounds
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        rayDirection.Normalize();
        enemyDirection.Normalize();
        float angleDot = Vector3.Dot(enemyDirection, rayDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean noWallfound = NoWall(PlayerObject, LevelMask);
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

        if (mr.transform.localScale.y < 6)
        {
            mr.transform.localScale += new Vector3(0, 0.02F, 0);
            //mr.transform.position += new Vector3(0, 0.01F, 0);
            distanceToFloor += 0.01F;
        }
        else if (mr.transform.localScale.y >= 6)
        {
            agent.height = 0.6F;
            state = TakaState.Flee;
            return;
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
        agent.ResetPath();
        agent.SetDestination(home);
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

        if (mr.transform.localScale.y > 5)
        {
            mr.transform.localScale -= new Vector3(0, 0.02F, 0);
            //mr.transform.position -= new Vector3(0, 0.01F, 0);
            distanceToFloor -= 0.01F;
        }
        else
        {
            agent.height = 1.0F;
        }
        //gameObject.transform.position.Set(gameObject.transform.position.x, distanceToFloor, gameObject.transform.position.z);
        if (rb.transform.position.x < home.x + 2 && rb.transform.position.x > home.x - 2)
        {
            if (rb.transform.position.z < home.z + 2 && rb.transform.position.z > home.z - 2)
            {
                state = TakaState.Idle;
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
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            state = TakaState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint == null)
            {
                state = TakaState.Idle;
            }
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                agent.SetDestination(foundFootprint.transform.position);
            }
        }
        else
        {
            if (rb.transform.position.x < nextFootprint.transform.position.x + 1 && rb.transform.position.x > nextFootprint.transform.position.x - 1)
            {
                if (rb.transform.position.z < nextFootprint.transform.position.z + 1 && rb.transform.position.z > nextFootprint.transform.position.z - 1)
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
        agent.SetDestination(rb.transform.position);
    }

    void SafeZoneCollision()
    {
        state = TakaState.Flee;
    }

    bool playerLookingUp()
    {
        Vector3 dir = cam.transform.rotation * Vector3.up;
        Vector3 enemyDirection = PlayerObject.transform.TransformDirection(Vector3.forward);
        dir.Normalize();
        enemyDirection.Normalize();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trap"))
        {
            //dead();
        }
        if (other.gameObject == PlayerObject)
        {
            if (state == TakaState.Taunt)
            {
                actorID = GetComponent<Actor>();
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
                GameManager.Instance.GameOver();
            }
        }
    }
}