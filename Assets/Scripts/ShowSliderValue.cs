using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
    private Text percentageText;
    [SerializeField] private Slider slider;

	void Start()
    {
        percentageText = GetComponent<Text>();
        SetSliderText(slider.value);
    }
	
	public void textUpdate(float value)
    {
        SetSliderText(slider.value);
    }

    private void SetSliderText(float value)
    {
        percentageText.text = Mathf.RoundToInt((value / 5) * 100) + "%";
    }
}
