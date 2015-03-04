using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;

public class UIManagerScript : MonoBehaviour {

	public Animator startButton;
	public Animator settingsButton;
	public Animator dialog;
	public Animator imgLevel;
	public Animator contentPanel;
	public Animator gearImage;
	
	public Text txtUsername;
	public InputField txtPassword;
	public Text txtLoginParagraph;

	public Slider sdr_level;

	public InputField input;
	public GameObject obj_input;	
	private List<InputField> inputs = new List<InputField>();
	
	public GameObject badgeDialog;
	
	public Text textFeedback;
	public GameObject pnl_messages;
	private List<Text> feedbackMessages = new List<Text>();

	public MouseController mouseC;

	public EngAGe engage;

	private static string username;
	private static string password;
	public static int idStudent;
	public static int idPlayer = -1;
	public static int version;
	public static int idGameplay;
	public static JSONArray parameters;
	public static JSONArray scores = new JSONArray ();
	public static JSONArray feedback = new JSONArray ();
	public static JSONNode seriousGame = new JSONNode();

	private static bool loginSuccess = true;
	private static bool guestLogin = false;

	public static JSONArray badgesWon = new JSONArray();

	public Text txtWelcome;

	public JSONArray getScores()
	{
		return scores;
	}
	public JSONArray getFeedback()
	{
		return feedback;
	}
	public JSONArray getBadges()
	{
		return badgesWon;
	}
	public JSONNode getSeriousgame()
	{
		return seriousGame;
	}

	void Start()
	{

		if (Application.loadedLevelName.Equals("LoginScene"))
		{
			txtLoginParagraph.enabled = (engage.getErrorCode() > 0);
			txtLoginParagraph.text = engage.getError();
		}

		else if (Application.loadedLevelName.Equals("ParametersScene"))
		{
			txtWelcome.text = "Welcome " + username ;
			
			int i = 0;
			foreach (JSONNode param in engage.getParameters())
			{
				InputField inputParam = (InputField)Instantiate(input);
				inputParam.name = "input_" + param["name"];
				inputParam.transform.SetParent(obj_input.transform);
				inputParam.text = "Enter your " + param["name"] + " ("+param["type"]+")";
				
				RectTransform transform = inputParam.transform as RectTransform;   
				transform.anchoredPosition = new Vector2(0, 20 - i*50 );
				
				inputs.Add(inputParam);
				
				i++;
			}
		}

		else if (Application.loadedLevelName.Equals("MenuScene"))
		{
			StartCoroutine(getGameDesc());
			RectTransform transform = contentPanel.gameObject.transform as RectTransform;        
			Vector2 position = transform.anchoredPosition;
			position.y -= transform.rect.height;
			transform.anchoredPosition = position;

			print ("setting badge dialog inactive");
			badgeDialog.SetActive (false);
		}

		else if (Application.loadedLevelName.Equals("StartScene"))
		{
			StartCoroutine(getGameDesc());
		}
	}

	public void GoToMenu()
	{
		foreach (JSONNode param in engage.getParameters())
		{
			foreach (InputField inputField in inputs)
			{
				if (inputField.name == "input_" + param["name"])
				{
					string value = inputField.text;					
					param.Add("value", value);
				}
			}
		}
		Application.LoadLevel("MenuScene");
	}
	
	public void StartGame()
	{
		StartCoroutine (startGameplay());
	}
	
	public void GetStarted()
	{
		
		username = txtUsername.text;
		password = txtPassword.text;

		StartCoroutine(engage.loginStudent(idSG, username, password, "LoginScene", "MenuScene", "ParametersScene"));
	}

	public void GetStartedGuest()
	{
		
		username = "Guest";
		password = "";
		guestLogin = true;

		StartCoroutine(engage.guestLogin(idSG, "LoginScene", "ParametersScene"));
	}

	public void OpenSettings()
	{
		startButton.SetBool("isHidden", true);
		settingsButton.SetBool("isHidden", true);

		dialog.enabled = true;
		dialog.SetBool("isHidden", false);
	}

	public void CloseSettings()
	{
		startButton.SetBool("isHidden", false);
		settingsButton.SetBool("isHidden", false);
		dialog.SetBool("isHidden", true);
	}

	public void SetDifficulty()
	{
		int difficulty = (int)sdr_level.value;
		imgLevel.SetInteger("difficulty", difficulty);
	}

