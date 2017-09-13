using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingScript : MonoBehaviour {

    public static Dictionary<string, KeyCode> buttons = new Dictionary<string, KeyCode>();
    public static Dictionary<string, KeyCode> controller = new Dictionary<string, KeyCode>();
    public static Dictionary<string, KeyCode> vr = new Dictionary<string, KeyCode>();

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

        controller.Add("Run", KeyCode.LeftShift);
        controller.Add("Jump", KeyCode.Space);
        controller.Add("Draw", KeyCode.Mouse0);
        controller.Add("Throw", KeyCode.Mouse1);

        c_run.text = buttons["Run"].ToString();
        c_jump.text = buttons["Jump"].ToString();
        c_chalk.text = buttons["Draw"].ToString();
        c_ofuda.text = buttons["Throw"].ToString();

        vr.Add("Run", KeyCode.LeftShift);
        vr.Add("Jump", KeyCode.Space);
        vr.Add("Draw", KeyCode.Mouse0);
        vr.Add("Throw", KeyCode.Mouse1);

        vr_run.text = buttons["Run"].ToString();
        vr_jump.text = buttons["Jump"].ToString();
        vr_chalk.text = buttons["Draw"].ToString();
        vr_ofuda.text = buttons["Throw"].ToString();
    }
	
	// Update is called once per frame
	void Update () {

	}

    private void OnGUI()
    {
        if(currentKey != null)
        {
            Event e = Event.current;
            if(e.isKey)
            {
                print(buttons[currentKey.name].ToString());
                buttons[currentKey.name] = e.keyCode;
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                print(currentKey.name);
                currentKey = null;
            }
        }
    }

    public void ChangeKey(GameObject clicked)
    {
        currentKey = clicked;
    }
}
