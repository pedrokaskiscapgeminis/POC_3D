using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using agora_gaming_rtc;
using agora_utilities;


// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class TestHelloUnityVideo 
{
    //Micro Sprites
    // public Sprite MicroOff = Resources.Load<Sprite>("Sprites/micro_off");
    // public Sprite MicroOn = Resources.Load<Sprite>("Sprites/micro_on");

    // instance of agora engine
    private IRtcEngine mRtcEngine { get; set; }
    private Text MessageText { get; set; }

    private AudioVideoStates AudioVideoState = new AudioVideoStates();

    // private string mChannelName { get; set; }
    //private Text ChannelNameLabel { get; set; }

    private ToggleStateButton MuteAudioButton { get; set; }
    
    // Testing Volume Indication
    private bool TestVolumeIndication = false;

    // load agora engine
    public void loadEngine(string appId)
    {
        // start sdk
        Debug.Log("initializeEngine");

        if (mRtcEngine != null)
        {
            Debug.Log("Engine exists. Please unload it first!");
            return;
        }

        // init engine
        mRtcEngine = IRtcEngine.GetEngine(appId);

        // enable log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }

    // public void SetupInitState()
    // {
    //     AudioVideoState.pubAudio = true;
    //     AudioVideoState.subAudio = true;
    // }

    /// <summary>
    ///    Joining a channel as a host. This is same as running in Communication mode for other hosts.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="muted"></param>
    public void join(string channel, bool muted = false)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // SetupInitState();

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
        mRtcEngine.OnUserJoined = onUserJoined;
        mRtcEngine.OnWarning = (int warn, string msg) =>
        {
            Debug.LogWarningFormat("Warning code:{0} msg:{1}", warn, IRtcEngine.GetErrorDescription(warn));
        };
        mRtcEngine.OnError = HandleError;

        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Turn this on to receive volumenIndication
        if (TestVolumeIndication)
        {
            mRtcEngine.EnableAudioVolumeIndication(500, 8, report_vad: true);
        }

        // NOTE, we use the third button to invoke JoinChannelByKey
        // otherwise, it joins using JoinChannelWithUserAccount
        if (muted)
        {
            AudioVideoState.pubAudio = false;

            // mute locally only. still subscribing
            mRtcEngine.EnableLocalAudio(false);
            mRtcEngine.MuteLocalAudioStream(true);
            mRtcEngine.JoinChannelByKey(channelKey: null, channelName: channel, info: "", uid: 0);
        }
        else
        {
            // join channel with string user name
            // ************************************************************************************* 
            // !!!  There is incompatibiity with string Native UID and Web string UIDs !!!
            // !!!  We strongly recommend to use uint uid only !!!!
            // mRtcEngine.JoinChannelWithUserAccount(null, channel, "user" + Random.Range(1, 99999),
            // ************************************************************************************* 
            AudioVideoState.pubAudio = true;
            mRtcEngine.JoinChannel(token: null, channelId: channel, info: "", uid: 0,
                 options: new ChannelMediaOptions(AudioVideoState.subAudio,
                 AudioVideoState.pubAudio));
        }

        // mChannelName = channel;
    }

    public string getSdkVersion()
    {
        string ver = IRtcEngine.GetSdkVersion();
        return ver;
    }

    public void leave()
    {
        Debug.Log("calling leave");

        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
    }

    // unload agora engine
    public void unloadEngine()
    {
        Debug.Log("calling unloadEngine");

        // delete
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }

    // accessing GameObject in Scene1
    // set video transform delegate for statically created GameObject
    public void onSceneHelloVideoLoaded()
    {
        GameObject text = GameObject.Find("MessageText");
        if (!ReferenceEquals(text, null))
        {
            MessageText = text.GetComponent<Text>();
        }

        SetButtons();
    }
    private void SetButtons()
    {
        MuteAudioButton = GameObject.Find("MuteButton").GetComponent<ToggleStateButton>();
        if (MuteAudioButton != null)
        {
            MuteAudioButton.Setup(initOnOff: false,
                onStateText: "", offStateText: "",
                callOnAction: () =>
                {
                    mRtcEngine.MuteLocalAudioStream(true);
                    AudioVideoState.pubAudio = false;
                    Debug.Log(AudioVideoState.pubAudio);
                    // GameObject.Find("MuteButton").GetComponent<Image>().sprite = MicroOff;
                },
                callOffAction: () =>
                {
                    mRtcEngine.MuteLocalAudioStream(false);
                    AudioVideoState.pubAudio = true;
                    Debug.Log(AudioVideoState.pubAudio);
                    // GameObject.Find("MuteButton").GetComponent<Image>().sprite = MicroOn; 
                }
            );
        }

        //ChannelNameLabel = GameObject.Find("ChannelName").GetComponent<Text>();
    }

    // implement engine callbacks
    private void onJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("JoinChannel " + channelName + " Success: uid = " + uid);
        GameObject textVersionGameObject = GameObject.Find("VersionText");
        textVersionGameObject.GetComponent<Text>().text = "SDK Version : " + getSdkVersion();
        //ChannelNameLabel.text = channelName;
    }

    // When a remote user joined, this delegate will be called. 
    private void onUserJoined(uint uid, int elapsed)
    {
        Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
    }

    #region Error Handling
    private int LastError { get; set; }
    private void HandleError(int error, string msg)
    {
        if (error == LastError)
        {
            return;
        }

        msg = string.Format("Error code:{0} msg:{1}", error, IRtcEngine.GetErrorDescription(error));

        switch (error)
        {
            case 101:
                msg += "\nPlease make sure your AppId is valid and it does not require a certificate for this demo.";
                break;
        }

        Debug.LogError(msg);
        if (MessageText != null)
        {
            if (MessageText.text.Length > 0)
            {
                msg = "\n" + msg;
            }
            MessageText.text += msg;
        }

        LastError = error;
    }

    #endregion
}
