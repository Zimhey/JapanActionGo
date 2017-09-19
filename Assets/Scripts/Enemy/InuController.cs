using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.AI;

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
    Creep, //inu stalking player
    Run, // inu killing player
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

    public InuState State
    {
        set
        {
            state = value;

            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorStateChange(actorID, (int)state);
            print("State" + state);
        }
    }

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
    //has player been too close
    private System.Boolean beenTooClose;
    //private float distanceToFloor = 0.8F;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private int posTimer;
    private GameObject nextFootprint;
    private NavMeshAgent agent;

    private Animator anim;
    private Vector3 oldSitPosition;
    private Vector3 newSitPosition;
    private int sitTimer;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        state = InuState.Idle;
        animState = InuAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponentInChildren<Animator>();
        beenTooClose = false;
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
        if (PlayerObject != null)
        {
            PlayerObject = GameObject.FindGameObjectWithTag("Player");
        }

        //manage state machine each update, call functions based on state
        if (state != InuState.Idle)
        {
            print("InuState " + state);
        }
       // print("AnimState " + animState);
        //print("AnimStateInt " + anim.GetInteger(" State"));

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

        //print("after second switch");
        
        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
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
            if (state != InuState.Stalk)
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
        MoveYokai();
    }

    void idle()
    {
        if (PlayerObject != null)
        {
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                awake = true;
                state = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = InuState.Follow;
            }
            else if (root != null)//awake == true && 
            {
                state = InuState.Patrol;
            }
        }
    }

    void patrol()
    {
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            awake = true;
            state = InuState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask);

        if (foundFootprint != null)
        {
            state = InuState.Follow;
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
                if (previous == null)
                {
                    previous = currentNode;
                    previous2 = previous;
                }
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
        //agent.ResetPath();
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (!seen)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = InuState.Follow;
            }
            else
            {
                state = InuState.Idle;
            }
        }
        
        Vector3 dest = PlayerObject.transform.position;
        agent.SetDestination(dest);

        if (rb.transform.position.x < dest.x + 3 && rb.transform.position.x > dest.x - 3)
        {
            if (rb.transform.position.y < dest.y + 3 && rb.transform.position.y > dest.y - 3)
            {
                if (rb.transform.position.z < dest.z + 3 && rb.transform.position.z > dest.z - 3)
                {
                    state = InuState.Stalk;
                    agent.SetDestination(rb.transform.position);
                }
            }
        }
    }

    void stalk()
    {
        animState = InuAnim.Creep;
        int maxDistance = 10;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = PlayerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                state = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = InuState.Follow;
                return;
            }
            else if (StartingNode != null)
            {
                state = InuState.Patrol;
                return;
            }
            else
            {
                state = InuState.Idle;
                return;
            }
        }
        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < maxDistance;
        if (playerTooCloseToEnemy && beenTooClose == false)
        {
            //signify the player is too close to the inu
            beenTooClose = true;
            //print("too close");
            //get the distance from inu to player
            Vector3 newdir = transform.localPosition - PlayerObject.transform.localPosition;
            newdir.y = 0;
            Vector3 newdir2 = newdir;
            //normalize to get direction only
            newdir.Normalize();
            //create a scalar
            float scalar = (float)Math.Sqrt(15);
            //scale direction vector to set distace to go
            newdir.Scale(new Vector3(scalar, 1, scalar));
            //set inu to go from current direction to scalar distance in normalized direction
            Vector3 goal = PlayerObject.transform.localPosition + newdir;

            //get distance to check
            float wallDistance = newdir.magnitude;
            //rayDirection = Vector3.MoveTowards
            newdir2.Normalize();
            Ray ray = new Ray(PlayerObject.transform.position, newdir);
            Debug.DrawRay(PlayerObject.transform.position, newdir, Color.green, 20.0F);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, wallDistance, LevelMask))
            {
                state = InuState.Cornered;
                print(rayHit.transform.position);
                //print("ray length" + newdir.magnitude);
                //print("ray hit wall");
                print(rayHit.collider.gameObject.name);
                return;
            }
            else
            {
                agent.ResetPath();
                agent.SetDestination(goal);
                //print("goal" + goal);
            }
        }
        else if (!playerTooCloseToEnemy)
        {
            beenTooClose = false;
            agent.SetDestination(PlayerObject.transform.position);
        }

        Vector3 dest = PlayerObject.transform.position;
        if (!playerTooCloseToEnemy)
        {
            if (rb.transform.position.x < dest.x + 3 && rb.transform.position.x > dest.x - 3)
            {
                if (rb.transform.position.y < dest.y + 3 && rb.transform.position.y > dest.y - 3)
                {
                    if (rb.transform.position.z < dest.z + 3 && rb.transform.position.z > dest.z - 3)
                    {
                        agent.SetDestination(rb.transform.position);
                    }
                }
            }
        }

        if (hasPlayerTripped())
        {
            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            print("GameOver");
        }
    }

    void cornered()
    {
        int maxDistance = 3;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = PlayerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;

        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                state = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                state = InuState.Follow;
                return;
            }
            else if (StartingNode != null)
            {
                state = InuState.Patrol;
                return;
            }
            else
            {
                state = InuState.Idle;
                return;
            }
        }

        //play growl

        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < maxDistance;
        if (playerTooCloseToEnemy && beenTooClose == false)
        {
            Vector3 goal = PlayerObject.transform.position;
            agent.ResetPath();
            agent.SetDestination(goal);
        }
    }

    void flee()
    {
        agent.ResetPath();
        agent.SetDestination(home);
        if (rb.transform.position.x < home.x + 2 && rb.transform.position.x > home.x - 2)
        {
            if (rb.transform.position.y < home.y + 1 && rb.transform.position.y > home.y - 1)
            {
                if (rb.transform.position.z < home.z + 2 && rb.transform.position.z > home.z - 2)
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
        agent.ResetPath();
        seen = false;
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            state = InuState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint == null)
            {
                state = InuState.Idle;
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
        agent.SetDestination(rb.transform.position);
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
        NavMeshAgent agent0 = GetComponent<NavMeshAgent>();
        if (agent0.velocity.magnitude < 0.5)
        {
            anim.SetInteger("State", 0);
        }
        else
        {
            animState = InuAnim.Walk;
        }
    }

    void animWalk()
    {
        NavMeshAgent agent0 = GetComponent<NavMeshAgent>();
        if (agent0.velocity.magnitude > 0.5)
        {
            anim.SetInteger("State", 1);
        }
        if (agent0.velocity.magnitude > 2.5)
        {
            animState = InuAnim.Run;
        }
        else
        {
            animState = InuAnim.Idle;
        }
    }

    void animRun()
    {
        NavMeshAgent agent0 = GetComponent<NavMeshAgent>();
        if (agent0.velocity.magnitude > 2.5)
        {
            anim.SetInteger("State", 2);
        }
        else
        {
            animState = InuAnim.Walk;
        }
    }
    
    void animCreep()
    {
        if (anim.GetInteger("State") == 2)
        {
            //anim.SetInteger("State", 1);
        }
        // if stalking player
        NavMeshAgent agent0 = GetComponent<NavMeshAgent>();

        if(state != InuState.Stalk)
        {
            if(state != InuState.Cornered)
            {
                animState = InuAnim.Idle;
            }
        }

        if (agent0.velocity.magnitude > 0.5)
        {
            anim.SetInteger("State", 3);
        }
        else
        {
            animState = InuAnim.Sit;
        }
    }

    void animAttack()
    {
        // if attacking player
        anim.SetInteger("State", 4);
    }

    void animStunned()
    {
        anim.SetInteger("State", 5);
    }

    void animSit()
    {
        // if stalking player but player is not moving
        //print("did something p1");
        NavMeshAgent agent0 = GetComponent<NavMeshAgent>();
       // print("vmag" + agent0.velocity.magnitude);
        if (agent0.velocity.magnitude < 0.5)
        {
            anim.SetInteger("State", 6);
            //print("did the thing");
        }
        else
        {
            animState = InuAnim.Creep;
            //print("thing didn't happen");
        }
        //print("did something p2");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trap"))
        {
            //dead();
        }
        if (state == InuState.Cornered)
        {
            if (other.gameObject == PlayerObject)
            {
                actorID = GetComponent<Actor>();
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
                GameManager.Instance.GameOver();
            }
        }
    }
}