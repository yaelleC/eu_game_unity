using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.IO;

public class EngageOffline : MonoBehaviour {

    // the url of the log file
    private static string jsonURL = "engage_logs.json";
    // the json (from previous file)
    private static JSONNode engageLogs;

    // Use this for initialization
    void Start () {
        StartCoroutine(LoadEngAGe());
    }
	
	// Update is called once per frame
	void Update () {
	
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
