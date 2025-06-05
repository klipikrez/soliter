using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    public int index;
    public Image selecotr;

    public Image image;
    public CreateButtons master;



    public void Deselelect()
    {
        if (selecotr != null)
            selecotr.color = new Color(1, 1, 1, 0);
    }

    public virtual void Select()
    {
        master.settings.SetCardBackTexture(index);

        master.DeselectAll();
        selecotr.color = new Color(1, 1, 1, 1);
        //master.mainImage.sprite = Sprite.Create((Texture2D)image.texture, new Rect(0, 0, 250, 350), new Vector2(0.5f, 0.5f));
    }

    public void SelectNoUpdate()
    {
        master.DeselectAll();
        selecotr.color = new Color(1, 1, 1, 1);
    }
}
