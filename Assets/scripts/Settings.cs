using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsClass
{
    public int cardBack = 1;
    public float volume = 0.52f;
    public float musicVolume = 1f;
    public int background = 0;
    public string name = "";
    public Color bgColor = new Color(0.19f, 0, 0);
    public int gameMode = 0;
    public bool showTutorial = true;
}

public class Settings : MonoBehaviour
{
    public List<Image> cardImages = new List<Image>();
    public CreateButtons createButtons;
    public Slider audioSliderMaster;
    public Slider audioSliderMusic;
    public AudioMixer audioMixer;
    public Material backgroundShader;
    public BackroundMode backroundMode;
    public ColorPreview colorPreview;
    public MediaSelectButtonBackground mediaSelectButtonBackground;
    public GameObject settingsCanvas;
    public GameObject tutorialCanvas;

    public GameObject[] gamemodeButtons;
    public Background backgroundScript;

    public void LoadSettings()
    {
        SettingsClass settingsClass = GetSettingsClass();
        LoadCardBackTexture(settingsClass.cardBack);
        LoadVolume(settingsClass.volume, settingsClass.musicVolume);
        LoadGameMode(settingsClass.gameMode);


        settingsCanvas.SetActive(true);
        colorPreview.Inicialize();


        LoadBackground();
        backroundMode.Check();
        settingsCanvas.SetActive(false);

        if (settingsClass.showTutorial)
        {
            tutorialCanvas.SetActive(true);
            SetShowTutorial(false);
        }
    }
    public SettingsClass GetSettingsClass()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "settings.rez");
        SettingsClass settingsClass = new SettingsClass();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            settingsClass = JsonUtility.FromJson<SettingsClass>(json);
        }
        return settingsClass;
    }
    public void SaveSettings(SettingsClass settingsClass)
    {

        string json = JsonUtility.ToJson(settingsClass, true);
        string path = Path.Combine(Application.persistentDataPath, "settings.rez");
        //Debug.Log("boban" + path);
        File.WriteAllText(path, json);
        Debug.Log("GameMode path: " + path);
        //Debug.Log("Saved JSON to: " + path);
    }

    public void ResetSettings()
    {
        SettingsClass settingsClass = new SettingsClass();
        SaveSettings(settingsClass);

        LoadCardBackTexture(settingsClass.cardBack);
        LoadVolume(settingsClass.volume, settingsClass.musicVolume);
        LoadBackground();
        backroundMode.Check();
    }

    public void SetCardBackTexture(int index)
    {
        Sprite spr = createButtons.buttons[index].image.sprite;
        //Sprite spr = Sprite.Create(image, new Rect(0, 0, 250, 350), new Vector2(0.5f, 0.5f));
        foreach (Image image1 in cardImages)
        {
            image1.sprite = spr;
        }

        GameObject[] cardObjects = GameObject.FindGameObjectsWithTag("card");
        Debug.Log(cardObjects.Length);
        foreach (GameObject obj in cardObjects)
        {
            Card card = obj.GetComponent<Card>();
            if (card == null) { Debug.Log("error loading card: " + obj.name); continue; }
            card.SetBackImage(spr);
        }

        GameManager.instance.cardBackSprite = spr;

        SettingsClass settingsClass = GetSettingsClass();
        settingsClass.cardBack = index;
        SaveSettings(settingsClass);
    }

    public void SetBackgroundTexture(Texture spr)
    {
        backgroundScript.SetBackgroundTexture(spr);

        SettingsClass settings = GetSettingsClass();
        settings.background = 1;
        SaveSettings(settings);
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundScript.SetBackgroundColor(color);

        SettingsClass settings = GetSettingsClass();
        settings.background = 0;
        settings.bgColor = color;
        SaveSettings(settings);
    }

    public void LoadBackground()
    {
        SettingsClass settingsClass = GetSettingsClass();
        backroundMode.toggles[settingsClass.background].isOn = true;
        if (settingsClass.background == 0)
        {
            LoadBackgroundColor();
        }
        else
        {
            LoadBackgroundTexture();
        }
    }

    public void LoadBackgroundTexture()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "BOBObig.png"))) return;

        mediaSelectButtonBackground.LoadOldImage("BOBObig.png");

        backgroundScript.SetBackgroundTexture(mediaSelectButtonBackground.image.mainTexture);


    }

    public void LoadBackgroundColor()
    {
        Color ser = GetSettingsClass().bgColor;
        colorPreview.SetColor(ser);
        backgroundScript.SetBackgroundColor(ser);
    }

    public void LoadCardBackTexture(int index)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "BOBOsmall.png")))
            ((MediaSelectButton)createButtons.buttons[0]).LoadOldImage("BOBOsmall.png");
        else if (index == 0) index = 1;
        createButtons.SelectNoUpdate(index);
        SetCardBackTexture(index);
    }

    public void SetVolumeMaster(float masterValue)
    {
        SetVolume(masterValue, -1);
    }

    public void SetVolumeMusic(float musicValue)
    {
        SetVolume(-1, musicValue);
    }

    public void SetVolume(float masterValue, float musicValue)
    {

        masterValue /= 100;
        musicValue /= 100;

        SettingsClass settingsClass = GetSettingsClass();

        if (masterValue >= 0) settingsClass.volume = masterValue;
        if (musicValue >= 0) settingsClass.musicVolume = musicValue;
        Debug.Log("SetVolume: " + settingsClass.volume + " " + settingsClass.musicVolume);

        audioMixer.SetFloat("Master", (Mathf.Log10(settingsClass.volume) * 20) != float.NegativeInfinity ? Mathf.Log10(settingsClass.volume) * 20 : -52);
        audioMixer.SetFloat("Music", (Mathf.Log10(settingsClass.musicVolume) * 20) != float.NegativeInfinity ? Mathf.Log10(settingsClass.musicVolume) * 20 : -52);
        SaveSettings(settingsClass);
    }
    public void LoadVolume(float masterValue, float musicValue)
    {
        SetVolume(masterValue * 100, musicValue * 100);
        audioSliderMaster.value = masterValue * 100;
        audioSliderMusic.value = musicValue * 100;
        //audioMixer.SetFloat("master", (Mathf.Log10(value) * 20) != float.NegativeInfinity ? Mathf.Log10(value) * 20 : -52);
    }

    public void LoadGameMode(int i)
    {
        Debug.Log("GameMode load: " + i);
        SetGamemode(i);

    }
    public void SetGamemode(int i)
    {
        Debug.Log("GameMode set: " + i);
        SettingsClass settingsClass = GetSettingsClass();
        settingsClass.gameMode = i;
        Debug.Log("GameMode set: " + settingsClass.gameMode);
        switch (i)
        {
            case 0:
                {
                    GameManager.instance.SetOneSuit();
                    SetSelectedGememodeGraphic(0);
                    break;
                }
            case 1:
                {
                    GameManager.instance.SetTwoSuit();
                    SetSelectedGememodeGraphic(1);
                    break;
                }
            case 2:
                {
                    GameManager.instance.SetFourSuit();
                    SetSelectedGememodeGraphic(2);
                    break;
                }
        }

        SaveSettings(settingsClass);
    }

    public void SetSelectedGememodeGraphic(int i)
    {
        Debug.Log(i + " NUTTON");
        foreach (GameObject nutton in gamemodeButtons)
            nutton.SetActive(false);
        gamemodeButtons[i].SetActive(true);
    }

    public void SetShowTutorial(bool value)
    {
        SettingsClass settingsClass = GetSettingsClass();
        settingsClass.showTutorial = value;
        SaveSettings(settingsClass);
    }
}
