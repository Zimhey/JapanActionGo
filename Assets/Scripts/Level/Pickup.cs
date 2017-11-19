using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    public float SpinSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 currRot = transform.rotation.eulerAngles;
        currRot.y += SpinSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(currRot);
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
