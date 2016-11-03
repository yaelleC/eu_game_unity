﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using SimpleJSON;
using System;

public class EngAGe : MonoBehaviour
{

    // singleton for this class
    static public EngAGe E;

    // the url of the config file
    private static string jsonConfigURL = "configFile.json";
    // the json (from config file)
    private static JSONNode engageConfig_v0;
    
    // list of gameplays + player info to serialise
    private static Logs logs = new Logs();
    // the url of the logs file
    private static string jsonLogsURL = "engageLogs.gd";
    // file with serialised logs
    private static string path;

    // base URL for web services (WS) calls
    private string baseURL = "http://engage.yaellechaudy.com:8080";
    // headers for WS calls
    private Dictionary<string, string> headers = new Dictionary<string, string>();

    // engage data
    private static int idStudent;
    private static int idPlayer = -1;
    private static string username = "";
    private static int version = 0;
    private static int idGameplay;
    private static JSONArray parameters;
    private static JSONArray scores = new JSONArray();
    private static JSONArray feedback = new JSONArray();
    private static JSONArray badgesWon = new JSONArray();
    private static JSONArray badges = new JSONArray();
    private static JSONNode leaderboard = new JSONNode();
    private static JSONNode seriousGame = new JSONNode();

    // web services errors
    private static string error;
    private static int errorCode;

    // instantiate singleton
    void Awake()
    {
        E = this;
    }

    // initialization
    void Start()
    {
        // load config file for local assessment
        LoadConfigFile();
        // initialise data path
        path = Path.Combine(Application.persistentDataPath, jsonLogsURL);

        // uncomment to clean log file
        //SaveLogs();

        // load log info (if any)
        LoadPlayerInfo();

        // add default header for web services calls
        headers.Add("Content-Type", "application/json");
    }

    #region save and load logs
    // ************* save and load logs ****************** //

