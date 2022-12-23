using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using agora_gaming_rtc;

public class EventDetection : MonoBehaviour
{
    // [SerializeField]
    private Button button;
    // private IRtcEngine mRtcEngine { get; set; }
    // private AudioVideoStates AudioVideoState = new AudioVideoStates();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("m")){
            Debug.Log("M WAS PRESSED");
            button.onClick.Invoke();
            //ExecuteEvents.Execute(button.gameObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
        }
    }
}
