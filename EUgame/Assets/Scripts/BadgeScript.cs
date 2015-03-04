using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using SimpleJSON;

public class BadgeScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public GameObject toolTip;
	public Text txt_tooltip;
	public UIManagerScript uiScript;
	public Sprite activeImage;

	private int wait = 0;

	// Use this for initialization
	void Start () {
		toolTip.SetActive (false);	
	}
	
	// Update is called once per frame
	void Update () {
		wait++;
		if (wait > 50)
		{
			JSONArray badges = uiScript.getBadges ();
			print (badges.ToString());
			string badgeName = this.name.Replace ("img_badge_", "");
				
			foreach (JSONNode b in badges)
			{
				if (string.Equals(b["name"], badgeName))
				{
					this.GetComponent<Image>().sprite = activeImage;
				}
			}
			wait = 0;
		}
	}



	public void OnPointerEnter(PointerEventData data)
	{
		print ("mouse over");

		JSONNode sg = uiScript.getSeriousgame ();
		string badgeName = this.name.Replace ("img_badge_", "");
		string desc = "description not available";
		if ((sg ["feedback"] != null) && (sg ["feedback"][badgeName] != null))
			desc = sg ["feedback"] [badgeName] ["message"];

		showToolTip (data.position, desc);

		JSONArray badges = uiScript.getBadges ();
		print (badges.ToString());
				
		foreach (JSONNode b in badges)
		{
			if (string.Equals(b["name"], badgeName))
			{
			//	this.GetComponent<SpriteRenderer> ().sprite = activeImage;
			}
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		closeTooltip ();
	}

	public void closeTooltip()
	{
		toolTip.SetActive (false);
	}


	public void showToolTip(Vector2 toolPosition, string badgeDialog)
	{
		txt_tooltip.text = badgeDialog;
		toolTip.SetActive(true);
	}
}
