using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour {

    public GameObject AchievementObject;
    public GameObject AchievementList;
    public GameObject VisualAchievement;
    private int counter = 0;

    public Dictionary<int, Achievement> achievements = new Dictionary<int, Achievement>();

	// Use this for initialization
	void Start () {
        for(int count = 0; count < 14; count++)
        {
            CreateAchievement();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.W))
        {
            EarnAchievement(1);
        }
	}

    public void EarnAchievement(int id)
    {
        if (achievements[id].EarnAchievement())
        {
            VisualAchievement = achievements[id].GetRef();
            GameObject visAchievement = Instantiate(VisualAchievement);
            StartCoroutine(HideAchievement(visAchievement));
        }
    }

    public IEnumerator HideAchievement(GameObject achievement)
    {
        yield return new WaitForSeconds(3);
        Destroy(achievement);
    }

    public void CreateAchievement()
    {
        GameObject achievement = Instantiate(AchievementObject);
        SetAchievementInfo(achievement);
    }

    public void SetAchievementInfo(GameObject achievement)
    {
        int id = counter + 1;
        achievement.transform.SetParent(AchievementList.transform);
        achievement.transform.position = AchievementList.transform.position;
        achievement.transform.localPosition = new Vector3(760, -80 - counter * 200, 0);
        achievement.transform.localScale = new Vector3(1, 1, 1);
        if(id == 1)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Obligitory" + "\n" + "Description: " + "You knew it was coming" + "\n" + "Status: " + "Locked";
        }
        if (id == 2)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Did the thing" + "\n" + "Description: " + "Congratulations" + "\n" + "Status: " + "Locked";
        }
        if (id == 3)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": No problem" + "\n" + "Description: " + "Didn't need a handicap" + "\n" + "Status: " + "Locked";
        }
        if (id == 4)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Decent" + "\n" + "Description: " + "A respectable start" + "\n" + "Status: " + "Locked";
        }
        if (id == 5)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Challenged yourself" + "\n" + "Description: " + "You did well, you did well" + "\n" + "Status: " + "Locked";
        }
        if (id == 6)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": You sure this was a wise investment?" + "\n" + "Description: " + "Perhaps something more rewarding is in order?" + "\n" + "Status: " + "Locked";
        }
        if (id == 7)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Guess not" + "\n" + "Description: " + "Prove us wrong" + "\n" + "Status: " + "Locked";
        }
        if (id == 8)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Seriously, go outside." + "\n" + "Description: " + "Wasted or spent, you've invested too much time here, consider that" + "\n" + "Status: " + "Locked";
        }
        if (id == 9)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Gotta go fast" + "\n" + "Description: " + "Had to get the speed running crowd involed somehow" + "\n" + "Status: " + "Locked";
        }
        if (id == 10)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": You player dark souls don't you?" + "\n" + "Description: " + "Ok now do it while acomplishing the next achievement" + "\n" + "Status: " + "Locked";
        }
        if (id == 11)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": True Coward" + "\n" + "Description: " + "The concept of interaction is that scary huh?" + "\n" + "Status: " + "Locked";
        }
        if (id == 12)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Turned the tables" + "\n" + "Description: " + "Are you sure youre playing the right game?" + "\n" + "Status: " + "Locked";
        }
        if (id == 13)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Darwin award" + "\n" + "Description: " + "You earned it" + "\n" + "Status: " + "Locked";
        }
        if (id == 14)
        {
            achievement.GetComponent<UnityEngine.UI.Text>().text =
                "Achievement " + id + ": Ok boss" + "\n" + "Description: " + "Truly you are the biggest boss" + "\n" + "Status: " + "Locked";
        }
        else
        {
            //print("invalid achievement id");
        }
        achievements.Add(id, new Achievement(id, achievement));
        counter++;
    }
}
