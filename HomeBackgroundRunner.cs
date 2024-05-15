using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBackgroundRunner : MonoBehaviour
{
    public GameObject[] backgrounds; // Array to hold your background GameObjects
    public float fadeDuration = 1f; // Duration of fade animation
    public float minTimeBetweenFades = 7f; // Minimum time between fades
    public float maxTimeBetweenFades = 12f; // Maximum time between fades
    public bool isOnHome = true;

    private int currentBackgroundIndex = 0; // Index of the currently active background
    private Coroutine fadeCoroutine; // Reference to the fade coroutine

    void Start()
    {
        int toStartWith = Random.Range(0, backgrounds.Length);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            SpriteRenderer spriteRenderer = backgrounds[i].GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;

            color.r *= 0.4f; // Red component
            color.g *= 0.4f; // Green component
            color.b *= 0.4f; // Blue component

            spriteRenderer.color = color;
            
            if (i == toStartWith)
            {
                continue;
            }

            color.a = 0f;
            spriteRenderer.color = color;
        }

        // Ensure that only the first background is active at start
        backgrounds[toStartWith].SetActive(true);

        // Start the fading loop
        StartCoroutine(FadeLoop());
    }

    IEnumerator FadeLoop()
    {
        while (isOnHome)
        {
            // Wait for random time before initiating fade
            yield return new WaitForSeconds(Random.Range(minTimeBetweenFades, maxTimeBetweenFades));

            // Get the next background index excluding the current one
            int nextBackgroundIndex = GetNextBackgroundIndex();

            // Fade out the current background
            fadeCoroutine = StartCoroutine(FadeBackground(currentBackgroundIndex, 0f));

            // Fade in the next background
            backgrounds[nextBackgroundIndex].SetActive(true);
            fadeCoroutine = StartCoroutine(FadeBackground(nextBackgroundIndex, 1f));

            // Update the current background index
            currentBackgroundIndex = nextBackgroundIndex;
        }
    }

    int GetNextBackgroundIndex()
    {
        int nextIndex = Random.Range(0, backgrounds.Length - 1);
        if (nextIndex >= currentBackgroundIndex)
        {
            nextIndex++;
        }
        return nextIndex % backgrounds.Length;
    }

    IEnumerator FadeBackground(int index, float targetAlpha)
{
    // Get the sprite renderer component of the background
    SpriteRenderer spriteRenderer = backgrounds[index].GetComponent<SpriteRenderer>();

    // Get the current alpha value
    float startAlpha = spriteRenderer.color.a;

    // Time elapsed
    float elapsedTime = 0f;

    // While we haven't reached the target alpha
    while (elapsedTime < fadeDuration)
    {
        // Calculate the new alpha value based on the elapsed time
        float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

        // Set the new color with the updated alpha value
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);

        // Update the elapsed time
        elapsedTime += Time.deltaTime;

        // Wait for the next frame
        yield return null;
    }

    // Ensure the final alpha value is exactly the target alpha
    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, targetAlpha);

    // If the target alpha is 0, deactivate the GameObject
    if (targetAlpha == 0f)
    {
        backgrounds[index].SetActive(false);
    }
}


    void OnDestroy()
    {
        // Stop the fade coroutine if the script is destroyed
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}
