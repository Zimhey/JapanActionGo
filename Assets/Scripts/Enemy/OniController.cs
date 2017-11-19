using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

//state machine for oni AI
public enum OniState
{
    Idle, // oni currently doing nothing
    Patrol, // oni has not seen player, wandering maze
    LookAround, // on reaching intersection in patrol look around
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
    //layermasks to raycast against
    public LayerMask LevelMask;
    public LayerMask PlayerMask;
    //distance at which the oni kills the player while in chase
    public int KillDistance;

    //oni physics body
    private Rigidbody rb;
    //oni starting point
    private Vector3 home;
    //oni starting rotation
    private Quaternion startingRotation;
    
    //current anim state
    private OniAnim animState;

    //actorID for database
    private Actor actorID;

    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //list of patrol nodes for a section
    private List<MazeNode> nodes;
    //current node for patrol
    private MazeNode currentNode;
    //root node for a section
    private MazeNode root;
    //the latest visited patrol node
    private MazeNode previous;
    //the second latest visited patrol node
    private MazeNode previous2;
    //the node the oni spawned in
    private MazeNode homeNode;
    //countdown to continue patroling
    private int lookTimer;
    //countdown until no longer stunned
    private int stunTimer;
    //the last visited position recored
    private Vector3 oldPosition;
    //the second latest visited position recored
    private Vector3 oldPosition2;
    //the currently recored visited position
    private Vector3 newPosition;
    //countdown timer for updating new position and old position
    private int posTimer;
    //countdown timer for updating old position2
    private int posTimer2;
    //the footprint the agent is going to attempt to navigate towards
    private GameObject nextFootprint;
    //the oni's navigation agent
    private NavMeshAgent agent;
    //the countdown timer ensuring the oni flees for a short while before being capable of other actions
    private int fleeTimer;
    private MazeNode fleeTarget = null;
    LinkedList<MazeNode> fleePath = new LinkedList<MazeNode>();

    //the animator controlling which animations play
    private Animator anim;

    //the current state in the state machine
    private OniState state;

    //the player's transform position
    private Transform playerTransform;
    //the oni's character controller
    private CharacterController controller;
    //a bool tracking if the oni should be focusing on fleeing
    private bool fleeingInu;

    public OniState State
    {
        set
        {
            if(state == OniState.Patrol)
            {
                ClearPaths();
            }

            state = value;
            //log state change to database
            GameManager.Instance.ActorStateChange(actorID, (int) state);
            //set fleetimer if changing state to flee
            if(state == OniState.Flee)
            {
                fleeTimer = 30;
                //print(fleeTimer);
            }
        }
    }

    //function called at initialization of oni
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
        if(root != null)
        {
            nodes = MazeGenerator.GetIntersectionNodes(root);
        }
        fleeTimer = 30;
        fleeingInu = false;

        currentNode = StartingNode;
        //find home node based on location, a nodes location is its individual values - 8 and then divided by 6
        int column = (int)((home.x - 8) / 6);
        int row = (int)((home.z - 8) / 6);

        foreach (MazeNode n in MazeGenerator.nodesInSection(root))
            if (n.Col == column && n.Row == row)
                homeNode = n;

