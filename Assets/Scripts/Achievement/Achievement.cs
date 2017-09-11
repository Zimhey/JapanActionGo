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
            if (id == 1)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Obligitory" + "\n" + "Description: " + "You knew it was coming" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 2)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Did the thing" + "\n" + "Description: " + "Congratulations" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 3)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": No problem" + "\n" + "Description: " + "Didn't need a handicap" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 4)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Decent" + "\n" + "Description: " + "A respectable start" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 5)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Challenged yourself" + "\n" + "Description: " + "You did well, you did well" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 6)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": You sure this was a wise investment?" + "\n" + "Description: " + "Perhaps something more rewarding is in order?" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 7)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Guess not" + "\n" + "Description: " + "Prove us wrong" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 8)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Seriously, go outside." + "\n" + "Description: " + "Wasted or spent, you've invested too much time here, consider that" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 9)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Gotta go fast" + "\n" + "Description: " + "Had to get the speed running crowd involed somehow" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 10)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": You player dark souls don't you?" + "\n" + "Description: " + "Ok now do it while acomplishing the next achievement" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 11)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": True Coward" + "\n" + "Description: " + "The concept of interaction is that scary huh?" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 12)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Turned the tables" + "\n" + "Description: " + "Are you sure youre playing the right game?" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 13)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Darwin award" + "\n" + "Description: " + "You earned it" + "\n" + "Status: " + "Unlocked";
            }
            if (id == 14)
            {
                achievementRef.GetComponent<UnityEngine.UI.Text>().text =
                    "Achievement " + id + ": Ok boss" + "\n" + "Description: " + "Truly you are the biggest boss" + "\n" + "Status: " + "Unlocked";
            }
            else
            {
            }
            return true;
        }
        return false;
    }

    public GameObject GetRef()
    {
        return achievementRef;
    }
}
