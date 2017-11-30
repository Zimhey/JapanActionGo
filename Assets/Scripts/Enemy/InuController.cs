using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.VR;

//state machine for inu AI
public enum InuState
{
    Idle, // inu currently doing nothing
    Patrol, // inu has not seen player, wandering maze
    LookAround,
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
    //distance to stay stalking the player
    public int StalkDistance;
    //distance to start trying to avoid being cornered
    public int StartCorneredDistance;
    //distance to stay cornered
    public int StayCorneredDistance;
    //distance lash back at player in cornered
    public int CorneredChaseDistance;
    //distance to kill player while cornered
    public int KillDistance;
    //timer for inu to attack
    public int AttackTime;
    public bool TestDebug;

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
    //bool containing if the inu is attempting to avoid being cornered
    private bool retreating;
    private float AttackTimer;

    public InuState State
    {
        set
        {
            int checker = 0;
            if (state == InuState.Patrol)
            {
                ClearPaths();
                if (currentNode != null)
                    currentNode.EnemyPathNode = true;
            }

            state = value;
            GameManager.Instance.ActorStateChange(actorID, (int)state);

            if (state == InuState.Flee)
            {
                fleeTimer = 5;
            }
            else if (state == InuState.Stalk)
            {
                checker = 1;
            }
            else if (state == InuState.Cornered)
            {
                checker = 1;
            }

            if(checker == 0)
            {
                AttackTimer = AttackTime;
            }
        }
    }

    public InuAnim AnimState
    {
        set
        {
            animState = value;

            anim.SetInteger("State", (int) animState);
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
    private List<MazeNode> nodes;
    private List<MazeNode> allNodes;
    private MazeNode currentNode;
    private MazeNode root;
    private MazeNode previous;
    private MazeNode previous2;
    private MazeNode homeNode;
    private float lookTimer;
    //countdown until no longer stunned
    private float stunTimer;
    //has player been too close
    private System.Boolean beenTooClose;
    //private float distanceToFloor = 0.8F;
    private Vector3 oldPosition;
    private Vector3 oldPosition2;
    private Vector3 newPosition;
    private float posTimer;
    private float posTimer2;
    private FootprintList nextFootprint;
    private NavMeshAgent agent;
    private float fleeTimer;
    private MazeNode fleeTarget = null;
    LinkedList<MazeNode> fleePath = new LinkedList<MazeNode>();

    private Animator anim;
    private Vector3 oldSitPosition;
    private Vector3 newSitPosition;
    private float sitTimer;

    private Transform playerTransform;
    private CharacterController controller;

    private FootprintList foundFootprint;
    private Vector3 currentNodePosition;
    private MazeNode closest;
    private Vector3 rayDirection;
    private System.Boolean playerCloseToEnemy;
    private int column;
    private int row;
    private Actor player;
    private Vector3 dest;
    private System.Boolean playerTooCloseToEnemy;
    private Vector3 newdir;
    private MazeNode destinationNode;
    private MazeNode secondDestNode;
    private MazeNode tertDestNode;
    private Vector3 currentLocation;
    private MazeNode fromNode;
    private MazeNode playerNode;
    private int rand;
    private List<MazeNode> adjacent;
    private Vector3 inuToPlayer;
    private Vector3 inuToTert;
    private float scalar;
    private Vector3 goal;
    private float wallDistance;
    private Ray ray;
    private RaycastHit rayHit;
    private System.Boolean playerKillDistance;


    void Start()
    {
        //intialize variables
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingRotation = gameObject.transform.rotation;
        actorID = GetComponent<Actor>();
        State = InuState.Idle;
        AnimState = InuAnim.Idle;
        awake = false;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        beenTooClose = false;
        oldPosition = home;
        posTimer = 10;
        posTimer2 = 5;
        stunTimer = 0;
        AttackTimer = AttackTime;
        root = MazeGenerator.getSectionBasedOnLocation(home);
        if (root != null)
        {
            nodes = MazeGenerator.GetIntersectionNodes(root);
        }
        currentNode = StartingNode;
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;
        agent.nextPosition = transform.position;
        //transform.position = agent.nextPosition;
        agent.Warp(transform.position);
        retreating = false;

        column = (int)((home.x - 8) / 6);
        row = (int)((home.z - 8) / 6);

        allNodes = MazeGenerator.nodesInSection(root);

        foreach (MazeNode n in allNodes)
            if (n.Col == column && n.Row == row)
                homeNode = n;
    }

    void LateUpdate()
    {
        if (TestDebug)
        {
             print(state);
        }
        if (actorID == null)
        {
            actorID = GetComponent<Actor>();
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (PlayerObject != null)
            playerTransform = PlayerObject.transform;
        else
            playerTransform = null;

        if (stunTimer > 0)
        {
            if (state != InuState.Stun)
            {
                state = InuState.Stun;
            }
        }

        switch (state)
        {
            case InuState.Idle:
                idle();
                break;
            case InuState.Patrol:
                patrol();
                break;
            case InuState.LookAround:
                look();
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

        if (awake == true)
        {
            TurnTowardsPlayer(PlayerObject);
        }

        posTimer -= Time.deltaTime;
        if (posTimer <= 0)
        {
            posTimer = 30;
            oldPosition = newPosition;
            newPosition = transform.position;
        }
        posTimer2 -= Time.deltaTime;
        if (posTimer2 <= 0)
        {
            posTimer2 = 25;
            oldPosition2 = oldPosition;
            oldPosition = transform.position;
        }
        
        MoveYokai(controller, agent);
    }

    void idle()
    {
        posTimer = 30;
        posTimer2 = 25;
        if (PlayerObject != null)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                awake = true;
                State = InuState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                State = InuState.Follow;
            }
            else if (root != null)
            {
                State = InuState.Patrol;
            }
        }
    }

    void patrol()
    {
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = InuState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            awake = true;
            State = InuState.Chase;
            return;
        }

        foundFootprint = SeeFootprint(allNodes, LevelMask, home);

        if (foundFootprint != null)
        {
            nextFootprint = foundFootprint;
            State = InuState.Follow;
            return;
        }

        if (root != null)
        {
            bool setCurrent = false;

            if (currentNode == null)
            {
                closest = null;
                closest = SetClosest(closest, homeNode, nodes, rb);
                currentNode = closest;
                if (previous == null)
                {
                    previous = currentNode;
                    previous2 = previous;
                }
                setCurrent = true;
            }

            if (currentNode != null)
            {
                currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);

                if (setCurrent == false)
                {
                    if (Vector3.Distance(transform.position, currentNodePosition) < 2)
                    {
                        lookTimer = 4;
                        agent.SetDestination(transform.position);
                        state = InuState.LookAround;
                        /*{
                            MazeNode closest = null;
                            closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                            if (closest != null)
                            {
                                previous2 = previous;
                                previous = currentNode;
                                currentNode = closest;
                            }
                            
                            if (Vector3.Distance(transform.position, currentNodePosition) < 2)
                            { 
                                lookTimer = 6;
                                    agent.SetDestination(transform.position);
                                    state = InuState.LookAround;
                            }
                        }
                            */
                        if (currentNode != null)
                            currentNodePosition = new Vector3(currentNode.Col * 6 + 8, currentNode.Floor * 30, currentNode.Row * 6 + 8);
                    }
                    agent.SetDestination(currentNodePosition);
                }
            }
        }
    }

