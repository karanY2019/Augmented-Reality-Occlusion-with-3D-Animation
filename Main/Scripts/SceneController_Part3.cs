using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

[RequireComponent(typeof(ARRaycastManager))]
public class SceneController_Part3 : MonoBehaviour
{
    public Button danceButton;
    public Button resetButton;
    public bool isDance ;
    Animation Animation;
    //public Animation dance;
    [SerializeField]
    GameObject m_ARSessionOrigin;

    [SerializeField]
    GameObject m_PlacedObjectPrefab;

    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    GameObject m_BookOccluCube;

    //public GameObject spidermanDance;
    [SerializeField]
    GameObject m_spidermanDancePrefab;

    public GameObject BookOccluCube
    {
        get { return m_BookOccluCube; }
        set { m_BookOccluCube = value; }
    }

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
    public GameObject placedObject { get; private set; }

    public GameObject SpidermanDancePrefab
    {
        get { return m_spidermanDancePrefab; }
        set { m_spidermanDancePrefab = value; }
    }
    public GameObject spidermanDance { get; private set; }

    private Vector2 touchPosition = default;

    private ARRaycastManager arRaycastManager;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    ARRaycastHit currentHit;
    void Awake()
    {
        arRaycastManager = ARSessionOrigin.GetComponent<ARRaycastManager>();
        
        danceButton.onClick.AddListener(() => dance());
        resetButton.onClick.AddListener(() => Restart());
        //Animation = placedObject.GetComponent<Animation>();
    }

    //ref: PlaceOnPlane.cs
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
        Animation = placedObject.GetComponent<Animation>();
        isDance = false;
        spidermanDance.SetActive(false);
        placedObject.SetActive(true);
    }

    void Update()
    {
        if (!TryGetTouchPostion(out Vector2 touchPosition))
            return;

        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (placedObject == null)
            {
                placedObject = Instantiate(PlacedObjectPrefab, hitPose.position, hitPose.rotation);
                placedObject.GetComponent<Animation>().playAutomatically = false;
               
            }
            else
            {
                currentHit = hits[0];
                StartCoroutine(Move());

                //Vector3 direction_to_book = BookOccluCube.transform.position - placedObject.transform.position;
                //placedObject.transform.position = hitPose.position;
                
                
            }        
        }

        if (isDance == false)
        {           
            return;
        }
        else if (isDance == true)
        {           
            Animation.Play();
        }

    }
    IEnumerator Move()
    {
        var pose = currentHit.pose;
        while (placedObject.transform.position != pose.position)
        {
            placedObject.transform.position = Vector3.Lerp(placedObject.transform.position, pose.position, Time.deltaTime * 0.1f * 10);
            Quaternion rotation = Quaternion.LookRotation(BookOccluCube.transform.position, Vector3.up);
            placedObject.transform.rotation = rotation;
            // placedObject.transform.RotateAround(BookOccluCube.transform.position, new Vector3(1f, 0f, 0f), 20 * Time.deltaTime);

        }
        yield return null;
    }

    void dance()
    {
        isDance = true;
        //placedObject.GetComponent<Animation>().Play("Animation");
        //placedObject.GetComponent<Animation>().playAutomatically = true;
        spidermanDance = Instantiate(SpidermanDancePrefab, placedObject.transform.position, placedObject.transform.rotation);
        placedObject.SetActive(false);
        spidermanDance.SetActive(true);       
    }

    public void Restart()
    {
        Destroy(placedObject);
        Destroy(spidermanDance);       
    }

}













