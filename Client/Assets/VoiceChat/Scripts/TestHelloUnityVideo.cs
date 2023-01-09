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
    public Sprite MicroOff = Resources.Load<Sprite>("Sprites/micro_off");
    public Sprite MicroOn = Resources.Load<Sprite>("Sprites/micro_on");

    // Instance of agora engine
    private IRtcEngine mRtcEngine { get; set; }

    public AudioVideoStates AudioVideoState = new AudioVideoStates();
    
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

        AudioVideoState.pubAudio = !muted;
        mRtcEngine.JoinChannel(token: null, channelId: channel, info: "", uid: 0,
                options: new ChannelMediaOptions(AudioVideoState.subAudio,
                AudioVideoState.pubAudio));
        mRtcEngine.MuteLocalAudioStream(muted);
    }

    public void leave()
    {
        Debug.Log("calling leave");

        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
    }

    /// <summary>
    ///   Unload agora engine
    /// </summary>
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

    /// <summary>
    ///   Changes the state of the audio
    /// </summary>
    public void MuteAudio(string activeScene){
        if (AudioVideoState.pubAudio == true){
            mRtcEngine.MuteLocalAudioStream(true);
            AudioVideoState.pubAudio = false;
            Debug.Log("MicroOff");
            
            if (activeScene != "Lobby")
            GameObject.Find("Micro").GetComponent<Image>().sprite = MicroOff;
        } else {
            mRtcEngine.MuteLocalAudioStream(false);
            AudioVideoState.pubAudio = true;
            Debug.Log("MicroOn");

            if (activeScene != "Lobby")
            GameObject.Find("Micro").GetComponent<Image>().sprite = MicroOn;
        }
    }

    /// <summary>
    ///   Implement engine callbacks
    /// </summary>
    private void onJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("JoinChannel " + channelName + " Success: uid = " + uid);
    }

    /// <summary>
    ///   When a remote user joined, this delegate will be called.
    /// </summary>
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

        LastError = error;
    }

    #endregion
}
