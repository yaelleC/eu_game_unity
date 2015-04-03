using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using SimpleJSON;

public class BadgeScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public GameObject toolTip;
	public Text txt_tooltip;
	public Sprite activeImage;

	public EngAGe engage;

	// Use this for initialization
	void Start () {

		// hide the tooltip for now
		toolTip.SetActive (false);	

		// get the badges won from EngAGe 
		JSONArray badges = engage.getBadges ();

		// get name of the badge represented
		string badgeName = this.name.Replace ("img_badge_", "");

		// if the badge is in EngAGe returned list, use the active image (color badge)
		foreach (JSONNode b in badges)
		{
			if (string.Equals(b["name"], badgeName))
			{				
				this.GetComponent<Image>().sprite = activeImage;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		// get name of the badge represented
		string badgeName = this.name.Replace ("img_badge_", "");
		// if the badge is in EngAGe returned list, use the active image (color badge)
		foreach (JSONNode b in engage.getBadges())
		{
			if (string.Equals(b["name"], badgeName) && b["earned"].AsBool)
			{				
				this.GetComponent<Image>().sprite = activeImage;
			}
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		// get the configuration file parsed in json format
		JSONNode sg = engage.getSG ();

		// get name of the badge represented
		string badgeName = this.name.Replace ("img_badge_", "");

		// update the description to the message defined in the config file
		// if no message is found the tooltip will display "description not available"
		string desc = "description not available";
		if ((sg ["feedback"] != null) && (sg ["feedback"][badgeName] != null))
		{
			desc = sg ["feedback"] [badgeName] ["message"];
		}

		showToolTip (data.position, desc);
	}

	public void OnPointerExit(PointerEventData data)
	{
		closeTooltip ();
	}

	public void closeTooltip()
	{
		toolTip.SetActive (false);
	}


	public void showToolTip(Vector2 toolPosition, string tooltipText)
	{
		txt_tooltip.text = tooltipText;
		toolTip.SetActive(true);
	}
}
