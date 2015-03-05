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
	private int idSG = 90;
	private JSONArray badgesWon;

	public Text txtWelcome;

	public JSONArray getBadges()
	{
		return badgesWon;
	}

	void Start()
	{
		badgesWon = new JSONArray ();
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
			StartCoroutine(engage.getGameDesc(idSG));
			StartCoroutine(engage.getBadgesWon(idSG));
			RectTransform transform = contentPanel.gameObject.transform as RectTransform;        
			Vector2 position = transform.anchoredPosition;
			position.y -= transform.rect.height;
			transform.anchoredPosition = position;

			print ("setting badge dialog inactive");
			badgeDialog.SetActive (false);
		}

		else if (Application.loadedLevelName.Equals("StartScene"))
		{
			StartCoroutine(engage.getGameDesc(idSG));
			StartCoroutine(engage.getBadgesWon(idSG));
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
		StartCoroutine (engage.startGameplay(idSG, "StartScene"));
	}
	
	public void GetStarted()
	{
		
		username = txtUsername.text;
		password = txtPassword.text;

		StartCoroutine(engage.loginStudent(idSG, username, password, "LoginScene", "MenuScene", "ParametersScene"));
	}

	public void GetStartedGuest()
	{
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


	public void RecieveFeedback()
	{
		foreach (JSONNode f in engage.getFeedback())
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

			// trigger end of game
			if (string.Equals(f["final"], "lose"))
			{
				mouseC.loseGame();
			}
			else if (string.Equals(f["final"], "win"))
			{
				mouseC.winGame();
			}
		}
		mouseC.UpdateScores ();
	}

	
	public void RecieveBadges()
	{
		badgesWon = engage.getBadges();
	}
}


