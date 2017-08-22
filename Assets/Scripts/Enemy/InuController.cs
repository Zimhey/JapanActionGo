using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

//state machine for inu AI
public enum inustate
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

public class InuController : MonoBehaviour
{

    //player game object
    public GameObject playerObject;
    //starting node for patrol
    public GameObject startingNode;
    //inu movement speed, set in unity
    public float speed;
    //layermask to raycast against
    public LayerMask levelmask;

    //inu physics body
    private Rigidbody rb;
    //inu starting point
    private Vector3 home;
    //inu starting rotation
    private Quaternion startingrotation;
    //current inu state
    private inustate state;
    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //array of locations the inu has been
    private ArrayList previousLocations = new ArrayList();
    private int lessenough = 5;
    //current node for patrol
    private GameObject currentNode;
    //countdown until no longer stunned
    private int stuntimer;
    //has player been too close
    private System.Boolean beenTooClose;

    private GameObject footprintPrefab;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingrotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = inustate.Idle;
        awake = false;
        currentNode = startingNode;
        playerObject = GameObject.FindGameObjectWithTag("Player");
        footprintPrefab = Actors.Prefabs[ActorType.Okuri_Inu_Footprint];
        beenTooClose = false;
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        if (state != inustate.Idle)
            print("State" + state);
        switch (state)
        {
            case inustate.Idle:
                idle();
                break;
            case inustate.Patrol:
                patrol();
                break;
            case inustate.Search:
                search();
                break;
            case inustate.Chase:
                chase();
                break;
            case inustate.Stalk:
                stalk();
                break;
            case inustate.Flee:
                flee();
                break;
            case inustate.Dead:
                dead();
                break;
            case inustate.Follow:
                follow();
                break;
            case inustate.Stun:
                stun();
                break;
        }

