using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int ChalkCharges;
    public int OfudaCharges;
    public bool hasMirror;
    public bool hasCompass;

    public void Found(ItemType item)
    {
        switch(item)
        {
            case ItemType.Chalk:
                ChalkCharges++;
                break;
            case ItemType.Ofuda:
                OfudaCharges++;
                break;
            case ItemType.Mirror:
                hasMirror = true;
                break;
            case ItemType.Compass:
                hasCompass = true;
                break;
        }
    }

    public void Used(ItemType item)
    {
        switch(item)
        {
            case ItemType.Chalk:
                ChalkCharges--;
                break;
            case ItemType.Ofuda:
                OfudaCharges--;
                break;
        }
    }

    public bool CanUse(ItemType item)
    {
        switch(item)
        {
            case ItemType.Chalk:
                return ChalkCharges > 0;
            case ItemType.Ofuda:
                return OfudaCharges > 0;
            case ItemType.Mirror:
                return hasMirror;
            case ItemType.Compass:
                return hasCompass;
        }
        return false;
    }
}
