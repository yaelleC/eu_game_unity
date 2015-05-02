﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using SimpleJSON;
using System.Text.RegularExpressions;

public class UIManagerScript : MonoBehaviour {

	public EngAGe engage;
	private const int idSG = 113;

	// MenuScene
	public Text txt_title; 
	public Text txt_description;
	public Animator startButton;
	public Animator settingsButton;
	public Animator dialog;
	public Animator imgLevel;
	public Animator contentPanel;
	public Animator gearImage;
	public Slider sdr_level;
	public GameObject badgeDialog;
	public GameObject infoDialog;
	public GameObject leaderboardDialog;
	public Text txt_listBestPlayers;

	// parameter scene
	public Text txtWelcome;
	public InputField inputPrefab; 
	public GameObject inputParent; 
	public Toggle toogleBoy;
	private List<InputField> inputFields = new List<InputField>();

	// game scene
	public Text txtFeedback;
	public MouseController mouseC;
	public Texture2D coinIconTexture;
	public Texture2D livesIconTexture;
	public Texture2D euIconTexture;
	
	public Text pointsLabel;
	public Text euLabel;
	public GameObject restartWinDialog;
	public GameObject restartLoseDialog;
	
	public GameObject feedbackDialog;
	
	public Image life1;
	public Image life2;
	public Image life3;

	private static int difficulty = 2;
	private static string ABtest;

	public int getDifficulty()
	{
		return difficulty;
	}

	void Start()
	{
		if (Application.loadedLevelName.Equals("ParametersScene"))
		{
			txtWelcome.text = "Welcome " ; 
			int i = 0; 
			// loop on all the player's characteristics needed 
			foreach (JSONNode param in engage.getParameters()) { 

				// creates a text field in the panel parameters of the scene
				InputField inputParam = (InputField)Instantiate(inputPrefab); 
				inputParam.name = "input_" + param["name"]; 
				inputParam.transform.SetParent(inputParent.transform); 

				inputParam.text = param["question"] + "" ; 

				if (!string.Equals(inputParam.name, "input_gender") && param["question"] != null)
				{
					// position them, aligned vertically 
					RectTransform transform = inputParam.transform as RectTransform; 
					transform.anchoredPosition = new Vector2(0, 20 - i*45 ); 
				}
				if (string.Equals(param["name"], "ABtest"))
				{
					ABtest = (Random.value >= 0.5f)? "group1" : "group2";
					inputParam.text = ABtest; 
				}
				// save the input in the input array 
				inputFields.Add(inputParam); i++; 
			}

		}
		else if (Application.loadedLevelName.Equals("MenuScene"))
		{
			// retrieve EngAGe data about the game 
			StartCoroutine(engage.getGameDesc(idSG));
			StartCoroutine(engage.getBadgesWon(idSG));
			StartCoroutine(engage.getLeaderboard(idSG));

			RectTransform transform = contentPanel.gameObject.transform as RectTransform;        
			Vector2 position = transform.anchoredPosition;
			position.y -= transform.rect.height;
			transform.anchoredPosition = position;

			// close all three windows
			badgeDialog.SetActive (false);
			infoDialog.SetActive (false);
			leaderboardDialog.SetActive (false);

			//ToggleMenu();
		}
		else if (Application.loadedLevelName.Equals("GameScene"))
		{
			restartWinDialog.SetActive(false);
			restartLoseDialog.SetActive(false);
			feedbackDialog.SetActive (false);

			// initialise scores
			UpdateScores();
		}
	}

	public void saveGenderBoy()
	{
		if(toogleBoy.isOn)
		{
			string paramName="gender";
			foreach (InputField inputField in inputFields) { 
				if (inputField.name == "input_" + paramName) { 
					// and store the value in the JSON 
					inputField.text = "male"; 
					return;
				} 
			}
		}
		else
		{
			string paramName="gender";
			foreach (InputField inputField in inputFields) { 
				if (inputField.name == "input_" + paramName) { 
					// and store the value in the JSON 
					inputField.text = "female"; 
					return;
				} 
			}
		}
	}

	public void GoToMenu()
	{
		// for each parameter required 
		foreach (JSONNode param in engage.getParameters()) 
		{ 	
			// find the corresponding input field 
			foreach (InputField inputField in inputFields) { 
				if (inputField.name == "input_" + param["name"]) { 
					// and store the value in the JSON 
					param.Add("value", inputField.text); 
				} 
			} 
		} 
		Application.LoadLevel("MenuScene");
	}

	public void QuitGame()
	{
		Application.Quit();
	}
	
