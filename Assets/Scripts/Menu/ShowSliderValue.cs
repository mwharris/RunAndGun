using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private Text percentageText;

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
		percentageText.text = ""+value;
    }
}
