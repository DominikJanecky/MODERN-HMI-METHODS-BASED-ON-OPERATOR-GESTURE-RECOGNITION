// Skript pre nastavenie Render Texture pre Raw Image
using UnityEngine;
using UnityEngine.UI;

public class SetupRenderTexture : MonoBehaviour
{
    public RawImage rawImage;

    void Start()
    {
        // Nájde Render Texture a priradí ju Raw Image
        RenderTexture characterRenderTexture = Resources.Load<RenderTexture>("CharacterRenderTexture");
        rawImage.texture = characterRenderTexture;
    }
}