using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement{

    private int id;
    private bool unlocked;
    private GameObject achievementRef;

    public Achievement(int id, GameObject achievementRef)
    {
        this.id = id;
        this.unlocked = false;
        this.achievementRef = achievementRef;
    }

	// Use this for initialization
	void Start () {
        //Achievement newAchievement = new Achievement(1, );
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool EarnAchievement()
    {
        if (!unlocked)
        {
            unlocked = true;
            return true;
        }
        return false;
    }

    public GameObject GetRef()
    {
        return achievementRef;
    }
}
