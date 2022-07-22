using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPoint : MonoBehaviour
{

    public Vector2 eventPoint;
    public EventPanel eventPanel;
    public Color dimColor;

    public Vector3 onDragPosition;
    public Vector3 mouseOnDrag;
    public Vector3 dragOffset;
    public bool isDragged;
    public Camera cam;


    void OnMouseDown(){
        mouseOnDrag = Input.mousePosition;
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

    void Awake(){
        transform.localScale = (1080f / Screen.height) * Vector3.one * 15;
    }

    void Update()
    {
        if(isDragged){
            dragOffset = cam.ScreenToWorldPoint(Input.mousePosition) - cam.ScreenToWorldPoint(mouseOnDrag);
            Vector3 movePoint = onDragPosition + dragOffset;
            Vector3 screenPoint = cam.WorldToScreenPoint(movePoint);
            if(screenPoint.y < 250){
                GetComponent<SpriteRenderer>().enabled = false;
            } else{
                GetComponent<SpriteRenderer>().enabled = true;
            }
            transform.position = new Vector3(movePoint.x, movePoint.y, -1);
            Vector3 offsetTransform = cam.WorldToScreenPoint(transform.position) - GameManager.instance.graph.originOffset - new Vector3(Screen.width/2 + 150, Screen.height/2, 0);
            eventPoint = new Vector2(offsetTransform.x, offsetTransform.y);
            eventPanel.UpdateFromDrag(eventPoint.x, eventPoint.y);
        }
        
    }

    public void UpdateFromInput(float pos, float t){
        eventPoint = new Vector2(pos, t);
        Vector3 screenPoint = new Vector3(Screen.width/2 + 150 + pos + GameManager.instance.graph.originOffset.x, Screen.height/2 + t + GameManager.instance.graph.originOffset.y, 0);
        if(screenPoint.y < 250){
            GetComponent<SpriteRenderer>().enabled = false;
        } else{
            GetComponent<SpriteRenderer>().enabled = true;
        }
        Vector3 offsetTransform = cam.ScreenToWorldPoint(screenPoint);
        transform.position = new Vector3(offsetTransform.x, offsetTransform.y, -1);
    }

    public void UpdateFromGridDrag(){
        transform.localScale = (1080f / Screen.height) * Vector3.one * 15;
        Vector3 screenPoint = new Vector3(Screen.width/2 + 150 + eventPoint.x + GameManager.instance.graph.originOffset.x, Screen.height/2 + eventPoint.y + GameManager.instance.graph.originOffset.y, 0);
        if(screenPoint.y < 250){
            GetComponent<SpriteRenderer>().enabled = false;
        } else{
            GetComponent<SpriteRenderer>().enabled = true;
        }
        Vector3 offsetTransform = cam.ScreenToWorldPoint(screenPoint);
        transform.position = new Vector3(offsetTransform.x, offsetTransform.y, -1);
    }

}
