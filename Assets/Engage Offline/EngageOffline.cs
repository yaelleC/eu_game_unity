using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class EngageOffline : MonoBehaviour {

    // use Singleton for this class
    static public EngageOffline E;

    // the url of the log file
    private static string jsonURL = "/engage_logs.json";
    // the json (from previous file)
    private static JSONNode engageLogs;

    // Use this for initialization
    void Start()
    {
        engageLogs = JSON.Parse(LoadResourceTextfile());
        print(engageLogs);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public string LoadResourceTextfile()
    {
        string filePath = jsonURL.Replace(".json", "");

        TextAsset targetFile = Resources.Load<TextAsset>(filePath);

        print("ok");

        return targetFile.text;
    }

    // read the json file for engage logs and load it here
    IEnumerator LoadEngAGe()
    {
        WWW www = new WWW(jsonURL);
        yield return www;
        if (www.error == null)
        {
            engageLogs = JSON.Parse(www.text);
            print(engageLogs);
        }
        else
        {
            Debug.Log("ERROR: " + www.error);
        }
    }
}
