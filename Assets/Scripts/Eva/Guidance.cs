using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Guidance : MonoBehaviour
{
    public enum GuideState{
        Idle, Waiting, Guiding
    }
    [SerializeField]
    public Camera AR_Camera;
    public List<GameObject> wayPoints = new List<GameObject>();
    public Dictionary<string, GameObject> wayPointsDict = new Dictionary<string, GameObject>();
    public AnchorStore anchorStore;
    public bool guiding = false;
    public int wayPointIndex = 0;
    public GameObject guide;
    public float rotationSpeed = 7.0f;
    public GuideState state= GuideState.Idle;
    string waiting = "Room1";

    //TODO prob special method for every child
    public void StartGuidance()
    {
        guiding = true;
        Vector3 pos = AR_Camera.transform.position + AR_Camera.transform.forward * 1.5f;
        Vector3 dir = (pos - AR_Camera.transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        guide.transform.position = pos;
        guide.transform.rotation = rot;
    }
    public  abstract void GuidanceComplete();
    public abstract void PersueWaypoint();
    public void AssignPath(List<GameObject> wayPoints, int wayPointIndex) {
        this.wayPoints = wayPoints;
        this.wayPointIndex = wayPointIndex;
    }
    public void WaypointRached()
    {
        wayPointIndex++;
    }

    public void AddNewFoundAnchor(string key, GameObject value)
    {
        wayPointsDict.Add(key,value);
        wayPoints.Add(value);
        // anchorStore.anchor.children[0].name
    }

    //public abstract void resetTarget();
    //public abstract void AddWaypoint();



}
