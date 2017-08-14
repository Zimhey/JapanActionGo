﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

//state machine for oni AI
public enum onistate
{
    Idle, // oni currently doing nothing
    Patrol, // oni has not seen player, wandering maze
    Search, // oni has seen player, cannot see player or footprints, is looking for player in maze
    Chase, // oni sees player, is moving towards player to attack
    Flee, // oni has encountered safe zone, is returning to home position
    Dead, // oni has encountered trap, no longer active
    Follow // oni does not see player, oni sees footprints, is moving towards footprints
}

public class OniController : MonoBehaviour
{

    //player game object
    public GameObject playerObject;
    //oni movement speed, set in unity
    public float speed;
    //layermask to raycast against
    public LayerMask levelmask;

    //oni physics body
    private Rigidbody rb;
    //oni starting point
    private Vector3 home;
    //oni starting rotation
    private Quaternion startingrotation;
    //current oni state
    private onistate state;
    //is player seen
    private System.Boolean seen;
    //has player been seen
    private System.Boolean awake;

    void Start()
    {
        //intialize variables
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        startingrotation = gameObject.transform.rotation;
        //print("OriHome" + home);
        state = onistate.Idle;
        awake = false;
    }

    void LateUpdate()
    {
        //manage state machine each update, call functions based on state
        print("State" + state);
        switch(state)
        {
            case onistate.Idle:
                idle();
                break;
            case onistate.Patrol:
                idle();
                break;
            case onistate.Search:
                idle();
                break;
            case onistate.Chase:
                chase();
                break;
            case onistate.Flee:
                flee();
                break;
            case onistate.Dead:
                dead();
                break;
            case onistate.Follow:
                follow();
                break;
        }
    }

    void idle()
    {
        seen = false;
        awake = false;
        seen = seePlayer();
        if (seen)
        {
            state = onistate.Chase;
        }
        else if (seeFootprint() && awake == true)
        {
            state = onistate.Follow;
        }
    }

    void patrol()
    {

    }

    void search()
    {

    }

    void chase()
    {
        seen = false;
        seen = seePlayer();
        if (!seen)
        {
            if (seeFootprint())
            {
                state = onistate.Follow;
            }
            else
            {
                state = onistate.Idle;
                awake = false;
            }
        }

        //by using a Raycast you make sure an enemy does not see you
        //if there is a building separating you from his view, for example
        //the enemy only sees you if it has you in open view
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, rayDirection, 100.0F);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.gameObject == playerObject) //player object here will be your Player GameObject
            {
                Transform goal = playerObject.transform; // set current player location as desired location
                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // get oni's navigation agent
                agent.destination = goal.position; // set destination to player's current location
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
        if (rb.transform.position.x < home.x + 0.5 && rb.transform.position.x > home.x - 0.5)
        {
            if (rb.transform.position.y < home.y + 0.5 && rb.transform.position.y > home.y - 0.5)
            {
                if (rb.transform.position.z < home.z + 0.5 && rb.transform.position.z > home.z - 0.5)
                {
                    state = onistate.Idle;
                    awake = false;
                    gameObject.transform.rotation = startingrotation;
                }
            }
        }
    }

    void dead()
    {
        Destroy(gameObject);
    }

    void follow()
    {
        seen = false;
        seen = seePlayer();
        if (seen)
        {
            state = onistate.Chase;
        }
        else if (!seeFootprint())
        {
            state = onistate.Idle;
            awake = false;
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

    bool seePlayer()
    {
        int maxDistance = 5;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        rayDirection.Normalize();
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        enemyDirection.Normalize();
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;

        Vector3 dir = gameObject.transform.rotation * new Vector3(0, 0, 1);

        Ray ray = new Ray(gameObject.transform.position, dir);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, maxDistance))
        {
            print(rayHit.GetType());
            print("ray sent");
            if (!rayHit.Equals(playerObject))
            {
                return false;
            }
        }

        if (playerInFrontOfEnemy)
        {
            System.Boolean seenPlayer = playerInFrontOfEnemy && playerCloseToEnemy;
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

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Footprint"))
        {
            Destroy(col.gameObject);
        }
        if (col.gameObject.CompareTag("Trap"))
        {
            dead();
        }
        if (col.gameObject.CompareTag("SafeZone"))
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (state != onistate.Flee)
            {
                agent.ResetPath();
                state = onistate.Flee;
            }
        }
        if (col.gameObject == playerObject)
        {
            string curlevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(curlevel);
        }
    }
}
//Physics.Raycast(transform.position, rayDirection, maxDistance)
/*Vector3 chase = playerObject.transform.position - transform.position;
                    Transform goal = playerObject.transform.position;
                    chase.Normalize();
                    Vector3 onimove = chase * speed;
                    UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                    rb.AddForce(onimove);
                    print(onimove);
                    seen = true;
                    
     //if an enemy as further than maxDistance from you, it cannot see you
        int maxDistance = 20;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;

        if (playerInFrontOfEnemy && playerCloseToEnemy)
        {
            //by using a Raycast you make sure an enemy does not see you
            //if there is a bulduing separating you from his view, for example
            //the enemy only sees you if it has you in open view
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, rayDirection, 100.0F);

            for (int i = 0; i < hits.Length; i++)
            {
                if (i == 0)
                {
                    seen = false;
                }
                RaycastHit hit = hits[i];
                if (hit.collider.gameObject == playerObject) //player object here will be your Player GameObject
                {
                    //enemy sees you - perform some action
                    Transform goal = playerObject.transform;
                    UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                    agent.destination = goal.position;
                    if (returning == false)
                    {
                        seen = true;
                        awake = true;
                    }
                }
                else if (i == hits.Length - 1 && seen == false)
                {
                    //enemy doesn't see you
                    //rb.velocity = new Vector3(0, 0, 0);
                }
            }
        }
        if (awake == true && seen == false)
        {
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
                    print("Chase" + agent.pathEndPosition);
                }
                else if (i == hitsft.Length - 1 && seen == false)
                {
                    //enemy doesn't see your footprints
                    //rb.velocity = new Vector3(0, 0, 0);
                }
            }
        }
        
        if (returning == true)
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            print("OriDest" + agent.destination);
            agent.destination = home;
            print("NewDest" + agent.destination);
            print(rb.transform.position);
            if (agent.isStopped == true)
            {
                agent.isStopped = false;

            }
            if(rb.transform.position.x < home.x + 0.5 && rb.transform.position.x > home.x - 0.5)
            {
                if (rb.transform.position.y < home.y + 0.5 && rb.transform.position.y > home.y - 0.5)
                {
                    if (rb.transform.position.z < home.z + 0.5 && rb.transform.position.z > home.z - 0.5)
                    {
                        returning = false;
                    }
                }
            }
        }
     */
//agent.isStopped = true;