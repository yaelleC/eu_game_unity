using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class MouseController : MonoBehaviour {

	public UIManagerScript uiScript;

	public float jetpackForce = 75.0f;
	public float forwardMovementSpeed = 3.0f;

	public Transform groundCheckTransform;	
	private bool grounded;	
	public LayerMask groundCheckLayerMask;	
	Animator animator;
	
	public ParticleSystem jetpack;
	public ParticleSystem winningExplosion;
	
	public AudioClip coinCollectSound;
	public AudioSource jetpackAudio;	
	public AudioSource footstepsAudio;

	public EngAGe engage;

	private List<string> countriesFound;
	
	private bool endWin = false;
	private bool endLose = false;	

	// Use this for initialization
	void Start () {
		countriesFound = new List<string>();
		animator = GetComponent<Animator>();
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
		AudioSource.PlayClipAtPoint(coinCollectSound, transform.position);

		// get the name of the country selected
		Sprite spr_flag = flagCollider.gameObject.GetComponent<SpriteRenderer>().sprite;

		// country already selected
		if (countriesFound.Contains(spr_flag.name))
		{
			// create a JSON with one value, "country" (only parameter of countryReSelected)
			JSONNode values = JSON.Parse("{ \"country\" : \"" + spr_flag.name + "\" }");
			// ask EngAGe to assess the action based on the config file
			StartCoroutine(engage.assess("countryReSelected", values, uiScript.ActionAssessed));
		}
		// country selected for the first time
		else
		{
			// create a JSON with one value, "country" (only parameter of newCountrySelected)
			JSONNode values = JSON.Parse("{ \"country\" : \"" + spr_flag.name + "\" }");
			// ask EngAGe to assess the action based on the config file
			StartCoroutine(engage.assess("newCountrySelected", values, uiScript.ActionAssessed));
		}
		// save country selected
		countriesFound.Add (spr_flag.name);

		flagCollider.gameObject.SetActive (false);
	}

	void FixedUpdate () 
	{
		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !endLose && !endWin;

		if (jetpackActive)
		{
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jetpackForce));
		}
		if (!endWin && !endLose)
		{
			Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
			newVelocity.x = forwardMovementSpeed;
			GetComponent<Rigidbody2D>().velocity = newVelocity;
		}
		else if (endWin) 
		{
			animator.SetBool("win", true);
		}
		else if (endLose) 
		{
			animator.SetBool ("dead", true);
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
		jetpack.enableEmission = !grounded && !endWin;
		jetpack.emissionRate = jetpackActive ? 300.0f : 75.0f; 
	}

	void AdjustWinningExplosion ()
	{
		winningExplosion.enableEmission = endWin;
	}

	void AdjustFootstepsAndJetpackSound(bool jetpackActive)    
	{
		footstepsAudio.enabled = !endLose && grounded && !endWin;		
		jetpackAudio.enabled = !endLose && !grounded && !endWin;		
		jetpackAudio.volume = jetpackActive ? 1.0f : 0.5f; 
	}

	public void pause()
	{
		Time.timeScale = 0;
		footstepsAudio.enabled = false;
		jetpackAudio.enabled = false;	
	}
	
	public void unpause()
	{
		Time.timeScale = 1;

		bool jetpackActive = Input.GetButton("Fire1");
		jetpackActive = jetpackActive && !endLose && !endWin;
		AdjustFootstepsAndJetpackSound (jetpackActive);
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
