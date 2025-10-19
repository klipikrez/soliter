using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Background : MonoBehaviour
{
    Color color = Color.white;
    public Material shader;
    Vector2[] range = { new Vector2(0.4f, 0.5f), new Vector2(-0.5f, -0.4f) };

    public static Background Instance;
    private void Start()
    {
        Instance = this;
    }

    public void SetBackgroundColor(Color col)
    {
        color = col;
        shader.SetColor("col", col);
        shader.SetTexture("tex", null);
    }

    public void SetBackgroundTexture(Texture tex)
    {
        color = Color.white;
        shader.SetColor("col", Color.white);
        shader.SetTexture("tex", tex);

    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("HOHOHOHOO");
            Flash(1);
        }
        shader.SetColor("col", Color.Lerp(shader.GetColor("col"), color, Time.deltaTime));
    }

    public void Flash(float intensety)
    {
        if (shader.GetTexture("tex") == null)
            shader.SetColor("col", shader.GetColor("col") + new Color(UnityEngine.Random.Range(ColorDark() ? range[0].x : range[1].x, ColorDark() ? range[0].y : range[1].y), UnityEngine.Random.Range(ColorDark() ? range[0].x : range[1].x, ColorDark() ? range[0].y : range[1].y), UnityEngine.Random.Range(ColorDark() ? range[0].x : range[1].x, ColorDark() ? range[0].y : range[1].y)) * intensety);
        else
            shader.SetColor("col", shader.GetColor("col") + new Color(UnityEngine.Random.Range(range[0].y, range[1].x), UnityEngine.Random.Range(range[0].y, range[1].x), UnityEngine.Random.Range(range[0].y, range[1].x)) * intensety);

    }

    bool ColorDark()
    {
        Color color = shader.GetColor("col");
        float f = (color.r * 0.3f + color.g * 0.59f + color.b * 0.11f) - 0.5f;
        return f < 0;
    }

    float easeOutElastic(float x)
    {
        float c4 = (2 * math.PI) / 3;

        return x == 0
          ? 0
          : x == 1
          ? 1
          : math.pow(2, -10 * x) * math.sin((x * 10 - 0.75f) * c4) + 1;
    }

}
