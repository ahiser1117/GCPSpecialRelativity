using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSelector : MonoBehaviour
{
    public ObjectPanel objPanel;
    public Image iconSelectorToggle;
    public GameObject iconSelectorMenu;

    public void changeIcon(int spriteNumber){
        iconSelectorMenu.SetActive(false);
        objPanel.ChangeIcon(spriteNumber);
    }

    public void Toggle(){
        iconSelectorMenu.SetActive(!iconSelectorMenu.activeSelf);
    }
}
