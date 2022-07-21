using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Interval : MonoBehaviour
{
    
    public EventPoint to;
    public EventPoint from;
    public GameObject hoverData;
    public BoxCollider2D intCollider;

    public TMP_Text betaText;
    public TMP_Text delTimeText;
    public TMP_Text nameText;


    public float beta;

    void Start(){
        intCollider = GetComponent<BoxCollider2D>();
    }

    void OnMouseEnter(){
        hoverData.SetActive(true);
        GameManager.instance.highlightedInterval = GameManager.instance.objectsIdx[GameManager.instance.objects.IndexOf(from.eventPanel.objPanel)].x + from.eventPanel.objPanel.eventPanels.IndexOf(from.eventPanel);
        GameManager.instance.UpdateGraph();
    }

    void OnMouseExit(){
        hoverData.SetActive(false);
        GameManager.instance.highlightedInterval = -1;
        GameManager.instance.UpdateGraph();
    }

    public void UpdateCollider(){
        Vector3 delta = (to.transform.position - from.transform.position);
        transform.position = from.transform.position + delta / 2;
        transform.rotation = Quaternion.FromToRotation(Vector3.right, delta);
        hoverData.transform.position = transform.position + Vector3.Cross(delta, Vector3.forward).normalized * 100;
        hoverData.GetComponent<RectTransform>().rotation = Quaternion.identity;
        intCollider.size = new Vector2(delta.magnitude-15, intCollider.size.y);
        beta = delta.x / delta.y;
        betaText.text = beta.ToString("0.000");
        float gamma = 1 / Mathf.Sqrt(1 - beta * beta);
        delTimeText.text = (gamma * (delta.y - beta * delta.x)).ToString("0.0") + "s";
    }


}
