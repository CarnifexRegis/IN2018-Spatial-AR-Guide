using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Guidance : MonoBehaviour
{
    public enum GuideState
    {
        Idle, Waiting, Guiding, Explaining
    }
    public List<GameObject> wayPoints = new List<GameObject>();
    public Dictionary<string, GameObject> wayPointsDict = new Dictionary<string, GameObject>();
    public List<string> textsToSpeak = new List<string>();
    public AnchorStore anchorStore;
    //public bool guiding = false;
    public int wayPointIndex = 0;
    //public GameObject guide;
    public float rotationSpeed = 7.0f;
    public GuideState state = GuideState.Idle;
    string waiting = "Room1";
    public Vector3 lookDir;
    protected Anchor lastAnchor = null;

    //TODO prob special method for every child
    public void InitializeGuidance()
    {
        //guiding = true;
        Vector3 pos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z) * 3.0f;
        pos.y -= 0.2f;
        Vector3 dir = (pos - Camera.main.transform.position).normalized;
        lookDir = dir;
        Quaternion rot = Quaternion.LookRotation(dir);
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
    }
    public abstract void GuidanceComplete();
    public abstract void PersueWaypoint();
    public void AssignPath(List<GameObject> wayPoints, int wayPointIndex)
    {
        this.wayPoints = wayPoints;
        this.wayPointIndex = wayPointIndex;
    }
    public abstract IEnumerator WaypointRached();
    public void SetHead()
    {

    }
    public void AddNewFoundAnchor(string key, GameObject value)
    {
        wayPointsDict.Add(key, value);
        if (lastAnchor == null)
        {
            if (key.Equals(anchorStore.anchor.name))
            {
                lastAnchor = anchorStore.anchor;
                textsToSpeak.Add(lastAnchor.text);
                wayPoints.Add(value);
                Debug.Log("Initial Guiding True");
                FirstAnchorFound();
                //guiding = true;
            }
            else
            {
                Debug.Log("Wrong Waypoint advertised as head");
            }

        }
        else
        {
            CheckAgain:
            if (key.Equals(lastAnchor.children[0].name))
            {
                wayPoints.Add(value);
                //guiding = true;
                Debug.Log("following Guiding True");
                lastAnchor = lastAnchor.children[0];
                textsToSpeak.Add(lastAnchor.text);
            }
            else if (wayPointsDict[lastAnchor.children[0].name] != null)
            {
                wayPoints.Add(wayPointsDict[lastAnchor.children[0].name]);
                lastAnchor = lastAnchor.children[0];
                textsToSpeak.Add(lastAnchor.text);
                goto CheckAgain;
            }
            else
            {
                return;
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


    public abstract void FirstAnchorFound();
}