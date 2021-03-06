using UnityEngine;
using System.Collections;

public class LaserScript : MonoBehaviour {
	
	public Sprite laserOnSprite;    
	public Sprite laserOffSprite;
   
	public float interval = 0.5f;    
	public float rotationSpeed = 0.0f;

	private bool isLaserOn = true;    
	private float timeUntilNextToggle;

	// Use this for initialization
	void Start () {
		timeUntilNextToggle = interval;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () {
		//1
		timeUntilNextToggle -= Time.fixedDeltaTime;
		
		//2
		if (timeUntilNextToggle <= 0) {
			
			//3
			isLaserOn = !isLaserOn;
			
			//4
			GetComponent<Collider2D>().enabled = isLaserOn;
			
			//5
			SpriteRenderer spriteRenderer = ((SpriteRenderer)this.GetComponent<Renderer>());
			if (isLaserOn)
				spriteRenderer.sprite = laserOnSprite;
			else
				spriteRenderer.sprite = laserOffSprite;
			
			//6
			timeUntilNextToggle = interval;
		}
		
		//7
		transform.RotateAround(transform.position, Vector3.forward, rotationSpeed * Time. fixedDeltaTime);
	}
}
