using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ObjectPanel : MonoBehaviour
{

    public TMP_InputField Name; 
    public Transform EventContentPanel; // Where Event Panels are in heirarchy
    public GameObject EventPanelPrefab;
    public GameObject EventPointPrefab;
    public GameObject IntervalPrefab;

    [HideInInspector] public GameManager gm;

    public List<EventPanel> eventPanels;

    void Start(){
        eventPanels = new List<EventPanel>();
        gm = GameManager.instance;
    }

    public void AddEvent(){
        EventPanel newEventPanel = Instantiate(EventPanelPrefab, Vector3.zero, Quaternion.identity, EventContentPanel).GetComponent<EventPanel>();
        EventPoint newEventPoint = Instantiate(EventPointPrefab, Vector3.zero, Quaternion.identity).GetComponent<EventPoint>();
        eventPanels.Add(newEventPanel);
        newEventPanel.gm = gm;
        newEventPanel.eventPoint = newEventPoint;
        newEventPanel.gameObject.name = "EventPanel" + eventPanels.Count;
        newEventPoint.eventPanel = newEventPanel;
        newEventPoint.cam = gm.cam;
        newEventPanel.objPanel = this; // Allow reference back to this object
        if(gm.objectsIdx[gm.objects.IndexOf(this)].y+1 >= gm.events.Count)
            gm.events.Add(new Vector3(0, 0, gm.objects.IndexOf(this)));
        else
            gm.events.Insert(gm.objectsIdx[gm.objects.IndexOf(this)].y, new Vector3(0, 0, gm.objects.IndexOf(this)));
        gm.FindObjectsIdx();
        newEventPanel.EventUpdated(); // Update Objects event list
    }

    public void ConfirmName(){
        if(Name.text == ""){
            Name.text = "Object " + (gm.objects.IndexOf(this) + 1);
        }
    }

    public void UpdateObject(bool sort = true){
        if(sort)
            SortEventPanels();
        gm.UpdateGraph(); // Render the graph
    }

    public void SortEventPanels(){
        eventPanels.Sort(CompareEventPanels);
        gm.events.Sort(gm.CompareEvents);
        foreach(EventPanel evt in eventPanels){
            evt.transform.SetSiblingIndex(eventPanels.IndexOf(evt));
            evt.EventUpdated(false);
        }
    }

    int CompareEventPanels(EventPanel evt1, EventPanel evt2){
        if(Convert.ToSingle(evt1.time.text) > Convert.ToSingle(evt2.time.text)){
            return 1;
        } else if(Convert.ToSingle(evt1.time.text) < Convert.ToSingle(evt2.time.text)){
            return -1;
        } else{
            return 0;
        }
    }

    void GenerateIntervals(){
        // Create implementation
    }

    public void DeleteObj(){ // Delete this object panel
        while(eventPanels.Count > 0){
            eventPanels[0].DeleteEvent(0);
        }
        int idx = gm.objects.IndexOf(this);
        gm.objectsIdx.RemoveAt(idx);
        for(int i = 0; i < gm.events.Count; i++){
            if(gm.events[i].z > idx)
                gm.events[i] = gm.events[i] - Vector3.forward;
        }
        gm.objects.Remove(this);
        Destroy(this.gameObject);
    }

    public void SwitchTo(){
        int minIdx = 0;
        foreach(EventPanel evt in eventPanels){
            if(Mathf.Abs(Convert.ToSingle(evt.time.text) - gm.currentTime) < Mathf.Abs(Convert.ToSingle(eventPanels[minIdx].time.text) - gm.currentTime)){
                minIdx = eventPanels.IndexOf(evt);
            }
        }
        float beta = 0;
        if(minIdx < eventPanels.Count-1 && Convert.ToSingle(eventPanels[minIdx].time.text) - gm.currentTime <= 0){
            beta = (Convert.ToSingle(eventPanels[minIdx+1].position.text) - Convert.ToSingle(eventPanels[minIdx].position.text)) / 
                    (Convert.ToSingle(eventPanels[minIdx+1].time.text) - Convert.ToSingle(eventPanels[minIdx].time.text));
        } else if(minIdx > 0 && Convert.ToSingle(eventPanels[minIdx].time.text) - gm.currentTime > 0){
            beta = (Convert.ToSingle(eventPanels[minIdx].position.text) - Convert.ToSingle(eventPanels[minIdx-1].position.text)) / 
                    (Convert.ToSingle(eventPanels[minIdx].time.text) - Convert.ToSingle(eventPanels[minIdx-1].time.text));
        } 
        

        foreach(ObjectPanel obj in gm.objects){
            for(int i = 0; i < obj.eventPanels.Count; i++){
                obj.eventPanels[i].oldEvent = new Vector2(Convert.ToSingle(obj.eventPanels[i].position.text), Convert.ToSingle(obj.eventPanels[i].time.text));
            }
        }
        gm.beta = beta;
        gm.transitionProgress = 0;

    }


}
