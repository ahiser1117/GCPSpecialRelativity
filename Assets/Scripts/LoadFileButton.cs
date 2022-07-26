using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadFileButton : MonoBehaviour
{
    
    public string pathname;

    public void LoadFile(){
        GameManager.instance.LoadFromFile(pathname);
    }

}
