using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

//state machine for taka AI
public enum takastate
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

public class TakaController : MonoBehaviour
{

    //player game object
    public GameObject playerObject;
    //starting node for patrol
    public GameObject startingNode;
    //taka movement speed, set in unity
    public float speed;
    //layermask to raycast against
    public LayerMask levelmask;

    //taka physics body
    private Rigidbody rb;
    //taka starting point
    private Vector3 home;
    //taka starting rotation
    private Quaternion startingrotation;
    //current taka state
    private takastate state;
    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;
    //array of locations the taka has been
    private ArrayList previousLocations = new ArrayList();
    private int lessenough = 5;
    //current node for patrol
    private GameObject currentNode;
    //countdown until no longer stunned
    private int stuntimer;
    private Camera cam;
    private float distanceToFloor = 2.5F;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingrotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = takastate.Idle;
        awake = false;
        currentNode = startingNode;
        playerObject = GameObject.FindGameObjectWithTag("Player");
        cam = playerObject.GetComponentInChildren<Camera>();
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        print("State" + state);
        switch (state)
        {
            case takastate.Idle:
                idle();
                break;
            case takastate.Patrol:
                patrol();
                break;
            case takastate.Search:
                search();
                break;
            case takastate.Chase:
                chase();
                break;
            case takastate.Taunt:
                taunt();
                break;
            case takastate.Flee:
                flee();
                break;
            case takastate.Dead:
                dead();
                break;
            case takastate.Follow:
                follow();
                break;
            case takastate.Stun:
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
            //check to see if taka has gone far enough for footprints to form
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
                        Vector3 dest = firstlocation + norm * (float)lessenough - new Vector3(0, distanceToFloor, 0);
                        //make rotation
                        Quaternion rot = Quaternion.Euler(0, 0, 0);
                        //add to level
                        Instantiate(Resources.Load("Prefabs/OniFootprint"), dest, rot);
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

        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, rayDirection, 100.0F);
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.CompareTag("Inu")) //if inu is seen
            {
                Vector3 distancetoinu = hit.collider.transform.position - gameObject.transform.position;
                float mag = distancetoinu.magnitude;
                if (mag < 50.0F)
                {
                    state = takastate.Flee;
                }
                Transform goal = playerObject.transform; // set current player location as desired location
                agent.destination = goal.position; // set destination to player's current location

            }
        }
    }

    void idle()
    {
        seen = false;
        seen = seePlayer();
        if (seen)
        {
            state = takastate.Chase;
        }
        else if (seeFootprint() && awake == true)
        {
            state = takastate.Follow;
        }
        else if (awake == true && startingNode != null)
        {
            state = takastate.Patrol;
        }
    }

    void patrol()
    {
        seen = false;
        seen = seePlayer();
        if (seen)
        {
            state = takastate.Chase;
        }
        else if (seeFootprint() && awake == true)
        {
            state = takastate.Follow;
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
        seen = seePlayer();
        System.Boolean chasing = false;
        if (!seen)
        {
            if (seeFootprint())
            {
                state = takastate.Follow;
            }
            else
            {
                state = takastate.Idle;
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
                        state = takastate.Taunt;
                        agent.destination = rb.transform.position;
                        gameObject.transform.rotation = startingrotation;
                    }
                }
            }
        }
    }

    void taunt()
    {
        int maxDistance = 7;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - (transform.localPosition - new Vector3(0,distanceToFloor,0));
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;
        if (!playerCloseToEnemy)
        {
            seen = false;
            seen = seePlayer();
            if (seen)
            {
                state = takastate.Chase;
            }
            else if (seeFootprint())
            {
                state = takastate.Follow;
            }
            else if(startingNode != null)
            {
                state = takastate.Patrol;
            }
            else 
            {
                state = takastate.Idle;
            }
        }

        //play taunt sounds
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean nowallfound = noWall();

        if (gameObject.transform.localScale.y < 10)
        {
            gameObject.transform.localScale += new Vector3(0, 0.01F, 0);
            gameObject.transform.position += new Vector3(0, 0.005F, 0);
            distanceToFloor += 0.005F;
        }
        else if (gameObject.transform.localScale.y >= 10)
        {
            state = takastate.Flee;
        }
        if (playerInFrontOfEnemy)
        {
            if (nowallfound)
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
        //print("OriDest" + agent.destination);
        agent.destination = home;
        //print("NewDest" + agent.destination);
        //print("Curpos" + rb.transform.position);
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
                    state = takastate.Idle;
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
            state = takastate.Chase;
        }
        else if (!seeFootprint())
        {
            state = takastate.Idle;
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
                state = takastate.Chase;
            }
            else if (seeFootprint() && awake == true)
            {
                state = takastate.Follow;
            }
            else
            {
                state = takastate.Idle;
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void Stun()
    {
        state = takastate.Stun;
        stuntimer = 120;
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

    bool playerLookingUp()
    {
        Vector3 dir = cam.transform.rotation * Vector3.up;
        Vector3 enemyDirection = playerObject.transform.TransformDirection(Vector3.forward);
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
        if (col.gameObject.CompareTag("SafeZone"))
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (state != takastate.Flee)
            {
                agent.ResetPath();
                state = takastate.Flee;
            }
        }
        if (col.gameObject == playerObject)
        {
            string curlevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(curlevel);
        }
    }
}