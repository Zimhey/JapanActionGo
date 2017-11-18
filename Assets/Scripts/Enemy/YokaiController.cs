using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR;

//Class to contain shared functionality between A.I.
public class YokaiController : MonoBehaviour {

    //Lists of nodes to track paths of A.I. to find better patrol routes
    private LinkedList<MazeNode> previousPath;
    private LinkedList<MazeNode> currentPath;

    //function to destroy an A.I. when it dies
    public void Die()
    {
        print(gameObject);
        Destroy(gameObject);
    }

    //function that takes in an object to try and see as well as the observer's home location 
    //  in order to determine if there exists an instance of levelMask obstructing vision
    public bool NoWall(GameObject desiredObject,  LayerMask levelMask, Vector3 home)
    {
        if (RCHelper(desiredObject))
        {
            // creating a float to hold the maximun distance the observation ray should cast
            float maxDistance = 25;
            //get the observer's location
            Vector3 rayOrigin = gameObject.transform.position;
            //ensure the y value is correct due to the decreasing y transform issue
            // y value is based on type of yokai
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
            // get the desired object's location
            Vector3 objectLoc = desiredObject.transform.position;
            //if looking for player
            if (desiredObject.CompareTag("Player"))
            {
                //if using VR adjust for different player character
                if (VRDevice.isPresent)
                    objectLoc = desiredObject.transform.TransformPoint(objectLoc);
            }
            //get the vector describing how to get from observer to desired object
            Vector3 rayDirection = desiredObject.transform.position - gameObject.transform.position;
            //ensure y direction does not create errors by extending into the ground
            rayDirection.y = 0;
            //update max distance with the rays magnitude
            maxDistance = rayDirection.magnitude;
            //normalize the vector to ensure it is a directional vector
            rayDirection.Normalize();
            //scale the vector to the desired length
            rayDirection.Scale(new Vector3(maxDistance, 1, maxDistance));
            //create ray to be used in raycasting
            Ray ray = new Ray(rayOrigin, rayDirection);
            //Debug.DrawRay(rayOrigin, rayDirection, Color.green, 5.0F);
            RaycastHit rayHit;

            //check to see if the ray hits any walls, if it did then return false
            if (Physics.Raycast(ray, out rayHit, maxDistance, levelMask))
            {
                return false;
            }
            //no wall found so return true
            return true;
        }
        return false;
    }

    //function to move an A.I. after decisions have been made for that game loop
    public void MoveYokai(CharacterController controller, NavMeshAgent agent)
    {
        //update A.I.'s current position
        agent.nextPosition = transform.position;
        //generate a velocity vector with gravity included
        Vector3 velocity = agent.desiredVelocity + Physics.gravity;
        //update agent's velocity vector
        agent.velocity = velocity;
        //tell the controller to move the A.I.
        controller.Move(velocity * Time.deltaTime * 1);
    }

