using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

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

       //Checking pass and username validation
        if(ValidatePassword(password) && ValidateUsername(username))
        {
                m_TextComponent.text = "Validando...";
            //Construcción del formulario de la petición
                WWWForm form = new WWWForm();
                form.AddField("username", username);
                form.AddField("password", password);
                
            //Enviamos la petición
                UnityWebRequest request = UnityWebRequest.Post("https://meta-login.onrender.com/users/create",form);
                var handler = request.SendWebRequest();
                m_TextComponent.text = "Conectando...";

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
        else
        {
            Debug.Log("Credenciales invalidas");
        }
    }

    public bool ValidatePassword(string password)
    {
        bool hasLowercase = false;
        bool hasUppercase = false;
        bool hasNumber = false;
        bool hasSpecial = false;

        // Check minimum length
        if (password.Length < 8)
        {
            return false;
        }

        // Check maximum length
        if (password.Length > 20)
        {
            return false;
        }

        // The pass must contain Check for required characters for security in user passwords
        foreach (char c in password)
        {
            if (char.IsLower(c))
            {
                hasLowercase = true;
            }
            else if (char.IsUpper(c))
            {
                hasUppercase = true;
            }
            else if (char.IsNumber(c))
            {
                hasNumber = true;
            }
            else if (char.IsSymbol(c) || char.IsPunctuation(c))
            {
                hasSpecial = true;
            }
        }
        if (!hasLowercase || !hasUppercase || !hasNumber || !hasSpecial)
        {
            Debug.Log("Not secure password");
            return false;//Not a secure password
        }

        // Check for forbidden characters
        string forbidden = "!@#$%^&*()+";

        foreach (char c in forbidden)
        {
            if (password.Contains(c))
            {
                Debug.Log("Invalid characters");
                return false; //Pass Invalid return false
            }
        }

        return true;// Password is valid when pass everycheck
    }

    public bool ValidateUsername(string username)
    {
        // Check minimum length
        if (username.Length < 3)
        {
            return false;
        }

        // Check maximum length
        if (username.Length > 20)
        {
            return false;
        }

        // Check allowed characters
        string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.";
        foreach (char c in username)
        {
            if (!allowed.Contains(c))
            {
                return false;
            }
        }

        // Check forbidden characters
        string forbidden = "!@#$%^&*()+=";
        foreach (char c in forbidden)
        {
            if (username.Contains(c))
            {
                return false;
            }
        }

        // Check reserved words
        string[] reserved = {"admin", "root", "system"};
        if (reserved.Contains(username.ToLower()))
        {
            return false;
        }

        // Username is valid
        return true;
    }
}


