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

    private string[] PlaySceneName = new string[]{"Mapa1"};

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
    private void CheckAppId()
    {
        Debug.Assert(AppID.Length > 10, "Please fill in your AppId first on Game Controller object.");
        GameObject go = GameObject.Find("AppIDText");
        if (go != null)
        {
            Text appIDText = go.GetComponent<Text>();
            if (appIDText != null)
            {
                if (string.IsNullOrEmpty(AppID))
                {
                    appIDText.text = "AppID: " + "UNDEFINED!";
                }
                else
                {
                    appIDText.text = "AppID: " + AppID.Substring(0, 4) + "********" + AppID.Substring(AppID.Length - 4, 4);
                }
            }
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

    public void onJoinButtonClicked(string ChannelName, bool muted = false)
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
