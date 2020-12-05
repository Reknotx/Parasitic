/// AUTHOR: Jeremy Casada
/// DATE: 9/2/2020
/// 
/// Controls movement of the Camera. Provides in Editor Tools to limit the camera's movement range
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance;

    private Camera _mainCamera;
    private Transform _mainTransform;

    public bool useRotateAround = false;

    private Vector3 rotationPoint;

    Plane plane = new Plane(Vector3.up, 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        _mainTransform = this.transform;
        _mainCamera = Camera.main;
        zoomPoint1 = _mainTransform.Find("ZoomPoint_1");
        zoomPoint2 = _mainTransform.Find("ZoomPoint_2");

        if(PlayerPrefs.GetInt("Camera Pivot", 0) == 0)
        {
            useRotateAround = false;
        }
        else
        {
            useRotateAround = true;
        }

        UI.Instance.SetPivotState(useRotateAround);
            
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        float dist;
        if (plane.Raycast(ray, out dist))
        {
            rotationPoint = ray.GetPoint(dist);
        }
    }

    // Update is called once per frame
    void Update()
    {
        DoMovement();   
    }

    void DoMovement()
    {
        Move();

        if(useRotateAround)
        {
            CenterRotate();
        }
        else
        {
            NormalRotate();
        }

        Zoom();

        LimitCamPos();
    }

    #region Movement & Rotation

    [Header("Speed at Which the Player Moves")]
    public float cameraMoveSpeed = 40f;
    [Header("Speed at which the Player Can turn the Camera")]
    public float cameraRotationSpeed = 40f;


    /// <summary>
    /// Moves the camera using the WASD keys (W and S = Vertical , A and D = Horizontal)
    /// </summary>
    private void Move()
    {
        Vector3 desiredMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        desiredMove *= cameraMoveSpeed * Time.deltaTime;
        desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;

        _mainTransform.Translate(desiredMove, Space.World);
        rotationPoint += desiredMove;
    }

    /// <summary>
    /// Rotates Camera Position Using the "Rotation" axis (Q and E keys)
    /// </summary>
    private void NormalRotate()
    {
        _mainTransform.Rotate(Vector3.up, Time.deltaTime * cameraRotationSpeed * Input.GetAxis("Rotation"), Space.World);
    }

    private void CenterRotate()
    {
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
        {
            float dist;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

            if (plane.Raycast(ray, out dist))
            {
                rotationPoint = ray.GetPoint(dist);
            }
        }

        _mainTransform.RotateAround(rotationPoint, Vector3.up, -Time.deltaTime * cameraRotationSpeed * Input.GetAxis("Rotation"));
 
    }
    #endregion


    #region Zooming
    private string _zoomAxis = "Mouse ScrollWheel";//Zoom Axis Name
    private float _zoomPos = 0f; //value from 0 to 1, used in Math.Lerp as t


    [Header("Sensitivity of The Scroll Wheel")]
    public float zoomSensitivity = 25f;//Zoom Sensitivity

    [HideInInspector]
    public Transform zoomPoint1, zoomPoint2;

    /// <summary>
    /// scrollWheel holds the value of the Scroll Wheel Input Axis
    /// </summary>
    private float _scrollWheel
    {
        get { return Input.GetAxisRaw(_zoomAxis); }

    }

    


    /// <summary>
    /// Allows User to Zoom when funtion is in Update. Zoom is Clamped by maxHeight and minHeight
    /// </summary>
    private void Zoom()
    {
        
        _zoomPos += _scrollWheel * Time.deltaTime * zoomSensitivity;
        _zoomPos = Mathf.Clamp01(_zoomPos);


        _mainCamera.transform.position = Vector3.Lerp(zoomPoint1.position, zoomPoint2.position, _zoomPos);

       

       // mainCamera.orthographicSize = targetZoom;
       // mainCamera.fieldOfView = targetZoom;
    }

    #endregion


    #region Transform Limiting

    [Header("Shown In Scene View With Purple Border Lines")]
    public float mapLimitX1 = 450f; //x limit of map

    public float mapLimitX2 = -450f; //x limit of map

    public float mapLimitY1 = 450f; //z limit of map

    public float mapLimitY2 = -450f; //z limit of map

    public float borderHeight = 25f;

    /// <summary>
    /// Limits The Camera's Position to within mapLimitX to -mapLimitX, and mapLimitY to -mapLimitY
    /// </summary>
    private void LimitCamPos()
    {
        _mainTransform.position = new Vector3(Mathf.Clamp(_mainTransform.position.x, mapLimitX2, mapLimitX1), _mainTransform.position.y, Mathf.Clamp(_mainTransform.position.z, mapLimitY2, mapLimitY1));

        
    }

    /*private void LimitCamRotation()
    {
        mainCamera.transform.rotation = Quaternion.Euler(45f, _mainTransform.rotation.eulerAngles.y, Mathf.Clamp(_mainTransform.rotation.eulerAngles.y, 0f, 0f));
    }*/
    #endregion


    /// <summary>
    /// Draw Lines in Editor for Level Designers
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(mapLimitX2, borderHeight, mapLimitY2), new Vector3(mapLimitX1, borderHeight, mapLimitY2));
        Gizmos.DrawLine(new Vector3(mapLimitX2, borderHeight, mapLimitY2), new Vector3(mapLimitX2, borderHeight, mapLimitY1));
        Gizmos.DrawLine(new Vector3(mapLimitX2, borderHeight, mapLimitY1), new Vector3(mapLimitX1, borderHeight, mapLimitY1));
        Gizmos.DrawLine(new Vector3(mapLimitX1, borderHeight, mapLimitY1), new Vector3(mapLimitX1, borderHeight, mapLimitY2));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(zoomPoint1.position, zoomPoint2.position);
    }

    

    

    
}


