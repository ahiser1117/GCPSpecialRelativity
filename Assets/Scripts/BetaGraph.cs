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
                if(Mathf.Abs(evt.currEvent.y - gm.currentTime) < Mathf.Abs(obj.eventPanels[minIdx].currEvent.y - gm.currentTime)){
                    minIdx = obj.eventPanels.IndexOf(evt);
                }
            }
            float beta = 0;
            if(minIdx < obj.eventPanels.Count-1 && obj.eventPanels[minIdx].currEvent.y - gm.currentTime <= 0){
                if(obj.eventPanels[minIdx+1].currEvent.y - obj.eventPanels[minIdx].currEvent.y == 0){
                    continue;
                }
                beta = (obj.eventPanels[minIdx+1].currEvent.x - obj.eventPanels[minIdx].currEvent.x) / 
                        (obj.eventPanels[minIdx+1].currEvent.y - obj.eventPanels[minIdx].currEvent.y);
            } else if(minIdx > 0 && obj.eventPanels[minIdx].currEvent.y - gm.currentTime >= 0){
                if(obj.eventPanels[minIdx].currEvent.y - obj.eventPanels[minIdx-1].currEvent.y == 0){
                    continue;
                }
                beta = (obj.eventPanels[minIdx].currEvent.x - obj.eventPanels[minIdx-1].currEvent.x) / 
                        (obj.eventPanels[minIdx].currEvent.y - obj.eventPanels[minIdx-1].currEvent.y);
            } else{
                continue;
            }
            if(Mathf.Abs(beta) > 1){
                continue;
            }

            Vector2 intersect = new Vector2(obj.eventPanels[minIdx].currEvent.x + beta * (gm.currentTime - obj.eventPanels[minIdx].currEvent.y), beta);

            Vector3 offsetTransform = gm.cam.ScreenToWorldPoint(new Vector3(150 + intersect.x + GameManager.instance.graph.originOffset.x + Screen.width/2, intersect.y + beta * 120 + 120, 0));
            
            GameObject newIcon = Instantiate(iconPrefab, new Vector3(offsetTransform.x, offsetTransform.y, 0), Quaternion.identity);
            newIcon.GetComponent<SpriteRenderer>().sprite = obj.sprite;
            if(beta >= 0){
                newIcon.transform.rotation = Quaternion.Euler(0, 0, -90);
            } else if(beta < 0){
                newIcon.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            
            newIcon.transform.localScale = (1080f / Screen.height) * 25 * new Vector3(1, Mathf.Sqrt(1 - beta * beta), 1);
            newIcon.GetComponent<SpriteRenderer>().color = obj.color;
            newIcon.GetComponent<Icon>().betaText.text = "β: " + beta.ToString("0.000");
            newIcon.GetComponent<Icon>().xText.text = "X: " + intersect.x.ToString("0.0");
            newIcon.GetComponent<Icon>().nameText.text = obj.Name.text;
            newIcon.GetComponent<Icon>().UpdateHover(beta);
            
            icons.Add(newIcon);

            float properTime = 0;
            

            if(gm.currentTime > 0){
                for(int i = obj.eventPanels.Count - 1; i > 0; i--){
                    Vector2 currentEvent = obj.eventPanels[i].currEvent;
                    Vector2 nextEvent = obj.eventPanels[i-1].currEvent;
                    if(currentEvent.y - gm.currentTime > 0 && nextEvent.y - gm.currentTime < 0){
                        intersect = new Vector2(obj.eventPanels[minIdx].currEvent.x + beta * (gm.currentTime - obj.eventPanels[minIdx].currEvent.y), gm.currentTime);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        if(nextEvent.y > 0)
                            properTime += gamma * ((intersect.y - nextEvent.y) - beta * (intersect.x - nextEvent.x));
                        else
                            properTime += gamma * ((intersect.y) - beta * (intersect.x - (obj.eventPanels[minIdx].currEvent.x - beta * (obj.eventPanels[minIdx].currEvent.y))));
                    } else if(currentEvent.y - gm.currentTime <= 0 && nextEvent.y > 0){
                        beta = (currentEvent.x - nextEvent.x) /
                                (currentEvent.y - nextEvent.y);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        properTime += gamma * ((currentEvent.y - nextEvent.y)
                                                 - beta * (currentEvent.x - nextEvent.x));
                    } else if(currentEvent.y - gm.currentTime <= 0 && currentEvent.y > 0 && nextEvent.y <= 0){
                        beta = (currentEvent.x - nextEvent.x) /
                                (currentEvent.y - nextEvent.y);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        properTime += gamma * ((currentEvent.y)
                                                 - beta * (currentEvent.x - (currentEvent.x - beta * (currentEvent.y))));
                    }
                }
            } else if(gm.currentTime < 0){
                for(int i = 0; i < obj.eventPanels.Count - 1; i++){
                    Vector2 currentEvent = obj.eventPanels[i].currEvent;
                    Vector2 nextEvent = obj.eventPanels[i+1].currEvent;
                    if(currentEvent.y - gm.currentTime < 0 && nextEvent.y - gm.currentTime > 0){
                        intersect = new Vector2(obj.eventPanels[minIdx].currEvent.x + beta * (gm.currentTime - obj.eventPanels[minIdx].currEvent.y), gm.currentTime);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        if(nextEvent.y < 0)
                            properTime += gamma * ((intersect.y - nextEvent.y) - beta * (intersect.x - nextEvent.x));
                        else
                            properTime += gamma * ((intersect.y) - beta * (intersect.x - (obj.eventPanels[minIdx].currEvent.x - beta * (obj.eventPanels[minIdx].currEvent.y))));
                    } else if(currentEvent.y - gm.currentTime >= 0 && nextEvent.y < 0){
                        beta = (currentEvent.x - nextEvent.x) /
                                (currentEvent.y - nextEvent.y);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        properTime += gamma * ((currentEvent.y - nextEvent.y)
                                                 - beta * (currentEvent.x - nextEvent.x));
                    } else if(currentEvent.y - gm.currentTime >= 0 && currentEvent.y < 0 && nextEvent.y >= 0){
                        beta = (currentEvent.x - nextEvent.x) /
                                (currentEvent.y - nextEvent.y);
                        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
                        properTime += gamma * ((currentEvent.y)
                                                 - beta * (currentEvent.x - (currentEvent.x - beta * (currentEvent.y))));
                    }
                }
            }
            newIcon.GetComponent<Icon>().properTimeText.text = properTime.ToString("0.00");
        }
    }


}
