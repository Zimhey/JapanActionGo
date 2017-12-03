using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISessionUpdater : MonoBehaviour {

    public UnityEngine.UI.Text SessionText;

    private int sessionID = -1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(sessionID != GameManager.Instance.SessionID)
        {
            sessionID = GameManager.Instance.SessionID;
            SessionText.text = "Session ID: " + sessionID;
        }
	}
}
