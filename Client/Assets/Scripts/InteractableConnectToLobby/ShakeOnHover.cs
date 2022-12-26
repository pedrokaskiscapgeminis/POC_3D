using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ShakeOnHover : MonoBehaviour
{
    Vector3 originalPosition;
    public float shakeometro = 0.1f;
    bool shouldShake = false;

    void Start()
    {
        // Store the original position of the object
        originalPosition = transform.position;
    }

    void OnMouseEnter()
    {
        // Start the coroutine when the mouse enters the object's collider
        shouldShake = true;
        StartCoroutine(Shake());
    }

    void OnMouseExit()
    {
        // Stop the coroutine when the mouse exits the object's collider
        shouldShake = false;
        StopCoroutine(Shake());

        // Return the object to its original position
        transform.position = originalPosition;
    }

    IEnumerator Shake()
    {
        while (shouldShake)
        {
            // Move the object slightly off its original position
            transform.position = originalPosition + Random.insideUnitSphere * shakeometro;

            // Wait a short time before shaking the object again
            yield return new WaitForSeconds(0.05f);
        }
    }
}

