using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class YokaiController : MonoBehaviour {

    public void Die()
    {
        print(gameObject);
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
        //print("player found");
        return true;
    }

    public void MoveYokai()
    {
        CharacterController controller = GetComponent<CharacterController>(); // TODO make this and other components fields
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        agent.nextPosition = transform.position;
        Vector3 velocity = agent.desiredVelocity + Physics.gravity;
        agent.velocity = velocity;
        controller.Move(velocity * Time.deltaTime * 1);

    }

    public GameObject SeeFootprint(LayerMask levelMask)
    {
        GameObject[] footprints;
        List<GameObject> close = new List<GameObject>();
        List<GameObject> valid = new List<GameObject>();
        footprints = GameObject.FindGameObjectsWithTag("Footprint");
        for(int iter = 0; iter < footprints.Length; iter++)
        {
            if (footprints[iter] != null)
            {
                Vector3 distanceToFootprint = footprints[iter].transform.position - transform.position;
                float mag = distanceToFootprint.magnitude;
                if (mag < 25)
                {
                    close.Add(footprints[iter]);
                }
            }
        }
        for (int iter2 = 0; iter2 < close.Count; iter2++)
        {
            if (close[iter2] != null)
            {
                Vector3 rayDirection = close[iter2].transform.localPosition - transform.localPosition;
                Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
                float angleDot = Vector3.Dot(rayDirection, observerDirection);
                if (angleDot > 0)
                {
                    valid.Add(close[iter2]);
                }
            }
        }
        for (int iter3 = 0; iter3 < valid.Count; iter3++)
        {
            if (valid[iter3] != null)
            {
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask);
                if (noWallfound)
                {
                    //print("found footprint at" + valid[iter3].transform.position);
                    return valid[iter3];
                }
                //print("wallfound");
            }
        }
        return null;
    }

    public bool FleeInu(LayerMask levelMask)
    {
        GameObject[] inus;
        List<GameObject> close = new List<GameObject>();
        List<GameObject> valid = new List<GameObject>();
        inus = GameObject.FindGameObjectsWithTag("Inu");
        for (int iter = 0; iter < inus.Length; iter++)
        {
            if (inus[iter] != null)
            {
                Vector3 distanceToFootprint = inus[iter].transform.position - transform.position;
                float mag = distanceToFootprint.magnitude;
                if (mag < 15)
                {
                    close.Add(inus[iter]);
                }
            }
        }
        for (int iter2 = 0; iter2 < close.Count; iter2++)
        {
            if (close[iter2] != null)
            {
                Vector3 rayDirection = close[iter2].transform.localPosition - transform.localPosition;
                Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
                float angleDot = Vector3.Dot(rayDirection, observerDirection);
                if (angleDot > 0)
                {
                    valid.Add(close[iter2]);
                }
            }
        }
        for (int iter3 = 0; iter3 < valid.Count; iter3++)
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
        //print(rayDirection);
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        //print(observerDirection);
        float angleDot = Vector3.Dot(rayDirection, observerDirection);
        //print(angleDot);
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

    public MazeNode setClosest(MazeNode closest, List<MazeNode> nodes, Rigidbody rb)
    {
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            if (closest == null)
            {
                closest = nodes[iter];
            }
            Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - rb.transform.position;
            float closestMag = closestPosition.magnitude;
            Vector3 iterPosition = new Vector3(nodes[iter].Col * 6 + 8, nodes[iter].Floor * 30, nodes[iter].Row * 6 + 8) - rb.transform.position;
            float iterMag = iterPosition.magnitude;
            if (iterMag < closestMag)
            {
                closest = nodes[iter];
            }
        }
        return closest;
    }

    public MazeNode updateClosest(MazeNode closest, List<MazeNode> nodes, MazeNode currentNode, MazeNode previous, Rigidbody rb)
    {
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            if (nodes[iter] != currentNode && nodes[iter] != previous)
            {
                if (closest == null)
                {
                    closest = nodes[iter];
                }
                Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - rb.transform.position;
                float closestMag = closestPosition.magnitude;
                Vector3 iterPosition = new Vector3(nodes[iter].Col * 6 + 8, nodes[iter].Floor * 30, nodes[iter].Row * 6 + 8) - rb.transform.position;
                float iterMag = iterPosition.magnitude;
                if (iterMag < closestMag)
                {
                    closest = nodes[iter];
                }
            }
        }
        return closest;
    }

    public bool isStuck(Vector3 oldPosition, Vector3 newPosition)
    {
        if (oldPosition == newPosition)
        {
            return true;
        }
        return false;
    }
}
