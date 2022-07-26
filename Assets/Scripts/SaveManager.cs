using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class SaveManager : MonoBehaviour
{

    public static string SPACE_TIME_FOLDER;

    public Transform loadMenuContent;

    void Start(){
        SPACE_TIME_FOLDER = Application.dataPath + "/Saves/";
        if(!Directory.Exists(SPACE_TIME_FOLDER)){
            Directory.CreateDirectory(SPACE_TIME_FOLDER);
        }
    }

    // Create the file and write all contents of current situation
    public static void SaveToFile(string saveString){
        int saveNumber = 1;
        if(!File.Exists(SPACE_TIME_FOLDER + GameManager.instance.saveName.text + ".spacetime")){
            File.WriteAllText(SPACE_TIME_FOLDER + GameManager.instance.saveName.text + ".spacetime", saveString);
        } else{
            while(File.Exists(SPACE_TIME_FOLDER + GameManager.instance.saveName.text + "_(" + saveNumber + ").spacetime")){
                saveNumber++;
            }
            File.WriteAllText(SPACE_TIME_FOLDER + GameManager.instance.saveName.text + "_(" + saveNumber + ").spacetime", saveString);
        }

    }


    // Load the files and create an open menu
    public static void LoadFiles(){
        DirectoryInfo directoryInfo = new DirectoryInfo(SPACE_TIME_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles("*.spacetime");
        foreach(Transform child in GameManager.instance.loadMenuContent){
            Destroy(child.gameObject);
        }
        foreach(FileInfo fileInfo in saveFiles){
            LoadFileButton newButton = Instantiate(GameManager.instance.loadFileButtonPrefab, Vector3.zero, Quaternion.identity, GameManager.instance.loadMenuContent).GetComponent<LoadFileButton>();
            newButton.pathname = fileInfo.FullName;
            newButton.GetComponentInChildren<TMP_Text>().text = fileInfo.Name.Substring(0, fileInfo.Name.Length - 10);
        }
    }

    public static string LoadFromFile(string pathname){
        Debug.Log("Loading: " + pathname);
        string saveString = File.ReadAllText(pathname);
        return saveString;
    }
}
