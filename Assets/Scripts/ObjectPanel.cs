using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;


/*

The ObjectPanel class contains all the behavior for a space time object.

It includes the objects name, color, spriteIcon, and events.
Global references to all the objects in the scene are stored in the GameManager singleton "instance"

*/


public class ObjectPanel : MonoBehaviour
{

    public TMP_InputField Name; 
    public Color color;
    public Sprite sprite;
    public Transform EventContentPanel; // Where Event Panels are in heirarchy
    public GameObject EventPanelPrefab;
    public GameObject EventPointPrefab;
    public GameObject IntervalPrefab;
    public Image colorSelector;
    public Image iconSelector;

    public List<Sprite> sprites;

    [HideInInspector] public GameManager gm;

    public List<EventPanel> eventPanels;
    public List<Interval> intervals;

    public void Init(){ // Setup this object and initialize some of its variables
        eventPanels = new List<EventPanel>();
        gm = GameManager.instance;
        ConfirmName();
        AddEvent();
    }


    // When creating a new event we need a new eventPanel as well as eventPoint. They need to reference eachother and adopt some global references
    public void AddEvent(bool update = true){
        EventPanel newEventPanel = Instantiate(EventPanelPrefab, Vector3.zero, Quaternion.identity, EventContentPanel).GetComponent<EventPanel>();
        EventPoint newEventPoint = Instantiate(EventPointPrefab, Vector3.zero, Quaternion.identity).GetComponent<EventPoint>();
        eventPanels.Add(newEventPanel);
        newEventPanel.gm = gm;
        newEventPanel.eventPoint = newEventPoint;
        newEventPoint.GetComponent<SpriteRenderer>().color = color;
        newEventPanel.gameObject.name = "EventPanel" + eventPanels.Count; // This is for the name in the hierarchy in the editor
        newEventPoint.eventPanel = newEventPanel;
        newEventPoint.cam = gm.cam;
        newEventPanel.objPanel = this; // Allow reference back to this object

        if(gm.objectsIdx[gm.objects.IndexOf(this)].y+1 >= gm.events.Count) // Is this event being added to the last object?
            gm.events.Add(new Vector3(0, 0, gm.objects.IndexOf(this)));
        else // Or to a previous object and needs to be inserted
            gm.events.Insert(gm.objectsIdx[gm.objects.IndexOf(this)].y, new Vector3(0, 0, gm.objects.IndexOf(this)));

        gm.FindObjectsIdx(); // Recalculate objects events in GameManager references

        if(update) // We dont want to update when adding many events at once (Loading from a file)
            newEventPanel.EventUpdated(); // Update Objects event list
    }

    public void ConfirmName(){ // If the user deletes the entire name give a default
        if(Name.text == ""){
            Name.text = "Object " + (gm.objects.IndexOf(this) + 1);
        }
        UpdateObject(false);
    }

    public void UpdateObject(bool sort = true){ // General update for the object we dont always have to resort the events so its faster to skip that
        if(sort)
            SortEventPanels();
        GenerateIntervals();
        gm.UpdateGraph(); // Render the graph
    }

    public void SortEventPanels(){ // Sorting the panels and dealing with the switched indices
        eventPanels.Sort(CompareEventPanels);
        gm.events.Sort(gm.CompareEvents);
        foreach(EventPanel evt in eventPanels){ // Move the eventPanel UI objects in the actual menu
            evt.transform.SetSiblingIndex(eventPanels.IndexOf(evt));
            evt.EventUpdated(false);
        }
        for(int i = 0; i < eventPanels.Count - 1; i++){ // Redefine the direction that tabbing takes us
            eventPanels[i].time.GetComponent<Tabbable>().forward = eventPanels[i+1].position;
            eventPanels[i+1].position.GetComponent<Tabbable>().backward = eventPanels[i].time;
        }
        eventPanels[0].position.GetComponent<Tabbable>().backward = Name;
        gm.UpdateTabStructure();
    }

    int CompareEventPanels(EventPanel evt1, EventPanel evt2){ // Helper function for sorting. Defines what event comes before another
        if(evt1.currEvent.y > evt2.currEvent.y){
            return 1;
        } else if(evt1.currEvent.y < evt2.currEvent.y){
            return -1;
        } else{
            return 0;
        }
    }

    public void GenerateIntervals(){ // Generate the objects between each event of the object
        while(intervals.Count > 0){ // Start by deleting existing incorrect intervals from previous generation
            Interval remove = intervals[0];
            intervals.Remove(remove);
            Destroy(remove.gameObject);
        }
        for(int i = 0; eventPanels.Count > 1 && i < eventPanels.Count-1; i++){ // Create the intervals and establish their references
            Interval newInterval = Instantiate(IntervalPrefab, Vector3.zero, Quaternion.identity).GetComponent<Interval>();
            intervals.Add(newInterval);
            newInterval.hoverData.GetComponent<Canvas>().worldCamera = gm.cam;
            newInterval.from = eventPanels[i].eventPoint;
            newInterval.to = eventPanels[i+1].eventPoint;
            newInterval.UpdateCollider();
            newInterval.nameText.text = Name.text;
        }
    }

