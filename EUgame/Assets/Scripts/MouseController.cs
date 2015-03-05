using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class MouseController : MonoBehaviour {

	public float jetpackForce = 75.0f;
	public float forwardMovementSpeed = 3.0f;

	public Transform groundCheckTransform;	
	private bool grounded;	
	public LayerMask groundCheckLayerMask;	
	Animator animator;
	
	public ParticleSystem jetpack;
	public ParticleSystem winningExplosion;
	
	private bool dead = false;
	private bool win = false;

	public int lives;
	public JSONArray scores;
	public JSONArray feedback;
	
	public Texture2D coinIconTexture;
	public Texture2D livesIconTexture;
	public Texture2D euIconTexture;
	
	public AudioClip coinCollectSound;
	public AudioClip laserSound;
	public AudioSource jetpackAudio;	
	public AudioSource footstepsAudio;
	
	public Text pointsLabel;
	public Text euLabel;
	public GameObject restartWinDialog;
	public GameObject restartLoseDialog;
	public GameObject badgeDialog;
	public GameObject feedbackDialog;

	public Image life1;
	public Image life2;
	public Image life3;

	public UIManagerScript uiScript;
	public EngAGe engage;

	private List<string> countriesFound;
	
	private bool endWin = false;
	private bool endLose = false;

	private int waitFeedback = 0;

	private bool scoreUpdated = false;

	// Use this for initialization
	void Start () {
		countriesFound = new List<string>();
		scores = new JSONArray ();
		feedback = new JSONArray ();
		animator = GetComponent<Animator>();
		restartWinDialog.SetActive(false);
		restartLoseDialog.SetActive(false);
		badgeDialog.SetActive (false);
		feedbackDialog.SetActive (false);
		
		//correctEUcountries = new List<string>(euCountriesToFind);

		scores = engage.getScores();
		foreach (JSONNode score in scores)
		{
			string scoreName = score["name"];
			string scoreValue = score["value"];
			
			string s = "score (" + scoreName + ") : ";
			if (string.Equals (score["name"], "eu_score"))
			{
				s += "eu_score";
				pointsLabel.text = float.Parse(scoreValue).ToString();
			}
			else if (string.Equals (score["name"], "eu_countries"))
			{
				s += "eu_countries";
				euLabel.text = float.Parse(scoreValue).ToString();
			}
			else if (string.Equals (score["name"], "lives"))
			{
				s += "lives";
				float livesFloat = float.Parse(scoreValue);
				lives = Mathf.RoundToInt(livesFloat);
			}
			print(s);
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.CompareTag("Flags"))
			CollectFlag(collider);
	}
	
	void CollectFlag(Collider2D flagCollider)
	{
		Sprite spr_flag = flagCollider.gameObject.GetComponent<SpriteRenderer>().sprite;

		AudioSource.PlayClipAtPoint(coinCollectSound, transform.position);

		// country already selected
		if (countriesFound.Contains(spr_flag.name))
		{
			JSONNode values = JSON.Parse("{ \"country\" : \"" + spr_flag.name + "\" }");
			string action = "countryReSelected";
			StartCoroutine(engage.assess(action, values));
		}
		// country selected for the first time
		else
		{
			JSONNode values = JSON.Parse("{ \"country\" : \"" + spr_flag.name + "\" }");
			string action = "newCountrySelected";
			StartCoroutine(engage.assess(action, values));
		}

		UpdateScores();
		UpdateFeedback();

		// save country selected
		countriesFound.Add (spr_flag.name);


		flagCollider.gameObject.SetActive (false);
		//Destroy(flagCollider.gameObject);
	}

	void FixedUpdate () 
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !dead && !win;

		if (scoreUpdated)
		{
			UpdateScores();
			foreach (JSONNode score in scores)
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
					lives = Mathf.RoundToInt(livesFloat);
				}
			}
			scoreUpdated = false;
		}

		waitFeedback++;

		if (jetpackActive)
		{
			rigidbody2D.AddForce(new Vector2(0, jetpackForce));
		}
		if (!dead && !win)
		{
			Vector2 newVelocity = rigidbody2D.velocity;
			newVelocity.x = forwardMovementSpeed;
			rigidbody2D.velocity = newVelocity;
		}
		if (endWin && !restartWinDialog.activeInHierarchy) 
		{
			StartCoroutine (engage.endGameplay(true));
			win = true;
			animator.SetBool("win", win);
			restartWinDialog.SetActive(true);
		}
		if (endLose && !restartLoseDialog.activeInHierarchy) 
		{
			StartCoroutine (engage.endGameplay(false));
			dead = true;
			animator.SetBool ("dead", true);
			restartLoseDialog.SetActive(true);
		}

		if (lives >= 3)
		{
			life3.gameObject.SetActive(true);
			life2.gameObject.SetActive(true);
			life1.gameObject.SetActive(true);
		}
		else if (lives == 2)
		{
			life3.gameObject.SetActive(false);
			life2.gameObject.SetActive(true);
			life1.gameObject.SetActive(true);
		}
		else if (lives == 1)
		{
			life3.gameObject.SetActive(false);
			life2.gameObject.SetActive(false);
			life1.gameObject.SetActive(true);
		}
		else if (lives == 0)
		{
			life3.gameObject.SetActive(false);
			life2.gameObject.SetActive(false);
			life1.gameObject.SetActive(false);
		}


		UpdateGroundedStatus();
		AdjustJetpack(jetpackActive);
		AdjustWinningExplosion();
		AdjustFootstepsAndJetpackSound(jetpackActive);
	}

	void UpdateGroundedStatus()
	{
		grounded = Physics2D.OverlapCircle(groundCheckTransform.position, 0.1f, groundCheckLayerMask);
		animator.SetBool("grounded", grounded);
	}

	void AdjustJetpack (bool jetpackActive)
	{
		jetpack.enableEmission = !grounded && !win;
		jetpack.emissionRate = jetpackActive ? 300.0f : 75.0f; 
	}

	void AdjustWinningExplosion ()
	{
		winningExplosion.enableEmission = win;
	}

	void AdjustFootstepsAndJetpackSound(bool jetpackActive)    
	{
		footstepsAudio.enabled = !dead && grounded && !win && !badgeDialog.activeSelf && !feedbackDialog.activeSelf;		
		jetpackAudio.enabled = !dead && !grounded && !win && !badgeDialog.activeSelf && !feedbackDialog.activeSelf;		
		jetpackAudio.volume = jetpackActive ? 1.0f : 0.5f; 
	}

	public void OpenBadges()
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !dead && !win;

		badgeDialog.SetActive (!badgeDialog.activeSelf);
		Time.timeScale = (badgeDialog.activeSelf)? 0 : 1;
		AdjustFootstepsAndJetpackSound (jetpackActive);
	}

	public void CloseBadges()
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !dead && !win;
		badgeDialog.SetActive (false);
		Time.timeScale = 1;
		AdjustFootstepsAndJetpackSound (jetpackActive);
	}

	public void OpenFeedback()
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !dead && !win;
		
		feedbackDialog.SetActive (!feedbackDialog.activeSelf);
		Time.timeScale = (feedbackDialog.activeSelf)? 0 : 1;
		AdjustFootstepsAndJetpackSound (jetpackActive);
	}
	
	public void CloseFeedback()
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !dead && !win;
		feedbackDialog.SetActive (false);
		Time.timeScale = 1;
		AdjustFootstepsAndJetpackSound (jetpackActive);
	}

	public void RestartGame()
	{
		uiScript.StartGame ();
		//Application.LoadLevel (Application.loadedLevelName);
	}
	
	public void ExitToMenu()
	{
		Application.LoadLevel ("MenuScene");
	}
	
	public void UpdateScores()
	{
		print ("-- Update scores --");
		scores = engage.getScores();
		scoreUpdated = true;
	}

	public void UpdateFeedback()
	{
		print ("-- Update feedback --");
		feedback = engage.getFeedback();
	}

	public void winGame()
	{
		endWin = true;
	}
	public void loseGame()
	{
		endLose = true;
	}
}
