using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadLevel : MonoBehaviour {

    public GameObject player;

	// Use this for initialization
	void Start () {
		
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject == player)
        {
            if (SceneManager.GetActiveScene().name == "Tutorial1")
            {
                SceneManager.LoadScene("Tutorial2");
            }
            else if (SceneManager.GetActiveScene().name == "Tutorial2")
            {
                SceneManager.LoadScene("Tutorial3");
            }
            else if (SceneManager.GetActiveScene().name == "Tutorial3")
            {
                SceneManager.LoadScene("Tutorial4");
            }

        }
    }
}
 
 