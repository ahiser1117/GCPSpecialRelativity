using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Icon : MonoBehaviour
{

    public GameObject hoverData;
    public GameObject properTimeCanvas;

    public TMP_Text betaText;
    public TMP_Text xText;
    public TMP_Text nameText;

    public TMP_Text properTimeText;


    void OnMouseEnter(){
        hoverData.SetActive(true);
    }

    void OnMouseExit(){
        hoverData.SetActive(false);
    }

    public void UpdateHover(float beta){
        if(beta >= 0){
            hoverData.transform.position = transform.position + new Vector3(120, -50, 0);
            properTimeCanvas.transform.position = transform.position + new Vector3(0, -30, 0);
        } else{
            hoverData.transform.position = transform.position + new Vector3(120, 50, 0);
            properTimeCanvas.transform.position = transform.position + new Vector3(0, 30, 0);
        }
        hoverData.transform.localScale = new Vector3(1 / (transform.localScale.y+0.001f),1 / transform.localScale.x,1 / transform.localScale.z);
        hoverData.GetComponent<RectTransform>().rotation = Quaternion.identity;
        properTimeCanvas.transform.localScale = new Vector3(1 / (transform.localScale.y+0.001f),1 / transform.localScale.x,1 / transform.localScale.z);
        properTimeCanvas.GetComponent<RectTransform>().rotation = Quaternion.identity;
        
    }
}
