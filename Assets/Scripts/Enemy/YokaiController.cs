using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

public class YokaiController : MonoBehaviour {

    public void Die()
    {
        print(gameObject);
        Destroy(gameObject);
    }

    public bool NoWall(GameObject desiredObject,  LayerMask levelMask, Vector3 home)
    {
        float maxDistance = 25;
        Vector3 rayOrigin = gameObject.transform.position;
        //print("ray origin " + rayOrigin);
        if (gameObject.CompareTag("Oni"))
        {
            rayOrigin = new Vector3(rayOrigin.x, home.y + 2.5F, rayOrigin.z);
        }
        if (gameObject.CompareTag("Inu"))
        {
            rayOrigin = new Vector3(rayOrigin.x, home.y + 1.5F, rayOrigin.z);
        }
        if (gameObject.CompareTag("Taka"))
        {
            rayOrigin = new Vector3(rayOrigin.x, home.y + 4.5F, rayOrigin.z);
        }
        Vector3 rayDirection = desiredObject.transform.position - gameObject.transform.position;
        if (VRDevice.isPresent)
        {
            if (desiredObject.CompareTag("Player"))
            {
                Transform playerTransform = desiredObject.transform.GetChild(0);
                rayDirection = playerTransform.localPosition - transform.localPosition;
            }
        }
        rayDirection.y = 0;
        maxDistance = rayDirection.magnitude;
        //rayDirection = Vector3.MoveTowards
        rayDirection.Normalize();
        rayDirection.Scale(new Vector3(maxDistance, 1, maxDistance));
        Ray ray = new Ray(rayOrigin, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection, Color.green, 5.0F);
        RaycastHit rayHit;
        
        if (Physics.Raycast(ray, out rayHit, maxDistance, levelMask))
        {
            return false;
        }
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

    public GameObject SeeFootprint(LayerMask levelMask, Vector3 home)
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
                distanceToFootprint.y = 0;
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
                rayDirection.Normalize();
                observerDirection.Normalize();
                float angleDot = Vector3.Dot(observerDirection, rayDirection);
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
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask, home);
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

    public bool FleeInu(LayerMask levelMask, Vector3 home)
    {
        GameObject[] inus;
        List<GameObject> close = new List<GameObject>();
        List<GameObject> valid = new List<GameObject>();
        inus = GameObject.FindGameObjectsWithTag("Inu");
        for (int iter = 0; iter < inus.Length; iter++)
        {
            if (inus[iter] != null)
            {
                Vector3 distanceToInu = inus[iter].transform.position - transform.position;
                float mag = distanceToInu.magnitude;
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
                rayDirection.Normalize();
                observerDirection.Normalize();
                float angleDot = Vector3.Dot(observerDirection, rayDirection);
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
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask, home);
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
        Transform playerTransform = playerObject.transform;
        if (VRDevice.isPresent)
        {
            playerTransform = playerObject.transform.GetChild(0);
        }
        float turnspeed = 1.0F;
        Vector3 targetDir = playerTransform.position - transform.position;
        float step = turnspeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    public bool SeeObject(GameObject desiredObject, LayerMask levelMask, Vector3 home)
    {
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection = desiredObject.transform.localPosition - transform.localPosition;
        if (VRDevice.isPresent)
        {
            if (desiredObject.CompareTag("Player"))
            {
                Transform playerTransform = desiredObject.transform.GetChild(0);
                rayDirection = playerTransform.localPosition - transform.localPosition;
            }
        }
        rayDirection.y = 0;
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        System.Boolean objectCloseToObserver = rayDirection.sqrMagnitude < maxDistanceSquared;
        rayDirection.Normalize();
        observerDirection.Normalize();
        float angleDot = Vector3.Dot(observerDirection, rayDirection);
        //print("see angle " + angleDot);
        
        System.Boolean objectInFrontOfObserver = angleDot > 0.0;
        //print("in front " + objectInFrontOfObserver);
        System.Boolean noWallfound = NoWall(desiredObject, levelMask, home);
        //print("no wall " + noWallfound);
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

    public MazeNode setClosest(MazeNode closest, MazeNode home, List<MazeNode> nodes, Rigidbody rb)
    {
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            foreach (MazeNode n in MazeGenerator.GetPath2(home, nodes[iter]))
                if (GameManager.trapNode(n))
                    trapInWay = true;
            if (!trapInWay)
            {
                if (closest == null)
                {
                    closest = nodes[iter];
                }
                Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - transform.position;
                float closestMag = closestPosition.magnitude;
                Vector3 iterPosition = new Vector3(nodes[iter].Col * 6 + 8, nodes[iter].Floor * 30, nodes[iter].Row * 6 + 8) - transform.position;
                float iterMag = iterPosition.magnitude;
                if (iterMag < closestMag)
                    closest = nodes[iter];
            }
        }
        return closest;
    }

    public MazeNode updateClosest(MazeNode closest, List<MazeNode> nodes, MazeNode currentNode, MazeNode previous, MazeNode previous2, Rigidbody rb)
    {
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            foreach (MazeNode n in MazeGenerator.GetPath2(currentNode, nodes[iter]))
                if (GameManager.trapNode(n))
                    trapInWay = true;
            if (!trapInWay)
            {
                if (nodes[iter] != currentNode && nodes[iter] != previous && nodes[iter] != previous2)
                {
                    if (closest == null)
                    {
                        closest = nodes[iter];
                    }
                    Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - transform.position;
                    float closestMag = closestPosition.magnitude;
                    Vector3 iterPosition = new Vector3(nodes[iter].Col * 6 + 8, nodes[iter].Floor * 30, nodes[iter].Row * 6 + 8) - transform.position;
                    float iterMag = iterPosition.magnitude;
                    if (iterMag < closestMag)
                    {
                        closest = nodes[iter];
                    }
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
