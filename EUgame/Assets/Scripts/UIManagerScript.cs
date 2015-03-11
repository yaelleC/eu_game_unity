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

	// MenuScene
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

	// LoginScene
	public Text txtUsername;
	public InputField txtPassword;
	public Text txtLoginParagraph;
	
	private static string username;
	private static string password;

	// parameter scene
	public Text txtWelcome;

	// game scene
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
			txtLoginParagraph.enabled = false;
		}
		else if (Application.loadedLevelName.Equals("ParametersScene"))
		{
			txtWelcome.text = "Welcome " + username ;			
		}
		else if (Application.loadedLevelName.Equals("MenuScene"))
		{
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
		}
	}

	public void GoToMenu()
	{
		Application.LoadLevel("MenuScene");
	}
	
	public void StartGame()
	{
		Application.LoadLevel("GameScene");
	}
	
	public void GetStarted()
	{
		username = txtUsername.text;
		password = txtPassword.text;
		
		Application.LoadLevel("ParametersScene");
	}

	public void GetStartedGuest()
	{
		Application.LoadLevel("ParametersScene");
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
		// open the window
		infoDialog.SetActive (!infoDialog.activeSelf);
	}
	
	public void CloseInfo()
	{
		infoDialog.SetActive (false);
	}
	public void OpenLeaderboard()
	{
		// open the window
		leaderboardDialog.SetActive (!leaderboardDialog.activeSelf);
	}
	
	public void CloseLeaderboard()
	{
		leaderboardDialog.SetActive (false);
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


