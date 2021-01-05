using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using UnityEngine.UI;
using System.Transactions;
using TMPro;

[RequireComponent(typeof(ARRaycastManager))]
public class SceneController_Part1 : MonoBehaviour
{
  
    public Button undoButton;
    public Button resetButton;

    [SerializeField]    
    GameObject m_ARSessionOrigin;

    [SerializeField]    
    GameObject m_PlacedObjectPrefab;

    public LineRenderer DistanceVisualizer;
  
    public GameObject ARSessionOrigin
    {
        get { return m_ARSessionOrigin; }
        set { m_ARSessionOrigin = value; }
    }

    public GameObject PlacedObjectPrefab
    {
        get { return m_PlacedObjectPrefab; }
        set { m_PlacedObjectPrefab = value; }
    }

    /// The object instantiated as a result of a successful raycast intersection with a plane.
    public GameObject spawnedObject { get; private set; }
    public GameObject distanceCalc;

    /// Invoked whenever an object is placed in on a plane.    
    ARRaycastManager m_RaycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private List<GameObject> cubes = new List<GameObject>();
    
    public List<GameObject> distanceTexts;

    private float distance;
    public float speed = 1f;
    bool isDistViz = false;
   
    public GameObject spawnedDisText { get; private set; }
  
    [SerializeField]
    private Camera arCamera;

    LineRenderer LR;  

    private GameObject lastAddedCube;
    private GameObject prevAddedCube;

    void Awake()
    {
        m_RaycastManager = ARSessionOrigin.GetComponent<ARRaycastManager>();

        //buttons
        undoButton.onClick.AddListener(() => Undo());
        resetButton.onClick.AddListener(() => Restart());    
    }

    bool TryGetTouchPostion(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    void Start()
    {
        arCamera = Camera.main; 
    }
   
    void Update()
    {
        // TODO: Handle Touch Events
        if (!TryGetTouchPostion(out Vector2 touchPosition))
            return;

        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = s_Hits[0].pose;

                    spawnedObject = Instantiate(m_PlacedObjectPrefab, hitPose.position, hitPose.rotation);

                    cubes.Add(spawnedObject);
                    GenerateLineRenderer();
                }
            }
        }
    }
   
    void GenerateLineRenderer()
    {
        if (cubes.Count > 1)
        {
            lastAddedCube = cubes[cubes.Count - 1];
            prevAddedCube = cubes[cubes.Count - 2];

            if (!isDistViz)
            {
                LR = Instantiate(DistanceVisualizer);
                isDistViz = true;
                LR.SetPosition(0, prevAddedCube.transform.position);
                LR.positionCount = 2;
                StartCoroutine(DrawLine());
            }

            else
            {
                LR.positionCount += 1;
                StartCoroutine(DrawLine());
            }
        }
    }

    IEnumerator DrawLine()
    {
        float start = 0f;
        float duration = 2f;

        while (start <= duration && LR.positionCount > 0 && cubes.Count >= 2)
        {
            start = start + Time.deltaTime;
        
            LR.SetPosition(LR.positionCount - 1, Vector3.Lerp(prevAddedCube.transform.position, lastAddedCube.transform.position, start / duration));
            yield return null;
        }

        ShowDistanceText();
    }

    void ShowDistanceText()
    {
        if (cubes.Count >= 2)
        {
            distance = Vector3.Distance(prevAddedCube.transform.position, lastAddedCube.transform.position);
        }
        else
        {
            return;
        }
        //transform.LookAt(target);
        GameObject dist = (GameObject)Instantiate(distanceCalc);
        dist.transform.rotation = arCamera.transform.rotation;
        dist.transform.position = (cubes[cubes.Count - 2].transform.position + cubes[cubes.Count - 1].transform.position) / 2.0f;
        TextMesh disText = dist.GetComponent<TextMesh>();
        
        disText.text = $"{distance.ToString("F2")}m";

        distanceTexts.Add(dist);
    }

    public void Undo()
    {
        if (cubes.Count == 0)
        {
            LR.positionCount = 0;
            isDistViz = false;
            return;
        }
        GameObject removeCube = cubes[cubes.Count - 1];
        Destroy(removeCube);
        cubes.RemoveAt(cubes.Count - 1);

        if (distanceTexts.Count > 0)
        {
            GameObject removeText = distanceTexts[distanceTexts.Count - 1];
            distanceTexts.RemoveAt(distanceTexts.Count - 1);
            Destroy(removeText);
        }

        if (cubes.Count == 0)
        {
            LR.positionCount = 0;
            isDistViz = false;
            return;
        }

        if (LR.positionCount > 0)
        {
            LR.positionCount = LR.positionCount - 1;
        }
    }

    public void Restart()
    {
        for (int i = cubes.Count - 1; i >= 0; i--)
        {
            GameObject removeCube = cubes[i];
            Destroy(removeCube);
            cubes.RemoveAt(i);         
        }

        for (int i = distanceTexts.Count - 1; i >= 0; i--)
        {
            GameObject removeText = distanceTexts[i];
            Destroy(removeText);
            distanceTexts.RemoveAt(i);          
        }

        LR.positionCount = 0;
        isDistViz = false;
    }

}







