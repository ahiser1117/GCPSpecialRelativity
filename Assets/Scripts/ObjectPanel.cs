using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class ObjectPanel : MonoBehaviour
{

    public TMP_InputField Name; 
    public Color color;
    public Transform EventContentPanel; // Where Event Panels are in heirarchy
    public GameObject EventPanelPrefab;
    public GameObject EventPointPrefab;
    public GameObject IntervalPrefab;

    [HideInInspector] public GameManager gm;

    public List<EventPanel> eventPanels;
    public List<Interval> intervals;

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
        newEventPoint.GetComponent<SpriteRenderer>().color = color;
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
        foreach(Interval intv in intervals){
            intv.nameText.text = Name.text;
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

    public void GenerateIntervals(){
        // Create implementation
        while(intervals.Count > 0){
            Interval remove = intervals[0];
            intervals.Remove(remove);
            Destroy(remove.gameObject);
        }
        for(int i = 0; eventPanels.Count > 1 && i < eventPanels.Count-1; i++){
            Interval newInterval = Instantiate(IntervalPrefab, Vector3.zero, Quaternion.identity).GetComponent<Interval>();
            intervals.Add(newInterval);
            newInterval.from = eventPanels[i].eventPoint;
            newInterval.to = eventPanels[i+1].eventPoint;
            newInterval.UpdateCollider();
            newInterval.nameText.text = Name.text;
        }
    }

    public void DeleteObj(){ // Delete this object panel
        while(eventPanels.Count > 0){
            eventPanels[0].DeleteEvent(0);
        }
        while(intervals.Count > 0){
            Interval remove = intervals[0];
            intervals.Remove(remove);
            Destroy(remove.gameObject);
        }
        int idx = gm.objects.IndexOf(this);
        gm.objectsIdx.RemoveAt(idx);
        gm.objectColors.RemoveAt(idx);
        for(int i = 0; i < gm.events.Count; i++){
            if(gm.events[i].z > idx)
                gm.events[i] = gm.events[i] - Vector3.forward;
        }
        gm.objects.Remove(this);
        gm.UpdateGraph();
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
        } else if(minIdx > 0 && Convert.ToSingle(eventPanels[minIdx].time.text) - gm.currentTime >= 0){
            beta = (Convert.ToSingle(eventPanels[minIdx].position.text) - Convert.ToSingle(eventPanels[minIdx-1].position.text)) / 
                    (Convert.ToSingle(eventPanels[minIdx].time.text) - Convert.ToSingle(eventPanels[minIdx-1].time.text));
        } else{
            return;
        }
        if(beta > 1){
            Debug.Log("Cannot switch to Superluminal Reference Frame");
            return;
        }
        
        float gamma = 1f / Mathf.Sqrt(1 - beta * beta);

        Vector2 intersect = new Vector2(Convert.ToSingle(eventPanels[minIdx].position.text) + beta * (gm.currentTime - Convert.ToSingle(eventPanels[minIdx].time.text)), gm.currentTime);

        foreach(ObjectPanel obj in gm.objects){
            for(int i = 0; i < obj.eventPanels.Count; i++){
                obj.eventPanels[i].oldEvent = new Vector2(Convert.ToSingle(obj.eventPanels[i].position.text), Convert.ToSingle(obj.eventPanels[i].time.text));
                obj.eventPanels[i].newEvent = new Vector2(gamma * (obj.eventPanels[i].oldEvent.x - intersect.x
                                                            - beta * (obj.eventPanels[i].oldEvent.y - intersect.y)),
                                                         gamma * (obj.eventPanels[i].oldEvent.y - intersect.y 
                                                            - beta * (obj.eventPanels[i].oldEvent.x - intersect.x)));
            }
        }
        gm.oldTime = intersect;
        gm.newTime = Vector2.zero;
        gm.transitionProgress = 0;
    }

    public void ChangeColor(Color newColor){
        color = newColor;
        gm.objectColors[gm.objects.IndexOf(this)] = color;
        foreach(EventPanel evt in eventPanels){
            evt.eventPoint.GetComponent<SpriteRenderer>().color = color;
        }
        gm.UpdateGraph();
    }

    public void MoveFromGridDrag(){
        foreach(EventPanel evt in eventPanels){
            evt.eventPoint.UpdateFromGridDrag();
        }
        foreach(Interval intv in intervals){
            intv.UpdateCollider();
        }
    }

}
