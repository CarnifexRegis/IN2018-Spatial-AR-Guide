using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.CoreModule;

public class Guide : Guidance
{

    float movementSpeed = 0.4f;
    // Start is called before the first frame update
    public float followDistance;
    public float waitDistance;
    public UnityEvent startGuidance;
    public bool startedGuidance = false;
    public bool guiding = false;
    void Start()
    {
        rotationSpeed = 14.0f;
        InitializeGuidance();
        SpeechManager.Instance.Speak("Hello Stranger 77777777777777777777777777"); //
    }
    void Update()
    {
        // if()
        if (wayPointIndex < wayPoints.Count)
        {
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
    public void IdleUpdate()
    {
        Vector3 dir = (gameObject.transform.position - Camera.main.transform.position).normalized;
        lookDir = dir;
        //LookAtDir(dir);
        Vector2 player = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 guide = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        if (Vector2.Distance(player, guide) < followDistance)
        {
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
    public void MoveTowards(Vector3 target)
    {
        Vector3 pos = Vector3.MoveTowards(gameObject.transform.position, target, movementSpeed * Time.deltaTime);
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
    }
    public override void GuidanceComplete()
    {
        //guiding = false;
        //lookDir = (Camera.main.transform.position - gameObject.transform.position).normalized;
    }
    public void StartGuidance()
    {
        guiding = true;
        startGuidance.Invoke();
    }
}