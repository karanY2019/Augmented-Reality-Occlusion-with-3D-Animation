using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))]
public class SceneController_Part2 : MonoBehaviour
{
    public Slider slider;
    public Button placeButton;
    public Button undoButton;
    public Button resetButton;

    private Camera cam;
    public GameObject ARSessionOrigin;
    public GameObject m_PlacedCube;

    public LineRenderer LRPrefab;
    public LineRenderer LRPrefabCurve;
    LineRenderer LR_BCurve;
    LineRenderer LR;
    public GameObject LRStart;
    
    public GameObject mainCube;
    public GameObject shadow;
    private GameObject shadowCube;
    
    public List<GameObject> distanceTexts;
    public GameObject distanceCalc;

    public ARRaycastManager raycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    
    private GameObject Cube; 
    private List<GameObject> cubes = new List<GameObject>();

    private GameObject lastAddedCube;
    private GameObject prevAddedCube;

    
    float z_Pos = 2f;
    Vector3 start, end;
    private int BCPoints = 50;
    private float smoothTime = 0.2F;
    private Vector3 speed = Vector3.zero;
    

    bool isDistViz = false; 
    public float value;
    float distance;
    
    /// <summary>
	/// The object instantiated as a result of a successful raycast intersection with a plane.
	/// </summary>
	public GameObject spawnedObject { get; private set; }
    public Vector3 point1;
    void Awake()
    {
        placeButton.onClick.AddListener(() => PlaceObject());
        undoButton.onClick.AddListener(() => Undo());
        resetButton.onClick.AddListener(() => Restart());
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    void Start()
    {
        //instantiate cube at the origin
        cam = Camera.main;
        raycastManager = ARSessionOrigin.GetComponent<ARRaycastManager>();
        start = LRStart.transform.position;
        end = mainCube.transform.position;

        Cube = Instantiate(m_PlacedCube, end, new Quaternion(0,0,0,0));    
        LR_BCurve = Instantiate(LRPrefabCurve);
        mainCube.transform.LookAt(cam.transform);
        shadowCube = Instantiate(shadow);
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }

        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    // Update is called once per frame
    void Update() 
    {
        
        start = LRStart.transform.position;
        end = mainCube.transform.position;
        DrawCurve();

        Ray ray = new Ray(Cube.transform.position, -2.0f * Cube.transform.up);

        if (raycastManager.Raycast(ray, s_Hits, TrackableType.PlaneWithinPolygon))
        {
         
            Pose hit = s_Hits[0].pose;

            shadowCube.transform.position = hit.position;
            shadowCube.transform.rotation = hit.rotation;
        }
    }

    //
    void DrawCurve()
    {      
        Cube.transform.position = Vector3.SmoothDamp(Cube.transform.position, end, ref speed, smoothTime);
        Vector3 point1 = Vector3.Lerp(start, Cube.transform.position, 0.5f);
        point1 = point1 + speed / 4;
        LR_BCurve.positionCount = BCPoints;

        for (int i = 0; i < BCPoints; i++)
        {
            float t = i / (BCPoints - 1.0f);           
            Vector3 position = CalcBezier(t, start, point1, Cube.transform.position);
            LR_BCurve.SetPosition(i, position);
        }
    }

    //ref code: https://www.gamasutra.com/blogs/VivekTank/20180806/323709/How_to_work_with_Bezier_Curve_in_Games_with_Unity.php
    Vector3 CalcBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * uu * t * p1;
            p += tt * p2;

            return p;        
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
            cubes.RemoveAt(i);
            Destroy(removeCube);
        }

        for (int i = distanceTexts.Count - 1; i >= 0; i--)
        {
            GameObject removeText = distanceTexts[i];
            distanceTexts.RemoveAt(i);
            Destroy(removeText);
        }
        LR.positionCount = 0;
        isDistViz = false;
    }

	public void PlaceObject()
    {
		   spawnedObject = (GameObject) Instantiate(m_PlacedCube, mainCube.transform.position, new Quaternion(0,0,0,0));
		   cubes.Add(spawnedObject);
		   GenerateLineRenderer();
	}

	void GenerateLineRenderer()
    {
            if (cubes.Count > 1)
            {
                lastAddedCube = cubes[cubes.Count - 1];
                prevAddedCube = cubes[cubes.Count - 2];

                if (!isDistViz)
                {
                    LR = Instantiate(LRPrefab);
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

	public void ValueChangeCheck()
    {
       
       float value = slider.value;
       float newFinalPosition = z_Pos - value;
       mainCube.transform.position = new Vector3(mainCube.transform.position.x, mainCube.transform.position.y, newFinalPosition);
   	}

    void ShowDistanceText() 
    {
        if (cubes.Count >= 2) 
        {
            distance = Vector3.Distance(prevAddedCube.transform.position, lastAddedCube.transform.position);
        }
        else {
            return;
        }
        
        GameObject dist = (GameObject) Instantiate(distanceCalc);
        dist.transform.rotation = cam.transform.rotation;
        dist.transform.position = (cubes[cubes.Count - 2].transform.position + cubes[cubes.Count - 1].transform.position) / 2.0f;
        TextMesh disText = dist.GetComponent<TextMesh>();
        disText.text = $"{distance.ToString("F2")}m";

        distanceTexts.Add(dist);
    }
}



