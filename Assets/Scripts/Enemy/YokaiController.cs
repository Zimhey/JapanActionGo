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
        return SeeObject(playerObject, levelMask);
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
        //rather than raycast, given a list of footprints, iterate through once determining close enough and make list of that, then iterate through to see if correct direction
        //then iterate through if no wall, take first, seek to it
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
        //when passed a footprint, go to it
    }

    public bool FleeInu(GameObject playerObject)
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
        //given a list of inu iterate through to check validity
    }

    public void TurnTowardsPlayer(GameObject playerObject)
    {
        float turnspeed = 1.0F;
        Vector3 targetDir = playerObject.transform.position - transform.position;
        float step = turnspeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    public bool SeeObject(GameObject gameObject, LayerMask levelMask)
    {
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = gameObject.transform.localPosition - transform.localPosition;
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, observerDirection);
        System.Boolean objectCloseToObserver = rayDirection.sqrMagnitude < maxDistanceSquared;

        //float crossangle = Vector3.Angle(enemyDirection, rayDirection);
        System.Boolean objectInFrontOfObserver = angleDot > 0.0;

        System.Boolean noWallfound = NoWall(gameObject, levelMask);
        if (objectInFrontOfObserver)
        {
            System.Boolean seenPlayer = objectInFrontOfObserver && objectCloseToObserver && noWallfound;
            return seenPlayer;
        }
        else
        {
            return false;
        }
    }
}
