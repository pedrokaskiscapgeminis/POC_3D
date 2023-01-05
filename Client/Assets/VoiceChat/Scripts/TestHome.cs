using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
#if(UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using System.Collections;

/// <summary>
///    TestHome serves a game controller object for this application.
/// </summary>
public class TestHome : MonoBehaviour
{

    // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList();
#endif
    static TestHelloUnityVideo app = null;

    private string HomeSceneName = "Lobby";

    // List that contains all the PlayScenes, needs to be updated. Same values as LobbyManager.ROOM_NAMES
    private string[] PlaySceneName = new string[]{"Mapa1","Mapa2"}; 

    public GameObject roomPanel;

    // PLEASE KEEP THIS App ID IN SAFE PLACE
    // Get your own App ID at https://dashboard.agora.io/
    [SerializeField]
    private string AppID = "your_appid";

    void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
		permissionList.Add(Permission.Microphone);         
		permissionList.Add(Permission.Camera);               
#endif
        // keep this alive across scenes
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        CheckAppId();
        CheckRoomPanel();
    }

    void Update()
    {
        CheckPermissions();

        if (Input.GetKeyDown("m") && (PlaySceneName.Contains(SceneManager.GetActiveScene().name)  || ((SceneManager.GetActiveScene().name == HomeSceneName) && roomPanel.activeSelf))){
            Debug.Log("M WAS PRESSED");
            app.MuteAudio(SceneManager.GetActiveScene().name);
        }
    }

    private void CheckRoomPanel(){
        if (roomPanel == null)
            Destroy(this.gameObject);
    }

    /// <summary>
    ///   Checks if the Agora App ID has been introduced.
    /// </summary>
    private void CheckAppId()
    {
        Debug.Assert(AppID.Length > 10, "Please fill in your AppId first on Game Controller object.");

        if (string.IsNullOrEmpty(AppID))
        {
            Debug.Log("AppID: " + "UNDEFINED!");
        }
        else
        {
            Debug.Log("AppID: " + AppID.Substring(0, 4) + "********" + AppID.Substring(AppID.Length - 4, 4));
        }


    }

    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach(string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {                 
				Permission.RequestUserPermission(permission);
			}
        }
#endif
    }

    ///<summary>
    /// Checks if the Microphone image concurs with the audio state, and if not, changes the image.
    /// </summary>
    public void CheckMicroImage(){
        Debug.Log("hola");
        if (app.AudioVideoState.pubAudio==true)
            GameObject.Find("Micro").GetComponent<Image>().sprite = app.MicroOn;
        else
            GameObject.Find("Micro").GetComponent<Image>().sprite = app.MicroOff;
    }


    ///<summary>
    /// Called when the Join Button is clicked. This method creates the voice chat room.
    /// </summary>
    public void onJoinButtonClicked(string ChannelName, bool muted = true)
    {
        // create app if nonexistent
        if (ReferenceEquals(app, null))
        {
            app = new TestHelloUnityVideo(); // create app
            app.loadEngine(AppID); // load engine
        }

        // join channel and jump to next scene
        app.join(ChannelName, muted);
    }

    ///<summary>
    /// Called when the Leave Button is clicked. This method deletes the voice chat room.
    /// </summary>
    public void onLeaveButtonClicked()
    {
        if (!ReferenceEquals(app, null))
        {
            app.leave(); // leave channel
            app.unloadEngine(); // delete engine
            app = null; // delete app
            if (SceneManager.GetActiveScene().name!="Lobby"){
                    SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
                    Destroy(this.gameObject);
            }
        }
    }

    ///<summary>
    /// Called when Application is being closed. This method deletes the voice chat room.
    /// </summary>
    public void OnApplicationQuit()
    {
        if (!ReferenceEquals(app, null))
        {
            app.unloadEngine();
            SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
            Destroy(this.gameObject);
        }
    }
}