        //if any locations exist
        if (previousLocations.Count > 0)
        {
            //get latest
            int lastentry = previousLocations.Count - 1;
            Vector3 lastlocation = (Vector3)previousLocations[lastentry];
            //make sure it is not current location
            if (lastlocation != rb.position)
            {
                //add current location
                previousLocations.Add(rb.position);
            }

            //get oldest
            Vector3 firstlocation = (Vector3)previousLocations[0];
            //check to see if inu has gone far enough for footprints to form
            if ((lastlocation - firstlocation).magnitude > lessenough)
            {
                //iterate through
                for (int iter = 0; iter < lastentry; iter++)
                {
                    //check locations along path
                    Vector3 currentlocation = (Vector3)previousLocations[iter];
                    //if locations are far enough apart make footprint at a given distance
                    if ((currentlocation - firstlocation).magnitude > lessenough)
                    {
                        //get direction
                        Vector3 norm = (currentlocation - firstlocation);
                        norm.Normalize();
                        //multiply by desired distance to get desired vector and add to first location
                        Vector3 dest = firstlocation + norm * (float)lessenough - new Vector3(0, 0.8F, 0);
                        //make rotation
                        Quaternion rot = Quaternion.Euler(0, 0, 0);
                        //add to level
                        Instantiate(footprintPrefab, dest, rot);
                        //remove old steps
                        for (int remove = 0; remove < iter; remove++)
                        {
                            previousLocations.RemoveAt(0);
                        }
                        break;
                    }
                }
            }
        }
        else
        {
            previousLocations.Add(rb.position);
        }
        if (awake == true)
        {
            float turnspeed = 1.0F;
            Vector3 targetDir = playerObject.transform.position - transform.position;
            float step = turnspeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
            transform.rotation = Quaternion.LookRotation(newDir);
        }

    }

    void idle()
    {
        seen = false;
        seen = seePlayer();
        if (seen)
        {
            state = inustate.Chase;
        }
        else if (seeFootprint() && awake == true)
        {
            state = inustate.Follow;
        }
        else if (awake == true && startingNode != null)//awake == true && 
        {
            state = inustate.Patrol;
        }
    }

    void patrol()
    {
        seen = false;
        seen = seePlayer();
        if (seen)
        {
            state = inustate.Chase;
        }
        else if (seeFootprint() && awake == true)
        {
            state = inustate.Follow;
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
        seen = seePlayer();
        System.Boolean chasing = false;
        if (!seen)
        {
            if (seeFootprint())
            {
                state = inustate.Follow;
            }
            else
            {
                state = inustate.Idle;
            }
        }

        //by using a Raycast you make sure an enemy does not see you
        //if there is a building separating you from his view, for example
        //the enemy only sees you if it has you in open view
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, rayDirection, 100.0F);
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.gameObject == playerObject) //player object here will be your Player GameObject
            {
                Transform goal = playerObject.transform; // set current player location as desired location
                agent.destination = goal.position; // set destination to player's current location
                chasing = true;
            }
        }

        if (chasing == true)
        {
            Vector3 dest = playerObject.transform.position;
            agent.destination = dest;
            if (rb.transform.position.x < dest.x + 5 && rb.transform.position.x > dest.x - 5)
            {
                if (rb.transform.position.y < dest.y + 5 && rb.transform.position.y > dest.y - 5)
                {
                    if (rb.transform.position.z < dest.z + 5 && rb.transform.position.z > dest.z - 5)
                    {
                        state = inustate.Stalk;
                        agent.destination = rb.transform.position;
                        gameObject.transform.rotation = startingrotation;
                    }
                }
            }
        }
    }

    void stalk()
    {
        int maxDistance = 10;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;
        if (!playerCloseToEnemy)
        {
            beenTooClose = false;
            seen = false;
            seen = seePlayer();
            if (seen)
            {
                state = inustate.Chase;
            }
            else if (seeFootprint())
            {
                state = inustate.Follow;
            }
            else if (startingNode != null)
            {
                state = inustate.Patrol;
            }
            else
            {
                state = inustate.Idle;
            }
        }
        System.Boolean playerTooCloseToEnemy = rayDirection.sqrMagnitude < maxDistance;
        if (playerTooCloseToEnemy && beenTooClose == false)
        {
            beenTooClose = true;
            print("too close");
            Vector3 newdir = playerObject.transform.localPosition - transform.localPosition;
            newdir.Normalize();
            float scalar = (float)Math.Sqrt(100 / 3);
            newdir.Scale(new Vector3(scalar, scalar, scalar));
            Vector3 goal = playerObject.transform.localPosition - newdir;

            float wallDistance = 25;
            wallDistance = rayDirection.magnitude;
            //rayDirection = Vector3.MoveTowards
            rayDirection.Normalize();
            Ray ray = new Ray(gameObject.transform.position, rayDirection);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, maxDistance, levelmask))
            {
                state = inustate.Cornered;
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
        //print("OriDest" + agent.destination);
        agent.destination = home;
        //print("NewDest" + agent.destination);
        //print("Curpos" + rb.transform.position);
        if (rb.transform.position.x < home.x + 1 && rb.transform.position.x > home.x - 1)
        {
            if (rb.transform.position.y < home.y + 1 && rb.transform.position.y > home.y - 1)
            {
                if (rb.transform.position.z < home.z + 1 && rb.transform.position.z > home.z - 1)
                {
                    state = inustate.Idle;
                    gameObject.transform.rotation = startingrotation;
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
        seen = seePlayer();
        if (seen)
        {
            state = inustate.Chase;
        }
        else if (!seeFootprint())
        {
            state = inustate.Idle;
        }

        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hitsft;
        hitsft = Physics.RaycastAll(transform.position, rayDirection, 100.0F);
        for (int i = 0; i < hitsft.Length; i++)
        {
            RaycastHit hitft = hitsft[i];
            if (hitft.collider.gameObject.CompareTag("Footprint"))
            {
                //enemy sees your footprints - perform some action
                Transform goal = hitft.collider.gameObject.transform;
                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                agent.destination = goal.position;
                //print("Follow" + agent.pathEndPosition);
            }
        }
    }

    void stun()
    {
        stuntimer--;
        if (stuntimer <= 0)
        {
            seen = false;
            seen = seePlayer();
            if (seen)
            {
                state = inustate.Chase;
            }
            else if (seeFootprint() && awake == true)
            {
                state = inustate.Follow;
            }
            else
            {
                state = inustate.Idle;
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void Stun()
    {
        state = inustate.Stun;
        stuntimer = 480;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = rb.position;
    }

    void SafeZoneCollision()
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.ResetPath();
        state = inustate.Flee;
    }

    bool seePlayer()
    {
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;

        float crossangle = Vector3.Angle(enemyDirection, rayDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;

        System.Boolean nowallfound = noWall();
        if (playerInFrontOfEnemy)
        {
            System.Boolean seenPlayer = playerInFrontOfEnemy && playerCloseToEnemy && nowallfound;
            if (seenPlayer)
            {
                awake = true;
            }
            return seenPlayer;
        }
        else
        {
            return false;
        }
    }

    bool noWall()
    {
        float maxDistance = 25;
        Vector3 rayDirection = playerObject.transform.position - transform.position;
        maxDistance = rayDirection.magnitude;
        //rayDirection = Vector3.MoveTowards
        rayDirection.Normalize();
        Ray ray = new Ray(gameObject.transform.position, rayDirection);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, maxDistance, levelmask))
        {
            return false;
        }
        return true;
    }

    bool seeFootprint()
    {
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hitsft;
        hitsft = Physics.RaycastAll(transform.position, rayDirection, 100.0F);
        for (int i = 0; i < hitsft.Length; i++)
        {
            RaycastHit hitft = hitsft[i];
            if (hitft.collider.gameObject.CompareTag("Footprint"))
            {
                return true;
            }
        }
        return false;
    }

    bool hasPlayerTripped()
    {
        return false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Trap"))
        {
            dead();
        }
        if (col.gameObject.CompareTag("SafeZone"))
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (state != inustate.Flee)
            {
                agent.ResetPath();
                state = inustate.Flee;
            }
        }
        if (col.gameObject == playerObject)
        {
            //string curlevel = SceneManager.GetActiveScene().name;
            //SceneManager.LoadScene(curlevel);
        }
    }
}