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

    public EngAGe engage;
    private const int idSG = 104;

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
    public GameObject pnl_error_connection;

    // LoginScene
    public Text txtUsername;
	public InputField txtPassword;
	public Text txtLoginParagraph;
	
	private static string username;
	private static string password;

	// parameter scene
	public Text txtWelcome;
	public InputField inputPrefab; 
	public GameObject inputParent; 
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

	public int getDifficulty()
	{
		return difficulty;
	}

	void Start()
	{
		if (Application.loadedLevelName.Equals("LoginScene"))
		{
            // if the player already logged in, save username and 
            // if questions are needed, ask them, otherwise load menuScene
            if (EngAGe.E.playerIsKnown())
            {
                username = EngAGe.E.getUsername();

                if (EngAGe.E.QuestionsNeeded())
                {
                    Application.LoadLevel("ParametersScene");
                }
                else {
                    Application.LoadLevel("MenuScene");
                }
            }

            txtLoginParagraph.enabled = (EngAGe.E.getErrorCode() > 0); 
			txtLoginParagraph.text = EngAGe.E.getError();
		}
		else if (Application.loadedLevelName.Equals("ParametersScene"))
		{
			txtWelcome.text = "Welcome " + username ; 
			int i = 0; 
			// loop on all the player's characteristics needed 
			foreach (JSONNode param in EngAGe.E.getParameters()) { 
				// creates a text field in the panel parameters of the scene
				InputField inputParam = (InputField)Instantiate(inputPrefab); 
				inputParam.name = "input_" + param["name"]; 
				inputParam.transform.SetParent(inputParent.transform); 
				inputParam.text = param["question"] + " (" + param["type"] + ")"; 

				// position them, aligned vertically 
				RectTransform transform = inputParam.transform as RectTransform; 
				transform.anchoredPosition = new Vector2(0, 20 - i*50 ); 

				// save the input in the input array 
				inputFields.Add(inputParam); i++; 
			}

		}
		else if (Application.loadedLevelName.Equals("MenuScene"))
		{
            // retrieve EngAGe.E data about the game 
            EngAGe.E.testConnectionAndGetGameDesc(idSG);

            // retrieve (on or off line) badges won by player
            EngAGe.E.testConnectionAndGetBadgesWon(idSG);

            // get leaderboard if internet available local copy otherwise
			EngAGe.E.testConnectionAndGetLeaderboard(idSG);

			RectTransform transform = contentPanel.gameObject.transform as RectTransform;        
			Vector2 position = transform.anchoredPosition;
			position.y -= transform.rect.height;
			transform.anchoredPosition = position;

			// close all three windows
			badgeDialog.SetActive (false);
			infoDialog.SetActive (false);
			leaderboardDialog.SetActive (false);
		}
		else if (Application.loadedLevelName.Equals("GameScene"))
		{
			restartWinDialog.SetActive(false);
			restartLoseDialog.SetActive(false);
			feedbackDialog.SetActive (false);

			// initialise scores
			UpdateScores();
        }
        pnl_error_connection.SetActive(EngAGe.E.getErrorCode() == 200);
    }


	public void GoToMenu()
	{
		// for each parameter required 
		foreach (JSONNode param in EngAGe.E.getParameters()) { 
			// find the corresponding input field 
			foreach (InputField inputField in inputFields) { 
				if (inputField.name == "input_" + param["name"]) { 
					// and store the value in the JSON 
					param.Add("value", inputField.text); 
				} 
			} 
		}
        EngAGe.E.SaveParameters();
		Application.LoadLevel("MenuScene");
	}
	
	public void StartGame()
	{
		StartCoroutine (EngAGe.E.startGameplay(idSG, "GameScene"));
	}
	
	public void GetStarted()
	{
		username = txtUsername.text;
		password = txtPassword.text;

        //EngAGe.E.LoadConfigAndLogsFiles();
		EngAGe.E.testConnectionAndLoginStudent(idSG, username, password, "LoginScene", "MenuScene", "ParametersScene");
	}

	public void GetStartedGuest()
	{
		StartCoroutine(EngAGe.E.guestLogin(idSG, "LoginScene", "ParametersScene"));
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
	}
	
	public void CloseBadges()
	{
		badgeDialog.SetActive (false);
	}
	
	public void OpenInfo()
	{
		// get the seriousGame object from EngAGe.E 
		JSONNode SGdesc = EngAGe.E.getSG () ["seriousGame"];

		// display the title and description 
		txt_title.text = SGdesc["name"];
        txt_description.text = SGdesc["description"];

        // open the window 
        infoDialog.SetActive (!infoDialog.activeSelf);
	}
	
	public void CloseInfo()
	{
		infoDialog.SetActive (false);
	}
	public void OpenLeaderboard()
	{
		// get the leaderboard object from EngAGe.E
		JSONNode leaderboard = EngAGe.E.getLeaderboardList ();
		
		// look only at the eu_score (if it exists)
        JSONArray euScorePerf = (leaderboard["eu_score"] != null)? leaderboard ["eu_score"].AsArray : new JSONArray();
		
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
				// tell EngAGe.E it’s the end of the game (lost)
				StartCoroutine (EngAGe.E.endGameplay(false));
				// tell the mouse it lost the game 
				mouseC.loseGame();
				// open a dialog window to go to menu or restart game
				restartLoseDialog.SetActive(true);
			}
			else if (string.Equals(f["final"], "win"))
			{
				// tell EngAGe.E it’s the end of the game (won)
				StartCoroutine (EngAGe.E.endGameplay(true));
				// tell the mouse it won the game 
				mouseC.winGame();
				// open a dialog window to go to menu or restart game
				restartWinDialog.SetActive(true);
			}
			else if (string.Equals(f["type"], "ADAPTATION"))
			{
				if (string.Equals(f["name"], "speedGame"))
				{
					mouseC.forwardMovementSpeed += 1;
				}
				else if (string.Equals(f["name"], "slowGame"))
				{
					mouseC.forwardMovementSpeed -= 1;
				}
			}
		}
	}


	public void UpdateScores()
	{	
		foreach (JSONNode score in EngAGe.E.getScores())
		{
			string scoreName = score["name"];
			string scoreValue = score["value"];
			
			if (string.Equals(scoreName, "eu_score"))
			{
				pointsLabel.text = float.Parse(scoreValue).ToString();
			}
			else if (string.Equals(scoreName, "eu_countries"))
			{
				euLabel.text = float.Parse(scoreValue).ToString();
			}
			else if (string.Equals(scoreName, "lives"))
			{
				float livesFloat = float.Parse(scoreValue);
				int lives = Mathf.RoundToInt(livesFloat);
				
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


