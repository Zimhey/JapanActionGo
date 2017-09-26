using System.Collections;
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
    public int TauntDistance;
    public int KillDistance;

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
    private Vector3 oldPosition2;
    private Vector3 newPosition;
    private int posTimer;
    private int posTimer2;
    private GameObject nextFootprint;
    private NavMeshAgent agent;

    private Transform playerTransform;

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
        distanceToFloor = home.y + 0.1F;
        startingRotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        State = TakaState.Idle;
        animState = TakaAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        if (!VRDevice.isPresent)
        {
            cam = PlayerObject.GetComponentInChildren<Camera>();
        }
        else
        {
            //code to assign camera once VR player is understood
        }
        oldPosition = home;
        posTimer = 60;
        posTimer2 = 27;
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
        playerTransform = PlayerObject.transform;
        if (VRDevice.isPresent)
        {
            if (PlayerObject.transform.childCount != 0)
            {
                playerTransform = PlayerObject.transform.GetChild(0);
            }
            else
            {
                print("player has no children");
            }
        }

        //manage state machine each update, call functions based on state
        if (state != TakaState.Idle)
        {
            //print("TakaState " + state);
        }

        if (newPosition != null)
        {
            if (oldPosition2 != null)
            {
                if (state != TakaState.Idle && state != TakaState.Taunt)
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
                            State = TakaState.Idle;
                        }
                    }
                }
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
        
        if (FleeInu(LevelMask, home))
        {
            State = TakaState.Flee;
        }

        posTimer--;
        if (posTimer <= 0)
        {
            posTimer = 90;
            if (newPosition != null)
            {
                oldPosition = newPosition;
                //print("oldpos" + oldPosition);
            }
            newPosition = transform.position;
            //print("newpos" + newPosition);
        }
        posTimer2--;
        if (posTimer2 <= 0)
        {
            posTimer = 77;
            if (oldPosition != null)
            {
                oldPosition2 = oldPosition;
            }
            oldPosition = transform.position;
        }

        if (distanceToFloor < 0.0F)
        {
            distanceToFloor = 0.0F;
        }

        //print("Taka is grounded " + gameObject.GetComponent<CharacterController>().isGrounded);
        //gameObject.transform.position = new Vector3(gameObject.transform.position.x, distanceToFloor, gameObject.transform.position.z);
        MoveYokai();
    }

    void idle()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = TakaState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            State = TakaState.Follow;
        }
        else if (root != null)//awake == true && 
        {
            State = TakaState.Patrol;
        }
    }

    void patrol()
    {
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = TakaState.Chase;
            return;
        }

        GameObject foundFootprint = SeeFootprint(LevelMask, home);

        if (foundFootprint != null)
        {
            State = TakaState.Follow;
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
                State = TakaState.Follow;
            }
            else
            {
                State = TakaState.Idle;
            }
        }
        
        agent.SetDestination(PlayerObject.transform.position);

        Vector3 dest = PlayerObject.transform.position;
        //agent.destination = dest;
        //print(dest);

        if (transform.position.x < dest.x + 5 && transform.position.x > dest.x - 5)
        {
            if (transform.position.z < dest.z + 5 && transform.position.z > dest.z - 5)
            {
                State = TakaState.Taunt;
                agent.SetDestination(transform.position);
                gameObject.transform.rotation = startingRotation;
            }
        }
        
    }

    void taunt() //getting shoved into ground
    {
        Vector3 rayDirection = playerTransform.localPosition - transform.localPosition;
        rayDirection.y = 0;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < TauntDistance;
        if (!playerCloseToEnemy)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = TakaState.Chase;
                return;
            }
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
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
        /*Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        rayDirection.Normalize();
        enemyDirection.Normalize();
        float angleDot = Vector3.Dot(enemyDirection, rayDirection);
        print("player in front " + angleDot);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean noWallfound = NoWall(PlayerObject, LevelMask, home);*/
        MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
        //print("mr size " + mr.transform.localScale.y);
        if (mr.transform.localScale.y < 8)
        {
            mr.transform.localScale += new Vector3(0, 0.02F, 0);
            mr.transform.position += new Vector3(0, 0.01F, 0);
            distanceToFloor += 0.01F;
        }
        else if (mr.transform.localScale.y >= 8)
        {
            //agent.height = 0.6F;
            State = TakaState.Flee;
            return;
        }
        /*print("in taunt");
        if (playerInFrontOfEnemy)
        {
            print("taunt in front");
            if (noWallfound)
            {
                print("taunt no wall");
                if (playerLookingUp())
                {
                    actorID = GetComponent<Actor>();
                    GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
                    GameManager.Instance.GameOver();
                    PlayerObject.SetActive(false);
                    print("GameOver");
                }
            }
        }*/
        if (playerLookingUp())
        {
            actorID = GetComponent<Actor>();
            GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            print("GameOver");
        }
    }

    void flee()
    {
        agent.ResetPath();
        MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();

        if (mr.transform.localScale.y > 5)
        {
            mr.transform.localScale -= new Vector3(0, 0.02F, 0);
            mr.transform.position -= new Vector3(0, 0.01F, 0);
            distanceToFloor -= 0.01F;
        }
        //gameObject.transform.position.Set(gameObject.transform.position.x, distanceToFloor, gameObject.transform.position.z);
        if (transform.position.x < home.x + 2 && transform.position.x > home.x - 2)
        {
            if (transform.position.z < home.z + 2 && transform.position.z > home.z - 2)
            {
                State = TakaState.Idle;
                gameObject.transform.rotation = startingRotation;
            }
        }

        agent.SetDestination(home);
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
            State = TakaState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
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
    }

    void SafeZoneCollision()
    {
        State = TakaState.Flee;
    }

    bool playerLookingUp()
    {
        Vector3 dir = cam.transform.rotation * Vector3.up;
        Vector3 enemyDirection = PlayerObject.transform.TransformDirection(Vector3.forward);
        dir.Normalize();
        enemyDirection.Normalize();
        float angleDot = Vector3.Dot(dir, enemyDirection);
        //print("player looking " + angleDot);
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
            //anim.SetInteger("State", 0);
        }
        else
            animState = TakaAnim.Walk;
    }

    void animWalk()
    {
        if (agent.velocity.magnitude > 0.5)
        {
            //anim.SetInteger("State", 1);
        }
        else
            animState = TakaAnim.Idle;
    }

    void animRun()
    {
        if (agent.velocity.magnitude > 5.5)
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
}