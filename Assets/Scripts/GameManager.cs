using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    void Awake(){
        if(instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
        }
        instance = this;
    }

    public float currentTime = 0;
    public List<ObjectPanel> objects;
    public List<Vector2Int> objectsIdx;
    public List<Color> objectColors;
    public List<Vector3> events;
    public Graph graph;
    public GameObject ObjectPanelPrefab;
    public Transform ObjectContentPanel;
    public Camera cam;

    [HideInInspector] public float transitionProgress = 1;
    [HideInInspector] public float beta;


    public void AddObject(){
        ObjectPanel newPanel = Instantiate(ObjectPanelPrefab, Vector3.zero, Quaternion.identity, ObjectContentPanel).GetComponent<ObjectPanel>();
        objects.Add(newPanel);
        objectsIdx.Add(new Vector2Int(events.Count, events.Count));
        objectColors.Add(newPanel.color);
        newPanel.gm = this;
        newPanel.ConfirmName();
        
    }

    void Start(){
        transitionProgress = 1;
        graph.UpdateGraph();
    }

    void Update(){
        if(transitionProgress < 1){
            if(transitionProgress + Time.deltaTime > 1){
                transitionProgress = 1;
            }
            foreach(ObjectPanel obj in objects){
                for(int i = 0; i < obj.eventPanels.Count; i++){
                    Vector2 oldEvent = obj.eventPanels[i].oldEvent;
                    float betaLerp = beta * transitionProgress;
                    float gamma = 1f / Mathf.Sqrt(1 - betaLerp * betaLerp);
                    obj.eventPanels[i].time.text = (gamma * (oldEvent.y - betaLerp * oldEvent.x)).ToString("0.0");
                    obj.eventPanels[i].position.text = (gamma * (oldEvent.x - betaLerp * oldEvent.y)).ToString("0.0");
                    //Debug.Log("Event Updated (" + oldEvent.x + ", " + oldEvent.y + ") => (" + obj.eventPanels[i].position.text + ", " + obj.eventPanels[i].time.text + ")");
                    obj.eventPanels[i].EventUpdated(false);
                }
                if(transitionProgress >= 1){
                    obj.GenerateIntervals();
                }
            }
            UpdateGraph();
            transitionProgress += Time.deltaTime;
        }
    }

    public int CompareEvents(Vector3 evt1, Vector3 evt2){
        if(evt1.z > evt2.z){
            return 1;
        } else if(evt1.z < evt2.z){
            return -1;
        } else{
            if(evt1.y > evt2.y){
                return 1;
            } else if(evt1.y < evt2.y){
                return -1;
            } else{
                return 0;
            }
        }
    }

    public void FindObjectsIdx(){
        for(int i = 0; i < objects.Count; i++){
            int  j = 0;
            int firstIdx;
            while(j < events.Count && events[j].z != i){
                j++;
            }
            firstIdx = j;
            while(j < events.Count && events[j].z == i){
                j++;
            }
            objectsIdx[i] = new Vector2Int(firstIdx, j-1);
        }
        
    }

    public void UpdateGraph(){
        graph.UpdateGraph();
    }
}
