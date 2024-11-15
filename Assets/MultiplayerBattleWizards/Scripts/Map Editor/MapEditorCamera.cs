#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapEditorCamera : MonoBehaviour
{
    [Header("Speeds")]
    public float moveSpeed;                 // Cam move speed with WASD or arrow keys.
    public float middleMouseDragSpeed;      // Cam move speed with middle mouse drag.
    public float zoomSpeed;                 // Cam zoom speed with scroll wheel.

    [Header("Bounds")]
    public float minX;                      // -X clamp pos.
    public float maxX;                      // +X clamp pos.
    public float minY;                      // -Y clamp pos.
    public float maxY;                      // +Y clamp pos.

    public float minZoom;                   // Furthest in we can zoom.
    public float maxZoom;                   // Furthest out we can zoom.

    private Vector3 middleMouseClickPos;    // Position of the initial middle mouse down click.

    // Components
    private Camera cam;                     // The Camera component.
    private InputField mapInput;            // We need this to check if we're currently typing.

    void Awake ()
    {
        cam = Camera.main;
    }
    
    void Start ()
    {
        // Get the input field.
        mapInput = FindObjectOfType<MapEditor>().mapNameInput;
    }

    void Update ()
    {
        // Don't move if we're typing in the input field.
        if(!mapInput.isFocused)
            Move();

        MiddleMouseDrag();

        // Don't zoom if we're hovering over a UI element.
        if(!EventSystem.current.IsPointerOverGameObject())
            Zoom();

        if(Input.GetKeyDown(KeyCode.F))
            CenterAsGameCamera();
    }

    // Move the camera with WASD or arrow keys.
    void Move ()
    {
        // Get the inputs.
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // Calculate a new position for the camera.
        Vector3 newPos = transform.position + (new Vector3(x, y, 0) * moveSpeed * Time.deltaTime);

        // Clamp that position.
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // Apply it to the camera.
        transform.position = newPos;
    }

    // Move the camera by holding middle mouse and moving the mouse.
    void MiddleMouseDrag ()
    {
        // On middle mouse DOWN - set the middle mouse click position.
        if(Input.GetMouseButtonDown(2))
        {
            middleMouseClickPos = Input.mousePosition;
        }
        // When we drag with middle mouse, move the camera.
        else if(Input.GetMouseButton(2))
        {
            // Calculate direction and new position for camera.
            Vector3 dir = Input.mousePosition - middleMouseClickPos;
            Vector3 newPos = transform.position + (dir * middleMouseDragSpeed * Time.deltaTime);

            // Clamp that position.
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            // Apply it to the camera.
            transform.position = newPos;
        }
    }

    // Zoom in and out with the scroll wheel (change orthographic size).
    void Zoom ()
    {
        // Get scroll input.
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Calculate a new orthographic size and clamp it.
        float newOrthoSize = cam.orthographicSize + (-scroll * zoomSpeed);
        newOrthoSize = Mathf.Clamp(newOrthoSize, minZoom, maxZoom);

        // Apply it to the camera.
        cam.orthographicSize = newOrthoSize;
    }

    // Sets the camera to how it would be during the game.
    void CenterAsGameCamera ()
    {
        // Get the tiles.
        List<GameObject> tiles = FindObjectOfType<MapEditor>().tiles;
        Rect bounds = new Rect(0, 0, 0, 0);

        if(tiles.Count == 0)
        {
            transform.position = new Vector3(0, 0, -10);
            cam.orthographicSize = 7.170732f;
            return;
        }

        foreach(GameObject tile in tiles)
        {
            Vector3 pos = tile.transform.position;

            if(pos.x < bounds.xMin) bounds.xMin = pos.x;
            if(pos.x > bounds.xMax) bounds.xMax = pos.x;
            if(pos.y < bounds.yMin) bounds.yMin = pos.y;
            if(pos.y > bounds.yMax) bounds.yMax = pos.y;
        }

        transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
        
        cam.orthographicSize = 7.170732f;
    }
}
#endif