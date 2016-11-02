using System;

[Serializable]
public class Player {
    public int idPlayer;
    public int idStudent;
    public string username;
    // only used if no internet connection available
    public string password;
    // store badges for offline connection (JSON string)
    public string badges;
    public int version;
    public string parameters;
}
