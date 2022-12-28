using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class ScriptBrightness : MonoBehaviour
{
   public Slider slider;

   public PostProcessProfile brightness;
   public PostProcessLayer layer;

   AutoExposure exposure;
  

    // Start is called before the first frame update
    void Start()
    {
        brightness.TryGetSettings(out exposure);
        exposure.keyValue.value = 0.75f;
   
    }

    //Update is called once per frame
   
    public void AdjustBrightness(float value)
    {

       if(value != 0)
       {
        exposure.keyValue.value = value;
       }
      
       
    }
}
