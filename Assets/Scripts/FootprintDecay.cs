using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintDecay : MonoBehaviour {

    public float Lifetime;

    private float timeAlive;

    private Material mat;

	// Use this for initialization
	void Start () {
        mat = gameObject.GetComponentInChildren<Renderer>().material;
        timeAlive = 0;
    }
	
	// Update is called once per frame
	void Update () {
        timeAlive += Time.deltaTime;

        float complete = timeAlive / Lifetime;
        mat.SetFloat("_Trans", 1 - complete);

        if (timeAlive > Lifetime)
        {
            int key = GameManager.Instance.GetKey(gameObject.transform.position);
            GameManager.Instance.FootprintMap.Remove(key);
            //print("footprint destroyed at " + gameObject.transform.position);
            Destroy(gameObject);
        }
	}

    public void SetLifeTime(float lifeTime)
    {
        Lifetime = lifeTime;
    }

}
