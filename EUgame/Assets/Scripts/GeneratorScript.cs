using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GeneratorScript : MonoBehaviour {

	public GameObject[] availableRooms;
	
	public List<GameObject> currentRooms;
	
	private float screenWidthInPoints;

	public GameObject[] availableObjects;    
	public List<GameObject> objects;  
	public List<Text> textLabels;
	//public Text txtCountry;
	//public Canvas canvas;
	
	public Sprite[] availableFlags;
	public Sprite[] availableCountries;
	
	public float objectsMinDistance = 5.0f;    
	public float objectsMaxDistance = 10.0f;
	
	public float objectsMinY = -1.4f;
	public float objectsMaxY = 1.4f;
	
	public float objectsMinRotation = -45.0f;
	public float objectsMaxRotation = 45.0f;

	// Use this for initialization
	void Start () {
		float height = 2.0f * Camera.main.orthographicSize;
		screenWidthInPoints = height * Camera.main.aspect;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () 
	{    
		GenerateRoomIfRequired();
		GenerateObjectsIfRequired ();	}

	void AddRoom(float farhtestRoomEndX)
	{
		int randomRoomIndex = Random.Range(0, availableRooms.Length);
		GameObject room = (GameObject)Instantiate(availableRooms[randomRoomIndex]);
		float roomWidth = room.transform.FindChild("floor").localScale.x;
		float roomCenter = farhtestRoomEndX + roomWidth * 0.5f;
		room.transform.position = new Vector3(roomCenter, 0, 0);
		currentRooms.Add(room);         
	}

	void GenerateRoomIfRequired()
	{
		List<GameObject> roomsToRemove = new List<GameObject>();
		bool addRooms = true;        
		float playerX = transform.position.x;
		float removeRoomX = playerX - screenWidthInPoints;        
		float addRoomX = playerX + screenWidthInPoints;
		float farthestRoomEndX = 0;
		
		foreach(var room in currentRooms)
		{
			float roomWidth = room.transform.FindChild("floor").localScale.x;
			float roomStartX = room.transform.position.x - (roomWidth * 0.5f);    
			float roomEndX = roomStartX + roomWidth;                            
			if (roomStartX > addRoomX)
				addRooms = false;
			if (roomEndX < removeRoomX)
				roomsToRemove.Add(room);
			farthestRoomEndX = Mathf.Max(farthestRoomEndX, roomEndX);
		}
		foreach(var room in roomsToRemove)
		{
			currentRooms.Remove(room);
			Destroy(room);            
		}
		if (addRooms)
			AddRoom(farthestRoomEndX);
	}

	void AddObject(float lastObjectX)
	{
		int randomIndex = Random.Range(0, availableObjects.Length);

		GameObject obj = (GameObject)Instantiate(availableObjects[randomIndex]);

		float objectPositionX = lastObjectX + Random.Range(objectsMinDistance, objectsMaxDistance);
		float randomY = Random.Range(objectsMinY, objectsMaxY);
		obj.transform.position = new Vector3(objectPositionX,randomY,0); 

		float rotation = Random.Range(objectsMinRotation, objectsMaxRotation);
		obj.transform.rotation = Quaternion.Euler(Vector3.forward * rotation);

		int randomSprite = Random.Range (0, availableCountries.Length);
		obj.GetComponent<SpriteRenderer> ().sprite = availableCountries [randomSprite];

		// automatically write country name
		// doesn't work because not on canvas
		/*
		Text country = (Text)Instantiate (txtCountry);
		country.name = "txt_" + availableSprites [randomSprite].name;
		country.transform.SetParent(canvas.transform);
		country.text = availableSprites [randomSprite].name;
		
		RectTransform transform = country.transform as RectTransform;   
		transform.anchoredPosition = new Vector2(objectPositionX, randomY-50 );

		textLabels.Add (country);*/
		objects.Add(obj);            
	}

	void GenerateObjectsIfRequired()
	{
		float playerX = transform.position.x;        
		float removeObjectsX = playerX - screenWidthInPoints;
		float addObjectX = playerX + screenWidthInPoints;
		float farthestObjectX = 0;

		List<GameObject> objectsToRemove = new List<GameObject>();

		foreach (var obj in objects)
		{
			float objX = obj.transform.position.x;
			farthestObjectX = Mathf.Max(farthestObjectX, objX);
			if (objX < removeObjectsX)            
				objectsToRemove.Add(obj);
		}
		foreach (var obj in objectsToRemove)
		{
			objects.Remove(obj);
			Destroy(obj);
		}
		if (farthestObjectX < addObjectX)
			AddObject(farthestObjectX);
	}
}
