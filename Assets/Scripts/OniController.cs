﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class OniController : MonoBehaviour
{

    //player game object
    public GameObject playerObject;

    //oni movement speed, set in unity
    public float speed;

    private Rigidbody rb;

    private System.Boolean seen = false;
    private System.Boolean awake = false;
    private System.Boolean returning = false;

    private Vector3 home;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        home = gameObject.transform.position;
        print("OriHome" + home);
    }

    void LateUpdate()
    {
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

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Footprint"))
        {
            Destroy(col.gameObject);
        }
        if (col.gameObject.CompareTag("Trap"))
        {
            Destroy(gameObject);
        }
        if (col.gameObject.CompareTag("SafeZone"))
        {
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (returning == false)
            {
                //agent.isStopped = true;
                agent.ResetPath();
                returning = true;
                awake = false;
                seen = false;
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
                    seen = true;*/
                     
