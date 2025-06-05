using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorPreview : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("previewGraphic")] private Graphic _previewGraphic;
    [SerializeField, FormerlySerializedAs("colorPicker")] private ColorPicker _colorPicker;

    public void SetColor(Color c)
    {
        _colorPicker.color = c;
    }

    private void Start()
    {
        _colorPicker.onColorChanged += OnColorChanged;
        OnColorChanged(_colorPicker.color);

        SetColor(Color.cyan);
    }

    public void OnColorChanged(Color c)
    {
        _previewGraphic.color = c;
    }

    private void OnDestroy()
    {
        if (_colorPicker != null)
            _colorPicker.onColorChanged -= OnColorChanged;
    }
}