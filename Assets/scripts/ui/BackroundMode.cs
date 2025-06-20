using UnityEngine;
using UnityEngine.UI;

public class BackroundMode : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public Toggle[] toggles;
    public GameObject[] gameObjects;
    public Settings settings;

    public void Check()
    {
        if (toggleGroup.GetFirstActiveToggle().gameObject.name == "1")
        {
            //color mode
            gameObjects[0].SetActive(true);
            gameObjects[1].SetActive(false);
            settings.LoadBackgroundColor();
        }
        else
        {
            //image mode
            gameObjects[0].SetActive(false);
            gameObjects[1].SetActive(true);
            settings.LoadBackgroundTexture();
        }
    }

}
