using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BetaGraph : MonoBehaviour
{
    
    public GameObject iconPrefab;

    GameManager gm;

    List<GameObject> icons;

    public void UpdateGraph(){
        if(icons == null){
            icons = new List<GameObject>();
        }
        if(gm == null){
            gm = GameManager.instance;
        }
        while(icons.Count > 0){
            GameObject remove = icons[0];
            icons.RemoveAt(0);
            Destroy(remove);
        }
        foreach(ObjectPanel obj in gm.objects){
            int minIdx = 0;
            foreach(EventPanel evt in obj.eventPanels){
                if(Mathf.Abs(Convert.ToSingle(evt.time.text) - gm.currentTime) < Mathf.Abs(Convert.ToSingle(obj.eventPanels[minIdx].time.text) - gm.currentTime)){
                    minIdx = obj.eventPanels.IndexOf(evt);
                }
            }
            float beta = 0;
            if(minIdx < obj.eventPanels.Count-1 && Convert.ToSingle(obj.eventPanels[minIdx].time.text) - gm.currentTime <= 0){
                if(Convert.ToSingle(obj.eventPanels[minIdx+1].time.text) - Convert.ToSingle(obj.eventPanels[minIdx].time.text) == 0){
                    continue;
                }
                beta = (Convert.ToSingle(obj.eventPanels[minIdx+1].position.text) - Convert.ToSingle(obj.eventPanels[minIdx].position.text)) / 
                        (Convert.ToSingle(obj.eventPanels[minIdx+1].time.text) - Convert.ToSingle(obj.eventPanels[minIdx].time.text));
            } else if(minIdx > 0 && Convert.ToSingle(obj.eventPanels[minIdx].time.text) - gm.currentTime >= 0){
                if(Convert.ToSingle(obj.eventPanels[minIdx].time.text) - Convert.ToSingle(obj.eventPanels[minIdx-1].time.text) == 0){
                    continue;
                }
                beta = (Convert.ToSingle(obj.eventPanels[minIdx].position.text) - Convert.ToSingle(obj.eventPanels[minIdx-1].position.text)) / 
                        (Convert.ToSingle(obj.eventPanels[minIdx].time.text) - Convert.ToSingle(obj.eventPanels[minIdx-1].time.text));
            } else{
                continue;
            }
            if(Mathf.Abs(beta) > 1){
                continue;
            }

            Vector2 intersect = new Vector2(Convert.ToSingle(obj.eventPanels[minIdx].position.text) + beta * (gm.currentTime - Convert.ToSingle(obj.eventPanels[minIdx].time.text)), beta);

            Vector3 offsetTransform = gm.cam.ScreenToWorldPoint(new Vector3(150 + intersect.x + GameManager.instance.graph.originOffset.x + Screen.width/2, intersect.y + beta * 120 + 120, 0));
            
            GameObject newIcon = Instantiate(iconPrefab, new Vector3(offsetTransform.x, offsetTransform.y, 0), Quaternion.identity);
            newIcon.transform.localScale = (1080f / Screen.height) * Vector3.one * 15;
            newIcon.GetComponent<SpriteRenderer>().color = obj.color;
            icons.Add(newIcon);
        }
    }


}
