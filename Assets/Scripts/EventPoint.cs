using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPoint : MonoBehaviour
{

    public Vector2 eventPoint;
    public EventPanel eventPanel;

    Vector3 onDragPosition;
    Vector3 mouseOnDrag;
    Vector3 dragOffset;
    public bool isDragged;
    public Camera cam;

    void OnMouseDown(){
        mouseOnDrag = cam.ScreenToWorldPoint(Input.mousePosition);
        onDragPosition = transform.position;
        isDragged = true;
    }

    void OnMouseUp(){
        isDragged = false;
        eventPanel.objPanel.GenerateIntervals();
    }

    void Update()
    {
        if(isDragged){
            dragOffset = cam.ScreenToWorldPoint(Input.mousePosition) - mouseOnDrag;
            transform.position = onDragPosition + dragOffset;
            eventPoint = new Vector2(transform.position.x-150 - GameManager.instance.graph.originOffset.x, transform.position.y - GameManager.instance.graph.originOffset.y);
            eventPanel.UpdateFromDrag(eventPoint.x, eventPoint.y);
        }
        
    }

    public void UpdateFromInput(float pos, float t){
        eventPoint = new Vector2(pos, t);
        transform.position = new Vector3(pos+150 + GameManager.instance.graph.originOffset.x, t + GameManager.instance.graph.originOffset.y, transform.position.z);
    }

    public void UpdateFromGridDrag(){
        transform.position = new Vector3(eventPoint.x+150 + GameManager.instance.graph.originOffset.x, eventPoint.y + GameManager.instance.graph.originOffset.y, transform.position.z);
    }

}
