using UnityEngine;
using UnityEngine.UI;

public class ImageResizer
{
    public void Resize(GameObject mainCanvasGO, Texture2D currentTexture, RawImage GUIImage, bool FlipXRandomly)
    {
        // scale based on reference resolution of canvas
        var refResolutionCanvas = mainCanvasGO.GetComponent<CanvasScaler>().referenceResolution;
        float widthRatioCanvas = refResolutionCanvas.x / currentTexture.width;
        float heightRatioCanvas = refResolutionCanvas.y / currentTexture.height;

        float ratio;
        ratio = heightRatioCanvas < widthRatioCanvas ? heightRatioCanvas : widthRatioCanvas;

        //Resize rect
        var rectSize = new Vector2(currentTexture.width * ratio, currentTexture.height * ratio);

        //Resize texture
        currentTexture = Texture2DResizeTrilinear(currentTexture, (int)(currentTexture.width * ratio), (int)(currentTexture.height * ratio));

        if (FlipXRandomly)
        {
            if (MakeFlipDecision())
            {
                GUIImage.uvRect = new Rect(0, 0, -1, 1);
            }
            else
            {
                GUIImage.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        GUIImage.rectTransform.sizeDelta = rectSize;
    }

    private Texture2D Texture2DResizeTrilinear(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Trilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Trilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        var nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        return nTex;
    }

    private bool MakeFlipDecision()
    {
        var random = new System.Random();
        var next = random.Next(0, 10);

        if (next > 5)
        {
            return true;
        }

        return false;
    }
}
