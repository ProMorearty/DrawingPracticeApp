using UnityEngine;
using UnityEngine.UI;

public class ImageResizer
{
    public void Resize(GameObject mainCanvasGO, Texture2D currentTexture, RawImage GUIImage, bool FlipXRandomly)
    {
        //resize the rect
        //var refResolutionScreen = Screen.currentResolution;
        //float widthRatioScreen = refResolutionScreen.width / currentTexture.width;
        //float heightRatioScreen = refResolutionScreen.height / currentTexture.height;

        // scale based on reference resoluion of canvas
        var refResolutionCanvas = mainCanvasGO.GetComponent<CanvasScaler>().referenceResolution;
        float widthRatioCanvas = refResolutionCanvas.x / currentTexture.width;
        float heightRatioCanvas = refResolutionCanvas.y / currentTexture.height;

        //widthRatioCanvas *= widthRatioScreen;
        //heightRatioCanvas *= heightRatioScreen;

        float ratio;
        ratio = heightRatioCanvas < widthRatioCanvas ? heightRatioCanvas : widthRatioCanvas;

        var rectSize = new Vector2(currentTexture.width * ratio, currentTexture.height * ratio);

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
