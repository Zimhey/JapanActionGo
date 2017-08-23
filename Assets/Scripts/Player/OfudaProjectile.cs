using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfudaProjectile : MonoBehaviour {

    public float ProjectileSpeed = 10;
    private Rigidbody body;

    // Use this for initialization
    void Start () {
        body = gameObject.GetComponent<Rigidbody>();
        body.velocity = transform.forward * ProjectileSpeed;
        body.AddForce(transform.forward * ProjectileSpeed, ForceMode.Force);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collider)
    {
        if (collider.gameObject != null && collider.gameObject.tag == "Oni")
        {
            collider.gameObject.SendMessage("Stun");
            Destroy(gameObject);
        }
        if (collider.gameObject != null && collider.gameObject.tag == "Inu")
        {
            collider.gameObject.SendMessage("Stun");
            Destroy(gameObject);
        }
        if (collider.gameObject != null && collider.gameObject.tag == "Taka")
        {
            collider.gameObject.SendMessage("Stun");
            Destroy(gameObject);
        }
    }
}
