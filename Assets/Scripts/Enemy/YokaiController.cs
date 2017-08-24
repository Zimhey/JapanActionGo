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

    public GameObject SeeFootprint(LayerMask levelMask)
    {
        GameObject[] footprints = new GameObject[100];
        GameObject[] close = new GameObject[100];
        GameObject[] valid = new GameObject[100];
        footprints = GameObject.FindGameObjectsWithTag("Footprint");
        for(int iter = 0; iter < 100; iter++)
        {
            if (footprints[iter] != null)
            {
                Vector3 distanceToFootprint = footprints[iter].transform.position - transform.position;
                float mag = distanceToFootprint.magnitude;
                if (mag < 50)
                {
                    close[iter] = footprints[iter];
                }
            }
        }
        for (int iter2 = 0; iter2 < 100; iter2++)
        {
            if (close[iter2] != null)
            {
                Vector3 rayDirection = close[iter2].transform.localPosition - transform.localPosition;
                Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
                float angleDot = Vector3.Dot(rayDirection, observerDirection);
                if (angleDot > 0)
                {
                    valid[iter2] = close[iter2];
                }
            }
        }
        for (int iter3 = 0; iter3 < 100; iter3++)
        {
            if (valid[iter3] != null)
            {
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask);
                if (noWallfound)
                {
                    return valid[iter3];
                }
            }
        }
        return null;
    }

    public bool FleeInu(LayerMask levelMask)
    {
        GameObject[] inus = new GameObject[100];
        GameObject[] close = new GameObject[100];
        GameObject[] valid = new GameObject[100];
        inus = GameObject.FindGameObjectsWithTag("Inu");
        for (int iter = 0; iter < 100; iter++)
        {
            if (inus[iter] != null)
            {
                Vector3 distanceToFootprint = inus[iter].transform.position - transform.position;
                float mag = distanceToFootprint.magnitude;
                if (mag < 50)
                {
                    close[iter] = inus[iter];
                }
            }
        }
        for (int iter2 = 0; iter2 < 100; iter2++)
        {
            if (close[iter2] != null)
            {
                Vector3 rayDirection = close[iter2].transform.localPosition - transform.localPosition;
                Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
                float angleDot = Vector3.Dot(rayDirection, observerDirection);
                if (angleDot > 0)
                {
                    valid[iter2] = close[iter2];
                }
            }
        }
        for (int iter3 = 0; iter3 < 100; iter3++)
        {
            if (valid[iter3] != null)
            {
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask);
                if (noWallfound)
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

    public bool SeeObject(GameObject desiredObject, LayerMask levelMask)
    {
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = desiredObject.transform.localPosition - transform.localPosition;
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        float angleDot = Vector3.Dot(rayDirection, observerDirection);
        System.Boolean objectCloseToObserver = rayDirection.sqrMagnitude < maxDistanceSquared;

        //float crossangle = Vector3.Angle(enemyDirection, rayDirection);
        System.Boolean objectInFrontOfObserver = angleDot > 0.0;

        System.Boolean noWallfound = NoWall(desiredObject, levelMask);
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
