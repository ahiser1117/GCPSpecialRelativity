using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
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
    public BetaGraph betaGraph;
    public GameObject ObjectPanelPrefab;
    public Transform ObjectContentPanel;
    public Camera cam;
    public TMP_InputField timeInputField;

    [HideInInspector] public float transitionProgress = 1;
    [HideInInspector] public float beta;
    public int highlightedInterval = -1;

    [HideInInspector] public Vector2 oldTime = new Vector3(0,0);
    [HideInInspector] public Vector2 newTime = new Vector3(0,0);

    Vector2 res;


    public void AddObject(){
        ObjectPanel newPanel = Instantiate(ObjectPanelPrefab, Vector3.zero, Quaternion.identity, ObjectContentPanel).GetComponent<ObjectPanel>();
        objects.Add(newPanel);
        objectsIdx.Add(new Vector2Int(events.Count, events.Count));
        objectColors.Add(newPanel.color);
        newPanel.Init();
    }

    void Start(){
        transitionProgress = 1;
        graph.UpdateGraph();
    }

    void Update(){
        if(res.x != Screen.width || res.y != Screen.height){
            foreach(ObjectPanel obj in objects){
                foreach(EventPanel evt in obj.eventPanels){
                    evt.eventPoint.UpdateFromGridDrag();
                }
                obj.GenerateIntervals();
            }
            UpdateGraph();
            
            res = new Vector2(Screen.width, Screen.height);
        }

        if(transitionProgress < 1){
            if(transitionProgress + Time.deltaTime > 1){
                transitionProgress = 1;
            }
            foreach(ObjectPanel obj in objects){
                for(int i = 0; i < obj.eventPanels.Count; i++){
                    Vector2 lerpEvent = obj.eventPanels[i].oldEvent + transitionProgress * (obj.eventPanels[i].newEvent - obj.eventPanels[i].oldEvent);
                    currentTime = oldTime.y + transitionProgress * (newTime.y - oldTime.y);
                    timeInputField.text = currentTime.ToString("0.0");
                    obj.eventPanels[i].time.text = (lerpEvent.y).ToString("0.0");
                    obj.eventPanels[i].position.text = (lerpEvent.x).ToString("0.0");
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
            objectsIdx[i] = new Vector2Int(firstIdx, Mathf.Max(firstIdx, j-1));
        }
        
    }

    public void UpdateGraph(){
        graph.UpdateGraph();
    }

    public void AddTime(float step){
        currentTime += step;
        timeInputField.text = currentTime.ToString("0.0");
        UpdateGraph();
    }

    public void UpdateTimeFromInput(){
        if(timeInputField.text == ""){
            timeInputField.text = "0";
        }
        currentTime = Convert.ToSingle(timeInputField.text);
        UpdateGraph();
    }

    public void SaveToFile(){
        List<string> objNames = new List<string>();
        List<Vector4> objColors = new List<Vector4>();

        foreach(ObjectPanel obj in objects){
            objNames.Add(obj.Name.text);
            objColors.Add(obj.color);
        }

        SaveSituation save = new SaveSituation{
            objectNames = objNames.ToArray(),
            objectColors = objColors.ToArray(),
            events = events.ToArray()
        };

        string json = JsonUtility.ToJson(save);
        Debug.Log(json);

        SaveManager.SaveToFile(json);
    }

    public void LoadFromFile(string pathname){
        string saveString = SaveManager.LoadFromFile(pathname);

        while(objects.Count > 0){
            objects[0].DeleteObj();
        }

        if(saveString != null){
            SaveSituation save = JsonUtility.FromJson<SaveSituation>(saveString);

            for(int i = 0; i < save.objectNames.Length; i++){
                AddObject();
            }
            for(int i = 0; i < save.objectNames.Length; i++){
                objects[i].Name.text = save.objectNames[i];
                objects[i].ChangeColor(save.objectColors[i], false);
            }
            for(int i = 0; i < save.events.Length; i++){
                objects[(int) save.events[i].z].eventPanels[objects[(int) save.events[i].z].eventPanels.Count - 1].UpdateFromDrag(save.events[i].x, save.events[i].y, false);
                objects[(int) save.events[i].z].AddEvent(false);
            }
            foreach(ObjectPanel obj in objects){
                Debug.Log("Object:" + objects.IndexOf(obj) + ", " + obj.eventPanels.Count);
                obj.eventPanels[obj.eventPanels.Count - 1].DeleteEvent();
            }
            for(int i = 0; i < save.events.Length; i++){
                
            }
            foreach(ObjectPanel obj in objects){
                obj.SortEventPanels();
                obj.GenerateIntervals();
            }

        }
    }

    public void LoadMostRecent(){
        SaveManager.LoadFiles();
    }

    public class SaveSituation{
        public string[] objectNames;
        public Vector4[] objectColors;
        public Vector3[] events;
    }
}
