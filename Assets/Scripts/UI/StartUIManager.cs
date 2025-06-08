using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class StartUIManager : MonoBehaviour
{
    [Header("Dot Buttons")]
    public Button[] dots;                    // The floating dot buttons

    [Header("Button Prefabs")]
    public GameObject[] buttonPrefabs;       // Prefabs corresponding to each dot

    [Header("Line Renderer")]
    public LineRenderer line;                // LineRenderer for drawing the connection

    [Header("UI Raycaster")]
    public GraphicRaycaster uiRaycaster;     // Raycaster for blank-click detection

    private Button spawnedButton;            // Currently instantiated button
    private RectTransform canvasRect;        // RectTransform of the parent Canvas

    void Awake()
    {
        // Cache the Canvas’s RectTransform
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // Hook up each dot so clicking it spawns its matching button
        for (int i = 0; i < dots.Length; i++)
        {
            int idx = i; // capture loop variable
            dots[idx].onClick.AddListener(() => OnDotClicked(idx));
        }
    }

    void Update()
    {
        // On left mouse down, check if click landed on a dot or on the spawned button.
        // If not, clear everything.
        if (Input.GetMouseButtonDown(0))
        {
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerData, results);

            bool clickedInteractive = false;
            foreach (var r in results)
            {
                var b = r.gameObject.GetComponentInParent<Button>();
                if (b != null &&
                    (System.Array.Exists(dots, dot => dot == b) || b == spawnedButton))
                {
                    clickedInteractive = true;
                    break;
                }
            }

            if (!clickedInteractive)
                ClearSpawned();
        }
    }

    /// <summary>
    /// Called when a dot is clicked. Clears any existing button/line,
    /// spawns the new button, centers it, hooks its click, and draws the line.
    /// </summary>
    public void OnDotClicked(int index)
    {

        Debug.Log($"OnDotClicked: dot #{index}");

        // Remove any existing button & clear the line
        ClearSpawned();

        // Instantiate the corresponding prefab under the Canvas
        var go = Instantiate(buttonPrefabs[index], canvasRect);
        go.SetActive(true);
        spawnedButton = go.GetComponent<Button>();

        // Center the new button
        var rt = spawnedButton.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        // Hook up its click to StartGame()
        spawnedButton.onClick.AddListener(StartGame);

        // Draw a line from the clicked dot to the new button
        DrawConnection(dots[index].GetComponent<RectTransform>(), rt);
    }

    /// <summary>
    /// Destroys the spawned button (if any) and clears the line.
    /// </summary>
    private void ClearSpawned()
    {
        if (spawnedButton != null)
        {
            Destroy(spawnedButton.gameObject);
            spawnedButton = null;
        }
        if (line != null)
            line.positionCount = 0;
    }

    /// <summary>
    /// Draws a straight line between two UI RectTransforms using the LineRenderer.
    /// </summary>
    public void DrawConnection(RectTransform from, RectTransform to)
    {
        if (line == null || canvasRect == null) return;

        // reset
        line.positionCount = 0;

        // world → screen → Canvas 
        Vector2 localA, localB;
        Vector2 screenA = RectTransformUtility.WorldToScreenPoint(null, from.position);
        Vector2 screenB = RectTransformUtility.WorldToScreenPoint(null, to.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenA, null, out localA);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenB, null, out localB);

        // Debug
        Debug.Log($"DrawConnection: from={from.name}@{localA}, to={to.name}@{localB}");

        // redraw
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(localA.x, localA.y, 0f));
        line.SetPosition(1, new Vector3(localB.x, localB.y, 0f));
    }


        private void StartGame()
    {
        Debug.Log("Start Game!");
        // e.g. SceneManager.LoadScene("GameScene");
    }
}