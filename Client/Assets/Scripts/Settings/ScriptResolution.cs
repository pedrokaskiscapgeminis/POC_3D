using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ScriptResolution : MonoBehaviour
{

    public TMP_Dropdown resolutionsDropDown;
    Resolution[] resolutions;

    void Start(){
        checkResolution();
  
    }


     public void onChange()
    {
        
        Screen.fullScreen = !Screen.fullScreen;
     
    }

    public void checkResolution()
    {
        resolutions = Screen.resolutions;
        resolutionsDropDown.ClearOptions();
        List<string> options = new List<string>();
        int actualResolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + "@" + resolutions[i].refreshRate;
            options.Add(option);


            if (Screen.fullScreen && resolutions[i].width == Screen.currentResolution.width &&resolutions[i].height == Screen.currentResolution.height)
            {
                actualResolution = i;
            }
        }

        resolutionsDropDown.AddOptions(options);
        resolutionsDropDown.value = actualResolution;
        resolutionsDropDown.RefreshShownValue();
    }

    public void CambiarResolucion(int indiceResolution)
    {
        Resolution resolution = resolutions[indiceResolution];
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen);
    }
}
