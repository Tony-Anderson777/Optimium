using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Cube_Color : MonoBehaviour
{
    private Color neutralRatioColor;
    private Color bordeauRatioColor;
    private Color greenRatioColor;
    private Color orangeRatioColor;
    private Color redRatioColor;
    public float ratio = -1.0f;

    void Awake()
    {
        this.redRatioColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        this.orangeRatioColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        this.greenRatioColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        this.bordeauRatioColor = new Color(109f / 255f, 7.0f / 255f, 26.0f / 255.0f, 1.0f);
        this.neutralRatioColor = new Color(200.0f / 255.0f, 196.0f / 255.0f, 220.0f / 255.0f, 1.0f);
        colorStatic();
    }

    void Update()
    {
        colorCube();
    }

    public void colorStatic()
    {
        GetComponent<Renderer>().material.color = neutralRatioColor;
    }

    public void colorCube()
    {
        if (this.ratio == -1.0f)
        {
            GetComponent<Renderer>().material.color = neutralRatioColor;
        }
        else if (this.ratio > 1.0f)
        {
            GetComponent<Renderer>().material.color = bordeauRatioColor;
        }
        else if (this.ratio < 1.0f / 3.0f)
        {
            GetComponent<Renderer>().material.color = redRatioColor;
        }
        else if (this.ratio >= 1.0f / 3.0f && this.ratio < 2.0f / 3.0f)
        {
            GetComponent<Renderer>().material.color = orangeRatioColor;
        }
        else if (this.ratio >= 2.0f / 3.0f && this.ratio <= 1.0f)
        {
            GetComponent<Renderer>().material.color = greenRatioColor;
        }
    }
}