using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : Guidance
{
    //Stays on Idle if no target available and Guiding if they are available
    // Start is called before the first frame update
    void Start()
    {
        rotationSpeed = 14.0f;
        InitializeGuidance();
    }

    // Update is called once per frame
    void Update()
    {
        if (guiding)
        {
            if (wayPointIndex >= wayPoints.Count)
            {
                //guiding = false;
                GuidanceComplete();
                return;

            }
                PersueWaypoint();

        }
    }
    public override void GuidanceComplete() {

    }
    public override void PersueWaypoint() {
        state = GuideState.Guiding;
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y += 0.4f;
        Vector3 dir = (gameObject.transform.position - wayPointPos).normalized;
        lookDir = dir;
        LookAtDir(lookDir);

        float distance = Vector2.Distance(new Vector2(wayPointPos.x, wayPointPos.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z));
        if (distance <=0.6f)
        {
            WaypointRached();
            //wayPointIndex %= (wayPoints.Count);
        }
    }

}
