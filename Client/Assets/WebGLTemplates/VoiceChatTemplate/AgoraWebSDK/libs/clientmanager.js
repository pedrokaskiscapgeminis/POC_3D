class ClientManager {
  constructor() {
    this.client = undefined; // first client
    this.options = {
      appid: null,
      channel: null,
      uid: null,
      token: null,
    };
    this.videoEnabled = false; // if true then camera is created, if false then not
    this.audioEnabled = false; // if true then mic access is created, if false then not
    this.videoSubscribing = true; 
    this.audioSubscribing = true; 
    this.remoteUserAudioMuted = {};
    this.remoteUserVideoMuted = {};
    this.client_role = 1; // default is host, 2 is audience
    this._storedChannelProfile = 0; // channel profile saved before join is called
    this._inChannel = false;
    this._streamMessageRetry = false;
    this.is_screensharing = false;
    this.tempLocalTracks = null;
    this.enableLoopbackAudio = false;
    this.virtualBackgroundProcessor = null;
    this.spatialAudio = undefined;
    this._customVideoConfiguration = {
      bitrateMax:undefined,
      bitrateMin:undefined,
      optimizationMode:"detail",
      frameRate:undefined,
      width:undefined,
      height:undefined
    };
    this.userJoinedHandle = this.handleUserJoined.bind(this);
    this.userPublishedHandle = this.handleUserPublished.bind(this);
    this.userUnpublishedHandle = this.handleUserUnpublished.bind(this);
    this.userLeftHandle = this.handleUserLeft.bind(this);
    this.userExceptionHandle = this.handleException.bind(this);
    this.userErrorHandle = this.handleError.bind(this);
    this.userVolumeHandle = this.handleVolumeIndicator.bind(this);
    this.userStreamHandle = this.handleStreamMessage.bind(this);
    this.userInfoUpdateHandler = this.handleUserInfoUpdate.bind(this);
    this.userTokenWillExpireHandle = this.handleTokenPrivilegeWillExpire.bind(this);
    this.userTokenDidExpireHandle = this.handleTokenPrivilegeDidExpire.bind(this);
  }

  manipulate() {}

  setVideoEnabled(enabled) {
    // not publishing if it is Live Audience
    this.videoEnabled = this.client_role == 2 ? false : enabled;
    this.videoSubscribing = enabled;
  }

  getMainAppId() {
    return this.options.appid;
  }

  getClient() {
    return this.client;
  }

  setChannelProfile(profile) {
    _logger("in client manager, setChannelProfile: " + profile);
    this._storedChannelProfile = profile;
    var mode = this.getChannelProfileMode();
    this.client = null;
    _logger("created new client");
    this.client = AgoraRTC.createClient({ mode: mode, codec: "vp8" });
    if (mode == "live") {
      this.client_role = 1;
      setClientMode_LIVE(); // let multichannel know
    } // else default is "rtc"
  }

  getChannelProfileMode = function () {
    _logger("in getChannelProfileMode client manager");
    if (this._storedChannelProfile == 0) {
      return "rtc";
    } else if (this._storedChannelProfile == 1) {
      return "live";
    }
  };

  setOptions(channelkey, channelName, uid) {
    this.options.token = channelkey;
    this.options.channel = channelName;
    this.options.uid = uid;
  }

  setAVControl(subAudio, subVideo, pubAudio, pubVideo) {
    this.audioEnabled = pubAudio;
    this.videoEnabled = pubVideo;
    this.audioSubscribing = subAudio;
    this.videoSubscribing = subVideo;
  }

  createEngine(appID) {
    if (this.client == undefined) {
      var mode = this.getChannelProfileMode();
      wrapper.log("using mode: " + mode);
      this.client = AgoraRTC.createClient({ mode: mode, codec: "vp8" });
      this.options.appid = appID;

      wrapper.setup(this.client);
      // audioEffects.initialize(this.client);
      cacheDevices();

      return true;
    } else {
      wrapper.setup(this.client);
      // audioEffects.initialize(this.client);
      cacheDevices();
      return false;
    }
  }

  createEngine2(appID, areaCode) {
    var AreaCodeValue;
    switch(areaCode) {
      case 1:
        AreaCodeValue = "CHINA";
        break;
      case 2:
        AreaCodeValue = "NORTH_AMERICA";
        break;
      case 4:
        AreaCodeValue = "EUROPE";
        break; 
      case 8:
        AreaCodeValue = "ASIA";
        break; 
      case 0x10:
        AreaCodeValue = "JAPAN";
        break; 
      case 0x20:
        AreaCodeValue = "INDIA";
        break; 
      default:
        AreaCodeValue = "GLOBAL";
    }
    AgoraRTC.setArea({
      areaCode:AreaCodeValue
    });
    return this.createEngine(appID);
  }

  // called after user published event
  // subscribe to the user and raise OnRemoteUserJoined
  async subscribe_remoteuser(user, mediaType) {
    var strUID = user.uid.toString();
    // subscribe to a remote user
    if (remoteUsers[user.uid] === null ||
      user.hasVideo && mediaType === "video" && 
      (this.remoteUserVideoMuted[user.uid] == null 
        || this.remoteUserVideoMuted[user.uid] == false) ||
      user.hasAudio && mediaType === "audio" && 
      (this.remoteUserAudioMuted[user.uid] == null 
        || this.remoteUserAudioMuted[user.uid] == false)) {
      await this.client.subscribe(user, mediaType);
    }
    if (mediaType === "video" && user.hasVideo && user.videoTrack && !this.remoteUserVideoMuted[user.uid]) {
      user.videoTrack.play(`player-${strUID}`, { fit: "cover", mirror: mremote });
      if (remoteUsers[user.uid] == null)
      {
        event_manager.raiseOnRemoteUserJoined(strUID);
      } 
    } else {
      if (mediaType === "audio" && user.hasAudio && user.audioTrack && !this.remoteUserAudioMuted[user.uid]) {
        user.audioTrack.play();
        // for Voice only subscription only, the raise won't happen above
        if (remoteUsers[user.uid] == null) {
          event_manager.raiseOnRemoteUserJoined(strUID);
        }
      }
    }
  }

  //============================================================================== 
  // Event Handlers
  //============================================================================== 
  async handleUserPublished(user, mediaType) {
    const id = user.uid;
    if (this.audioSubscribing && mediaType == "audio" && (mediaType == "audio" && this.screenShareClient == null
      || mediaType == "audio" && this.screenShareClient != null
      && id != this.screenShareClient.uid)) {
      await this.subscribe_remoteuser(user, mediaType);
    } else if(this.videoSubscribing && mediaType == "video" && remoteUsers) {
      await this.subscribe_remoteuser(user, mediaType);
      event_manager.raiseOnRemoteUserMuted(id.toString(), mediaType, 0);
      this.getRemoteVideoStats(id);
    }
    remoteUsers[id] = user;
  }

  // Note this event doesn't truly map to Unity's OnUserJoined
  // since it can called twice based on mediaType
  // see the event raised in subscribe_remoteuser instead
  handleUserJoined(user, mediaType) {
    const id = user.uid;
    console.log("remote user id" , id);

    if(this.spatialAudio !== undefined && this.spatialAudio.enabled === true){
      this.enableSpatialAudio(true, user);
    }
  }

  handleUserUnpublished(user, mediaType) {
    const id = user.uid;
    // delete remoteUsers[id];
    // $(`#player-wrapper-${id}`).remove();
    var strUID = id.toString();
    event_manager.raiseOnRemoteUserMuted(strUID, mediaType, 1);
  }

  handleUserLeft(user, reason) {
    const id = user.uid;
    delete remoteUsers[id];
    $(`#player-wrapper-${id}`).remove();
    var strUID = id.toString();
    var rcode = 0; // QUIT
    if (reason === "ServerTimeOut") {
      rcode = 1; //DROPPED
    } else if (reason === "BecomeAudience") {
      rcode = 2;
    }

    if(this.spatialAudio !== undefined){
      this.spatialAudio.localPlayerStop(user);
    }

    event_manager.raiseOnRemoteUserLeaved(strUID, rcode); 
  }

  // Note that the "enable-local-video" and "disable-local-video" states 
  // are only for synchronizing states with the clients that integrate the RTC Native SDK.
  handleUserInfoUpdate(uid, msg) {
    const strUID = uid.toString();
    switch(msg) {
      case "mute-audio" :
        event_manager.raiseOnRemoteUserMuted(strUID, "audio", 1);
        break;
      case  "mute-video" : 
        event_manager.raiseOnRemoteUserMuted(strUID, "video", 1);
        break; 
      case "enable-local-video" : 
        break;
      case "unmute-audio" : 
        event_manager.raiseOnRemoteUserMuted(strUID, "audio", 0);
        break;
      case "unmute-video" : 
        event_manager.raiseOnRemoteUserMuted(strUID, "video", 0);
        break;
      case "disable-local-video" :
        break;
    }
  }

  async handleVolumeIndicator(result) {
    var total = 0;
    var count = result.length;
    var info = "";
    const vad = 0;
    result.forEach(function(volume, index){
      console.log(`${index} UID ${volume.uid} Level ${volume.level}`);

      if (volume.level > total) {
        total = volume.level;
      }
      var level = volume.level.toFixed();
      const channel = client_manager.options.channel;
      info += `\t${volume.uid}\t${level}\t${vad}\t${channel}`;
    });
    event_manager.raiseVolumeIndicator(info, count, total.toFixed());
  }

  async handleStreamMessage(uid, data) {
    // const str = utf8ArrayToString(data);
    UnityHooks.InvokeStreamMessageCallback(uid, data, data.length);
  }

  handleException(e) {
    console.log(e);
  }

  handleError(e) {
    console.log(e);
  }
  //============================================================================== 

  async leave() {
    for (var trackName in localTracks) {
      var track = localTracks[trackName];
      if (track) {
        if(!Array.isArray(track)){
          track.stop();
          track.close();
          localTracks[trackName] = null;
        } else {
          for(var i = 0; i < track.length; i++){
            track[i].stop();
            track[i].close();
          }
          localTracks[trackName] = null;
        }
      }
    }
    this.audioEnabled = false; // set to default

    // audioEffects._stopAllEffects();

    wrapper.destructStats();

    // remove remote users and player views
    remoteUsers = {};
    this.remoteUserAudioMuted = {};
    this.remoteUserVideoMuted = {};

    // leave the channel
    await this.client.leave();
    this.client.off("user-published", this.userPublishedHandle);
    this.client.off("user-joined", this.userJoinedHandle);
    this.client.off("user-left", this.userLeftHandle);
    //unpublish is used to track mute/unmute, it is recommended to use UserInfoUpdate instead
    this.client.off("user-unpublished", this.userUnpublishedHandle);
    this.client.off("exception", this.userExceptionHandle);
    this.client.off("error", this.userErrorHandle);
    this.client.off("user-info-updated", this.userInfoUpdateHandler);
    this.client.off("volume-indicator", this.userVolumeHandle);
    this.client.off("stream-message", this.userStreamHandle);
    event_manager.raiseOnLeaveChannel();

    this._inChannel = false;
  }


  async switchChannel(token_str, channelId_str) {
    const enableVideoBefore = this.videoEnabled;
    const enableAudioBefore = this.audioEnabled;
    await this.leave();
    this.options.token = token_str;
    this.options.channel = channelId_str;
    this.options.uid = await Promise.all([
      this.client.join(
        this.options.appid,
        this.options.channel,
        this.options.token || null
      )
    ]);
    // restore a/v enable
    this.videoEnabled = enableVideoBefore;
    this.audioEnabled = enableAudioBefore;
    await this.processJoinChannelAVTrack();
  }

  // check if this is a host
  isHosting() {
    if (this._storedChannelProfile == 0) { return true; }
    if (this._storedChannelProfile == 1) {
	    return (this.client_role == 1);
    }
    return false;
  }

  async handleStopNewScreenShare(){
    stopNewScreenCaptureForWeb();
  }

  async handleStartNewScreenShare(){
    event_manager.raiseEngineScreenShareStarted(this.options.channel, this.options.uid);
  }

  async handleStartScreenShare(){
    event_manager.raiseEngineScreenShareStarted(this.options.channel, this.options.uid);
  }

  async handleStopScreenShare(){
    stopScreenCapture();
  }

  async handleStopNewScreenShare(){
    stopNewScreenCaptureForWeb();
  }

  async handleTokenPrivilegeWillExpire(){
    event_manager.raiseOnTokenPrivilegeWillExpire(this.options.token)
  }

  async handleTokenPrivilegeDidExpire(){
    event_manager.raiseOnTokenPrivilegeDidExpire(this.options.token)
  }

  //============================================================================== 
  // . JOIN CHANNEL METHOD 
  // Params: user - can be either string or uint
  //============================================================================== 
  async joinAgoraChannel(user)
  {
    
    if(localTracks.videoTrack != null){
      localTracks.videoTrack.stop();
      localTracks.videoTrack.close();
      localTracks.videoTrack = null;
    }

    this.client.on("user-published", this.userPublishedHandle);
    this.client.on("user-joined", this.userJoinedHandle);
    this.client.on("user-left", this.userLeftHandle);
    //unpublish is used to track mute/unmute, it is recommended to use UserInfoUpdate instead
    this.client.on("user-unpublished", this.userUnpublishedHandle);
    this.client.on("exception", this.userExceptionHandle);
    this.client.on("error", this.userErrorHandle);
    this.client.on("user-info-updated", this.userInfoUpdateHandler);
    this.client.on("volume-indicator", this.userVolumeHandle);
    this.client.on("stream-message", this.userStreamHandle);
    this.client.on("token-privilege-will-expire", this.userTokenWillExpireHandle);
    this.client.on("token-privilege-did-expire", this.userTokenDidExpireHandle);

    if (typeof(user) == "string") {
	    user = 0; // let system assign uid
    }

    [this.options.uid] = await Promise.all([
      this.client.join(
        this.options.appid,
        this.options.channel,
        this.options.token || null,
        user || null
      ).catch(error => {
        event_manager.raiseHandleUserError(error.code, error.message);
      }),
    ])

    this._inChannel = true;
    await this.processJoinChannelAVTrack();

    event_manager.raiseJoinChannelSuccess(
      this.options.uid.toString(),
      this.options.channel
    );
  }

  // Help function for JoinChannel
  async processJoinChannelAVTrack() {  
    // if (this.videoEnabled && this.isHosting()) {
    //   [localTracks.videoTrack] = await Promise.all([
    //     AgoraRTC.createCameraVideoTrack(this._customVideoConfiguration).catch(
    //       e => {
    //         event_manager.raiseHandleUserError(e.code, e.message);
    //       }
    //     )
    //   ]);
    //   if (localTracks.videoTrack) {
    //     currentVideoDevice = wrapper.getCameraDeviceIdFromDeviceName(
    //       localTracks.videoTrack._deviceName
    //     );
    //   }
    // }

    if (this.audioEnabled && this.isHosting()) {
      [localTracks.audioTrack] = await Promise.all([
        AgoraRTC.createMicrophoneAudioTrack()
      ]);
      currentAudioDevice = wrapper.getMicrophoneDeviceIdFromDeviceName(
          localTracks.audioTrack._deviceName
      );
    }
    // event_manager.raiseGetCurrentVideoDevice();
    event_manager.raiseGetCurrentAudioDevice();
    event_manager.raiseGetCurrentPlayBackDevice();

    // videoTrack exists implies videoEnabled
    if (localTracks.videoTrack) {
      localTracks.videoTrack.play("local-player", {
        fit: "cover",
        mirror: mlocal,
      });
    }

    $("#local-player-name").text(`localVideo(${this.options.uid})`);
    if (this.isHosting() && this._inChannel) {
      for (var trackName in localTracks) {
        var track = localTracks[trackName];
        if (track) {
          await this.client.publish(track);
        }
      }
    }
  } 

  async setClientRole(role, optionLevel) {
    if (this.client) {
      var wasAudience = (this.client_role == 2);
      this.client_role = role;
      if (role === 1) {
        await this.client.setClientRole("host", optionLevel);
        event_manager.raiseOnClientRoleChanged("2", "1");
        if (wasAudience) {
          await this.processJoinChannelAVTrack();
        }
      } else if (role === 2) {
        await this.unpublishAll();
        await this.client.setClientRole("audience", optionLevel);
        event_manager.raiseOnClientRoleChanged("1", "2");
      }
    }
  }

  async enableAudio(enabled) {
    this.audioEnabled = enabled;
    this.audioSubscribing = enabled;
  }

// Disables/Re-enables the local audio function.
  async enableLocalAudio(enabled) {
    if (enabled == false) {
      if (localTracks.audioTrack) {
        localTracks.audioTrack.setVolume(0);
      }
    } else {
      if (localTracks.audioTrack) {
        localTracks.audioTrack.setVolume(100);
      }
    }
    this.audioEnabled = enabled;
  }

  // mute the stream meaning unpublish, but local display 
  // can still be on
  // if wanting both off, call disableLocalVideo
  async muteLocalVideoStream(mute) {
    if (localTracks.videoTrack) {
      if (mute) {
        await this.client.unpublish(localTracks.videoTrack);
      } else {
        await this.client.publish(localTracks.videoTrack);
      }
    }
  }

  async muteLocalAudioStream(mute) {
    if (mute) {
      if (localTracks.audioTrack) {
        await this.client.unpublish(localTracks.audioTrack);
      }
    } else {
      if (localTracks.audioTrack) {
        await this.client.publish(localTracks.audioTrack);
      }
    }
  }

  async renewToken(token) {
    if (this.client) {
      await this.client.renewToken(token);
    }
  }

  enableAudioVolumeIndicator() {
    if (this.client) {
      this.client.enableAudioVolumeIndicator();
    }
  }

  setEncryptionSecret(secret) {
    if (this.client) {
      if (savedEncryptionMode != "") {
        this.client.setEncryptionConfig(savedEncryptionMode, secret);
      } else {
        console.log("Please call setEncryptionMode first");
      }
    }
  }

  async setExternalAudioSource_WGL(enabled, sampleRate, channels) {
    if (enabled == 1) {
      localTracks.audioTrack.stop();
      localTracks.audioTrack.close();

      await this.client.unpublish(localTracks.audioTrack);
      dest = audioCtx.createMediaStreamDestination();

      var CustomVideoTrackInitConfig = {
        mediaStreamTrack: dest.stream.getAudioTracks()[0],
      };

      [localTracks.audioTrack] = await Promise.all([
        AgoraRTC.createCustomAudioTrack(CustomVideoTrackInitConfig),
      ]);

      // play local video track
      await this.client.publish(localTracks.audioTrack);

      playMusc();
    } else if (enabled == 0) {
      localTracks.audioTrack.stop();
      localTracks.audioTrack.close();
      play = false;

      await this.client.unpublish(localTracks.audioTrack);

      [localTracks.audioTrack] = await Promise.all([
        AgoraRTC.createMicrophoneAudioTrack(),
      ]);

      await this.client.publish(localTracks.audioTrack);
    }
  }

  // Starts the last-mile network probe test.
  enableLastMile(enabled) {
    if (enabled == true) {
      this.client.getRemoteNetworkQuality();
      this.client.on("network-quality", this.handleNetworkQuality);
    } else {
      this.client.off("network-quality", this.handleNetworkQuality);
    }
  }

  handleNetworkQuality(quality) {
    event_manager.raiseLastMileQuality(quality);
  }

  async subscribe_mv(user, mediaType) {
    if (mediaType === "video" && user.hasVideo ||
      mediaType === "audio" && user.hasAudio) {
      try {
        await this.client.subscribe(user, mediaType);
        if (mediaType === "video") {
          user.videoTrack.play(`player-${user.uid}`);
        } else if (mediaType === "audio") {
          user.audioTrack.play();
        }
      } catch (error) {
        console.log("subscribe error ", error);
      }
    }
  }

  async unsubscribe(user, mediaType) {
    if (mediaType === "video" && user.hasVideo ||
      mediaType === "audio" && user.hasAudio) {
      try {
        await this.client.unsubscribe(user, mediaType);
      } catch (error) {
        console.log("unsubscribe error ", error);
      }
    }
  }

  async unpublishAll() {
    for (var trackName in localTracks) {
      var track = localTracks[trackName];
      if (track) {
        await this.client.unpublish(track);
      }
    }
  }

  // Sets the stream type of the remote video
  async SetRemoteVideoSTreamType(usersIdStream, streamType) {
    try {
      await this.client.setRemoteVideoStreamType(usersIdStream, streamType);
    } catch (error) {
      console.log("error from SetRemoteVideoSTreamType", error);
    }
  }

  async enableDualStream_WGL(enabled) {
    if (enabled == true) {
      await this.client.enableDualStream();
    } else {
      await this.client.disableDualStream();
    }
  }

  async setLocalPublishFallbackOption_WGL(option) {
    try {
      await this.client.setStreamFallbackOption(this.client.uid, option);
    } catch (error) {
      console.log("error from setLocalPublishFallbackOption", error);
    }
  }

  SetRemoteUserPriority(uid, userPriority) {
    if (userPriority == 50 || userPriority == 0)
      this.client.setRemoteVideoStreamType(uid, 0);
    else if (userPriority == 100 || userPriority == 1)
      this.client.setRemoteVideoStreamType(uid, 1);
  }

  setRemoteSubscribeFallbackOption_WGL(option) {
    Object.keys(remoteUsers).forEach((uid2) => {
      this.client.setRemoteVideoStreamType(uid2, option);
    });
  }

  enableLogUpload() {
    AgoraRTC.enableLogUpload();
    console.log("----------- log upload to server enabled -------- ");
  }

  disableLogUpload() {
    AgoraRTC.disableLogUpload();
    console.log("----------- log upload to server disabled -------- ");
  }

  createDataStream(needRetry) {
    this._streamMessageRetry = needRetry;
  }

  sendDataStream(data) {
    try {
        this.client.sendStreamMessage(data, this._streamMessageRetry);
    } catch(e) {
        // event_manager.raiseError
        return -1;
    }
    return 0;
  }
}