    void look()
    {
        posTimer = 30;
        posTimer2 = 25;
        lookTimer -= Time.deltaTime;
        //print(lookTimer);
        if (FleeInu(LevelMask, home))
        {
            State = InuState.Flee;
            return;
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            //if player has been seen chase
            awake = true;
            State = InuState.Chase;
            return;
        }
        foundFootprint = SeeFootprint(allNodes, LevelMask, home);
        if (foundFootprint != null)
        {
            //if footprints found follow
            nextFootprint = foundFootprint;
            State = InuState.Follow;
            return;
        }
        transform.Rotate(Vector3.up * (360 * Time.deltaTime));
        if (lookTimer <= 0)
        {
            if (root != null)
            {
                //old destination reached, update patrol path
                closest = null;
                closest = UpdateClosest(closest, nodes, currentNode, previous, previous2, rb);
                if (closest != null)
                {
                    previous2 = previous;
                    previous = currentNode;
                    currentNode = closest;
                }

                State = InuState.Patrol;
                return;
            }
        }
    }

    void search()
    {

    }

    void chase()
    {
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = InuState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (!seen)
        {
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                State = InuState.Follow;
            }
            else
            {
                State = InuState.Idle;
            }
        }
        
        dest = playerTransform.position;
        agent.SetDestination(dest);

