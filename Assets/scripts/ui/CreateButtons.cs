using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CreateButtons : MonoBehaviour
{
    public List<SelectButton> buttons = new List<SelectButton>();
    public Settings settings;
    public GameObject seletWindow;
    //public Image mainImage;


    private bool IsImage(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";
    }
    public void DeselectAll()
    {
        foreach (SelectButton but in buttons)
        {
            but.Deselelect();
        }
        CloaseWindow();
    }

    public void SelectNoUpdate(int index)
    {
        //mainImage.sprite = Sprite.Create((Texture2D)buttons[index].image.texture, new Rect(0, 0, 250, 350), new Vector2(0.5f, 0.5f));
        buttons[index].SelectNoUpdate();
    }

    public void CloaseWindow()
    {
        if (seletWindow != null)
            seletWindow.SetActive(false);
    }

}
