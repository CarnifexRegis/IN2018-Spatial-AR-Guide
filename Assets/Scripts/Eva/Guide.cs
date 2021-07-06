using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.CoreModule;

public class Guide : Guidance
{
    [SerializeField]
    private SpeechManager _speechManager;

    float movementSpeed = 0.4f;
    // Start is called before the first frame update
    public float followDistance;
    public float waitDistance;
    void Start()
    {
        rotationSpeed = 14.0f;
        InitializeGuidance();
    }
    void Update()
    {
        if (guiding)
        {
            if (wayPointIndex >= wayPoints.Count)
            {
                //guiding = false;
                GuidanceComplete();
                LookAtDir(lookDir);
                return;

            }
            if (!WaitForPlayer())
            {
                PersueWaypoint();
            }

        }
        else
        {
            IdleUpdate();
        }
        LookAtDir(lookDir);
    }
    public void IdleUpdate() {
        Vector3 dir = (gameObject.transform.position-Camera.main.transform.position).normalized;
        lookDir = dir;
        //LookAtDir(dir);
        Vector2 player = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 guide = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        if (Vector2.Distance(player, guide) < followDistance)
        {
            // Speak on initial transition to idle state
            if (state != GuideState.Idle)
                _speechManager.Speak("poop"); // Probably better to call on GuidanceComplete or WaypointReached etc.

            state = GuideState.Idle;
        }
        else
        {
            MoveTowards(Camera.main.transform.position);
        }
    }

    public bool WaitForPlayer()
    {
        //if distance to guide is too big wait for the player and look at him / Maby wave at the player
        Vector3 dir = (gameObject.transform.position - Camera.main.transform.position);
        if ((dir).magnitude > waitDistance)
        {
            state = GuideState.Waiting;
            lookDir = dir;
            //LookAtDir(dir);
             return true;
        }
        return false;

    }
 
    public override void PersueWaypoint()
    {
        state = GuideState.Guiding;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y += 0.2f;
        MoveTowards(wayPointPos);
        Vector3 dir = (gameObject.transform.position - wayPointPos).normalized;

        lookDir = dir;


        if (wayPointPos.Equals(gameObject.transform.position))
        {
            WaypointRached();
        }
    }
    public void MoveTowards(Vector3 target) {
        Vector3 pos = Vector3.MoveTowards(gameObject.transform.position, target, movementSpeed * Time.deltaTime);
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
    }
    public override void GuidanceComplete() {
        guiding = false;
        //lookDir = (Camera.main.transform.position - gameObject.transform.position).normalized;
    }
}
