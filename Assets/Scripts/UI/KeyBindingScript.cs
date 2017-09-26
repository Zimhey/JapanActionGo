using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class KeyBindingScript : MonoBehaviour {

    public static Dictionary<string, KeyCode> buttons = new Dictionary<string, KeyCode>();
    public static Dictionary<string, KeyCode> controller = new Dictionary<string, KeyCode>();
    public static Dictionary<string, string> vr = new Dictionary<string, string>();

    public static SteamVR_TrackedController LeftController;
    public static SteamVR_TrackedController RightController;

    public Text run, jump, chalk, ofuda, compass, mirror, c_run, c_jump, c_chalk, c_ofuda, c_compass, c_mirror, vr_run, vr_jump, vr_chalk, vr_ofuda, vr_compass, vr_mirror;

    private GameObject currentKey;

	// Use this for initialization
	void Start () {
        buttons.Add("Run", KeyCode.LeftShift);
        buttons.Add("Jump", KeyCode.Space);
        buttons.Add("Draw", KeyCode.Mouse0);
        buttons.Add("Throw", KeyCode.Mouse1);
        buttons.Add("Mirror", KeyCode.Z);
        buttons.Add("Compass", KeyCode.C);

        run.text = buttons["Run"].ToString();
        jump.text = buttons["Jump"].ToString();
        chalk.text = buttons["Draw"].ToString();
        ofuda.text = buttons["Throw"].ToString();
        mirror.text = buttons["Mirror"].ToString();
        compass.text = buttons["Compass"].ToString();

        controller.Add("C_Run", KeyCode.Joystick1Button0);
        controller.Add("C_Jump", KeyCode.Joystick1Button1);
        controller.Add("C_Draw", KeyCode.Joystick1Button2);
        controller.Add("C_Throw", KeyCode.Joystick1Button3);
        controller.Add("C_Mirror", KeyCode.Joystick1Button4);
        controller.Add("C_Compass", KeyCode.Joystick1Button5);

        c_run.text = controller["C_Run"].ToString();
        c_jump.text = controller["C_Jump"].ToString();
        c_chalk.text = controller["C_Draw"].ToString();
        c_ofuda.text = controller["C_Throw"].ToString();
        c_mirror.text = controller["C_Mirror"].ToString();
        c_compass.text = controller["C_Compass"].ToString();

        vr.Add("Run", "Pad1");
        vr.Add("Jump", "Pad2");
        vr.Add("Draw", "Trigger2");
        vr.Add("Throw", "Trigger1");
        vr.Add("Mirror", "None");
        vr.Add("Compass", "None");

        vr_run.text = vr["Run"];
        vr_jump.text = vr["Jump"];
        vr_chalk.text = vr["Draw"];
        vr_ofuda.text = vr["Throw"];
        vr_mirror.text = vr["Mirror"];
        vr_compass.text = vr["Compass"];
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
                if (!e.keyCode.ToString().Contains("JoyStick1Button"))
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
        if (!VRDevice.isPresent)
            return false;
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
        if (!VRDevice.isPresent)
            return false;
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
        if (!VRDevice.isPresent)
            return false;
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
        if (!VRDevice.isPresent)
            return false;
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
