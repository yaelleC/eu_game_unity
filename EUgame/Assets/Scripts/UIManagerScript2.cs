using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManagerScript2 : MonoBehaviour {
	
	public static string username;
	public static string password;
	public Text txtWelcome;

	// Use this for initialization
	void Start () 
	{
		print (username + " " + password);
		txtWelcome.text = "Welcome " + username + ", ";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void GoToMenu()
	{
		Application.LoadLevel("MenuScene");
		
		// call loginStudent WS -> idStudent
		
	}
}
