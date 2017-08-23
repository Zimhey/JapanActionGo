using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintPlacer : MonoBehaviour {
    public void PlaceFootprints(ArrayList previousLocations, int lessEnough, GameObject footprintPrefab, Rigidbody rb, float distanceToFloor)
    {
        //if any locations exist
        if (gameObject.CompareTag("Player"))
        {
            
            if (previousLocations.Count > 0)
            {
                //get latest
                int lastentry = previousLocations.Count - 1;
                Vector3 lastlocation = (Vector3)previousLocations[lastentry];
                //make sure it is not current location
                if (lastlocation != gameObject.transform.position)
                {
                    //add current location
                    previousLocations.Add(gameObject.transform.position);
                }

                //get oldest
                Vector3 firstlocation = (Vector3)previousLocations[0];
                //check to see if player has gone far enough for footprints to form
                if ((lastlocation - firstlocation).magnitude > lessEnough)
                {
                    //iterate through
                    for (int iter = 0; iter < lastentry; iter++)
                    {
                        //check locations along path
                        Vector3 currentlocation = (Vector3)previousLocations[iter];
                        //if locations are far enough apart make footprint at a given distance
                        if ((currentlocation - firstlocation).magnitude > lessEnough)
                        {
                            //get direction
                            Vector3 norm = (currentlocation - firstlocation);
                            norm.Normalize();
                            //multiply by desired distance to get desired vector and add to first location
                            if (transform.position.y != distanceToFloor)
                            {
                                distanceToFloor = transform.position.y - 0.55F;
                            }
                            Vector3 dest = firstlocation + norm * (float)lessEnough - new Vector3(0, distanceToFloor, 0);
                            //make rotation
                            Quaternion rot = Quaternion.Euler(0, 0, 0);
                            //add to level
                            Instantiate(footprintPrefab, dest, rot);
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
        }
        else if (gameObject.CompareTag("Oni")) {
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
                //check to see if oni has gone far enough for footprints to form
                if ((lastlocation - firstlocation).magnitude > lessEnough)
                {
                    //iterate through
                    for (int iter = 0; iter < lastentry; iter++)
                    {
                        //check locations along path
                        Vector3 currentlocation = (Vector3)previousLocations[iter];
                        //if locations are far enough apart make footprint at a given distance
                        if ((currentlocation - firstlocation).magnitude > lessEnough)
                        {
                            //get direction
                            Vector3 norm = (currentlocation - firstlocation);
                            norm.Normalize();
                            //multiply by desired distance to get desired vector and add to first location
                            Vector3 dest = firstlocation + norm * (float)lessEnough; // - new Vector3(0, 1.5F, 0);
                                                                                     //make rotation
                            Quaternion rot = Quaternion.Euler(0, 0, 0);
                            //add to level
                            Instantiate(footprintPrefab, dest, rot);
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
        }
        else if (gameObject.CompareTag("Inu"))
        {
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
                //check to see if oni has gone far enough for footprints to form
                if ((lastlocation - firstlocation).magnitude > lessEnough)
                {
                    //iterate through
                    for (int iter = 0; iter < lastentry; iter++)
                    {
                        //check locations along path
                        Vector3 currentlocation = (Vector3)previousLocations[iter];
                        //if locations are far enough apart make footprint at a given distance
                        if ((currentlocation - firstlocation).magnitude > lessEnough)
                        {
                            //get direction
                            Vector3 norm = (currentlocation - firstlocation);
                            norm.Normalize();
                            //multiply by desired distance to get desired vector and add to first location
                            Vector3 dest = firstlocation + norm * (float)lessEnough - new Vector3(0, distanceToFloor, 0);
                            //make rotation
                            Quaternion rot = Quaternion.Euler(0, 0, 0);
                            //add to level
                            Instantiate(footprintPrefab, dest, rot);
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
        }
        else if (gameObject.CompareTag("Taka"))
        {
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
                //check to see if oni has gone far enough for footprints to form
                if ((lastlocation - firstlocation).magnitude > lessEnough)
                {
                    //iterate through
                    for (int iter = 0; iter < lastentry; iter++)
                    {
                        //check locations along path
                        Vector3 currentlocation = (Vector3)previousLocations[iter];
                        //if locations are far enough apart make footprint at a given distance
                        if ((currentlocation - firstlocation).magnitude > lessEnough)
                        {
                            //get direction
                            Vector3 norm = (currentlocation - firstlocation);
                            norm.Normalize();
                            //multiply by desired distance to get desired vector and add to first location
                            Vector3 dest = firstlocation + norm * (float)lessEnough - new Vector3(0, distanceToFloor, 0);
                            //make rotation
                            Quaternion rot = Quaternion.Euler(0, 0, 0);
                            //add to level
                            Instantiate(footprintPrefab, dest, rot);
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
        }

    }
}
