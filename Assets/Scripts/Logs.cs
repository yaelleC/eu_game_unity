using System;
using System.Collections.Generic;

[Serializable]
public class Logs
{
    public int idPlayer;
    public int idStudent;
    public string username;
    public int version;
    public string parameters;

    public List<Gameplay> gameplays;
}
