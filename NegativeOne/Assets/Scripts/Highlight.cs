using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour {

    public Material normal;
    public Material highligted;
    public Material selectedMaterial;

    public bool selected = false;

    void Start () {
		
	}
	
	void Update () {
		
	}

    public void setHighlight(bool setValue)
    {
        if (!selected)
        {
            Material m = setValue ? highligted : normal;
            GetComponent<Renderer>().material = m;
        }
    }

    internal void setSelected(bool v)
    {
        selected = v;
        Material m = v ? selectedMaterial : normal;
        GetComponent<Renderer>().material = m;
    }

    internal void setMaterials(Material colorBlindMaterial, Material colorBlindHighlightMaterial)
    {
        normal = colorBlindMaterial;
        highligted = colorBlindHighlightMaterial;
        GetComponent<Renderer>().material = normal;
    }
}
