using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace IronSearch.Utils
{
    public static class UIUtils
    {
        public static Vector2 GetCaretVectorPosition(this InputField inputField)
        {
            if (inputField == null || inputField.textComponent == null)
            {
                return GetFallback(inputField);
            }

            var text = inputField.textComponent;
            var gen = text.cachedTextGenerator;

            float x;

            if (gen != null && gen.characterCountVisible > 0 && inputField.caretPosition > 0)
            {
                int index = Mathf.Clamp(inputField.caretPosition - 1, 0, gen.characterCountVisible - 1);
                var ch = gen.characters[index];

                x = ch.cursorPos.x + ch.charWidth;
            }
            else
            {
                x = 0f;
            }

            // Convert X from local text space → world
            Vector3 worldX = text.rectTransform.TransformPoint(new Vector3(x, 0f, 0f));

            // Get bottom-left of input field (for Y)
            RectTransform rt = inputField.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];

            // Combine X from caret + Y from input field bottom
            Vector3 finalWorld = new Vector3(worldX.x, bottomLeft.y, 0f);

            Canvas canvas = text.canvas;
            Camera? cam = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera;
            }

            var sc = RectTransformUtility.WorldToScreenPoint(cam, finalWorld);
            return sc;
        }

        private static Vector2 GetFallback(InputField? inputField)
        {
            if (inputField == null)
            {
                return Vector2.zero;
            }

            RectTransform rt = inputField.GetComponent<RectTransform>();
            if (rt == null)
            {
                return Vector2.zero;
            }

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];

            Canvas canvas = inputField.GetComponentInParent<Canvas>();
            Camera? cam = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera;
            }

            var sc = RectTransformUtility.WorldToScreenPoint(cam, bottomLeft);
            return sc;
        }
    }
}
