using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interval : MonoBehaviour
{
    
    public EventPoint toPos;
    public EventPoint fromPos;
    public GameObject hoverData;
    public BoxCollider2D intCollider;

    public float beta;

    void Start(){
        intCollider = GetComponent<BoxCollider2D>();
    }

    void OnMouseEnter(){
        hoverData.SetActive(true);
    }

    void OnMouseExit(){
        hoverData.SetActive(false);
    }

    public void UpdateCollider(){
        Vector3 delta = (toPos.transform.position - fromPos.transform.position);
        transform.position = delta / 2;
        transform.rotation = Quaternion.FromToRotation(fromPos.transform.position, toPos.transform.position);
        intCollider.size = new Vector2(intCollider.size.x, delta.magnitude / 2);
    }


}