	public void ToggleMenu()
	{
		contentPanel.enabled = true;
		
		bool isHidden = contentPanel.GetBool("isHidden");
		contentPanel.SetBool("isHidden", !isHidden);

		gearImage.enabled = true;
		gearImage.SetBool("isHidden", !isHidden);
	}

	public void OpenBadges()
	{
		badgeDialog.SetActive (!badgeDialog.activeSelf);
	}
	
	public void CloseBadges()
	{
		badgeDialog.SetActive (false);
	}






	// ************* Web services calls ****************** //
	private string baseURL = "http://146.191.107.189:8080";
	private int idSG = 88;
		
	public WebRequest getGETrequest(string url)
	{
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Proxy = WebRequest.DefaultWebProxy;
		webRequest.Method = "GET";
		
		return webRequest;
	}
	
	public WebRequest getPOSTrequest(string url, int postDataLength)
	{		
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Proxy = WebRequest.DefaultWebProxy;
		webRequest.ContentType = "application/json; ";
		webRequest.ContentLength = postDataLength;
		webRequest.Method = "POST";
		
		return webRequest;
	}
	
	public WebRequest getPOSTEmptyrequest(string url)
	{		
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Proxy = WebRequest.DefaultWebProxy;
		webRequest.Method = "POST";
		
		return webRequest;
	}

	public WebRequest getPUTrequest(string url, int putDataLength)
	{		
		WebRequest webRequest = WebRequest.Create(url);
		webRequest.Proxy = WebRequest.DefaultWebProxy;
		webRequest.ContentType = "application/json; ";
		webRequest.ContentLength = putDataLength;
		webRequest.Method = "PUT";
		
		return webRequest;
	}

	public IEnumerator loginStudent(int p_idSG, string p_username, string p_password)
	{
		print ("--- loginStudent ---");
		
		string URL = baseURL + "/SGaccess";

		string postDataString = 
			"{" + 
				"\"idSG\": " + p_idSG + 
				", \"username\": \"" + p_username + "\"" +
				", \"password\": \"" + p_password + "\"" +
			"}";
		print (postDataString);
		UTF8Encoding encoder = new UTF8Encoding();
		byte[] postData = encoder.GetBytes(postDataString);
				
		WebRequest wr = getPOSTrequest (URL, postData.Length);	
		
		WebAsync webAsync = new WebAsync();

		Stream dataStream = wr.GetRequestStream();
		dataStream.Write(postData, 0, postData.Length);
		dataStream.Close();

		StartCoroutine(webAsync.GetResponse(wr));
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		print (tmpMessage);

		JSONNode loginData = JSON.Parse(tmpMessage);

		loginSuccess = loginData["loginSuccess"].AsBool;

		if (!loginSuccess)
		{
			if ( guestLogin )
			{
				print (loginData.ToString());
				if (loginData["version"] != null)
				{
					idStudent = 0;
					parameters = loginData["params"].AsArray;
					version = loginData["version"].AsInt;
					Application.LoadLevel("ParametersScene");
				}
				else
				{
					txtLoginParagraph.text = "Sorry this game is not public";
					Application.LoadLevel("LoginScene");
				}
			}
			else
			{
				Application.LoadLevel("LoginScene");
			}
		}
		else
		{
			if (loginData["idPlayer"] != null)
			{
				idPlayer = loginData["idPlayer"].AsInt;
				version = loginData["version"].AsInt;
				idStudent = loginData["student"]["idStudent"].AsInt;
				parameters = loginData["params"].AsArray;

				Application.LoadLevel("MenuScene");
			}
			else
			{
				version = loginData["version"].AsInt;
				idStudent = loginData["student"]["idStudent"].AsInt;
				parameters = loginData["params"].AsArray;

				Application.LoadLevel("ParametersScene");
			}
		}
		stream.Close();
	}