    public void DeleteObj(){ // Delete this object panel
        while(eventPanels.Count > 0){ // Delete all the events on this object
            eventPanels[0].DeleteEvent(0);
        }
        while(intervals.Count > 0){ // Delete all the intervals on this object
            Interval remove = intervals[0];
            intervals.Remove(remove);
            Destroy(remove.gameObject);
        }
        int idx = gm.objects.IndexOf(this); // Delete all global references to this object
        gm.objectsIdx.RemoveAt(idx);
        gm.objectColors.RemoveAt(idx);
        gm.objectIcons.RemoveAt(idx);
        for(int i = 0; i < gm.events.Count; i++){ // Shift all events not pertaining to this object downward
            if(gm.events[i].z > idx)
                gm.events[i] = gm.events[i] - Vector3.forward;
        }
        gm.objects.Remove(this);
        gm.UpdateTabStructure(); // Update behavior now that this object is no longer referenced anywhere
        gm.UpdateGraph();
        Destroy(this.gameObject);
    }

    public void SwitchTo(){ // Switch to this objects reference frame where the current time line intersects with world line
        int minIdx = 0;
        foreach(EventPanel evt in eventPanels){ // Find event closest to intersection
            if(Mathf.Abs(evt.currEvent.y - gm.currentTime) < Mathf.Abs(eventPanels[minIdx].currEvent.y - gm.currentTime)){
                minIdx = eventPanels.IndexOf(evt);
            }
        }
        float beta = 0;
        if(minIdx < eventPanels.Count-1 && eventPanels[minIdx].currEvent.y - gm.currentTime <= 0){ // Define beta value from interval
            beta = (eventPanels[minIdx+1].currEvent.x - eventPanels[minIdx].currEvent.x) / 
                    (eventPanels[minIdx+1].currEvent.y - eventPanels[minIdx].currEvent.y);
        } else if(minIdx > 0 && eventPanels[minIdx].currEvent.y - gm.currentTime >= 0){
            beta = (eventPanels[minIdx].currEvent.x - eventPanels[minIdx-1].currEvent.x) / 
                    (eventPanels[minIdx].currEvent.y - eventPanels[minIdx-1].currEvent.y);
        } else{
            return; // If currentTime doen't fall on an interval don't switch
        }
        if(beta > 1){ // Stop switch to superluminal reference frames
            Debug.Log("Cannot switch to Superluminal Reference Frame");
            return;
        }
        
        float gamma = 1f / Mathf.Sqrt(1 - beta * beta);

        Vector2 intersect = new Vector2(eventPanels[minIdx].currEvent.x + beta * (gm.currentTime - eventPanels[minIdx].currEvent.y), gm.currentTime); // Calculate intersection point

        foreach(ObjectPanel obj in gm.objects){ // Find the points each other event will move to
            for(int i = 0; i < obj.eventPanels.Count; i++){
                obj.eventPanels[i].oldEvent = new Vector2(obj.eventPanels[i].currEvent.x, obj.eventPanels[i].currEvent.y);
                obj.eventPanels[i].newEvent = new Vector2(gamma * (obj.eventPanels[i].oldEvent.x - intersect.x
                                                            - beta * (obj.eventPanels[i].oldEvent.y - intersect.y)),
                                                         gamma * (obj.eventPanels[i].oldEvent.y - intersect.y 
                                                            - beta * (obj.eventPanels[i].oldEvent.x - intersect.x)));
            }
        }
        gm.oldTime = intersect;
        gm.newTime = Vector2.zero;
        gm.transitionProgress = 0; // Signal to GameManager to conduct the transition and interpolate between old points and new
    }

    public void ChangeColor(Color newColor, bool update = true){ // Change the color associated with this object
        color = newColor;
        colorSelector.color = newColor;
        gm.objectColors[gm.objects.IndexOf(this)] = color;
        foreach(EventPanel evt in eventPanels){ // Update the event points on the graph to reflect color
            evt.eventPoint.GetComponent<SpriteRenderer>().color = color;
        }
        if(update) // Don't want to update the graph when loading from a file
            gm.UpdateGraph();
    }

    public void MoveFromGridDrag(){ // When moving the grid update the events and intervals
        foreach(EventPanel evt in eventPanels){
            evt.eventPoint.UpdateFromGridDrag();
        }
        foreach(Interval intv in intervals){
            intv.UpdateCollider();
        }
    }

    public void ChangeSelected(Tabbable newSelect){ // When object name field gets selected update global reference
        gm.ChangeSelected(newSelect);
        newSelect.forward = eventPanels[0].position;
    }

    public void ChangeIcon(int newSprite, bool update = true){ // Change the icon associated with this object
        sprite = sprites[newSprite];
        iconSelector.sprite = sprites[newSprite];
        gm.objectIcons[gm.objects.IndexOf(this)] = newSprite;
        if(update) // Again don't update the graph when loading from a file
            gm.UpdateGraph();
    }

}
