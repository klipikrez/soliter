using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsClass
{
    public int cardBack = 1;
    public float volume = 1;
    public int background = 0;
    public string name = "";
    public Color bgColor = new Color(65, 0, 0);
}

public class Settings : MonoBehaviour
{
    public List<Image> cardImages = new List<Image>();
    public CreateButtons createButtons;
    public Slider audioSlider;
    public AudioMixer audioMixer;

    void Start()
    {
        SettingsClass settingsClass = GetSettingsClass();
        Debug.Log(settingsClass.volume);
        LoadCardBackTexture(settingsClass.cardBack);
        LoadVolume(settingsClass.volume);
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
        File.WriteAllText(path, json);

        Debug.Log("Saved JSON to: " + path);
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


    public void LoadCardBackTexture(int index)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "BOBOsmall.png")))
            ((MediaSelectButton)createButtons.buttons[0]).LoadOldImage();
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
}
