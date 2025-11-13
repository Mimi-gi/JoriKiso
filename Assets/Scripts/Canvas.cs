using Unity.VisualScripting;
using UnityEngine;

public class CanvasRect : MonoBehaviour
{
    RectTransform canvas;
    public static RectTransform Main;
    void Awake()
    {
        canvas = GetComponent<RectTransform>();
        Main = canvas;
    }
}
