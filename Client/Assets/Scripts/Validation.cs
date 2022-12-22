using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Validation : MonoBehaviour
{
    public static bool ValidatePassword(string password)
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
            return false;//Not a secure password
            Debug.Log("Not secure password");
        }

        // Check for forbidden characters
        string forbidden = "!@#$%^&*()+";

        foreach (char c in forbidden)
        {
            if (password.Contains(c))
            {
                return false; //Pass Invalid return false
                Debug.Log("Invalid characters");
            }
        }

        return true;// Password is valid when pass everycheck
    }

    public static bool ValidateUsername(string username)
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
