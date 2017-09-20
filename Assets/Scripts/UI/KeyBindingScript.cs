using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingScript : MonoBehaviour {

    public static Dictionary<string, KeyCode> buttons = new Dictionary<string, KeyCode>();
    public static Dictionary<string, KeyCode> controller = new Dictionary<string, KeyCode>();
    public static Dictionary<string, string> vr = new Dictionary<string, string>();

    public static SteamVR_TrackedController LeftController;
    public static SteamVR_TrackedController RightController;

    public Text run, jump, chalk, ofuda, c_run, c_jump, c_chalk, c_ofuda, vr_run, vr_jump, vr_chalk, vr_ofuda;

    private GameObject currentKey;

	// Use this for initialization
	void Start () {
        buttons.Add("Run", KeyCode.LeftShift);
        buttons.Add("Jump", KeyCode.Space);
        buttons.Add("Draw", KeyCode.Mouse0);
        buttons.Add("Throw", KeyCode.Mouse1);

        run.text = buttons["Run"].ToString();
        jump.text = buttons["Jump"].ToString();
        chalk.text = buttons["Draw"].ToString();
        ofuda.text = buttons["Throw"].ToString();

        controller.Add("C_Run", KeyCode.Joystick1Button0);
        controller.Add("C_Jump", KeyCode.Joystick1Button1);
        controller.Add("C_Draw", KeyCode.Joystick1Button2);
        controller.Add("C_Throw", KeyCode.Joystick1Button3);

        c_run.text = controller["C_Run"].ToString();
        c_jump.text = controller["C_Jump"].ToString();
        c_chalk.text = controller["C_Draw"].ToString();
        c_ofuda.text = controller["C_Throw"].ToString();

        vr.Add("Run", "Trigger1");
        vr.Add("Jump", "Trigger2");
        vr.Add("Draw", "Pad1");
        vr.Add("Throw", "Pad2");

        vr_run.text = vr["Run"];
        vr_jump.text = vr["Jump"];
        vr_chalk.text = vr["Draw"];
        vr_ofuda.text = vr["Throw"];
    }
	
	// Update is called once per frame
	void Update () {

	}

    private void OnGUI()
    {
        if(currentKey != null)
        {
            Event e = Event.current;
            if(e.isKey && !currentKey.name.Contains("C_"))
            {
                if (!e.keyCode.ToString().Contains("JoyStickButton"))
                {
                    buttons[currentKey.name] = e.keyCode;
                    currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                    currentKey = null;
                }
            }
            else if(e.isKey)
            {
                if (e.keyCode.ToString().Contains("Joystick1Button"))
                {
                    controller[currentKey.name] = e.keyCode;
                    currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                    currentKey = null;
                }
            }
            else if(LeftController.triggerPressed)
            {
                vr[currentKey.name] = "Trigger1";
                currentKey.transform.GetChild(0).GetComponent<Text>().text = "Trigger1";
            }
            else if (RightController.triggerPressed)
            {
                vr[currentKey.name] = "Trigger2";
                currentKey.transform.GetChild(0).GetComponent<Text>().text = "Trigger2";
            }
            else if (LeftController.padPressed)
            {
                vr[currentKey.name] = "Pad1";
                currentKey.transform.GetChild(0).GetComponent<Text>().text = "Pad1";
            }
            else if (RightController.padPressed)
            {
                vr[currentKey.name] = "Pad2";
                currentKey.transform.GetChild(0).GetComponent<Text>().text = "Pad2";
            }
        }
    }

    public void ChangeKey(GameObject clicked)
    {
        currentKey = clicked;
    }

    public static bool JumpPressedVR()
    {
        if (vr["Jump"] == "Trigger1")
            return LeftController.triggerPressed;
        else if (vr["Jump"] == "Trigger2")
            return RightController.triggerPressed;
        else if (vr["Jump"] == "Pad1")
            return LeftController.padPressed;
        else if (vr["Jump"] == "Pad2")
            return RightController.padPressed;
        else
            return false;
    }

    public static bool RunPressedVR()
    {
        if (vr["Run"] == "Trigger1")
            return LeftController.triggerPressed;
        else if (vr["Run"] == "Trigger2")
            return RightController.triggerPressed;
        else if (vr["Run"] == "Pad1")
            return LeftController.padPressed;
        else if (vr["Run"] == "Pad2")
            return RightController.padPressed;
        else
            return false;
    }

    public static bool DrawPressedVR()
    {
        if (vr["Draw"] == "Trigger1")
            return LeftController.triggerPressed;
        else if (vr["Draw"] == "Trigger2")
            return RightController.triggerPressed;
        else if (vr["Draw"] == "Pad1")
            return LeftController.padPressed;
        else if (vr["Draw"] == "Pad2")
            return RightController.padPressed;
        else
            return false;
    }

    public static bool ThrowPressedVR()
    {
        if (vr["Throw"] == "Trigger1")
            return LeftController.triggerPressed;
        else if (vr["Throw"] == "Trigger2")
            return RightController.triggerPressed;
        else if (vr["Throw"] == "Pad1")
            return LeftController.padPressed;
        else if (vr["Throw"] == "Pad2")
            return RightController.padPressed;
        else
            return false;
    }
}
