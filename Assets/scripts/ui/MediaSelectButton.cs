using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MediaSelectButton : SelectButton
{


    void GrantAcsess()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
using( AndroidJavaClass ajc = new AndroidJavaClass( "com.yasirkula.unity.NativeGalleryMediaPickerFragment" ) )
	ajc.SetStatic<bool>( "GrantPersistableUriPermission", true );
#endif
    }
    public override void Select()
    {
        Debug.Log("|||||||||||||||||||||||||select");
        openDialogueSelector();


    }

    public void openDialogueSelector()
    {
        if (NativeGallery.IsMediaPickerBusy())
            return;
        GrantAcsess();
        PickImage(-1, false);

    }

    private void PickImage(int maxSize, bool markTextureNonReadable)
    {
        Debug.Log("|||||||||||||||| PICK NEW IMAGE");
        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("|| Selected image path: " + path);
            if (path != null)
            {
                Debug.Log("||path not null");
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, markTextureNonReadable);
                if (texture == null)
                {
                    Debug.Log("||||||||Couldn't load texture from " + path);
                    return;
                }
                //texture = duplicateTexture(texture);
                texture.wrapMode = TextureWrapMode.Clamp;
                Sprite sprite = Sprite.Create(texture,
        new Rect(0, 0, texture.width, texture.height),
        new Vector2(0.5f, 0.5f));

                image.sprite = sprite;
                AfterPickImagePicked();


            }
        });
    }

    public virtual void AfterPickImagePicked()
    {


        master.settings.SetCardBackTexture(index);
        Debug.Log("|||||||||||||||||||||||||||||||||||||||||| index: " + index);

        SaveIamge("BOBOsmall.png");
        base.Select();
    }

    public void SaveIamge(string name)
    {
        byte[] bytes = image.sprite.texture.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, name);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Saved image to: " + filePath);
    }

    public void LoadOldImage(string name)
    {
        Debug.Log("|||||||||||||||| LOAD OLD IMAGE");
        string filePath = Path.Combine(Application.persistentDataPath, name);

        if (File.Exists(filePath))
        {
            GrantAcsess();
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2); // Create with dummy size, will resize on LoadImage
            texture.LoadImage(bytes); // Load PNG data
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

            image.sprite = sprite;
            Debug.Log("Loaded image from: " + filePath);
            //master.mainImage.sprite = Sprite.Create((Texture2D)image.texture, new Rect(0, 0, 250, 350), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning("Image file not found: " + filePath);
        }
    }
    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}