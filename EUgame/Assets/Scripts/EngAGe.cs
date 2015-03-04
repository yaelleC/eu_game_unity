using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using SimpleJSON;

public class EngAGe : MonoBehaviour {
		
	private static int idStudent;
	private static int idPlayer = -1;
	private static int version;
	private static int idGameplay;
	private static JSONArray parameters;
	private static JSONArray scores = new JSONArray ();
	private static JSONArray feedback = new JSONArray ();
	private static JSONNode seriousGame = new JSONNode();
	
	private static string error;
	private static int errorCode;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

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
		return idStudent;
	}
	public int getIdPlayer()
	{
		return idPlayer;
	}
	public int getVersion()
	{
		return version;
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
	public JSONNode getSG()
	{
		return seriousGame;
	}

	// ************* Web services calls ****************** //
	private string baseURL = "http://146.191.107.189:8080";

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
	
	public IEnumerator loginStudent(int p_idSG, string p_username, string p_password, 
	                                string sceneLoginFail, string sceneNoParameters, string sceneParameters)
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
		
		bool loginSuccess = loginData["loginSuccess"].AsBool;
		
		if (!loginSuccess)
		{
			errorCode = 201;
			error = "Login failed";
			Application.LoadLevel(sceneLoginFail);
		}
		else
		{
			errorCode = 0;
			error = "";
			if (loginData["idPlayer"] != null)
			{
				idPlayer = loginData["idPlayer"].AsInt;
				version = loginData["version"].AsInt;
				idStudent = loginData["student"]["idStudent"].AsInt;
				parameters = loginData["params"].AsArray;
				
				Application.LoadLevel(sceneNoParameters);
			}
			else
			{
				version = loginData["version"].AsInt;
				idStudent = loginData["student"]["idStudent"].AsInt;
				parameters = loginData["params"].AsArray;
				
				Application.LoadLevel(sceneParameters);
			}
		}
		stream.Close();
	}

	public IEnumerator guestLogin(int p_idSG, string sceneLoginFail, string sceneParameters)
	{
		print ("--- loginStudent ---");
		
		string URL = baseURL + "/SGaccess";
		
		string postDataString = 
			"{" + 
				"\"idSG\": " + p_idSG + 
				", \"username\": \" \"" +
				", \"password\": \" \"" +
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
				
		stream.Close();
	}
	
	/*
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
	*/
}
