using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

public class YokaiController : MonoBehaviour {

    private LinkedList<MazeNode> previousPath;
    private LinkedList<MazeNode> currentPath;

    public void Die()
    {
        print(gameObject);
        Destroy(gameObject);
    }

    public bool NoWall(GameObject desiredObject,  LayerMask levelMask, Vector3 home)
    {
        float maxDistance = 25;
        Vector3 rayOrigin = gameObject.transform.position;
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
        Vector3 playerLoc = desiredObject.transform.position;
        if (VRDevice.isPresent)
            playerLoc = desiredObject.transform.TransformPoint(playerLoc);
        Vector3 rayDirection = desiredObject.transform.position - gameObject.transform.position;
        rayDirection.y = 0;
        maxDistance = rayDirection.magnitude;
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

    public void MoveYokai(CharacterController controller, NavMeshAgent agent)
    {
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
                    return valid[iter3];
                }
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
        float turnspeed = 1.0F;
        Vector3 targetDir = playerTransform.position - transform.position;
        float step = turnspeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    public bool SeeObject(GameObject desiredObject, LayerMask levelMask, Vector3 home)
    {
        int maxDistance = 25;
        /*if (VRDevice.isPresent)
        {
            maxDistance = 50;
        }*/
        int maxDistanceSquared = maxDistance * maxDistance;
        Vector3 rayDirection;

        if (desiredObject != null)
            rayDirection = desiredObject.transform.position - transform.position;
        else
            rayDirection = new Vector3(0, 0, 0);

        rayDirection.y = 0;
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        System.Boolean objectCloseToObserver = rayDirection.sqrMagnitude < maxDistanceSquared;
        rayDirection.Normalize();
        observerDirection.Normalize();
        float angleDot = Vector3.Dot(observerDirection, rayDirection);
        System.Boolean objectInFrontOfObserver = angleDot > 0.0;
        System.Boolean noWallfound = false;
        if (objectInFrontOfObserver)
        {
            if (objectCloseToObserver)
            {
                noWallfound = NoWall(desiredObject, levelMask, home);
            }
        }
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

    public MazeNode SetClosest(MazeNode closest, MazeNode home, List<MazeNode> nodes, Rigidbody rb)
    {
        LinkedList<MazeNode> shortestPathNodes = new LinkedList<MazeNode>();
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            bool enemyInWay = false;
            LinkedList<MazeNode> pathNodes = MazeGenerator.GetPath2(home, nodes[iter]);
            foreach (MazeNode n in pathNodes)
            {
                if (GameManager.trapNode(n))
                    trapInWay = true;
                if (n.EnemyPathNode || (n != home && (n.actor == ActorType.Oni || n.actor == ActorType.Okuri_Inu || n.actor == ActorType.Taka_Nyudo)))
                {
                    //print("Home: " + home.Col + " " + home.Row);
                    //print("Target: " + nodes[iter].Col + " " + nodes[iter].Row);
                    //print("Column: " + n.Col + " Row: " + n.Row + " " + "ActorType: " + n.actor);
                    enemyInWay = true;
                }
            }
            if (!trapInWay && !enemyInWay)
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
                    shortestPathNodes = pathNodes;
                }
            }
        }
        if(shortestPathNodes != null)
            foreach(MazeNode n in shortestPathNodes)
                n.EnemyPathNode = true;
        previousPath = shortestPathNodes;
        currentPath = shortestPathNodes;
        return closest;
    }

    public MazeNode UpdateClosest(MazeNode closest, List<MazeNode> nodes, MazeNode currentNode, MazeNode previous, MazeNode previous2, Rigidbody rb)
    {
        foreach (MazeNode node in currentPath)
            node.EnemyPathNode = false;
        LinkedList<MazeNode> shortestPathNodes = new LinkedList<MazeNode>();
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            bool enemyInWay = false;
            LinkedList<MazeNode> pathNodes = MazeGenerator.GetPath2(currentNode, nodes[iter]);
            foreach (MazeNode n in pathNodes)
            {
                if (GameManager.trapNode(n))
                {
                    trapInWay = true;
                    break;
                }
                if ((n.EnemyPathNode && n != currentNode) || (n.actor == ActorType.Oni || n.actor == ActorType.Okuri_Inu || n.actor == ActorType.Taka_Nyudo))
                {
                    enemyInWay = true;
                    //print("Enemy in the way at Node: " + n.Col + " " + n.Row);
                    //print("Target Node: " + nodes[iter].Col + " " + nodes[iter].Row);
                    //print("Curent Node: " + currentNode.Col + " " + currentNode.Row);
                    break;
                }
            }
            if (!trapInWay && !enemyInWay)
            {
                if (nodes[iter] != currentNode && (nodes[iter] != previous || pathNodes.Count >= 3))
                {
                    //print("Previous: " + previous.Col + " " + previous.Row);
                    //print("Check: " + nodes[iter].Col + " " + nodes[iter].Row);
                    //print("Current: " + currentNode.Col + " " + currentNode.Row);
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
                        shortestPathNodes = pathNodes;
                    }
                }
            }
        }
        previousPath = currentPath;
        currentPath = shortestPathNodes;
        if (shortestPathNodes != null)
            foreach (MazeNode n in shortestPathNodes)
                n.EnemyPathNode = true;
        //foreach (MazeNode n in previousPath)
            //print("Previous Path Column: " + n.Col + " Row: " + n.Row + " Occupied: " + n.EnemyPathNode);
        //foreach (MazeNode n in currentPath)
            //print("Current Path Column: " + n.Col + " Row: " + n.Row + " Occupied: " + n.EnemyPathNode);
        return closest;
    }
}
