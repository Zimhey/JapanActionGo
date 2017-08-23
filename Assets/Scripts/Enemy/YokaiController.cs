using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokaiController : FootprintPlacer {
    public void Die()
    {
        Destroy(gameObject);
    }

    public bool SeePlayer(GameObject playerObject, LayerMask levelMask)
    {
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        Vector3 enemyDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, enemyDirection);
        System.Boolean playerCloseToEnemy = rayDirection.sqrMagnitude < maxDistanceSquared;

        //float crossangle = Vector3.Angle(enemyDirection, rayDirection);
        System.Boolean playerInFrontOfEnemy = angleDot > 0.0;

        System.Boolean noWallfound = NoWall(playerObject, levelMask);
        if (playerInFrontOfEnemy)
        {
            System.Boolean seenPlayer = playerInFrontOfEnemy && playerCloseToEnemy && noWallfound;
            return seenPlayer;
        }
        else
        {
            return false;
        }
    }

    public bool NoWall(GameObject playerObject,  LayerMask levelMask)
    {
        float maxDistance = 25;
        Vector3 rayDirection = playerObject.transform.position - transform.position;
        maxDistance = rayDirection.magnitude;
        //rayDirection = Vector3.MoveTowards
        rayDirection.Normalize();
        Ray ray = new Ray(gameObject.transform.position, rayDirection);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, maxDistance, levelMask))
        {
            return false;
        }
        return true;
    }

    public bool SeeFootprint(GameObject playerObject)
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

    public void ExecuteChase(UnityEngine.AI.NavMeshAgent agent, GameObject playerObject, LayerMask playerMask)
    {
        float maxDistance = 25;
        Vector3 rayDirection = playerObject.transform.position - transform.position;
        maxDistance = rayDirection.magnitude;
        //rayDirection = Vector3.MoveTowards
        rayDirection.Normalize();
        Ray ray = new Ray(gameObject.transform.position, rayDirection);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, maxDistance, playerMask))
        {
            Transform goal = playerObject.transform; // set current player location as desired location
            agent.destination = goal.position; // set destination to player's current location
        }
    }
    
    public void ExecuteFollow(UnityEngine.AI.NavMeshAgent agent, GameObject playerObject)
    {
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
                agent.destination = goal.position;
                //print("Follow" + agent.pathEndPosition);
            }
        }
    }

    public bool FleeInu(UnityEngine.AI.NavMeshAgent agent, GameObject playerObject)
    {
        Vector3 rayDirection = playerObject.transform.localPosition - transform.localPosition;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, rayDirection, 100.0F);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.CompareTag("Inu")) //if inu is seen
            {
                Vector3 distancetoinu = hit.collider.transform.position - gameObject.transform.position;
                float mag = distancetoinu.magnitude;
                if (mag < 50.0F)
                {
                    return true;
                }

            }
        }
        return false;
    }

    public void TurnTowardsPlayer(GameObject playerObject)
    {
        float turnspeed = 1.0F;
        Vector3 targetDir = playerObject.transform.position - transform.position;
        float step = turnspeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }
}
