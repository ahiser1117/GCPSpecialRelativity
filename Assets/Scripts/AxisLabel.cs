using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisLabel : MonoBehaviour
{
    
    public enum Axis{
        time,
        xMain,
        beta,
        xBeta
    };

    public Axis axis;

    public void UpdatePosition(){
        switch(axis){
            case Axis.time:
                GetComponent<RectTransform>().anchoredPosition = new Vector3(165 + GameManager.instance.graph.originOffset.x, -15);
                break;
            case Axis.xMain:
                GetComponent<RectTransform>().anchoredPosition = new Vector3(-15, -15 + GameManager.instance.graph.originOffset.y);
                break;
            case Axis.beta:
                GetComponent<RectTransform>().anchoredPosition = new Vector3(165 + GameManager.instance.graph.originOffset.x, 225);
                break;
            case Axis.xBeta:
                GetComponent<RectTransform>().anchoredPosition = new Vector3(-15, 105);
                break;
        }
    }


}
