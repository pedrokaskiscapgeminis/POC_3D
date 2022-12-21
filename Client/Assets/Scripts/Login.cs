using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class Login :  MonoBehaviourPunCallbacks
{
   
    //Inputs del componente
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    //Texto del componente
    [SerializeField] private TMP_Text m_TextComponent;
    //Botones del componente
    [SerializeField] private Button botonLogin;
    [SerializeField] private Button botonRegistro;
    [SerializeField] private Button botonInicio;

     public bool pasarNivel;
    public int indiceNivel;


    void Start(){
        botonInicio.gameObject.SetActive(false);
     }





    //Función para cambiar el nivel
    public void CambiarNivel(int indice)
    {
    SceneManager.LoadScene(indice);
    }
    public void OnLoginClick()
    {
        StartCoroutine(TryLogin());
    }
     public void OnCreateClick()
    {

        StartCoroutine(TryCreate());
    }

    public override void OnDisconnected(DisconnectCause cause) {
        TryLogin();
    }

    private IEnumerator TryLogin()
    {

       //Obtenemos los campos de los inputs
       string username = usernameInputField.text;
       string password = passwordInputField.text;
       Debug.Log(username + " " + password);
      m_TextComponent.text = "Conectando...";

       //Construcción del formulario de la petición
       WWWForm form = new WWWForm();
       form.AddField("username", username);
       form.AddField("password", password);

       //Enviamos la petición
       UnityWebRequest request = UnityWebRequest.Post("https://meta-login.onrender.com/users",form);
       var handler = request.SendWebRequest();
       
       //Tiempo de conexión con el servidor
       float startTime = 0.0f;
       while (!handler.isDone)
       {

        startTime += Time.deltaTime;

        if (startTime > 10.0f)
        {
            break;
        }
        yield return null;
       }

        //Manejo de la respuesta del servidor
       if (request.result == UnityWebRequest.Result.Success)
       {
            Debug.Log(request.downloadHandler.text);
            m_TextComponent.text = "Bienvenido";

              //Desactivación de objetos
            botonLogin.gameObject.SetActive(false);
            botonRegistro.gameObject.SetActive(false);
                     usernameInputField.gameObject.SetActive(false);
             passwordInputField.gameObject.SetActive(false);
             botonInicio.gameObject.SetActive(true);
           
            
       }
       //Manejo del error del servidor
       else{
         Debug.Log("Unable to connect to the server...");
       }


       yield return null;
    }


    private IEnumerator TryCreate()
    {

       //Obtenemos los campos de los inputs
       string username = usernameInputField.text;
       string password = passwordInputField.text;

       //Construcción del formulario de la petición
       WWWForm form = new WWWForm();
       form.AddField("username", username);
       form.AddField("password", password);
        m_TextComponent.text = "Conectando...";
       //Enviamos la petición
       UnityWebRequest request = UnityWebRequest.Post("https://meta-login.onrender.com/users/create",form);
       var handler = request.SendWebRequest();
       
       //Tiempo de conexión con el servidor
       float startTime = 0.0f;
       while (!handler.isDone)
       {

        startTime += Time.deltaTime;

        if (startTime > 10.0f)
        {
            break;
        }
        yield return null;
       }

        //Manejo de la respuesta del servidor
       if (request.result == UnityWebRequest.Result.Success)
       {
            Debug.Log(request.downloadHandler.text);
            m_TextComponent.text = "Bienvenido";

            //Desactivación de objetos
         botonLogin.gameObject.SetActive(false);
            botonRegistro.gameObject.SetActive(false);
             usernameInputField.gameObject.SetActive(false);
             passwordInputField.gameObject.SetActive(false);
             botonInicio.gameObject.SetActive(true);   
       }
       //Manejo del error del servidor
       else{
         Debug.Log("Unable to connect to the server...");
        m_TextComponent.text ="El usuario ya existe";
       }
       yield return null;
    }
}
