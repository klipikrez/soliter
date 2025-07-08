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
    public int background = 0;
    public string name = "";
    public Color bgColor = new Color(0.19f, 0, 0);
    public int gameMode = 0;
}

public class Settings : MonoBehaviour
{
    public List<Image> cardImages = new List<Image>();
    public CreateButtons createButtons;
    public Slider audioSlider;
    public AudioMixer audioMixer;
    public Material backgroundShader;
    public BackroundMode backroundMode;
    public ColorPreview colorPreview;
    public MediaSelectButtonBackground mediaSelectButtonBackground;
    public GameObject settingsCanvas;

    public GameObject[] gamemodeButtons;

    public void LoadSettings()
    {
        SettingsClass settingsClass = GetSettingsClass();
        LoadCardBackTexture(settingsClass.cardBack);
        LoadVolume(settingsClass.volume);
        LoadGameMode(settingsClass.gameMode);


        settingsCanvas.SetActive(true);
        colorPreview.Inicialize();


        LoadBackground();
        backroundMode.Check();
        settingsCanvas.SetActive(false);
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
        LoadVolume(settingsClass.volume);
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
        backgroundShader.SetColor("col", Color.white);
        backgroundShader.SetTexture("tex", spr);

        SettingsClass settings = GetSettingsClass();
        settings.background = 1;
        SaveSettings(settings);
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundShader.SetColor("col", color);
        backgroundShader.SetTexture("tex", null);

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

        backgroundShader.SetColor("col", Color.white);
        backgroundShader.SetTexture("tex", mediaSelectButtonBackground.image.mainTexture);



    }

    public void LoadBackgroundColor()
    {
        Color ser = GetSettingsClass().bgColor;
        colorPreview.SetColor(ser);
        backgroundShader.SetColor("col", ser);
        backgroundShader.SetTexture("tex", null);
    }

    public void LoadCardBackTexture(int index)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "BOBOsmall.png")))
            ((MediaSelectButton)createButtons.buttons[0]).LoadOldImage("BOBOsmall.png");
        else if (index == 0) index = 1;
        createButtons.SelectNoUpdate(index);
        SetCardBackTexture(index);
    }

    public void SetVolume(float value)
    {

        value /= 100;
        SettingsClass settingsClass = GetSettingsClass();
        settingsClass.volume = value;
        audioMixer.SetFloat("Master", (Mathf.Log10(value) * 20) != float.NegativeInfinity ? Mathf.Log10(value) * 20 : -52);
        SaveSettings(settingsClass);
    }
    public void LoadVolume(float value)
    {
        SetVolume(value * 100);
        audioSlider.value = value * 100;
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
}
