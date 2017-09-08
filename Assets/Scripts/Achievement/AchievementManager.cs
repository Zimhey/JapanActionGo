using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour {

    public GameObject achievementObject;
    private int counter = 0;

	// Use this for initialization
	void Start () {
        CreateAchievement(counter);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CreateAchievement(int id)
    {
        GameObject achievement = Instantiate(achievementObject);
        SetAchievementInfo(counter, achievement);
    }

    public void SetAchievementInfo(int id, GameObject achievement)
    {
        achievement.transform.SetParent(GameObject.FindGameObjectWithTag("AchievementList").transform);
    }
}
