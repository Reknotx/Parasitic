using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILook : MonoBehaviour
{
    private Transform _mainTransform;
    private Camera _mainCamera;


    

    // Start is called before the first frame update
    void Start()
    {
        _mainTransform = transform;
        _mainCamera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        
        _mainTransform.LookAt(_mainCamera.transform.position, Vector3.up);
    }
}
