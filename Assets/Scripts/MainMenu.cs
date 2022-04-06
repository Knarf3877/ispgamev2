using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using TMPro;

public class MainMenu : MonoBehaviour
{
  
    public AudioMixer audioMixer;
    public AudioMixer sfxMixer;
    GameObject audioSlider;
    GameObject sfxSlider;
    public GameObject quality;
    GameObject fullscreenToggle;

    //public GameObject showLives;
    //Transform livesSlider;
    //Transform difficultySlider;
    //public GameObject showDifficulty;
    // public Slider volume;
    public TMP_Dropdown resolutionDropdown;
    //public GameObject[] selectLevel;
    int[] resolutionsWidth = { 720, 960, 1280, 1600, 1800, 1920 };
    int[] resolutionsHeight = { 400, 540, 720, 900, 1050, 1080 };

    private void Start()
    {
        AudioListener.pause = false;
        fullscreenToggle = GameObject.Find("Fullscreen Toggle");
        audioSlider = GameObject.Find("Slider");
        sfxSlider = GameObject.Find("Slider Sfx");
       // quality = GameObject.Find("Quality Dropdown");

        //if (PlayerPrefs.GetFloat("lives") == 0)
        //{
        //    SetLives(3);
        //}
        //else
        //{
        //    SetLives(PlayerPrefs.GetFloat("lives"));
        //    livesSlider = showLives.transform.parent;
        //    livesSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("lives");
        //}

        //if (PlayerPrefs.GetFloat("difficulty") == 0)
        //{
        //    SetDifficulty(1);
        //}
        //else
        //{
        //    SetDifficulty(PlayerPrefs.GetFloat("difficulty"));
        //    difficultySlider = showDifficulty.transform.parent;
        //    difficultySlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("difficulty");
        //}
        if(PlayerPrefs.GetInt("resolution") != 0)
        {
            resolutionDropdown.GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("resolution");
        }
        if(PlayerPrefs.GetInt("isFullscreen") == 1)
        {
           // fullscreenToggle = GameObject.Find("Fullscreen Toggle");
            fullscreenToggle.GetComponent<Toggle>().isOn = true;
        }
        if(PlayerPrefs.GetInt("quality") != 0)
        {
            //quality = GameObject.Find("Quality Dropdown");
            quality.GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("quality") - 1;
        }
        if (PlayerPrefs.GetFloat("MasterVolume") != 0){
           // audioSlider = GameObject.Find("Slider");
            audioSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVolume");
        }
        if (PlayerPrefs.GetFloat("SFXVolume") != 0)
        {
            //sfxSlider = GameObject.Find("Slider Sfx");
            sfxSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("SFXVolume");
        }

    }



    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("game quit");
    }



    public void GoToLevel()
    {
        string levelName = EventSystem.current.currentSelectedGameObject.name;
        int currentValue = int.Parse(levelName.Substring(0, 1));
        Debug.Log(levelName);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + currentValue);
    }

    public void SetScreenRes(int resIndex)
    {

        // Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolutionsWidth[resIndex], resolutionsHeight[resIndex], Screen.fullScreen);
        PlayerPrefs.SetInt("resolution", resIndex);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log(isFullscreen);
        if (isFullscreen == true)
        {
            PlayerPrefs.SetInt("isFullscreen", 1);
        }
        else
        {
            PlayerPrefs.SetInt("isFullscreen", 0);
        }
        PlayerPrefs.Save();
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
        PlayerPrefs.SetInt("quality", quality + 1);
        PlayerPrefs.Save();
    }

    public void SetVolume(float value)
    {
       // Debug.Log(value);
        audioMixer.SetFloat("MasterVolume", value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

    }

    public void SetSFXVolume(float value)
    {
        sfxMixer.SetFloat("SFXVolume", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
    public void SetLives(float lives)
    {
        PlayerPrefs.SetFloat("lives", lives);
        Debug.Log(PlayerPrefs.GetFloat("lives"));
        PlayerPrefs.Save();
        //showLives.GetComponent<TMP_Text>().text = "Lives: " + PlayerPrefs.GetFloat("lives");
    }

    //public void SetDifficulty(float hardness)
    //{
    //    PlayerPrefs.SetFloat("difficulty", hardness);
    //    Debug.Log(PlayerPrefs.GetFloat("difficulty"));
    //    PlayerPrefs.Save();
    //    switch (hardness)
    //    {
    //        case 1f:
    //            showDifficulty.GetComponent<TMP_Text>().text = "Easy";
    //            break;
    //        case 2f:
    //            showDifficulty.GetComponent<TMP_Text>().text = "Medium";
    //            break;
    //        case 3f:
    //            showDifficulty.GetComponent<TMP_Text>().text = "Hard";
    //            break;
    //        default:
    //            showDifficulty.GetComponent<TMP_Text>().text = "Difficulty not found";
    //            break;


    //    }
    //}
}
