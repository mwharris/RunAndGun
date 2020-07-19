using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private Transform optionsMenu;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private List<GameObject> postProcessingVolumes;
    
    public static float MouseSensitivity { get; private set; }
    public bool InvertY { get; set; } = false;
    public bool AimAssist { get; set; } = true;

    private Transform _optionsPanel;
    private Resolution[] _resolutions;
    private bool _postProcessingActive = true;

    void Start()
    {
        _optionsPanel = optionsMenu.transform.GetChild(1);
        MouseSensitivity = _optionsPanel.GetComponentInChildren<Slider>().value;
        InitOptions();
    }

    private void InitOptions()
    {
        fullscreenToggle.isOn = Screen.fullScreen;
        InitResolutionOptions();
        InitQualityOptions();
    }

    private void InitResolutionOptions()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        _resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int selectedIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            Resolution r = _resolutions[i];
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData($"{r.width} x {r.height} ({r.refreshRate} Hz)");
            options.Add(option);

            if (r.width == Screen.currentResolution.width 
                && r.height == Screen.currentResolution.height 
                && r.refreshRate == Screen.currentResolution.refreshRate)
            {
                selectedIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = selectedIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    private void InitQualityOptions()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        qualityDropdown.ClearOptions();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(QualitySettings.names[i]));
        }
        qualityDropdown.AddOptions(options);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void ShowOptionsMenu()
    {
        //Set the Options menu children to active
        for (int i = 0; i < optionsMenu.transform.childCount; i++)
        {
            Transform t = optionsMenu.transform.GetChild(i);
            t.gameObject.SetActive(true);
        }

        //Default select the Close button
        Transform closeButton = _optionsPanel.transform.GetChild(_optionsPanel.childCount - 1);
        closeButton.GetComponent<Selectable>().Select();
    }

    public void CloseOptionsMenu()
    {
        //Set the Options menu children to inactive
        for (int i = 0; i < optionsMenu.transform.childCount; i++)
        {
            Transform t = optionsMenu.transform.GetChild(i);
            t.gameObject.SetActive(false);
        }
    }

    public void SetScreenResolution(int resolutionIndex)
    {
        Resolution r = _resolutions[resolutionIndex];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    // TODO: Maybe have more granular post processing settings?
    // TODO: For example, a Bloom option, AO option, Color option, etc.
    public void SetQuality(int qualityIndex)
    {
        // Disable post processing stack on Fastest quality
        bool setPostProcessingActive = qualityIndex > 0;
        if (_postProcessingActive != setPostProcessingActive)
        {
            _postProcessingActive = setPostProcessingActive;
            foreach (GameObject volume in postProcessingVolumes)
            {
                volume.SetActive(_postProcessingActive);
            }
        }
        // Tell Unity to change the quality setting
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    public void ChangeMouseSensitivity(float newValue)
    {
        MouseSensitivity = newValue;
    }

    public void ChangeInvertY(bool newValue)
    {
        InvertY = newValue;
    }

    public void ChangeAimAssist(bool newValue)
    {
        AimAssist = newValue;
    }
}
