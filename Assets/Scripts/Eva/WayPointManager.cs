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

    public bool guiding = false;
    public int wayPointIndex = 0;
    [SerializeField]
    GameObject guide;

    float rotationSpeed = 0.1f;
    float movementSpeed= 0.1f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // todo check for atleast one waypoint
    // Update is called once per frame
    void Update()
    {
        if (guiding) {
            if (!WaitForPlayer()) {
                PersueWaypoint();
            }
            
        } else {
            if (wayPointIndex>= wayPoints.Count) {
                //completed
                
            }
            CheckForMarkerPlacement();   
        }

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

    public void StartGuidance() {
        guiding = true;
        guide.transform.position = AR_Camera.transform.position;
    }
    public bool WaitForPlayer() {
        //if distance to guide is too big wait for the player and look at him / Maby wave at the player
        Vector3 dir = AR_Camera.transform.position - guide.transform.position;
        if ((dir).magnitude>1.5) {
           
           // dir.y = 0; // keep the direction strictly horizontal
          //  Quaternion rot = Quaternion.LookRotation(dir);
            // slerp to the desired rotation over time
          //  guide.transform.rotation = Quaternion.Slerp(guide.transform.rotation, rot, rotationSpeed * Time.deltaTime);
           // return true;
        }
        return false;
        
    }
    public void PersueWaypoint() {
     
        guide.transform.position = Vector3.MoveTowards(guide.transform.position, wayPoints[wayPointIndex].transform.position, movementSpeed*Time.deltaTime);
        if (wayPoints[wayPointIndex].transform.position.Equals(guide.transform.position))
        {
            wayPointIndex++;
            //wayPointIndex %= (wayPoints.Count);
        }
    }
}