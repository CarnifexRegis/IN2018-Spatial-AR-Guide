using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Guidance : MonoBehaviour
{
    public enum GuideState{
        Idle, Waiting, Guiding
    }
    public List<GameObject> wayPoints = new List<GameObject>();
    public Dictionary<string, GameObject> wayPointsDict = new Dictionary<string, GameObject>();
    public AnchorStore anchorStore;
    public bool guiding = false;
    public int wayPointIndex = 0;
    //public GameObject guide;
    public float rotationSpeed = 7.0f;
    public GuideState state= GuideState.Idle;
    string waiting = "Room1";
    public Vector3 lookDir;
    Anchor lastAnchor = null;

    //TODO prob special method for every child
    public void InitializeGuidance()
    {
        //guiding = true;
        Vector3 pos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z) * 2.0f;
        Vector3 dir = (Camera.main.transform.position-pos).normalized;
        lookDir = dir;
        Quaternion rot = Quaternion.LookRotation(dir);
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
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
    public void SetHead()
    { 
        
    }
    public void AddNewFoundAnchor(string key, GameObject value)
    {
        wayPointsDict.Add(key,value);
        if (lastAnchor == null)
        {
            lastAnchor = anchorStore.anchor;
            if (key.Equals(anchorStore.anchor.name))
            {
                wayPoints.Add(value);
                guiding = true;
            }
            else
            {
                Debug.Log("Wrong Waypoint advertised as head");
            }

        }
        else {
            if (key.Equals(lastAnchor.children[0].name))
            {
                wayPoints.Add(value);
                guiding = true;
                lastAnchor = lastAnchor.children[0];
            }

        }
        // anchorStore.anchor.children[0].name
    }

    public void LookAtDir(Vector3 dir)
    {
         // keep the direction strictly horizontal
        Quaternion rot = Quaternion.LookRotation(dir);
        //slerp to the desired rotation over time
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }

    //public abstract void resetTarget();
    //public abstract void AddWaypoint();



}
