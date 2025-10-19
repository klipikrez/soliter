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
        //Debug.Log("|||||||||||||||||||||||||select");
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
        //Debug.Log("|||||||||||||||| PICK NEW IMAGE");
        NativeGallery.GetImageFromGallery((path) =>
        {
            //  Debug.Log("|| Selected image path: " + path);
            if (path != null)
            {
                //    Debug.Log("||path not null");
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, markTextureNonReadable);
                if (texture == null)
                {
                    //      Debug.Log("||||||||Couldn't load texture from " + path);
                    return;
                }
                //texture = duplicateTexture(texture);
                texture.wrapMode = TextureWrapMode.Clamp;

                texture = ResizeAndCropTexture(texture);

                Sprite sprite = Sprite.Create(texture,
        new Rect(0, 0, texture.width, texture.height),
        new Vector2(0.5f, 0.5f));



                image.sprite = sprite;
                AfterPickImagePicked();


            }
        });
    }


    Texture2D ResizeAndCropTexture(Texture2D texture2D)
    {
        int targetWidth = 250;
        int targetHeight = 350;

        int width = texture2D.width;
        int height = texture2D.height;

        // Determine the aspect ratios
        float targetAspect = (float)targetWidth / targetHeight;
        float sourceAspect = (float)width / height;

        int cropWidth = width;
        int cropHeight = height;
        int xMin = 0;
        int yMin = 0;

        // Crop to match the target aspect ratio while keeping the center
        if (sourceAspect > targetAspect)
        {
            // Source is wider than target
            cropWidth = Mathf.RoundToInt(height * targetAspect);
            xMin = (width - cropWidth) / 2;
        }
        else if (sourceAspect < targetAspect)
        {
            // Source is taller than target
            cropHeight = Mathf.RoundToInt(width / targetAspect);
            yMin = (height - cropHeight) / 2;
        }

        // Crop the texture
        Texture2D croppedTexture = new Texture2D(cropWidth, cropHeight);
        croppedTexture.SetPixels(texture2D.GetPixels(xMin, yMin, cropWidth, cropHeight));
        croppedTexture.Apply();

        // Resize to target dimensions
        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // Sample color from cropped texture (bilinear)
                float u = (float)x / targetWidth;
                float v = (float)y / targetHeight;
                Color color = croppedTexture.GetPixelBilinear(u, v);
                resizedTexture.SetPixel(x, y, color);
            }
        }

        resizedTexture.Apply();
        return resizedTexture;
    }

    public virtual void AfterPickImagePicked()
    {


        master.settings.SetCardBackTexture(index);
        //Debug.Log("|||||||||||||||||||||||||||||||||||||||||| index: " + index);

        SaveIamge("BOBOsmall.png");
        base.Select();
    }

    public void SaveIamge(string name)
    {
        byte[] bytes = image.sprite.texture.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, name);
        File.WriteAllBytes(filePath, bytes);
        //Debug.Log("Saved image to: " + filePath);
    }

    public void LoadOldImage(string name)
    {
        //Debug.Log("|||||||||||||||| LOAD OLD IMAGE");
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
            //Debug.Log("Loaded image from: " + filePath);
            //master.mainImage.sprite = Sprite.Create((Texture2D)image.texture, new Rect(0, 0, 250, 350), new Vector2(0.5f, 0.5f));
        }
        else
        {
            //Debug.LogWarning("Image file not found: " + filePath);
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