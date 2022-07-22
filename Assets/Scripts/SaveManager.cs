using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{

    public static string SPACE_TIME_FOLDER;

    void Start(){
        SPACE_TIME_FOLDER = Application.dataPath + "/Saves/";
        if(!Directory.Exists(SPACE_TIME_FOLDER)){
            Directory.CreateDirectory(SPACE_TIME_FOLDER);
        }
    }

    // Create the file and write all contents of current situation
    public static void SaveToFile(string saveString){
        int saveNumber = 1;
        if(!File.Exists(SPACE_TIME_FOLDER + "Untitled" + ".spacetime")){
            File.WriteAllText(SPACE_TIME_FOLDER + "Untitled" + ".spacetime", saveString);
        } else{
            while(File.Exists(SPACE_TIME_FOLDER + "Untitled" + "_(" + saveNumber + ").spacetime")){
                saveNumber++;
            }
            File.WriteAllText(SPACE_TIME_FOLDER + "Untitled" + "_(" + saveNumber + ").spacetime", saveString);
        }

    }


    // Load the files and create an open menu
    public static void LoadFiles(){
        DirectoryInfo directoryInfo = new DirectoryInfo(SPACE_TIME_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles("*.spacetime");
        System.DateTime mostRecent = saveFiles[0].CreationTime;
        int mostRecentIdx = 0;
        foreach(FileInfo fileInfo in saveFiles){
            Debug.Log("Found: " + fileInfo.Name);
            if(mostRecent.CompareTo(fileInfo.CreationTime) < 0){
                mostRecent = fileInfo.CreationTime;
                mostRecentIdx = System.Array.IndexOf(saveFiles, fileInfo);
            }
        }
        if(saveFiles[mostRecentIdx].FullName != null)
            GameManager.instance.LoadFromFile(saveFiles[mostRecentIdx].FullName);
    }

    public static string LoadFromFile(string pathname){
        Debug.Log("Loading: " + pathname);
        string saveString = File.ReadAllText(pathname);
        return saveString;
    }
}
