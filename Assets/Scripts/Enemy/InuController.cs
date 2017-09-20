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

            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorStateChange(actorID, (int)state);
            //print("InuState " + state);
        }
    }

    public InuAnim AnimState
    {
        set
        {
            animState = value;

            anim.SetInteger("State", (int) animState);

            //print("InuAnimState " + animState);
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
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        State = InuState.Idle;
        AnimState = InuAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        beenTooClose = false;
        oldPosition = home;
        posTimer = 60;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        currentNode = StartingNode;
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;
        retreating = false;

        int column = (int)((home.x - 8) / 6);
        int floor = (int)(home.y / 30);
        int row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;
    }

    void LateUpdate()
    {
        //print("State" + state);
        if (PlayerObject != null)
        {
            PlayerObject = GameObject.FindGameObjectWithTag("Player");
        }

        //manage state machine each update, call functions based on state
        if (state != InuState.Idle)
        {
            //print("InuState " + state);
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
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
            if (foundFootprint != null)
            {
                State = InuState.Follow;
            }
            else if (root != null)//awake == true && 
            {
                State = InuState.Patrol;
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
            State = InuState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask);

        if (foundFootprint != null)
        {
            State = InuState.Follow;
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
                State = InuState.Follow;
            }
            else
            {
                State = InuState.Idle;
            }
        }
        
        Vector3 dest = PlayerObject.transform.position;
        agent.SetDestination(dest);

        if (rb.transform.position.x < dest.x + 5 && rb.transform.position.x > dest.x - 5)
        {
            if (rb.transform.position.z < dest.z + 5 && rb.transform.position.z > dest.z - 5)
            {
                State = InuState.Stalk;
                agent.SetDestination(rb.transform.position);
            }
        }
    }

    void stalk()
    {
        AnimState = InuAnim.Creep;
        Vector3 rayDirection = PlayerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < StalkDistance;
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
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
        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < StartCorneredDistance;
        if (playerTooCloseToEnemy && beenTooClose == false)
        {
            //signify the player is too close to the inu
            beenTooClose = true;
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

        if(!playerTooCloseToEnemy && beenTooClose == true)
        {
            retreating = false;
            beenTooClose = false;
        }

        if (retreating != true)
        {
            agent.ResetPath();
            Vector3 dest = PlayerObject.transform.position;

            if (rb.transform.position.x < dest.x + 3 && rb.transform.position.x > dest.x - 3)
            {
                if (rb.transform.position.z < dest.z + 3 && rb.transform.position.z > dest.z - 3)
                {
                    agent.ResetPath();
                    agent.SetDestination(rb.transform.position);
                }
            }
            else
            {
                print("stalking towards player");
                agent.SetDestination(dest);
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
        //print("inu is cornered");
        Vector3 rayDirection = PlayerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < StayCorneredDistance;

        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeePlayer(PlayerObject, LevelMask);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask);
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
            print("trying to kill player");
            Vector3 goal = PlayerObject.transform.position;
            agent.ResetPath();
            agent.SetDestination(goal);
        }

        System.Boolean playerKillDistance = rayDirection.sqrMagnitude < KillDistance;
        if (playerKillDistance && beenTooClose == true)
        {
            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            GameManager.Instance.GameOver();
        }


    }

    void flee()
    {
        agent.ResetPath();
        agent.SetDestination(home);
        if (rb.transform.position.x < home.x + 2 && rb.transform.position.x > home.x - 2)
        {
            if (rb.transform.position.z < home.z + 2 && rb.transform.position.z > home.z - 2)
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
        seen = SeePlayer(PlayerObject, LevelMask);
        if (seen)
        {
            State = InuState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask);
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
        stunTimer = 480;
        agent.SetDestination(rb.transform.position);
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
        // if attacking player
    }

    void animStunned()
    {
    }

    void animSit()
    {
        // if stalking player but player is not moving
        if (agent.velocity.magnitude < 0.5)
        {
        }
        else
        {
            AnimState = InuAnim.Creep;
        }
    }
}