using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptBrightness : MonoBehaviour
{
   public Slider slider;
    public float sliderValue;
     public float GammaCorrection;
  

    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.ambientLight = new Color(GammaCorrection, GammaCorrection, GammaCorrection, 0.5f);
    }

    //Update is called once per frame
    void Update()
    {

    }

    public void ChangeSlider(float value)
    {

        GammaCorrection = value;
       
    }
}