        if (Vector3.Distance(transform.position, dest) < 5)
        {
                //if close enough to player attempt to stalk player
                State = InuState.Stalk;
                agent.SetDestination(transform.position);
        }
    }

    //function to execute in stalk state, contains transitions, and code to maintain distance from player and attempt to avoid being cornered
    void stalk()
    {
        posTimer = 30;
        posTimer2 = 25;
        if (AttackTimer > 0)
        {
            AttackTimer -= Time.deltaTime;
        }
        //print(AttackTimer);
        //attack timer reachers 0 attack
        if (AttackTimer <= 0)
        {
            State = InuState.Cornered;
            return;
        }

        AnimState = InuAnim.Creep;
        //rayDirection = playerTransform.position - transform.position;
        //rayDirection.y = 0;
        playerCloseToEnemy = Vector3.Distance(playerTransform.position, transform.position) < StalkDistance;
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
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
       
        //check to see if player is close enough to trigger cornered state
        playerTooCloseToEnemy = Vector3.Distance(playerTransform.position, transform.position) < StartCorneredDistance;
        if (playerTooCloseToEnemy)
        {
            //signify the player is too close to the inu
            //print("too close");
            beenTooClose = true;
            //get the distance from player to inu
            newdir = transform.position - playerTransform.position;
            //create containers for attempted destinations
            destinationNode = null;
            secondDestNode = null;
            tertDestNode = null;
            //get current node based on location
            currentLocation = new Vector3(transform.position.x, home.y + 1.5F, transform.position.z);
            fromNode = MazeGenerator.getNodeBasedOnLocation(currentLocation);
            //print("current location " + new Vector3(transform.position.x, home.y + 1.5F, transform.position.z));
            //print("from node " + new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
            //get the player's current node
            playerNode = MazeGenerator.getNodeBasedOnLocation(playerTransform.position);

            //if the change in x is greater than the change in z try to move in the x direction first
            if (Math.Abs(newdir.x) > Math.Abs(newdir.z))
            {
                //if the change in x is positive
                if(newdir.x > 0)
                {
                    //set primary destination to be the node with the next higher value in the x direction
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1)  * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                    //if change in z is positive
                    if (newdir.z > 0)
                    {
                        //set secondary destination to be the node with the next higher value in the z direction
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                    }

                    if (newdir.z < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                    }
                }

                if(newdir.x < 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                    if (newdir.z > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                    }

                    if (newdir.z < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                    }
                }
            }
            //if the change in x is the same as the change in the z direction, used rand with two possible values
            if (Math.Abs(newdir.x) == Math.Abs(newdir.z))
            {
                rand = UnityEngine.Random.Range(0, 1);

                if (rand == 0)
                {
                    if (newdir.x > 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                        if (newdir.z > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                        }

                        if (newdir.z < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                        }
                    }

                    if (newdir.x < 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));

                        if (newdir.z > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));
                        }

                        if (newdir.z < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));
                        }
                    }
                }

                if (rand == 1)
                {
                    if (newdir.z > 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));

                        if (newdir.x > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }

                        if (newdir.x < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }
                    }

                    if (newdir.z < 0)
                    {
                        destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));


                        if (newdir.x > 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }

                        if (newdir.x < 0)
                        {
                            secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                        }
                    }
                }
            }
            //if the change in x is less than the change in z try to move in the z direction first
            if (Math.Abs(newdir.x) < Math.Abs(newdir.z))
            {

                if (newdir.z > 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row + 1) * 6 + 8));

                    if (newdir.x > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }

                    if (newdir.x < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }
                }

                if (newdir.z < 0)
                {
                    destinationNode = MazeGenerator.getNodeBasedOnLocation(new Vector3(fromNode.Col * 6 + 8, fromNode.Floor * 30, (fromNode.Row - 1) * 6 + 8));

                    if (newdir.x > 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col + 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }

                    if (newdir.x < 0)
                    {
                        secondDestNode = MazeGenerator.getNodeBasedOnLocation(new Vector3((fromNode.Col - 1) * 6 + 8, fromNode.Floor * 30, fromNode.Row * 6 + 8));
                    }
                }
            }

            //get the list of nodes adjacent to the inu's current node
            adjacent = fromNode.GetAdjacentNodes();
            for(int iter = 0; iter < adjacent.Count; iter++)
            {
                //if new node, it is recored as tertiary destination
                if(adjacent[iter] != destinationNode && adjacent[iter] != secondDestNode && adjacent[iter] != playerNode)
                {
                    tertDestNode = adjacent[iter];
                    //check to see if tertiary is behind player and thus not valid
                    inuToPlayer = playerTransform.position - transform.position;
                    inuToTert = new Vector3(tertDestNode.Col * 6 + 8, tertDestNode.Floor * 30, tertDestNode.Row * 6 + 8) - transform.position;
                    if(inuToPlayer.x > 0 && inuToTert.x > 0)
                    {
                        if(inuToPlayer.z > 0 && inuToTert.z > 0)
                        {
                            tertDestNode = null;
                        }

                        if (inuToPlayer.z < 0 && inuToTert.z < 0)
                        {
                            tertDestNode = null;
                        }
                    }
                    if (inuToPlayer.x < 0 && inuToTert.x < 0)
                    {
                        if (inuToPlayer.z > 0 && inuToTert.z > 0)
                        {
                            tertDestNode = null;
                        }

                        if (inuToPlayer.z < 0 && inuToTert.z < 0)
                        {
                            tertDestNode = null;
                        }
                    }
                }
            }

            //check if primary and secondary are valid nodes
            if(adjacent.Contains(destinationNode) == false)
            {
                if (destinationNode != null)
                {
                    //print("primary not adjacent " + new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                    destinationNode = null;
                }
                else
                {
                    //print("no primary dest found");
                }
            }
            if (adjacent.Contains(secondDestNode) == false)
            {
                if (secondDestNode != null)
                {
                    //print("secondary not adjacent " + new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    secondDestNode = null;
                }
                else
                {
                    //print("no secondary dest found");
                }
            }


            //try nodes in order
            if(destinationNode == null)
            {
                //print("primary not valid");
                if(secondDestNode == null)
                {
                    //print("secondary not valid");
                    if(tertDestNode == null)
                    {
                        //print("tertiary not valid");
                        //try to move backwards anyways
                        newdir.y = 0;
                        //normalize to get direction only
                        newdir.Normalize();
                        //create a scalar
                        scalar = (float)Math.Sqrt(15);
                        //scale direction vector to set distace to go
                        newdir.Scale(new Vector3(scalar, 1, scalar));
                        //set inu to go from current direction to scalar distance in normalized direction

                        goal = playerTransform.position + newdir;
                        wallDistance = newdir.magnitude;
                        ray = new Ray(playerTransform.position, newdir);

                        //if wall in the way transition to cornered
                        if (Physics.Raycast(ray, out rayHit, wallDistance, LevelMask))
                        {
                            State = InuState.Cornered;
                            return;
                        }
                        //else move backwards away from player
                        else
                        {
                            agent.ResetPath();
                            agent.SetDestination(goal);
                            retreating = true;
                            return;
                        }
                    }
                    //move to tertiary destination
                    else
                    {
                        //print("trying to go to tert " + new Vector3(tertDestNode.Col * 6 + 8, fromNode.Floor * 30, tertDestNode.Row * 6 + 8));
                        agent.ResetPath();
                        agent.SetDestination(new Vector3(tertDestNode.Col * 6 + 8, fromNode.Floor * 30, tertDestNode.Row * 6 + 8));
                        retreating = true;
                        return;
                    }
                }
                //move to secondary destination
                else
                {
                    //print("trying to go to second " + new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    agent.ResetPath();
                    agent.SetDestination(new Vector3(secondDestNode.Col * 6 + 8, fromNode.Floor * 30, secondDestNode.Row * 6 + 8));
                    retreating = true;
                    return;
                }
            }
            //move to primary destination
            else
            {
                print("trying to go to primary " + new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                agent.ResetPath();
                agent.SetDestination(new Vector3(destinationNode.Col * 6 + 8, fromNode.Floor * 30, destinationNode.Row * 6 + 8));
                retreating = true;
                return;
            }

        }

        //if player is not close enough for cornered
        if(!playerTooCloseToEnemy && beenTooClose == true)
        {
            retreating = false;
            beenTooClose = false;
        }

        //if not retrreating maintain distance from player
        if (retreating != true)
        {
            agent.ResetPath();
            dest = playerTransform.position;

            if (Vector3.Distance(transform.position, dest) < 5)
            {
                    agent.ResetPath();
                    agent.SetDestination(transform.position);
            }
            else
            {
                //print("stalking towards player");
                agent.SetDestination(dest);
            }
        }

        //if player has tripper kill them, trip not currently implemented
        if (hasPlayerTripped())
        {
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                player = PlayerObject.GetComponentInParent<Actor>();
                GameManager.Instance.ActorKilled(actorID, player);
            }
            else
            {
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            }
            GameManager.Instance.GameOver();
            PlayerObject.SetActive(false);
            print("GameOver");
        }
        
    }

    //function to execute in cornered state, contains transitions, and code to deal with player
    void cornered()
    {
        posTimer = 30;
        posTimer2 = 25;
        if (AttackTimer > 0)
        {
            AttackTimer -= Time.deltaTime;
        }
        //print(AttackTimer);
        //rayDirection = playerTransform.position - transform.position;
        //rayDirection.y = 0;
        playerKillDistance = Vector3.Distance(playerTransform.position, transform.position) < KillDistance;
        //attack timer reachers 0 attack
        if (AttackTimer <= 0)
        {
            goal = playerTransform.position;
            agent.ResetPath();
            agent.SetDestination(goal);

            if (playerKillDistance)
            {
                if (UnityEngine.XR.XRDevice.isPresent)
                {
                    player = PlayerObject.GetComponentInParent<Actor>();
                    GameManager.Instance.ActorKilled(actorID, player);
                }
                else
                {
                    GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
                }
                GameManager.Instance.GameOver();
            }
            return;
        }
        
        playerCloseToEnemy = Vector3.Distance(playerTransform.position, transform.position) < StayCorneredDistance;

        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
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

        //if player gets to close inu should charge at player
        playerTooCloseToEnemy = Vector3.Distance(playerTransform.position, transform.position) < CorneredChaseDistance;
        if (playerTooCloseToEnemy && beenTooClose == true)
        {
            goal = playerTransform.position;
            agent.ResetPath();
            agent.SetDestination(goal);
        }

        //if inu gets close enough to player kill the player
        if (playerKillDistance && beenTooClose == true)
        {
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                player = PlayerObject.GetComponentInParent<Actor>();
                GameManager.Instance.ActorKilled(actorID, player);
            }
            else
            {
                GameManager.Instance.ActorKilled(actorID, PlayerObject.GetComponent<Actor>());
            }
            GameManager.Instance.GameOver();
        }


    }

    void flee()
    {
        posTimer = 30;
        posTimer2 = 25;
        fleeTimer -= Time.deltaTime;
        if (fleeTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            if (seen)
            {
                awake = true;
                State = InuState.Chase;
                return;
            }
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
            if (foundFootprint != null)
            {
                nextFootprint = foundFootprint;
                State = InuState.Follow;
                return;
            }
        }
        agent.ResetPath();

        Vector3 targetPos = new Vector3();
        if (fleeTarget == null)
        {
            MazeNode presentNode = new MazeNode();
            bool obstacle = false;
            int column = homeNode.Col;
            int row = homeNode.Row;

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

        if (Vector3.Distance(transform.position, targetPos) < 2)
        {
            State = InuState.Idle;
            gameObject.transform.rotation = startingRotation;
        }

        /*
        if (Vector3.Distance(transform.position, home) < 2)
        {
                State = InuState.Idle;
                gameObject.transform.rotation = startingRotation;
        }
        */
    }

    void dead()
    {
        Die();
    }

    void follow()
    {
        //agent.ResetPath();
        if (Vector3.Distance(transform.position, home) < 2)
        {
            if (IsStuck(newPosition, oldPosition, oldPosition2))
            {
                posTimer = 0;
                posTimer = 5;
                if (TestDebug)
                {
                    print("resetting path");
                }
                agent.ResetPath();
                previous2 = previous;
                previous = currentNode;
                currentNode = null;
                State = InuState.Flee;
                return;
            }
        }
        seen = false;
        seen = SeeObject(PlayerObject, LevelMask, home);
        if (seen)
        {
            State = InuState.Chase;
            nextFootprint = null;
            return;
        }
        if (nextFootprint == null)
        {
            foundFootprint = SeeFootprint(allNodes, LevelMask, home);
            if (foundFootprint == null)
            {
                nextFootprint = foundFootprint;
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
            if (Vector3.Distance(transform.position, nextFootprint.transform.position) < 2)
            {
                    nextFootprint = nextFootprint.getNext();
            }

            agent.SetDestination(nextFootprint.transform.position);
        }
    }

    void stun()
    {
        posTimer = 30;
        posTimer2 = 25;
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            seen = false;
            seen = SeeObject(PlayerObject, LevelMask, home);
            foundFootprint = SeeFootprint(nodes, LevelMask, home);
            if (seen)
            {
                State = InuState.Chase;
            }
            else if (foundFootprint != null && awake == true)
            {
                nextFootprint = foundFootprint;
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
        AnimState = InuAnim.Stunned;
        stunTimer = 10;
        agent.SetDestination(transform.position);
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
    }

    void animStunned()
    {
    }

    void animSit()
    {
        if (agent.velocity.magnitude < 0.5)
        {
        }
        else
        {
            AnimState = InuAnim.Creep;
        }
    }
}