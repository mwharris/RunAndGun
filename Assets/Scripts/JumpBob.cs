using System;
using System.Collections;
using UnityEngine;

public class JumpBob : MonoBehaviour 
{
	public float BobDuration;
	public float BobAmount;

	private float currOffset = 0f;
	public float Offset()
	{
		return currOffset;
	}

	public IEnumerator DoBobCycle()
	{
		//First, move the camera downwards
		float t = 0f;
		while(t < BobDuration)
		{
			currOffset = Mathf.Lerp(0f, BobAmount, t / BobDuration);
			t += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}

		//Then return it to normal
		t = 0f;
		while(t < BobDuration)
		{
			currOffset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);
			t += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}
		currOffset = 0f;
	}
}
