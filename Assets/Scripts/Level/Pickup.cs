using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public ItemType item;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject != null && collider.gameObject.tag == "Player")
        {
            collider.gameObject.SendMessage("Found", item);
            Actor a = GetComponent<Actor>();
            if (a != null)
                GameManager.Instance.FoundItem(a);
            else
                Debug.LogWarning("Item found without Actor set");
            Destroy(gameObject);
        }
    }
}
