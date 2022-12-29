using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSphere : MonoBehaviour
{
    public float scaleFactor = 1.1f;
    public float scaleDuration = 0.5f;

    void OnMouseDown()
    {
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            transform.localScale *= scaleFactor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

