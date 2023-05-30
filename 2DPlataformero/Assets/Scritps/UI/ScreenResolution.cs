using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    public int width = 1065;
    public int height = 600;
    public bool fullScreen = false;

    private void Start()
    {
        SetResolution(width, height);
    }

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, fullScreen);
    }
}
