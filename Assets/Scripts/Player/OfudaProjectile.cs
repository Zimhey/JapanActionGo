using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfudaProjectile : MonoBehaviour {

    public float ProjectileSpeed = 10;
    private Rigidbody body;
    public int EventID;

    private float spawnTime;

    // Use this for initialization
    void Start () {
        body = gameObject.GetComponent<Rigidbody>();
        body.velocity = transform.forward * ProjectileSpeed;
        body.AddForce(transform.forward * ProjectileSpeed, ForceMode.Force);
        EventID = GameManager.Instance.UsedItem(ItemType.Ofuda);
        spawnTime = Time.time;
    }

    void Die()
    {
        // do nothing, Ofuda can't die
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null)
        {
            // hit a Yokai
            if (collider.gameObject.tag == "Oni" || collider.gameObject.tag == "Inu" || collider.gameObject.tag == "Taka")
            {
                collider.gameObject.SendMessage("Stun");
                GameManager.Instance.OfudaHit(EventID, collider.gameObject.GetComponentInChildren<Actor>());
                Destroy(gameObject);
            }

            // Player picked up
            if (collider.gameObject.tag == "Player" && Time.time > spawnTime + 0.25f)
            {
                Inventory PlayerInventory = collider.gameObject.GetComponent<Inventory>();
                GameManager.Instance.OfudaHit(EventID, collider.gameObject.GetComponentInChildren<Actor>()); 
                PlayerInventory.Found(ItemType.Ofuda);
                Destroy(gameObject);
            }
        }
    }
}
