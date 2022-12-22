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
    [SerializeField] private Button botonRegistro;
    [SerializeField] private Button botonInicio;


    void Start(){

     }
     public void OnCreateClick()
    {

        StartCoroutine(TryCreate());
    }

    public override void OnDisconnected(DisconnectCause cause) {
        
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
            m_TextComponent.text = "Registro realizado con éxito";

            //Desactivación de objetos
            botonRegistro.gameObject.SetActive(false);
             usernameInputField.gameObject.SetActive(false);
             passwordInputField.gameObject.SetActive(false);

             Vector3 anchoredPos = botonInicio.GetComponent<RectTransform>().anchoredPosition;
                anchoredPos.x = 0;
                botonInicio.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
       }
       //Manejo del error del servidor
       else{
         Debug.Log("Unable to connect to the server...");
        m_TextComponent.text ="El usuario ya existe";
       }
       yield return null;
    }
}