    public static void SaveLogs()
    {
        print("saving logs");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path);
        bf.Serialize(file, logs);
        file.Close();
    }

    public static void LoadLogs()
    {
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            logs = (Logs)bf.Deserialize(file);
            file.Close();
        }

        print("loading logs : " + logs.SaveToPrettyString());
    }
    // loads config file + save in JSON variable
    public void LoadConfigFile()
    {
        engageConfig_v0 = LoadResourceTextfile(jsonConfigURL);
    }

    // loads a file and returns content as JSONNode
    private JSONNode LoadResourceTextfile(string url)
    {
        string filePath = url.Replace(".json", "");
        TextAsset targetFile = Resources.Load<TextAsset>(filePath);

        return JSON.Parse(targetFile.text);
    }

    #endregion save and load logs

    #region Test connection
    // ************** Test connection ********************** //

    public IEnumerator testConnection(Action<bool, JSONNode> action, JSONNode parameters)
    {
        string URL = baseURL + "/seriousgame/113/version/" + version;
        WWW www = new WWW(URL);

        // wait for the request to finish
        yield return www;

        if (www.error != null)
        {
            action(false, parameters);
        }
        else
        {
            JSONNode game = JSON.Parse(www.text);
            if (game["seriousGame"] != null)
            {
                action(true, parameters);
            }
            else
            {
                action(false, parameters);
            }
        }
    }

    public Boolean internetNotAvailable()
    {
        return (errorCode == 200);
    }

    #endregion Test connection

    #region Log in and out
    // ************* Log in and Log out ****************** //

    public Boolean playerIsKnown()
    {
        if ((EngAGe.E.getIdPlayer() > 0) || (EngAGe.E.getIdStudent() > 0))
        {
            return true;
        }
        return false;
    }

    // try to load the player info, returns true if player logged before, false otherwise
    public Boolean LoadPlayerInfo()
    {
        // load logs and check whether player logged before
        LoadLogs();

        if ((logs != null) && (logs.player != null) && (logs.player.idPlayer > 0))
        {
            idPlayer = logs.player.idPlayer;
            idStudent = logs.player.idStudent;
            username = logs.player.username;
            version = logs.player.version;
            parameters = (JSONArray)JSON.Parse(logs.player.parameters);
            
            return true;
        }
        else if ((logs != null) && (logs.player != null) && (logs.player.idStudent > 0))
        {
            idStudent = logs.player.idStudent;
            username = logs.player.username;
            version = logs.player.version;
            parameters = (JSONArray)JSON.Parse(logs.player.parameters);
            
            return true;
        }
        return false;
    }

    public Boolean QuestionsNeeded()
    {
        foreach (JSONNode param in EngAGe.E.getParameters())
        {
            if (param["value"] == null)
            {
                return true;
            }
        }
        return false;
    }

    // this function is called to log in a player
    // if there is an internet connection the system will check the player log in
    // otherwise the system will save username/password and will proceed anyway
    public void testConnectionAndLoginStudent(int p_idSG, string p_username, string p_password,
                                        string sceneLoginFail, string sceneNoParameters, string sceneParameters)
    {
        string loginDataString = 
                "{" +
                    "\"idSG\": " + p_idSG +
                    ", \"username\": " + p_username +
                    ", \"password\": " + p_password +
                    ", \"sceneLoginFail\": " + sceneLoginFail +
                    ", \"sceneNoParameters\": " + sceneNoParameters +
                    ", \"sceneParameters\": " + sceneParameters +
                    "}";

        JSONNode loginData = JSON.Parse(loginDataString);

        StartCoroutine(testConnection(loginPlayer, loginData));
    }

    private void loginPlayer(bool internetAccess, JSONNode loginData)
    {
        if (internetAccess)
        {
            StartCoroutine(loginStudentOnline(loginData["idSG"].AsInt, loginData["username"], loginData["password"], loginData["sceneLoginFail"], 
                                            loginData["sceneNoParameters"], loginData["sceneParameters"]));
        }
        else
        {
            errorCode = 200;
            error = "No internet connection available";

            // create a temporary offline player
            Player offlinePlayer = new Player();
            offlinePlayer.idPlayer = -1;
            offlinePlayer.idStudent = -1;
            offlinePlayer.username = loginData["username"];
            offlinePlayer.password = loginData["password"];
            // get list of questions and game version from local config file
            // version should be 0, but just in case...
            offlinePlayer.parameters = engageConfig_v0["player"].ToString();
            offlinePlayer.version = engageConfig_v0["seriousGame"]["version"].AsInt;
            
            
            logs.offlinePlayers.Add(offlinePlayer);

            // set current player to the created one
            logs.player = offlinePlayer;

            // save logs
            SaveLogs();

            // set engage variables
            idStudent = logs.player.idStudent;
            username = logs.player.username;
            version = logs.player.version;
            parameters = (JSONArray)JSON.Parse(logs.player.parameters);
            

            Application.LoadLevel(loginData["sceneParameters"]);
        }
    }

    public IEnumerator loginStudentOnline(int p_idSG, string p_username, string p_password,
                                        string sceneLoginFail, string sceneNoParameters, string sceneParameters)
    {

        string URL = baseURL + "/SGaccess";

        string postDataString =
            "{" +
                "\"idSG\": " + p_idSG +
                ", \"username\": \"" + p_username + "\"" +
                ", \"password\": \"" + p_password + "\"" +
                "}";

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(postDataString), headers);

        // wait for the requst to finish
        yield return www;

        JSONNode loginData = JSON.Parse(www.text);

        bool loginSuccess = loginData["loginSuccess"].AsBool;

        if (!loginSuccess)
        {
            errorCode = 201;
            error = "Login failed";
            Application.LoadLevel(sceneLoginFail);
        }
        else if (loginData["version"] != null)
        {
            errorCode = 0;
            error = "";
            if (loginData["idPlayer"] != null)
            {
                idPlayer = loginData["idPlayer"].AsInt;
                version = loginData["version"].AsInt;
                idStudent = loginData["student"]["id"].AsInt;
                parameters = loginData["params"].AsArray;
                username = p_username;

                logs.player.idPlayer = idPlayer;
                logs.player.idStudent = idStudent;
                logs.player.username = username;
                logs.player.version = version;
                logs.player.parameters = parameters.ToString();
                
                SaveLogs();

                Application.LoadLevel(sceneNoParameters);
            }
            else
            {
                version = loginData["version"].AsInt;
                idStudent = loginData["student"]["id"].AsInt;
                parameters = loginData["params"].AsArray;

                logs.player.idStudent = idStudent;
                logs.player.username = username;
                logs.player.idPlayer = -1;
                logs.player.version = version;
                logs.player.parameters = parameters.ToString();

                SaveLogs();

                Application.LoadLevel(sceneParameters);
            }

            // if the version is not 0, save appropriate config file in logs
            if (version != 0)
            {
                StartCoroutine(getGameDescOnline(p_idSG));
            }
        }
        else
        {
            errorCode = 203;
            error = "Sorry, this game is not public and you don't have access to it.";
            Application.LoadLevel(sceneLoginFail);
        }
    }

    public IEnumerator guestLogin(int p_idSG, string sceneLoginFail, string sceneParameters)
    {

        string URL = baseURL + "/SGaccess";

        string postDataString =
            "{" +
                "\"idSG\": " + p_idSG +
                ", \"username\": \" \"" +
                ", \"password\": \" \"" +
                "}";

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(postDataString), headers);

        // wait for the requst to finish
        yield return www;

        JSONNode loginData = JSON.Parse(www.text);
        
        if (loginData["version"] != null)
        {
            errorCode = 0;
            error = "";
            idStudent = 0;
            parameters = loginData["params"].AsArray;
            version = loginData["version"].AsInt;
            Application.LoadLevel(sceneParameters);
        }
        else
        {
            errorCode = 202;
            error = "Sorry this game is not public";
            Application.LoadLevel(sceneLoginFail);
        }
    }

    public void logOut()
    {
        logs.player = new Player();
        SaveLogs();
    }

    public void SaveParameters()
    {
        if (logs.player != null)
        {
            logs.player.parameters = parameters.ToString();
        }
        SaveLogs();
    }

    #endregion Log in and out

    #region outside gameplay (get game, badges, leaderboard)
    // ******************** Outside gameplay ****************** //

    public void testConnectionAndGetGameDesc(int p_idSG)
    {
        string gameDataString =
                "{" +
                    "\"idSG\": " + p_idSG +
                    "}";

        JSONNode gameData = JSON.Parse(gameDataString);

        StartCoroutine(testConnection(getGameDesc, gameData));
    }

    private void getGameDesc(bool internetAccess, JSONNode gameData)
    {
        if (internetAccess)
        {
            StartCoroutine(getGameDescOnline(gameData["idSG"].AsInt));
        }
        else
        {
            errorCode = 200;
            error = "No internet connection available";

            // find offline game desc if any 
            seriousGame = (logs.configFile != null && JSON.Parse(logs.configFile)["seriousGame"] != null) ? JSON.Parse(logs.configFile) : engageConfig_v0;
        }
    }

    public IEnumerator getGameDescOnline(int p_idSG)
    {
        string URL = baseURL + "/seriousgame/" + p_idSG + "/version/" + version;

        WWW www = new WWW(URL);

        // wait for the requst to finish
        yield return www;

        seriousGame = JSON.Parse(www.text);

        logs.configFile = www.text;
        SaveLogs();        
    }

    public void testConnectionAndGetBadges(int p_idSG)
    {
        string badgesDataString =
               "{" +
                   "\"idSG\": " + p_idSG +
                   "}";

        JSONNode badgesData = JSON.Parse(badgesDataString);

        StartCoroutine(testConnection(getBadges, badgesData));
    }

    private void getBadges(bool internetAccess, JSONNode badgesData)
    {
        if (internetAccess)
        {
            StartCoroutine(getBadgesOnline(badgesData["idSG"].AsInt));
        }
        else
        {
            errorCode = 200;
            error = "No internet connection available";

            // find offline game desc if any 
            badges = (logs.player != null && logs.player.badges != null) ? JSON.Parse(logs.player.badges).AsArray : new JSONArray();
        }
    }

    public IEnumerator getBadgesOnline(int p_idSG)
    {
        string URL = baseURL + "/badges/all/seriousgame/" + p_idSG + "/version/" + version + "/player/" + idPlayer;

        WWW www = new WWW(URL);

        // wait for the requst to finish
        yield return www;

        badges = JSON.Parse(www.text).AsArray;

        print("Badges received! " + badges.ToString());

        // store badges for offline connection
        if (logs.player != null)
        {
            logs.player.badges = www.text;
        }
        SaveLogs();
    }

    public void testConnectionAndGetLeaderboard(int p_idSG)
    {
        string leaderboardDataString =
               "{" +
                   "\"idSG\": " + p_idSG +
                   "}";

        JSONNode leaderboardData = JSON.Parse(leaderboardDataString);

        StartCoroutine(testConnection(getLeaderboard, leaderboardData));
    }

    private void getLeaderboard (bool internetAccess, JSONNode leaderboardData)
    {
        if (internetAccess)
        {
            StartCoroutine(getLeaderboardOnline(leaderboardData["idSG"].AsInt));
        }
        else
        {
            errorCode = 200;
            error = "No internet connection available";

            // find offline leaderboard if any 
            leaderboard = (logs.leaderboard != null)? JSON.Parse(logs.leaderboard) : new JSONNode();            
        }
    }

    public IEnumerator getLeaderboardOnline(int p_idSG)
    {
        string URL = baseURL + "/learninganalytics/leaderboard/seriousgame/" + p_idSG + "/version/" + version;

        WWW www = new WWW(URL);

        // wait for the requst to finish
        yield return www;

        leaderboard = JSON.Parse(www.text);

        // save leaderboard to logs (for offline retrieval)
        logs.leaderboard = www.text;
        SaveLogs();
    }

    #endregion outside gameplay (get game, badges, leaderboard)

    #region during gameplay (start, ...)
    // ************* During gameplay ****************** //

    public void testConnectionAndStartGameplay(int p_idSG, string sceneGame)
    {
        string startGPDataString =
               "{" +
                   "\"idSG\": " + p_idSG +
                   ", \"sceneGame\": " + sceneGame +
                   "}";
        
        JSONNode startGPData = JSON.Parse(startGPDataString);

        StartCoroutine(testConnection(startGameplay, startGPData));
    }

    private void startGameplay (bool internetAccess, JSONNode startGPData)
    {
        if (internetAccess)
        {
            StartCoroutine(startGameplayOnline(startGPData["idSG"].AsInt, startGPData["sceneGame"]));
        }
        else
        {
            errorCode = 200;
            error = "No internet connection available";
            
            // create a new offline gameplay
            Gameplay gp = new Gameplay();
            // set its id (convention = for an offline gameplay id its negative position in the gameplays array)
            gp.idGP = (logs.gameplays.Count+1) * -1;
            gp.player = logs.player;

            // get score startings values from config file

            if (logs.player.version != 0 && logs.configFile != null && JSON.Parse(logs.configFile)["learningOutcomes"] != null)
            {
                scores = learningOutcomesToArray(JSON.Parse(logs.configFile)["learningOutcomes"].AsObject);
            }
            else
            {
                scores = learningOutcomesToArray(engageConfig_v0["learningOutcomes"].AsObject);
            }

            gp.scores = scores.ToString();

            logs.gameplays.Add(gp);
            SaveLogs();
            
            // saves idGP for engage.cs
            idGameplay = gp.idGP;

            print("Gameplay Started! id: " + idGameplay);

            Application.LoadLevel(startGPData["sceneGame"]);
        }
    }

    public IEnumerator startGameplayOnline(int p_idSG, string sceneGame)
    {
        scores = new JSONArray();
        feedback = new JSONArray();
        seriousGame = new JSONNode();
        
        string putDataString = "";

        // existing player
        if (idPlayer != -1)
        {
            putDataString =
                "{" +
                    "\"idSG\": " + p_idSG +
                    ", \"version\": " + version +
                    ", \"idPlayer\": " + idPlayer +
                    "}";
        }
        // new player -> create one
        else
        {
            putDataString =
                "{" +
                    "\"idSG\": " + p_idSG +
                    ", \"version\": " + version +
                    ", \"idStudent\": " + idStudent +
                    ", \"params\": " + parameters.ToString() +
                    "}";
        }

        string URL = baseURL + "/gameplay/start";

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(putDataString), headers);

        // wait for the requst to finish
        yield return www;
        
        idGameplay = int.Parse(www.text);

        print("Gameplay Started! id: " + idGameplay);

        string URL2 = baseURL + "/gameplay/" + idGameplay + "/scores/";

        WWW www2 = new WWW(URL2);

        // wait for the requst to finish
        yield return www2;

        scores = JSON.Parse(www2.text).AsArray;

        Application.LoadLevel(sceneGame);
    }

    private JSONArray learningOutcomesToArray(JSONClass scores)
    {
        JSONArray scoresArray = new JSONArray();
        // TODO turn JSON into Array

        foreach (string key in scores.GetKeys())
        {
            // create a json object with all the score info
            JSONClass score = new JSONClass();
            score.Add("id", "-1");
            score.Add("startingValue", scores[key]["value"]);
            score.Add("description", scores[key]["desc"]);
            score.Add("name", key);
            score.Add("value", scores[key]["value"]);
            score.Add("type", "null");

            print(key + " : " + score.ToString());

            // add the object to the list
            scoresArray.Add(score);
        }

        return scoresArray;
    }

    public IEnumerator assess(string p_action, JSONNode p_values, Action<JSONNode> callback)
    {
        print("--- assess action (" + p_action + ") ---");

        string putDataString =
            "{" +
                "\"action\": \"" + p_action + "\"" +
                ", \"values\": " + p_values.ToString() +
                "}";

        string URL = baseURL + "/gameplay/" + idGameplay + "/assessAndScore";

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(putDataString), headers);

        // wait for the requst to finish
        yield return www;

        JSONNode returnAssess = JSON.Parse(www.text);

        feedback = returnAssess["feedback"].AsArray;
        scores = returnAssess["scores"].AsArray;
        print("Action " + putDataString + " assessed! returned: " + returnAssess.ToString());
        foreach (JSONNode f in feedback)
        {
            // log badge
            if (string.Equals(f["type"], "BADGE"))
            {
                badgesWon.Add(f);
            }
        }

        callback(returnAssess);
    }

    public IEnumerator endGameplay(bool win)
    {
        print("--- end Gameplay ---");
        string winString = (win) ? "win" : "lose";
        string URL = baseURL + "/gameplay/" + idGameplay + "/end/" + winString;
        print(URL);

        Dictionary<string, string> headers2 = new Dictionary<string, string>();
        headers2.Add("Content-Type", "text/plain");

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(winString), headers2);

        // wait for the requst to finish
        yield return www;

        if (www.error != null && !www.error.Equals(""))
        {
            print("Error: " + www.error);
        }
        else
        {
            print("Gameplay Ended! return: " + www.text);
        }
    }

    public IEnumerator endGameplay(String win)
    {
        print("--- end Gameplay ---");
        string URL = baseURL + "/gameplay/" + idGameplay + "/end/" + win;
        print(URL);

        WWW www = new WWW(URL, Encoding.UTF8.GetBytes(win));

        // wait for the requst to finish
        yield return www;

        print("Gameplay Ended! return: " + www.text);
    }

    public IEnumerator updateScores(Action<JSONArray> callbackScore)
    {
        print("--- getScores ---");

        string URL = baseURL + "/gameplay/" + idGameplay + "/scores/";

        WWW www = new WWW(URL);

        // wait for the requst to finish
        yield return www;

        scores = JSON.Parse(www.text).AsArray;
        print("Scores received! " + scores.ToString());
        callbackScore(scores);
    }

    public IEnumerator updateFeedback(Action<JSONArray> callbackFeedback)
    {
        print("--- update Feedback ---");

        string URL = baseURL + "/gameplay/" + idGameplay + "/feedback/";

        WWW www = new WWW(URL);

        // wait for the requst to finish
        yield return www;

        feedback = JSON.Parse(www.text).AsArray;
        print("Feedback received! " + feedback.ToString());
        foreach (JSONNode f in feedback)
        {
            // log badge
            if (string.Equals(f["type"], "BADGE"))
            {
                badgesWon.Add(f);
            }
        }

        callbackFeedback(feedback);
    }

    #endregion

    #region get stuff from engage (error, idplayer, username, version...)
    // ************* Get and Set ****************** //

    public int getErrorCode()
    {
        return errorCode;
    }
    public string getError()
    {
        return error;
    }
    public int getIdStudent()
    {
        return (logs.player != null) ? logs.player.idStudent : -1;
    }
    public int getIdPlayer()
    {
        return (logs.player != null) ? logs.player.idPlayer : -1;
    }
    public string getUsername()
    {
        return (logs.player != null) ? logs.player.username : "";
    }
    public int getVersion()
    {
        return (logs.player != null) ? logs.player.version : 0;
    }
    public int getIdGameplay()
    {
        return idGameplay;
    }
    public JSONArray getParameters()
    {
        return parameters;
    }
    public JSONArray getScores()
    {
        return scores;
    }
    public JSONArray getFeedback()
    {
        return feedback;
    }
    public JSONArray getBadgesWon()
    {
        return badgesWon;
    }
    public JSONArray getBadges()
    {
        return badges;
    }
    public JSONNode getLeaderboardList()
    {
        return leaderboard;
    }
    public JSONNode getSG()
    {
        return seriousGame;
    }
    #endregion
}



