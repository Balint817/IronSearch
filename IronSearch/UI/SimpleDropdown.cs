using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDropdown : MonoBehaviour
{
    private List<string> items = null!;
    private Action<string, int>? onSelected;

    private Vector2 scroll;
    private int selectedIndex = 0;

    private Rect windowRect;
    private bool isClosing = false;

    private const float width = 200f;
    private const float itemHeight = 28f;
    private const int visibleItems = 4;
    private int windowId;
    private GUIStyle labelStyle;

    private float Height => itemHeight * visibleItems + 10f;

    public static SimpleDropdown Create(IEnumerable<string> items, Action<string, int> onSelected, Vector2? topLeft = null)
    {
        var go = new GameObject("SimpleDropdown");
        //DontDestroyOnLoad(go);
        var dropdown = go.AddComponent<SimpleDropdown>();
        dropdown.Init(items, onSelected, topLeft);

        return dropdown;
    }

    public void Awake()
    {
        windowId = GetInstanceID();
    }

    private void Init(IEnumerable<string> items, Action<string, int> onSelected, Vector2? topLeft = null)
    {
        this.items = new List<string>(items);
        this.onSelected = onSelected;

        if (topLeft is not { } v)
        {
            v = new((Screen.width / 2f) - width, (Screen.height / 2f) - Height);
        }
        v.x += width / 2;
        v.y += Height / 2;

        // Center screen
        windowRect = new Rect(
            v.x,
            v.y,
            width,
            Height
        );
    }

    public void Close()
    {
        isClosing = true;
    }

    private void Update()
    {
        if (isClosing)
        {
            Destroy(gameObject);
            return;
        }

        HandleKeyboard();
    }

    private void HandleKeyboard()
    {
        if (items.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % items.Count;
            EnsureVisible();
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            selectedIndex = (selectedIndex + visibleItems) % items.Count;
            EnsureVisible();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
            EnsureVisible();
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            selectedIndex = (selectedIndex - visibleItems + items.Count) % items.Count;
            EnsureVisible();
        }


        float scrollInput = Input.mouseScrollDelta.y;
        if (scrollInput > 0f)
        {
            selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
            EnsureVisible();
        }
        else if (scrollInput < 0f)
        {
            selectedIndex = (selectedIndex + 1) % items.Count;
            EnsureVisible();
        }


        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Select(selectedIndex);
        }
    }

    private void EnsureVisible()
    {
        float minY = selectedIndex * itemHeight;
        float maxY = minY + itemHeight;

        if (scroll.y > minY)
            scroll.y = minY;

        if (scroll.y + (visibleItems * itemHeight) < maxY)
            scroll.y = maxY - (visibleItems * itemHeight);
    }

    private void OnGUI()
    {
        if (isClosing)
        {
            return;
        }
        GUI.depth = 0;

        //// Dark background (optional)
        //GUI.color = new Color(0, 0, 0, 0.6f);
        //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        //GUI.color = Color.white;

        var t = DrawWindow;
        windowRect = GUI.Window(windowId, windowRect, t, "");
    }

    private void DrawWindow(int id)
    {
        Rect viewRect = new Rect(0, 0, width - 20f, items.Count * itemHeight);

        scroll = GUI.BeginScrollView(
            new Rect(5, 5, width - 10f, Height - 10f),
            scroll,
            viewRect
        );

        for (int i = 0; i < items.Count; i++)
        {
            Rect rect = new Rect(0, i * itemHeight, viewRect.width, itemHeight);

            bool isSelected = (i == selectedIndex);

            // background
            Color bg = isSelected ? new Color(0.3f, 0.6f, 1f, 0.8f) : new Color(0, 0, 0, 0.3f);
            GUI.color = bg;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            GUI.color = Color.white;


            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 0, 0, 0)
            };
            // label
            GUI.Label(rect, items[i], labelStyle);

            // click
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                Select(i);
            }
        }

        GUI.EndScrollView();

        GUI.DragWindow();
    }

    private void Select(int index)
    {
        if (index < 0 || index >= items.Count)
            return;
        try
        {
            onSelected?.Invoke(items[index], index);
        }
        finally
        {
            Close();
        }
    }
}