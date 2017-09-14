using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingScript : MonoBehaviour {

    public static Dictionary<string, KeyCode> buttons = new Dictionary<string, KeyCode>();
    public static Dictionary<string, KeyCode> controller = new Dictionary<string, KeyCode>();
    public static Dictionary<string, string> vr = new Dictionary<string, string>();

    public static SteamVR_TrackedController controller1 = new SteamVR_TrackedController();
    public static SteamVR_TrackedController controller2 = new SteamVR_TrackedController();

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

        controller.Add("Run", KeyCode.Joystick1Button0);
        controller.Add("Jump", KeyCode.Joystick1Button1);
        controller.Add("Draw", KeyCode.Joystick1Button2);
        controller.Add("Throw", KeyCode.Joystick1Button3);

        c_run.text = controller["Run"].ToString();
        c_jump.text = controller["Jump"].ToString();
        c_chalk.text = controller["Draw"].ToString();
        c_ofuda.text = controller["Throw"].ToString();

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
            if(e.isKey && !currentKey.name.Contains("c_"))
            {
                buttons[currentKey.name] = e.keyCode;
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                currentKey = null;
            }
            else if(e.isKey)
            {
                controller[currentKey.name] = e.keyCode;
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                currentKey = null;
            }
            else if(controller1.triggerPressed)
            {
                vr[currentKey.name] = "Trigger1";
            }
            else if (controller2.triggerPressed)
            {
                vr[currentKey.name] = "Trigger2";
            }
            else if (controller1.padPressed)
            {
                vr[currentKey.name] = "Pad1";
            }
            else if (controller1.padPressed)
            {
                vr[currentKey.name] = "Pad2";
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
            return controller1.triggerPressed;
        else if (vr["Jump"] == "Trigger2")
            return controller2.triggerPressed;
        else if (vr["Jump"] == "Pad1")
            return controller1.padPressed;
        else if (vr["Jump"] == "Pad2")
            return controller2.padPressed;
        else
            return false;
    }

    public static bool RunPressedVR()
    {
        if (vr["Run"] == "Trigger1")
            return controller1.triggerPressed;
        else if (vr["Run"] == "Trigger2")
            return controller2.triggerPressed;
        else if (vr["Run"] == "Pad1")
            return controller1.padPressed;
        else if (vr["Run"] == "Pad2")
            return controller2.padPressed;
        else
            return false;
    }

    public static bool DrawPressedVR()
    {
        if (vr["Draw"] == "Trigger1")
            return controller1.triggerPressed;
        else if (vr["Draw"] == "Trigger2")
            return controller2.triggerPressed;
        else if (vr["Draw"] == "Pad1")
            return controller1.padPressed;
        else if (vr["Draw"] == "Pad2")
            return controller2.padPressed;
        else
            return false;
    }

    public static bool ThrowPressedVR()
    {
        if (vr["Throw"] == "Trigger1")
            return controller1.triggerPressed;
        else if (vr["Throw"] == "Trigger2")
            return controller2.triggerPressed;
        else if (vr["Throw"] == "Pad1")
            return controller1.padPressed;
        else if (vr["Throw"] == "Pad2")
            return controller2.padPressed;
        else
            return false;
    }
}
