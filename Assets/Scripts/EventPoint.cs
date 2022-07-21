using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPoint : MonoBehaviour
{

    public Vector2 eventPoint;
    public EventPanel eventPanel;
    public Color dimColor;

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

    void OnMouseEnter(){
            GetComponent<SpriteRenderer>().color -= dimColor;
    }

    void OnMouseExit(){
            GetComponent<SpriteRenderer>().color += dimColor;
    }

    void Update()
    {
        if(isDragged){
            dragOffset = cam.ScreenToWorldPoint(Input.mousePosition) - mouseOnDrag;
            transform.position = onDragPosition + dragOffset;
            Vector3 offsetTransform = cam.ScreenToWorldPoint(GameManager.instance.graph.originOffset + new Vector3(Screen.width/2, Screen.height/2, 0));
            eventPoint = new Vector2(transform.position.x - offsetTransform.x, transform.position.y - offsetTransform.y);
            eventPanel.UpdateFromDrag(eventPoint.x, eventPoint.y);
        }
        
    }

    public void UpdateFromInput(float pos, float t){
        eventPoint = new Vector2(pos, t);
        Vector3 offsetTransform = cam.ScreenToWorldPoint(GameManager.instance.graph.originOffset + new Vector3(Screen.width/2, Screen.height/2, 0));
        transform.position = new Vector3(pos + offsetTransform.x + Screen.width, t + offsetTransform.y + Screen.height, transform.position.z);
    }

    public void UpdateFromGridDrag(){
        Vector3 offsetTransform = cam.ScreenToWorldPoint(GameManager.instance.graph.originOffset + new Vector3(Screen.width/2, Screen.height/2, 0));
        transform.position = new Vector3(eventPoint.x + offsetTransform.x, eventPoint.y + offsetTransform.y, transform.position.z);
    }

}