        agent = GetComponent<NavMeshAgent>();
        //turn of default nav mesh movement as it doesn't include gravity
        agent.updatePosition = false;
        agent.updateRotation = true;
    }

    //determin oni's actions for the current game loop
    void LateUpdate()
    {
        //print("Oni state " + state);
        if (state == OniState.Flee)
            print(homeNode.Col + " " + homeNode.Row);
        if (actorID == null)
        {
            actorID = GetComponent<Actor>();
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        //update player transform
        if (PlayerObject != null)
            playerTransform = PlayerObject.transform;
        else
            playerTransform = null;
        
        if (stunTimer > 0)
        {
            if (state != OniState.Stun)
            {
                state = OniState.Stun;
            }
        }
        //determine action to take based on state in state machine
        switch (state)
        {
            case OniState.Idle:
                idle();
                break;
            case OniState.Patrol:
                patrol();
                break;
            case OniState.LookAround:
                look();
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

        //update animations
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

        //if aware of player turn towards them
        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        //if enough timer has passed update recordings of positions
        posTimer--;
        if(posTimer <= 0)
        {
            posTimer = 90;
            oldPosition = newPosition;
            newPosition = transform.position;
        }
        posTimer2--;
        if (posTimer2 <= 0)
        {
            posTimer2 = 77;
            oldPosition2 = oldPosition;
            oldPosition = transform.position;
        }

        //move yokai based on state
        MoveYokai(controller, agent);
    }

    //function to be performed in idle state, containes transitions to other states
    void idle()
    {
        posTimer = 90;
        posTimer2 = 77;
        if (FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
            return;
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            //if player has been seen chase
            awake = true;
            State = OniState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            //if footprints found follow
            State = OniState.Follow;
            return;
        }
        if (root != null)
        {
            //if neither and root exist, attempt to patrol
            State = OniState.Patrol;
            return;
        }
    }
    
    //function to execute in patrol state, contains transitions as well as code to navigate patrol nodes
    void patrol()
    {
        if (FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
            return;
        }
        if (transform.position.x > home.x + 2 || transform.position.x < home.x - 2 ||
            transform.position.z > home.z + 2 || transform.position.z < home.z - 2)
        {
            //if positions have not changed enough determine Oni to be stuck and change behavior pattern
            //reset timers to give chance to move before checking again
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                print("resetting path");
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = OniState.Flee;
                return;
            }
        }

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

        //if nodes exist
        if (root != null)
        {
            //container for current node's position
            Vector3 currentNodePosition;
            //bool to check if current was set and not updated
            bool setCurrent = false;

            if (currentNode == null)
            {
                //if current does not exist
                MazeNode closest = null;
                //call set to start patrol path
                closest = SetClosest(closest, homeNode, nodes, rb);
                //update current
                currentNode = closest;
                //update previous
                if (previous == null)
                {
                    previous = currentNode;
                    previous2 = previous;
                }
                //indicate current was set
                setCurrent = true;
            }

            //if current exists
            if (currentNode != null)
            {
                //get current node's position
                currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);

                //if current was not set this game loop, check to see if updating is necessary
                if (setCurrent == false)
                {
                    if (transform.position.x < currentNodePosition.x + 2 && transform.position.x > currentNodePosition.x - 2)
                    {
                        if (transform.position.z < currentNodePosition.z + 2 && transform.position.z > currentNodePosition.z - 2)
                        {
                            lookTimer = 60;
                            agent.SetDestination(transform.position);
                            state = OniState.LookAround;
                        }
                    }
                    //update current node's postion
                    if(currentNode != null)
                        currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
                }
                //set A.I. to move to current node
                agent.SetDestination(currentNodePosition);
            }
        }
    }

    void look()
    {
        posTimer = 90;
        posTimer2 = 77;
        lookTimer--;
        //print(lookTimer);
        if (FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
            return;
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            //if player has been seen chase
            awake = true;
            State = OniState.Chase;
            return;
        }
        GameObject foundFootprint = SeeFootprint(LevelMask, home);
        if (foundFootprint != null)
        {
            //if footprints found follow
            State = OniState.Follow;
            return;
        }
        transform.Rotate(Vector3.up * (360 * Time.deltaTime));
        if (lookTimer <= 0)
        {
            if (root != null)
            {
                //old destination reached, update patrol path
                MazeNode closest = null;
                closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                if (closest != null)
                {
                    previous2 = previous;
                    previous = currentNode;
                    currentNode = closest;
                }

                State = OniState.Patrol;
                return;
            }
        }
    }

    //function to be used in search state, currently not implemented
    void search()
    {
        Vector3 newdir = transform.forward * 10;
        Vector3 goal = transform.position - newdir;
        agent.destination = goal; 
    }

    //function to execute in chase state, contains transitions, code to move towards player, and code to kill player
    void chase()
    {
        //ensure old path is cleared
        //agent.ResetPath();
        if (FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
            return;
        }
        if (transform.position.x > home.x + 2 || transform.position.x < home.x - 2 ||
            transform.position.z > home.z + 2 || transform.position.z < home.z - 2)
        {
            //if positions have not changed enough determine Oni to be stuck and change behavior pattern
            //reset timers to give chance to move before checking again
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                print("resetting path");
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = OniState.Flee;
                return;
            }
        }
        //check if oni can still see player
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (!seen)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint != null)
            {
                State = OniState.Follow;
                return;
            }
            else
            {
                State = OniState.Idle;
                return;
            }
        }

        Vector3 rayDirection = playerTransform.position - transform.position;
        rayDirection.y = 0;
        //check if player is within the kill distance
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < KillDistance;
        if (playerCloseToEnemy)
        {
            //if VR device is present the player's actor component is in its parent
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                Actor player = PlayerObject.GetComponentInParent<Actor>();
                GameManager.Instance.ActorKilled(actorID, player);
            }
            else
            {
                //otherwise the player's actor component is a part of the player object
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            }
            State = OniState.GameOver;
            //let game manager know that the game should end
            GameManager.Instance.GameOver();
            print("GameOver");
        }

        //have oni set the player's current position to be its destination
        agent.SetDestination(playerTransform.position);
    }

    //function to execute in flee state, contains transitions, and code to return to spawn position
    void flee()
    {
        posTimer = 90;
        posTimer2 = 77;
        //if enough time has passed the oni may interrupt flee to chase player or follow footprints
        fleeTimer--;
        if (fleeTimer <= 0)
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
        }
        //return to home position
        agent.ResetPath();
        foreach (MazeNode n in currentPath)
            n.EnemyPathNode = false;

        //make sure that all the current pathnodes are made not enemy path nodes
        //check iterate through to see if there is an obstacle in the way
        //if there is, set the new destination as the spot right before the obstacle
        //otherwise, set the home as destination
        //either way, set path to location as enemy path nodes
        Vector3 targetPos = new Vector3();
        if (fleeTarget == null)
        {
            MazeNode presentNode = new MazeNode();
            bool obstacle = false;
            int column = (int)((transform.position.x - 8) / 6);
            int row = (int)((transform.position.z - 8) / 6);

            foreach (MazeNode n in MazeGenerator.nodesInSection(root))
                if (n.Col == column && n.Row == row)
                    presentNode = n;

            LinkedList<MazeNode> possiblePath = MazeGenerator.GetPath2(presentNode, homeNode);
            MazeNode prevCheckNode = presentNode;

            foreach (MazeNode n in possiblePath)
            {
                if (n.EnemyPathNode || GameManager.trapNode(n))
                {
                    fleeTarget = prevCheckNode;
                    obstacle = true;
                    break;
                }
                prevCheckNode = n;
            }
            if (!obstacle)
                fleeTarget = homeNode;

            fleePath = MazeGenerator.GetPath2(presentNode, fleeTarget);
            foreach (MazeNode n in fleePath)
                n.EnemyPathNode = true;
        }

        targetPos = new Vector3(fleeTarget.Col * 6 + 8, fleeTarget.Floor * 30, fleeTarget.Row * 6 + 8);
        agent.SetDestination(targetPos);

        if (transform.position.x < targetPos.x + 2 && transform.position.x > targetPos.x - 2)
        {
            if (transform.position.z < targetPos.z + 2 && transform.position.z > targetPos.z - 2)
            {
                //undo path nodes except the one she ends on
                foreach(MazeNode n in fleePath)
                    if (n.Col != fleeTarget.Col || n.Row != fleeTarget.Row)
                        n.EnemyPathNode = false;
                fleeTarget = null;
                State = OniState.Idle;
                gameObject.transform.rotation = startingRotation;
                return;
            }
        }
    }

    //if the oni recieves the message "dead" the oni must kill itself
    void dead()
    {
        Die();
        print("oni has died");
    }

    //function to execute in follow state, contains transitions, and code to follow footprints towards player
    void follow()
    {
        //agent.ResetPath();
        if (FleeInu(LevelMask, home))
        {
            State = OniState.Flee;
            return;
        }
        if (transform.position.x > home.x + 2 || transform.position.x < home.x - 2 ||
            transform.position.z > home.z + 2 || transform.position.z < home.z - 2)
        {
            //if positions have not changed enough determine Oni to be stuck and change behavior pattern
            //reset timers to give chance to move before checking again
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                print("resetting path");
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = OniState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            State = OniState.Chase;
            nextFootprint = null;
            return;
        }
        //if there is no next footprint try to find a new footprint to follow
        if (nextFootprint == null)
        {
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (foundFootprint == null)
            {
                State = OniState.Idle;
                return;
            }
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                agent.SetDestination(foundFootprint.transform.position);
            }
        }
        //else move towards next footprint
        else
        {
            if (transform.position.x < nextFootprint.transform.position.x + 2 && transform.position.x > nextFootprint.transform.position.x - 2)
            {
                if (transform.position.z < nextFootprint.transform.position.z + 2 && transform.position.z > nextFootprint.transform.position.z - 2)
                {
                    //update next footprint to continue following the trail
                    nextFootprint = nextFootprint.GetComponent<FootprintList>().getNext();
                }
            }
            agent.SetDestination(nextFootprint.transform.position);
        }
    }

    //function to execute in stun state, containes transitions, and decrements stun timer
    void stun()
    {
        posTimer = 90;
        posTimer2 = 77;
        //decrement stun timer
        stunTimer--;
        //if enough timer has passed transition to appropiate state
        if(stunTimer <= 0)
        {
            animState = OniAnim.Idle;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            GameObject foundFootprint = SeeFootprint(LevelMask, home);
            if (seen)
            {
                State = OniState.Chase;
                return;
            }
            else if (foundFootprint != null && awake == true)
            {
                State = OniState.Follow;
                return;
            }
            else
            {
                State = OniState.Idle;
                return;
            }
        }
    }

    //if oni recieves stun message execute the following code
    void Stun()
    {
        //transition to stun state
        State = OniState.Stun;
        animState = OniAnim.Stunned;
        //set timer
        stunTimer = 300;
        //stop motion
        agent.SetDestination(transform.position);
        //stun has priority over fleeing inu
        fleeingInu = false;
    }

    void gameOver()
    {

    }

    //if oni collides with a safe zone it will flee back to home
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
        anim.SetInteger("State", 3);
    }

    void animStunned()
    {
        anim.SetInteger("State", 4);
    }

    void animLook()
    {
        anim.SetInteger("State", 5);
    }
}