	public IEnumerator startGameplay()
	{
		scores = new JSONArray ();
		feedback = new JSONArray ();
		seriousGame = new JSONNode();
		
		badgesWon = new JSONArray();

		print ("--- startGameplay ---");

		WebAsync webAsync = new WebAsync();
		string putDataString = "";

		// existing player
		if (idPlayer != -1)
		{		
			putDataString = 
				"{" + 
					"\"idSG\": " + idSG + 
					", \"version\": " + version + 
					", \"idPlayer\": " + idPlayer +
				"}";
		}
		// new player -> create one
		else 
		{
			putDataString = 
				"{" + 
					"\"idSG\": " + idSG + 
					", \"version\": " + version + 
					", \"idStudent\": " + idPlayer +
					", \"params\": " + parameters.ToString() +
				"}";
		}

		UTF8Encoding encoder = new UTF8Encoding();
		byte[] putData = encoder.GetBytes(putDataString);

		string URL = baseURL + "/gameplay/start";

		WebRequest wr = getPUTrequest (URL, putData.Length);

		Stream dataStream = wr.GetRequestStream();
		dataStream.Write(putData, 0, putData.Length);
		dataStream.Close();
		
		StartCoroutine(webAsync.GetResponse(wr));
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		idGameplay = int.Parse(tmpMessage);
		print ("Gameplay Started! id: " + tmpMessage);
		stream.Close();

		print ("--- getScores ---");
		
		string URL2 = baseURL + "/gameplay/" + idGameplay + "/scores/";
		
		WebRequest wr2 = getGETrequest (URL2);	
		
		WebAsync webAsync2 = new WebAsync();
		
		StartCoroutine(webAsync2.GetResponse(wr2));
		
		while(! webAsync2.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream2 = webAsync2.requestState.webResponse.GetResponseStream();
		string tmpMessage2= new StreamReader(stream2).ReadToEnd().ToString();
		scores = JSON.Parse(tmpMessage2).AsArray;
		print ("Scores received! " + scores.ToString());
		stream2.Close();

		Application.LoadLevel("StartScene");
	}

	public IEnumerator assessAndScore(String action, JSONNode values)
	{
		print ("--- assess ---");
		
		WebAsync webAsync = new WebAsync();
		
		string putDataString = 
			"{" + 
				"\"action\": \"" + action + "\"" +
				", \"values\": " + values.ToString() + 
				"}";
		
		UTF8Encoding encoder = new UTF8Encoding();
		byte[] putData = encoder.GetBytes(putDataString);
		
		string URL = baseURL + "/gameplay/" + idGameplay + "/assess";
		
		WebRequest wr = getPUTrequest (URL, putData.Length);
		
		Stream dataStream = wr.GetRequestStream();
		dataStream.Write(putData, 0, putData.Length);
		dataStream.Close();
		
		StartCoroutine(webAsync.GetResponse(wr));
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		feedback = JSON.Parse(tmpMessage).AsArray;
		print ("Action " + putDataString + " assessed! returned: " + feedback.ToString());
		foreach (JSONNode f in feedback)
		{
			// create a new text in panel messages
			Text txtFeedback = (Text)Instantiate(textFeedback);
			txtFeedback.name = "txt_" + f["name"];
			txtFeedback.transform.SetParent(pnl_messages.transform);
			txtFeedback.text = f["message"];
			if (string.Equals( f["type"], "POSITIVE"))
				txtFeedback.color = new Color(0, 1, 0);
			if (string.Equals( f["type"], "NEGATIVE"))
				txtFeedback.color = new Color(1, 0, 0);
			
			RectTransform transform = txtFeedback.transform as RectTransform;   
			transform.anchoredPosition = new Vector2(5, - 10 - feedbackMessages.Count * 30 );
			
			feedbackMessages.Add(txtFeedback);
			
			// log badge
			if (string.Equals(f["type"], "BADGE"))
			{
				badgesWon.Add(f);
			}
			
		}
		stream.Close();
		
		print ("--- getScores ---");
		
		string URL2 = baseURL + "/gameplay/" + idGameplay + "/scores/";
		
		WebRequest wr2 = getGETrequest (URL2);	
		
		WebAsync webAsync2 = new WebAsync();
		
		StartCoroutine(webAsync2.GetResponse(wr2));
		
		while(! webAsync2.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream2 = webAsync2.requestState.webResponse.GetResponseStream();
		string tmpMessage2= new StreamReader(stream2).ReadToEnd().ToString();
		scores = JSON.Parse(tmpMessage2).AsArray;
		print ("Scores received! " + scores.ToString());
		
		mouseC.UpdateScores();
		stream2.Close();
		
		print ("--- getFeedback ---");
		
		string URL3 = baseURL + "/gameplay/" + idGameplay + "/feedback/";
		
		WebRequest wr3 = getGETrequest (URL3);	
		
		WebAsync webAsync3 = new WebAsync();
		
		StartCoroutine(webAsync3.GetResponse(wr3));
		
		while(! webAsync3.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream3 = webAsync3.requestState.webResponse.GetResponseStream();
		string tmpMessage3 = new StreamReader(stream3).ReadToEnd().ToString();
		feedback = JSON.Parse(tmpMessage3).AsArray;
		print ("Feedback received! " + feedback.ToString());
		
		foreach (JSONNode f in feedback)
		{
			// create a new text in panel messages
			Text txtFeedback = (Text)Instantiate(textFeedback);
			txtFeedback.name = "txt_" + f["name"];
			txtFeedback.transform.SetParent(pnl_messages.transform);
			txtFeedback.text = f["message"];
			if (string.Equals( f["type"], "POSITIVE"))
				txtFeedback.color = new Color(0, 1, 0);
			if (string.Equals( f["type"], "NEGATIVE"))
				txtFeedback.color = new Color(1, 0, 0);
			
			RectTransform transform = txtFeedback.transform as RectTransform;   
			transform.anchoredPosition = new Vector2(0, feedbackMessages.Count * 50 );
			
			feedbackMessages.Add(txtFeedback);
			
			// log badge
			if (string.Equals(f["type"], "BADGE"))
			{
				badgesWon.Add(f);
			}
		}
		
		endGame ();
		stream3.Close();
		
		mouseC.UpdateFeedback ();
	}

	public IEnumerator assess(String action, JSONNode values)
	{
		print ("--- assess ---");

		WebAsync webAsync = new WebAsync();

		string putDataString = 
			"{" + 
				"\"action\": \"" + action + "\"" +
				", \"values\": " + values.ToString() + 
				"}";
		
		UTF8Encoding encoder = new UTF8Encoding();
		byte[] putData = encoder.GetBytes(putDataString);
		
		string URL = baseURL + "/gameplay/" + idGameplay + "/assess";
		
		WebRequest wr = getPUTrequest (URL, putData.Length);
		
		Stream dataStream = wr.GetRequestStream();
		dataStream.Write(putData, 0, putData.Length);
		dataStream.Close();
		
		StartCoroutine(webAsync.GetResponse(wr));
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		feedback = JSON.Parse(tmpMessage).AsArray;
		print ("Action " + putDataString + " assessed! returned: " + feedback.ToString());
		foreach (JSONNode f in feedback)
		{
			// create a new text in panel messages
			Text txtFeedback = (Text)Instantiate(textFeedback);
			txtFeedback.name = "txt_" + f["name"];
			txtFeedback.transform.SetParent(pnl_messages.transform);
			txtFeedback.text = f["message"];
			if (string.Equals( f["type"], "POSITIVE"))
				txtFeedback.color = new Color(0, 1, 0);
			if (string.Equals( f["type"], "NEGATIVE"))
				txtFeedback.color = new Color(1, 0, 0);
			
			RectTransform transform = txtFeedback.transform as RectTransform;   
			transform.anchoredPosition = new Vector2(5, - 10 - feedbackMessages.Count * 30 );
			
			feedbackMessages.Add(txtFeedback);

			// log badge
			if (string.Equals(f["type"], "BADGE"))
			{
				badgesWon.Add(f);
			}

		}
		stream.Close();
		
		print ("--- getScores ---");
		
		string URL2 = baseURL + "/gameplay/" + idGameplay + "/scores/";
		
		WebRequest wr2 = getGETrequest (URL2);	
		
		WebAsync webAsync2 = new WebAsync();
		
		StartCoroutine(webAsync2.GetResponse(wr2));
		
		while(! webAsync2.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream2 = webAsync2.requestState.webResponse.GetResponseStream();
		string tmpMessage2= new StreamReader(stream2).ReadToEnd().ToString();
		scores = JSON.Parse(tmpMessage2).AsArray;
		print ("Scores received! " + scores.ToString());
		
		mouseC.UpdateScores();
		stream2.Close();

		print ("--- getFeedback ---");
		
		string URL3 = baseURL + "/gameplay/" + idGameplay + "/feedback/";
		
		WebRequest wr3 = getGETrequest (URL3);	
		
		WebAsync webAsync3 = new WebAsync();
		
		StartCoroutine(webAsync3.GetResponse(wr3));
		
		while(! webAsync3.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream3 = webAsync3.requestState.webResponse.GetResponseStream();
		string tmpMessage3 = new StreamReader(stream3).ReadToEnd().ToString();
		feedback = JSON.Parse(tmpMessage3).AsArray;
		print ("Feedback received! " + feedback.ToString());

		foreach (JSONNode f in feedback)
		{
			// create a new text in panel messages
			Text txtFeedback = (Text)Instantiate(textFeedback);
			txtFeedback.name = "txt_" + f["name"];
			txtFeedback.transform.SetParent(pnl_messages.transform);
			txtFeedback.text = f["message"];
			if (string.Equals( f["type"], "POSITIVE"))
				txtFeedback.color = new Color(0, 1, 0);
			if (string.Equals( f["type"], "NEGATIVE"))
				txtFeedback.color = new Color(1, 0, 0);
			
			RectTransform transform = txtFeedback.transform as RectTransform;   
			transform.anchoredPosition = new Vector2(0, feedbackMessages.Count * 50 );

			feedbackMessages.Add(txtFeedback);

			// log badge
			if (string.Equals(f["type"], "BADGE"))
			{
				badgesWon.Add(f);
			}
		}

		endGame ();
		stream3.Close();
		
		mouseC.UpdateFeedback ();
	}

	public IEnumerator endGameplay()
	{
		print ("--- end Gameplay ---");
		WebAsync webAsync = new WebAsync();
				
		string URL = baseURL + "/gameplay/"+ idGameplay + "/end";
		
		WebRequest wr = getPOSTEmptyrequest (URL);
				
		StartCoroutine(webAsync.GetResponse(wr));
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();

		print ("Gameplay Ended! return: " + tmpMessage);
		stream.Close();
	}

	public IEnumerator updateScores()
	{
		print ("--- getScores ---");
		
		string URL = baseURL + "/gameplay/" + idGameplay + "/scores/";
		
		WebRequest wr = getGETrequest (URL);	
		
		WebAsync webAsync = new WebAsync();
		
		StartCoroutine(webAsync.GetResponse(wr));
		
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		scores = JSON.Parse(tmpMessage).AsArray;
		print ("Scores received! " + scores.ToString());
		stream.Close();

		mouseC.UpdateScores();
	}

	public void endGame()
	{
		foreach (JSONNode f in feedback)
		{
			if (string.Equals(f["final"], "lose"))
			{
				mouseC.loseGame();
			}
			else if (string.Equals(f["final"], "win"))
			{
				mouseC.winGame();
			}
		}
	}

	public IEnumerator updateFeedback()
	{
		print ("--- update Feedback ---");
		
		string URL = baseURL + "/gameplay/" + idGameplay + "/feedback/";
		
		WebRequest wr = getGETrequest (URL);	
		
		WebAsync webAsync = new WebAsync();
		
		StartCoroutine(webAsync.GetResponse(wr));
		
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		feedback = JSON.Parse(tmpMessage).AsArray;
		print ("Feedback received! " + feedback.ToString());
		foreach (JSONNode f in feedback)
		{
			// create a new text in panel messages
			Text txtFeedback = (Text)Instantiate(textFeedback);
			txtFeedback.name = "txt_" + f["name"];
			txtFeedback.transform.SetParent(pnl_messages.transform);
			txtFeedback.text = f["message"];
			if (string.Equals( f["type"], "POSITIVE"))
				txtFeedback.color = new Color(0, 1, 0);
			if (string.Equals( f["type"], "NEGATIVE"))
				txtFeedback.color = new Color(1, 0, 0);
			
			RectTransform transform = txtFeedback.transform as RectTransform;   
			transform.anchoredPosition = new Vector2(0, feedbackMessages.Count * 50 );
			
			feedbackMessages.Add(txtFeedback);
			// log badge
			if (string.Equals(f["type"], "BADGE"))
			{
				badgesWon.Add(f);
			}

		}

		endGame ();
		stream.Close();
		
		mouseC.UpdateFeedback();
	}

	public IEnumerator getGameDesc()
	{
		print ("--- get SG ---");
		
		string URL = baseURL + "/seriousgame/" + idSG + "/version/" + version;
		
		WebRequest wr = getGETrequest (URL);	
		
		WebAsync webAsync = new WebAsync();
		
		StartCoroutine(webAsync.GetResponse(wr));
		
		while(! webAsync.isResponseCompleted)
		{
			yield return null;
		}
		
		Stream stream = webAsync.requestState.webResponse.GetResponseStream();
		string tmpMessage = new StreamReader(stream).ReadToEnd().ToString();
		seriousGame = JSON.Parse(tmpMessage);
		print ("Serious game detailed received! " + seriousGame.ToString());
		stream.Close();
	}


}


