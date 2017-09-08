using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingScript : MonoBehaviour {

    private Dictionary<string, KeyCode> buttons = new Dictionary<string, KeyCode>();

    public Text run, jump, chalk, ofuda;

	// Use this for initialization
	void Start () {
        buttons.Add("Run", KeyCode.LeftShift);
        buttons.Add("Jump", KeyCode.Space);
        buttons.Add("Chalk", KeyCode.Mouse0);
        buttons.Add("Throw", KeyCode.Mouse1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
