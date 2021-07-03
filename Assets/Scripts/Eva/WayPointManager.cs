using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class WayPointManager : MonoBehaviour
{
    [SerializeField]
    public GameObject placementPrefab;
    [SerializeField]
    public Camera AR_Camera;
    [SerializeField]
    public ARRaycastManager raycastManager;
    public List<ARRaycastHit> hits = new List<ARRaycastHit>();
    public List<GameObject> wayPoints = new List<GameObject>();
    public GameObject guide;
    public GameObject arrow;

    void Start()
    {

    }

    // todo check for atleast one waypoint
    // Update is called once per frame
    void Update()
    {
            CheckForMarkerPlacement();   
    }
    public void CheckForMarkerPlacement() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = AR_Camera.ScreenPointToRay(Input.mousePosition);
            if (raycastManager.Raycast(ray, hits))
            {
                Pose pose = hits[0].pose;
                GameObject wayPoint = Instantiate(placementPrefab, pose.position, pose.rotation);
                wayPoints.Add(wayPoint);

            }
        }
    }
    public void StartArrow() {
        arrow.SetActive(true);
        Guidance guidance = (Guidance)arrow.GetComponent<Pointer>();
        guidance.AssignPath(wayPoints, 0);
        guidance.StartGuidance();
    }
    public void StartGuide() {

        guide.SetActive(true);
        Guidance guidance = (Guidance)guide.GetComponent<Guide>();
        guidance.AssignPath(wayPoints, 0);
        guidance.StartGuidance();
    }
}