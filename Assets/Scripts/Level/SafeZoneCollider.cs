using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneCollider : MonoBehaviour {
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null && collider.gameObject.tag == "Oni")
        {
            collider.gameObject.SendMessage("SafeZoneCollision");
        }
        if (collider.gameObject != null && collider.gameObject.tag == "Inu")
        {
            collider.gameObject.SendMessage("SafeZoneCollision");
        }
        if (collider.gameObject != null && collider.gameObject.tag == "Taka")
        {
            collider.gameObject.SendMessage("SafeZoneCollision");
        }
    }
}
