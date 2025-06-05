using UnityEngine;

public class MediaSelectButtonBackground : MediaSelectButton
{
    public override void AfterPickImagePicked()
    {


        master.settings.SetBackgroundTexture(image.sprite.texture);
        Debug.Log("|||||||||||||||||||||||||||||||||||||||||| index: " + index);

        SaveIamge("BOBObig.png");

    }
}
