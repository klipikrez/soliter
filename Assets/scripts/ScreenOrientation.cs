using System;
using UnityEngine;


public class ScreenOrientation : MonoBehaviour
{
    public RectTransform uiVertical;
    public RectTransform uiHorizontal;
    public RectTransform columnContainer;
    public bool isVertical = true;
    public static ScreenOrientation instance;
    public static event Action OnRotateScreen;

    void Awake()
    {
        instance = this;

    }
    void Start()
    {

        CheckOrientation(true);


    }
    void Update()
    {
        //  Debug.Log("yes??");
        CheckOrientation();
    }

    void CheckOrientation(bool forceUpdate = false)
    {
        // Debug.Log((float)Screen.height / Screen.width);
        if ((float)Screen.height / Screen.width > 1.1f)
        {
            SetVertical(forceUpdate);
        }
        else
        {
            SetHorizontal(forceUpdate);
        }
    }

    void SetVertical(bool forceUpdate = false)
    {
        if (isVertical) return;

        uiVertical.gameObject.SetActive(true);
        uiHorizontal.gameObject.SetActive(false);

        SetColumnContainerDistanceFromTop(-460.4095f);
        SetColumnContainerDistanceFromRight(0);
        // Debug.Log("vertical");
        isVertical = true;
        Canvas.ForceUpdateCanvases();
        OnRotateScreen.Invoke();
    }

    void SetHorizontal(bool forceUpdate = false)
    {
        if (!isVertical) return;

        uiVertical.gameObject.SetActive(false);
        uiHorizontal.gameObject.SetActive(true);

        SetColumnContainerDistanceFromTop(0);
        SetColumnContainerDistanceFromRight(-200f);
        //Debug.Log("horizontal");
        isVertical = false;
        Canvas.ForceUpdateCanvases();
        OnRotateScreen.Invoke();
    }

    void SetColumnContainerDistanceFromTop(float distance)
    {
        columnContainer.sizeDelta = new Vector2(columnContainer.sizeDelta.x, distance);
        columnContainer.anchoredPosition = new Vector2(columnContainer.anchoredPosition.x, distance / 2);
    }

    void SetColumnContainerDistanceFromRight(float distance)
    {
        columnContainer.sizeDelta = new Vector2(distance, columnContainer.sizeDelta.y);
        columnContainer.anchoredPosition = new Vector2(distance / 2, columnContainer.anchoredPosition.y);
    }

}

