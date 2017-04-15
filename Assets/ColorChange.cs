using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChange : MonoBehaviour {

	public float changeTimer = 0f;
	public Color color1;
	public Color color2;

	private float currChangeTime = 0f;
	private float lastChangeTime;
	private Text textComp;

	void Start()
	{
		textComp = GetComponent<Text>();
	}

	void Update () 
	{
		if(Time.time - currChangeTime > changeTimer)
		{
			//Reset the timer
			currChangeTime = Time.time;
			//Change the Color
			if(textComp.color == color1)
			{
				this.GetComponent<Text>().color = color2;
			}
			else
			{
				this.GetComponent<Text>().color = color1;
			}
		}
	}
}
