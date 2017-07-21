using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEvent
{
    public bool Clicking { get; private set; }

    public ClickEvent()
    {
        Clicking = false;
    }

    public bool raiseEvent(bool input)
    {
        if (!Clicking && !input)
        { }
        else if (!Clicking && input)
        {
            Clicking = input;
            return true;
        }
        else if (Clicking && input)
        { }
        else if (Clicking && !input)
        { }

        Clicking = input;
        return false;
    }
}

public class HandCursor : MonoBehaviour {

    private HandheldMessage message = null;
    private float yawOffset = 0f;

    private Quaternion attitude;

    private ClickEvent clickEvent;

    private Checkerboard _checkerboard;

	void Start ()
    {
        _checkerboard = GameObject.Find("Checkerboard").GetComponent<Checkerboard>();
        clickEvent = new ClickEvent();
        attitude = Quaternion.identity;
	}
	
	void Update ()
    {
        if (message == null) return;

        if (message.Reset)
        {
            Debug.Log(this.ToString() + ": reset");
            yawOffset += -transform.rotation.eulerAngles.y;
        }

        attitude = message.Attitude;
        transform.localRotation = Quaternion.AngleAxis(yawOffset, Vector3.up) * new Quaternion(-attitude.x, -attitude.z, -attitude.y, attitude.w);


        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(transform.position, transform.forward);

        _checkerboard.IAmPointing(ray, clickEvent.raiseEvent(message.Click));


        if (clickEvent.raiseEvent(message.Click))
        {
            Debug.Log(this.ToString() + ": selection event.");
        }
        else
        {

        }
	}

    internal void Update(HandheldMessage message)
    {
        this.message = message;
    }
}