	public void StartGame()
	{
		StartCoroutine (engage.startGameplay(idSG, "GameScene"));
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
		difficulty = (int)sdr_level.value;
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
		CloseLeaderboard ();
		CloseInfo ();
	}
	
	public void CloseBadges()
	{
		badgeDialog.SetActive (false);
	}
	
	public void OpenInfo()
	{
		// get the seriousGame object from engage 
		JSONNode SGdesc = engage.getSG () ["seriousGame"]; 

		// display the title and description 
		txt_title.text = SGdesc["name"]; 
		txt_description.text = SGdesc["description"]; 

		// open the window 
		infoDialog.SetActive (!infoDialog.activeSelf);
		CloseLeaderboard ();
		CloseBadges ();
	}
	
	public void CloseInfo()
	{
		infoDialog.SetActive (false);
	}
	public void OpenLeaderboard()
	{
		// get the leaderboard object from engage
		JSONNode leaderboard = engage.getLeaderboardList ();
		
		// look only at the eu_score 
		JSONArray euScorePerf = leaderboard ["eu_score"].AsArray;
		
		// display up to 10 best gameplays
		int max = 10;
		txt_listBestPlayers.text = "";
		foreach (JSONNode gameplay in euScorePerf) {
			if (max-- > 0)
			{
				// each gameplay has a "name" and a "score"
				float score = gameplay["score"].AsFloat ;
				txt_listBestPlayers.text += score + " - " +
					gameplay["name"] + "\n";
			}
		}
		// open the window
		leaderboardDialog.SetActive (!leaderboardDialog.activeSelf);
		CloseInfo ();
		CloseBadges ();
	}
	
	public void CloseLeaderboard()
	{
		leaderboardDialog.SetActive (false);
	}

	public void ActionAssessed(JSONNode jsonReturned) {
		UpdateScores ();
		UpdateFeedback (jsonReturned["feedback"].AsArray);
	}  

	public void UpdateFeedback(JSONArray feedbackReceived) 
	{
		foreach (JSONNode f in feedbackReceived)
		{
			// set color to write line into
			string color = "black";
			if (string.Equals( f["type"], "POSITIVE"))
				color = "green";
			if (string.Equals( f["type"], "NEGATIVE"))
				color="red";
			
			txtFeedback.text += "<color=\"" + color + "\">" + 
				f["message"] + "</color>\n";
			// trigger end of game?
			if (string.Equals(f["final"], "lose"))
			{
				// tell EngAGe it’s the end of the game (lost)
				StartCoroutine (engage.endGameplay(false));
				// tell the mouse it lost the game 
				mouseC.loseGame();
				// open a dialog window to go to menu or restart game
				restartLoseDialog.SetActive(true);
			}
			else if (string.Equals(f["final"], "win"))
			{
				// tell EngAGe it’s the end of the game (won)
				StartCoroutine (engage.endGameplay(true));
				// tell the mouse it won the game 
				mouseC.winGame();
				// open a dialog window to go to menu or restart game
				restartWinDialog.SetActive(true);
			}
			else if (string.Equals(f["type"], "ADAPTATION"))
			{
				if (string.Equals(f["name"], "speedGame"))
				{
					mouseC.forwardMovementSpeed += 1.5f;
				}
				else if (string.Equals(f["name"], "slowGame"))
				{
					mouseC.forwardMovementSpeed -= 0.5f;
				}
			}
		}
	}


	public void UpdateScores()
	{	
		foreach (JSONNode score in engage.getScores())
		{
			string scoreName = score["name"];
			float scoreValue = float.Parse(score["value"]);
			
			if (string.Equals(scoreName, "eu_score"))
			{
				pointsLabel.text = scoreValue.ToString();
			}
			else if (string.Equals(scoreName, "eu_countries"))
			{
				scoreValue = (ABtest.Equals("group1", System.StringComparison.Ordinal))? 28-scoreValue : scoreValue;
				euLabel.text = scoreValue.ToString();
			}
			else if (string.Equals(scoreName, "lives"))
			{
				int lives = Mathf.RoundToInt(scoreValue);
				
				life3.gameObject.SetActive(lives > 2);
				life2.gameObject.SetActive(lives > 1);
				life1.gameObject.SetActive(lives > 0);
			}
		}
	}

	
	public void OpenFeedback()
	{
		feedbackDialog.SetActive (!feedbackDialog.activeSelf);
		if (feedbackDialog.activeSelf)
		{
			mouseC.pause();
		} else {
			mouseC.unpause();
		}
	}
	
	public void CloseFeedback()
	{
		feedbackDialog.SetActive (false);
		mouseC.unpause();
	}

	public void ExitToMenu()
	{
		Application.LoadLevel ("MenuScene");
	}
}