    //function to determin if an A.I. can see any footprints at a given time
    public GameObject SeeFootprint(LayerMask levelMask, Vector3 home)
    {
        //container object to hold all extent player footprints
        GameObject[] footprints;
        //list to contain footprints that pass the closeness check
        List<GameObject> close = new List<GameObject>();
        //list to contain footprints that pass the in front of check
        List<GameObject> valid = new List<GameObject>();
        //get all extent player footprints and store them
        footprints = GameObject.FindGameObjectsWithTag("Footprint");
        //iterate through all footprints
        for(int iter = 0; iter < footprints.Length; iter++)
        {
            //if footprint exists
            if (footprints[iter] != null)
            {
                //check to see how far away it is from the A.I.
                Vector3 distanceToFootprint = footprints[iter].transform.position - transform.position;
                distanceToFootprint.y = 0;
                float mag = distanceToFootprint.magnitude;
                //if close enough add to the list of close footprints
                if (mag < 25)
                {
                    // if in a line with observer
                    if (RCHelper(footprints[iter]))
                    {
                        close.Add(footprints[iter]);
                    }
                }
            }
        }
        //iterate through list of close footprints
        for (int iter2 = 0; iter2 < close.Count; iter2++)
        {
            if (close[iter2] != null)
            {
                //get the vector between the observer and the footprint
                Vector3 rayDirection = close[iter2].transform.localPosition - transform.localPosition;
                //get the forward vector of the observer
                Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
                //normalize the vectors
                rayDirection.Normalize();
                observerDirection.Normalize();
                //perform dot product to get angle between
                float angleDot = Vector3.Dot(observerDirection, rayDirection);
                //if the footprint is in front of the A.I. add to list of valid footprints
                if (angleDot > 0)
                {
                    valid.Add(close[iter2]);
                }
            }
        }
        //iterate through the list of valid footprints
        for (int iter3 = 0; iter3 < valid.Count; iter3++)
        {
            if (valid[iter3] != null)
            {
                //check to see if there is a wall between the footprint and the A.I.
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask, home);
                if (noWallfound)
                {
                    //return the first valid footprint that is seen
                    return valid[iter3];
                }
            }
        }
        //if no footprints found return null
        return null;
    }

    //function to check if there is an Inu for the A.I. to flee from
    public bool FleeInu(LayerMask levelMask, Vector3 home)
    {
        //container for all inus
        GameObject[] inus;
        //list for inus that pass close check
        List<GameObject> close = new List<GameObject>();
        //list for inus that pass in front of check
        List<GameObject> valid = new List<GameObject>();
        //get all extent inus
        inus = GameObject.FindGameObjectsWithTag("Inu");
        //iterate through all inus to find which are close enough
        for (int iter = 0; iter < inus.Length; iter++)
        {
            if (inus[iter] != null)
            {
                Vector3 distanceToInu = inus[iter].transform.position - transform.position;
                float mag = distanceToInu.magnitude;
                if (mag < 15)
                {
                    if (RCHelper(inus[iter]))
                    {
                        close.Add(inus[iter]);
                    }
                }
            }
        }
        //iterate through all inus to find which are in front of the A.I.
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
        //iterate through all inus to find one which can be seen
        for (int iter3 = 0; iter3 < valid.Count; iter3++)
        {
            if (valid[iter3] != null)
            {
                System.Boolean noWallfound = NoWall(valid[iter3], levelMask, home);
                if (noWallfound)
                {
                    //return true indicating the A.I. should flee
                    return true;
                }
            }
        }
        //return false indicating that the A.I. doesn't need to consider inus for this game loop
        return false;
    }

    //function to perform the slowly turn towards player Aesthetic
    public void TurnTowardsPlayer(GameObject playerObject)
    {
        //get the player's transform
        Transform playerTransform = playerObject.transform;
        //decide speed at which the A.I. will turn
        float turnspeed = 1.0F;
        //get the vector of observer to player
        Vector3 targetDir = playerTransform.position - transform.position;
        //get the actual turn speed based on time and given turn speed
        float step = turnspeed * Time.deltaTime;
        //have the A.I. turn towards the player
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    //function to check if an A.I. can see a desired object taking into account the level and the A.I.'s home location
    public bool SeeObject(GameObject desiredObject, LayerMask levelMask, Vector3 home)
    {
        //determin see distance
        int maxDistance = 25;
        int maxDistanceSquared = maxDistance * maxDistance;
        //get the vector between the desired object and the observer
        Vector3 rayDirection;
        if (desiredObject != null)
            rayDirection = desiredObject.transform.position - transform.position;
        else
            rayDirection = new Vector3(0, 0, 0);
        //ensure no errors due to y
        rayDirection.y = 0;
        //get observer's forward direction
        Vector3 observerDirection = transform.TransformDirection(Vector3.forward);
        //determine if the desired object is close to the observer
        System.Boolean objectCloseToObserver = rayDirection.sqrMagnitude < maxDistanceSquared;
        //normalize the vectors
        rayDirection.Normalize();
        observerDirection.Normalize();
        //determine if the desired object is front of the observer
        float angleDot = Vector3.Dot(observerDirection, rayDirection);
        System.Boolean objectInFrontOfObserver = angleDot > 0.0;
        System.Boolean noWallfound = false;
        //if possibily valid raycast to see if there is a wall
        if (objectInFrontOfObserver)
        {
            if (objectCloseToObserver)
            {
                if (RCHelper(desiredObject))
                {
                    noWallfound = NoWall(desiredObject, levelMask, home);
                }
            }
        }
        if (objectInFrontOfObserver)
        {
            //if all are true return true, otherwise indicate the observer cannot see the object
            System.Boolean seenPlayer = objectInFrontOfObserver && objectCloseToObserver && noWallfound;
            return seenPlayer;
        }
        else
        {
            return false;
        }
    }

    //function to set the starting node in a patrol route for the A.I. to follow
    //  closest is either null or the nearest maze node to the A.I., home is the node the A.I. was spawned on, 
    //  nodes is the list of available nodes to patrol, and rb is the A.I.'s rigidbody used to obtain position
    public MazeNode SetClosest(MazeNode closest, MazeNode home, List<MazeNode> nodes, Rigidbody rb)
    {
        //print("here");
        List<MazeNode> notIntersections = new List<MazeNode>();
        //the container for the path 
        LinkedList<MazeNode> shortestPathNodes = new LinkedList<MazeNode>();
        //iterate through all nodes
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            bool enemyInWay = false;
            MazeNode prevCheck = null;
            bool obstacleFound = false;
            //list of nodes constituting a path from the starting node to the current node from iteration
            LinkedList<MazeNode> pathNodes = MazeGenerator.GetPath2(home, nodes[iter]);
            foreach (MazeNode n in pathNodes)
            {
                bool trapInWayTemp = false;
                bool enemyInWayTemp = false;
                //check to see if there is a trap node along the path
                if (GameManager.trapNode(n))
                {
                    trapInWay = true;
                    if (obstacleFound == false)
                    {
                        trapInWayTemp = true;
                        obstacleFound = true;
                    }
                }
                //check to see if there is an enemy or an enemy path along the currently checked path
                if (n.EnemyPathNode || (n != home && (n.actor == ActorType.Oni || n.actor == ActorType.Okuri_Inu || n.actor == ActorType.Taka_Nyudo)))
                {
                    enemyInWay = true;
                    if (obstacleFound == false)
                    {
                        enemyInWayTemp = true;
                        obstacleFound = true;
                    }
                }

                if (trapInWayTemp || enemyInWayTemp)
                    if (prevCheck != null)
                        notIntersections.Add(prevCheck);
                prevCheck = n;
            }
            //print(notIntersections.Count);
            //if no obstructions along path
            if (!trapInWay && !enemyInWay)
            {
                if (closest == null)
                {
                    //update closest if null
                    closest = nodes[iter];
                }
                Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - transform.position;
                float closestMag = closestPosition.magnitude;
                Vector3 iterPosition = new Vector3(nodes[iter].Col * 6 + 8, nodes[iter].Floor * 30, nodes[iter].Row * 6 + 8) - transform.position;
                float iterMag = iterPosition.magnitude;
                //current iteration node is closer than previous closest, update closest and shortest path
                if (iterMag < closestMag)
                {
                    closest = nodes[iter];
                    shortestPathNodes = pathNodes;
                }
            }
        }
        
        if(closest == null)
        {
            foreach(MazeNode n in notIntersections)
            {
                if (closest == null)
                {
                    closest = n;
                }
                Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - transform.position;
                float closestMag = closestPosition.magnitude;
                Vector3 iterPosition = new Vector3(n.Col * 6 + 8, n.Floor * 30, n.Row * 6 + 8) - transform.position;
                float iterMag = iterPosition.magnitude;
                if (iterMag < closestMag)
                {
                    closest = n;
                    shortestPathNodes = MazeGenerator.GetPath2(home, n);
                }
            }
        }
        //if shortest path exists, indicate that an A.I. will be using that path
        if (shortestPathNodes != null)
            foreach(MazeNode n in shortestPathNodes)
                n.EnemyPathNode = true;
        //update previous and current paths to not be null
        previousPath = shortestPathNodes;
        currentPath = shortestPathNodes;
        /*
        if (closest != null)
            print("Closest not null ");
        else
            print("Closest was null ");
        print("Home: " + home.Col + " " + home.Row);
        print("Destination " + closest.Col + " " + closest.Row);
        */
        return closest;
    }

    //function to set the starting node in a patrol route for the A.I. to follow
    //  closest is either null or the nearest maze node to the A.I., home is the node the A.I. was spawned on, 
    //  nodes is the list of available nodes to patrol, and rb is the A.I.'s rigidbody used to obtain position
    //  currentNode is the node being navigated to prior to update, previous and previous2 are the last two nodes navigated to prior to update
    public MazeNode UpdateClosest(MazeNode closest, List<MazeNode> nodes, MazeNode currentNode, MazeNode previous, MazeNode previous2, Rigidbody rb)
    {
        //print("Update" + currentNode.Col + " " + currentNode.Row);
        List<MazeNode> notIntersections = new List<MazeNode>();
        //indicate old path no longer being used
        foreach (MazeNode node in currentPath)
            node.EnemyPathNode = false;
        //get new path now including currentNode, previous, and previous2 as uneligable destinations
        LinkedList<MazeNode> shortestPathNodes = new LinkedList<MazeNode>();
        for (int iter = 0; iter < nodes.Count; iter++)
        {
            bool trapInWay = false;
            bool enemyInWay = false;
            MazeNode prevCheck = null;
            bool obstacleFound = false;
            LinkedList<MazeNode> pathNodes = MazeGenerator.GetPath2(currentNode, nodes[iter]);
            foreach (MazeNode n in pathNodes)
            {
                bool trapInWayTemp = false;
                bool enemyInWayTemp = false;
                if (GameManager.trapNode(n))
                {
                    trapInWay = true;
                    if (obstacleFound == false)
                    {
                        obstacleFound = true;
                        trapInWayTemp = true;
                    }
                }
                if ((n.EnemyPathNode && n != currentNode))// || (n.actor == ActorType.Oni || n.actor == ActorType.Okuri_Inu || n.actor == ActorType.Taka_Nyudo))
                {
                    enemyInWay = true;
                    if (obstacleFound == false)
                    {
                        obstacleFound = true;
                        enemyInWayTemp = true;
                    }
                }
                if (trapInWayTemp || enemyInWayTemp)
                {
                    //print("Current Location: " + currentNode.Col + " " + currentNode.Row);
                    //print("Blocked Destination: " + n.Col + " " + n.Row);
                    if (prevCheck != null)
                        notIntersections.Add(prevCheck);
                }
                prevCheck = n;
            }
            if (!trapInWay && !enemyInWay)
            {
                if (nodes[iter] != currentNode && (nodes[iter] != previous || pathNodes.Count >= 3))
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
        }
        //if(notIntersections.Count != 0)
            //print("Number of options for " + currentNode.Col + " "  + currentNode.Row + " " + notIntersections.Count);
        if (closest == null)
        {
            foreach (MazeNode n in notIntersections)
            {
                if (n != currentNode && (n != previous || MazeGenerator.GetPath2(currentNode, n).Count >= 3))
                {
                    if (closest == null)
                    {
                        closest = n;
                    }
                    Vector3 closestPosition = new Vector3(closest.Col * 6 + 8, closest.Floor * 30, closest.Row * 6 + 8) - transform.position;
                    float closestMag = closestPosition.magnitude;
                    Vector3 iterPosition = new Vector3(n.Col * 6 + 8, n.Floor * 30, n.Row * 6 + 8) - transform.position;
                    float iterMag = iterPosition.magnitude;
                    if (iterMag < closestMag)
                    {
                        closest = n;
                        shortestPathNodes = MazeGenerator.GetPath2(currentNode, n);
                    }
                }
            }
        }
        previousPath = currentPath;
        currentPath = shortestPathNodes;
        if (shortestPathNodes != null)
            foreach (MazeNode n in shortestPathNodes)
                n.EnemyPathNode = true;
        if (closest != null)
        {
            //print("Closest was not null ");
            //print("Current: " + currentNode.Col + " " + currentNode.Row);
            //print("Destination " + closest.Col + " " + closest.Row);
        }
        else
        {
            //print("Closest was null ");
            //print("Current: " + currentNode.Col + " " + currentNode.Row);
        }
        return closest;
    }

    //function to clear paths upon a yokai leaving patrol state
    public void ClearPaths()
    {
        foreach (MazeNode node in currentPath)
            node.EnemyPathNode = false;
    }

    public bool IsStuck(Vector3 newPosition, Vector3 oldPosition, Vector3 oldPosition2)
    {
        Vector3 difference = newPosition - oldPosition;
        difference.y = 0;
        float difMag = difference.magnitude;
        //print("dif1 " + difMag);
        if (difMag < .05)
        {
            Vector3 difference2 = oldPosition - oldPosition2;
            difference2.y = 0;
            float difMag2 = difference2.magnitude;
            //print("dif2 " + difMag2);
            if (difMag < .05)
            {
                return true;
            }
        }
        return false;
    }

    public bool RCHelper(GameObject desiredObject)
    {
        int oColumn = (int)((gameObject.transform.position.x - 8) / 6);
        int oRow = (int)((gameObject.transform.position.z - 8) / 6);
        int dColumn = (int)((desiredObject.transform.position.x - 8) / 6);
        int dRow = (int)((desiredObject.transform.position.z - 8) / 6);

        if (oColumn - dColumn > -2 && oColumn - dColumn < 2)
        {
            return true;
        }
        else if (oRow - dRow > -2 && oRow - dRow < 2)
        {
            return true;
        }
        return false;
    }
}
