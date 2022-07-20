using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public ObjectPanel objPanel;
    public Image colorSelectorToggle;
    public GameObject colorSelectorMenu;

    public void ChangeColor(){
        colorSelectorToggle.color = GetComponent<Image>().color;
        colorSelectorMenu.SetActive(false);
        objPanel.ChangeColor(GetComponent<Image>().color);
    }
}
