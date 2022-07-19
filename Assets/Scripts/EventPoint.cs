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
    }

    void Update()
    {
        if(isDragged){
            dragOffset = cam.ScreenToWorldPoint(Input.mousePosition) - mouseOnDrag;
            transform.position = onDragPosition + dragOffset;
            eventPoint = new Vector2(transform.position.x-150, transform.position.y);
            eventPanel.UpdateFromDrag(eventPoint.x, eventPoint.y);
        }
        
    }

    public void UpdateFromInput(float pos, float t){
        eventPoint = new Vector2(pos, t);
        transform.position = new Vector3(pos+150, t, transform.position.z);
    }

}
