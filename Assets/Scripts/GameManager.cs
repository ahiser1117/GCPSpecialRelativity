using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

/*

The GameManager acts as a library of global references so any script can access global variables

This is done with the use of a singleton. This is defined in the void Awake() function

The purpose of many of the lists here are for the graph script to access them when rendering the graph
These lists need to be formatted in such a way for the GPU to read the data in a predictable manner

*/


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
    public List<int> objectIcons;
    public List<AxisLabel> axisLabels;
    public Graph graph;
    public BetaGraph betaGraph;
    public GameObject ObjectPanelPrefab;
    public Transform ObjectContentPanel;
    public Camera cam;
    public TMP_InputField timeInputField;
    public Tabbable currentSelected;
    public Transform loadMenuContent;
    public GameObject loadFileButtonPrefab;
    public TMP_InputField saveName;

    [HideInInspector] public float transitionProgress = 1;
    [HideInInspector] public float beta;
    public int highlightedInterval = -1;

    [HideInInspector] public Vector2 oldTime = new Vector3(0,0);
    [HideInInspector] public Vector2 newTime = new Vector3(0,0);

    Vector2 res;

    void Start(){ // Starting off just update the graph and don't transition
        transitionProgress = 1;
        graph.UpdateGraph();
    }

    public void AddObject(){ // Create a new object and update any references to it
        ObjectPanel newPanel = Instantiate(ObjectPanelPrefab, Vector3.zero, Quaternion.identity, ObjectContentPanel).GetComponent<ObjectPanel>();
        objects.Add(newPanel);
        objectsIdx.Add(new Vector2Int(events.Count, events.Count));
        objectColors.Add(newPanel.color);
        objectIcons.Add(0);
        UpdateTabStructure();
        newPanel.Init();
    }

    void Update(){
        if(res.x != Screen.width || res.y != Screen.height){ // Detect if the screen size has changed and update the graph
            foreach(ObjectPanel obj in objects){
                foreach(EventPanel evt in obj.eventPanels){
                    evt.eventPoint.UpdateFromGridDrag();
                }
                obj.GenerateIntervals();
            }
            UpdateGraph();
            
            res = new Vector2(Screen.width, Screen.height);
        }

        if(transitionProgress < 1){ // Detect when switching reference frames has been requested. Linearly interpolate between old events and target events
            if(transitionProgress + Time.deltaTime > 1){
                transitionProgress = 1;
            }
            foreach(ObjectPanel obj in objects){
                for(int i = 0; i < obj.eventPanels.Count; i++){
                    Vector2 lerpEvent = obj.eventPanels[i].oldEvent + transitionProgress * (obj.eventPanels[i].newEvent - obj.eventPanels[i].oldEvent);
                    currentTime = oldTime.y + transitionProgress * (newTime.y - oldTime.y);
                    timeInputField.text = currentTime.ToString("0.0");
                    obj.eventPanels[i].UpdateFromDrag(lerpEvent.x, lerpEvent.y, false);
                    obj.eventPanels[i].EventUpdated(false);
                }
                if(transitionProgress >= 1){
                    obj.GenerateIntervals();
                }
            }
            UpdateGraph(); // Only update the graph once rather than every time we update each event
            transitionProgress += Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift)){ // Control tabbing behavior
            if(currentSelected.backward)
                currentSelected.backward.Select();
        } else if(Input.GetKeyDown(KeyCode.Tab)){
            if(currentSelected.forward)
                currentSelected.forward.Select();
        }
    }

    public void ChangeSelected(Tabbable newSelect){
        currentSelected = newSelect;
    }

    public int CompareEvents(Vector3 evt1, Vector3 evt2){ // A helper function that is used to resort events
        if(evt1.z > evt2.z){ // Look at associated object number first
            return 1;
        } else if(evt1.z < evt2.z){
            return -1;
        } else{
            if(evt1.y > evt2.y){ // Then time coordinate
                return 1;
            } else if(evt1.y < evt2.y){
                return -1;
            } else{
                return 0;
            }
        }
    }

    public void FindObjectsIdx(){ // Find when indices in the events list are associated with what objects
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

    public void UpdateGraph(){ // Update the graph (a little redundant I guess)
        graph.UpdateGraph();
        
    }

    public void AddTime(float step){ // Change the current time by the specified amount
        currentTime += step;
        timeInputField.text = currentTime.ToString("0.0");
        UpdateGraph();
    }

    public void UpdateTimeFromInput(){ // Set the current time to typed in amount
        if(timeInputField.text == ""){
            timeInputField.text = "0";
        }
        currentTime = Convert.ToSingle(timeInputField.text);
        UpdateGraph();
    }

    public void UpdateTabStructure(){ // Calculate the path taken when tabbing through input fields
        for(int i = 0; i < objects.Count-1; i++){
            objects[i].eventPanels[objects[i].eventPanels.Count-1].time.GetComponent<Tabbable>().forward = objects[i+1].Name;
            objects[i+1].Name.GetComponent<Tabbable>().backward = objects[i].eventPanels[objects[i].eventPanels.Count-1].time;
        }
    }

    public void SaveToFile(){ // Save the current scene to a file
        List<string> objNames = new List<string>();
        List<Vector4> objColors = new List<Vector4>();

        foreach(ObjectPanel obj in objects){ // Some reformatting in order to serialize
            objNames.Add(obj.Name.text);
            objColors.Add(obj.color);
        }

        SaveSituation save = new SaveSituation{ // Define the saved state
            objectNames = objNames.ToArray(),
            objectColors = objColors.ToArray(),
            events = events.ToArray(),
            icons = objectIcons.ToArray()
        };

        string json = JsonUtility.ToJson(save); // Generate a JSON describing this state
        Debug.Log(json);

        SaveManager.SaveToFile(json);
    }

    public void LoadFromFile(string pathname){ // Load from a JSON file
        string saveString = SaveManager.LoadFromFile(pathname); // Read the JSON and save to a string

        while(objects.Count > 0){ // Clear the current scene
            objects[0].DeleteObj();
        }

        if(saveString != null){
            SaveSituation save = JsonUtility.FromJson<SaveSituation>(saveString); // Generate the saved state from the string

            for(int i = 0; i < save.objectNames.Length; i++){ // Start by adding empty objects
                AddObject();
            }
            for(int i = 0; i < save.objectNames.Length; i++){ // Update their names, colors, and icons
                objects[i].Name.text = save.objectNames[i];
                objects[i].ChangeColor(save.objectColors[i], false);
                objects[i].ChangeIcon(save.icons[i], false);
            }
            for(int i = 0; i < save.events.Length; i++){ // Give them the correct events
                objects[(int) save.events[i].z].eventPanels[objects[(int) save.events[i].z].eventPanels.Count - 1].UpdateFromDrag(save.events[i].x, save.events[i].y, false);
                objects[(int) save.events[i].z].AddEvent(false);
            }
            foreach(ObjectPanel obj in objects){
                obj.eventPanels[obj.eventPanels.Count - 1].DeleteEvent(); // Each event starts with an event so we remove the extra event at the end
            }
            foreach(ObjectPanel obj in objects){ // Format and generate the objects for the graph
                obj.SortEventPanels();
                obj.GenerateIntervals();
            }

        }
    }

    public void LoadMostRecent(){ // Allow non static reference to static method
        SaveManager.LoadFiles();
    }

    public class SaveSituation{ // Define the format of the saved state
        public string[] objectNames;
        public Vector4[] objectColors;
        public Vector3[] events;
        public int[] icons;
    }
}
