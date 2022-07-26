using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class EventPanel : MonoBehaviour
{
    
    public TMP_InputField position;
    public TMP_InputField time;

    public Vector2 currEvent;

    public Vector2 oldEvent;
    public Vector2 newEvent;

    public ObjectPanel objPanel; 
    public EventPoint eventPoint;

    [HideInInspector] public GameManager gm;

    void Start(){
        gm = GameManager.instance;
    }

    public void EventUpdated(bool updateObj = true){
        gm.events[gm.objectsIdx[gm.objects.IndexOf(objPanel)].x + objPanel.eventPanels.IndexOf(this)] = new Vector3(Convert.ToSingle(position.text), Convert.ToSingle(time.text), gm.objects.IndexOf(objPanel));
        currEvent = new Vector2(Convert.ToSingle(position.text), Convert.ToSingle(time.text));
        eventPoint.UpdateFromInput(currEvent.x, currEvent.y);
        if(updateObj){
            objPanel.UpdateObject();
            objPanel.GenerateIntervals();
        }
            
    }

    public void UpdateFromDrag(float pos, float t, bool update = true){
        currEvent = new Vector2(pos, t);
        position.text = pos.ToString("0.0");
        time.text = t.ToString("0.0");
        gm.events[gm.objectsIdx[gm.objects.IndexOf(objPanel)].x + objPanel.eventPanels.IndexOf(this)] = new Vector3(pos, t, gm.objects.IndexOf(objPanel));
        if(update)
            objPanel.UpdateObject();
    }

    public void DeleteEvent(int minimum = 1){
        if(objPanel.eventPanels.Count == minimum){
            UpdateFromDrag(0,0);
            objPanel.GenerateIntervals();
        }
        if(objPanel.eventPanels.Count > minimum){
            gm.events.RemoveAt(gm.objectsIdx[gm.objects.IndexOf(objPanel)].x + objPanel.eventPanels.IndexOf(this));
            objPanel.eventPanels.Remove(this);
            gm.FindObjectsIdx();
            if(minimum > 0)
                objPanel.UpdateObject();
            Destroy(eventPoint.gameObject);
            Destroy(this.gameObject);
        }
    }

    public void ChangeSelected(Tabbable newSelect){
        gm.ChangeSelected(newSelect);
    }


}
