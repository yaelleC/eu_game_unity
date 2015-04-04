using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using SimpleJSON;

public class BadgeScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public GameObject toolTip;
	public Text txt_tooltip;
	public Sprite activeImage;

	// Use this for initialization
	void Start () {

		// hide the tooltip for now
		toolTip.SetActive (false);	
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void OnPointerEnter(PointerEventData data)
	{
		// get name of the badge represented
		string badgeName = this.name.Replace ("img_badge_", "");

		// update the tooltip
		string desc = badgeName + ": description not available";

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
