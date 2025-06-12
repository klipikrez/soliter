using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Events;
public class ColorPreview : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("previewGraphic")] private Graphic _previewGraphic;
    [SerializeField, FormerlySerializedAs("colorPicker")] private ColorPicker _colorPicker;
    public UnityEvent<Color> myEvent;
    int use = 0;
    public void SetColor(Color c)
    {
        _colorPicker.color = c;
    }

    public void Inicialize()
    {
        _colorPicker.onColorChanged += OnColorChanged;
        OnColorChanged(_colorPicker.color);

        //SetColor(Color.cyan);
    }




    public void OnColorChanged(Color c)
    {
        if (use <= 1) { use++; return; }
        _previewGraphic.color = c;
        myEvent.Invoke(c);

    }

    private void OnDestroy()
    {
        if (_colorPicker != null)
            _colorPicker.onColorChanged -= OnColorChanged;
    }
}