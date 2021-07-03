using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : Guidance
{

    float movementSpeed = 0.4f;
    // Start is called before the first frame update
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
            if (!WaitForPlayer())
            {
                PersueWaypoint();
            }

        }
        else
        {
            state = GuideState.Idle;
        }

    }

    public bool WaitForPlayer()
    {
        //if distance to guide is too big wait for the player and look at him / Maby wave at the player
        Vector3 dir = (AR_Camera.transform.position - guide.transform.position).normalized;
        if ((dir).magnitude > 2)
        {
            state = GuideState.Waiting;

            // dir.y = 0; // keep the direction strictly horizontal
            Quaternion rot = Quaternion.LookRotation(dir);
             //slerp to the desired rotation over time
            guide.transform.rotation = Quaternion.Slerp(guide.transform.rotation, rot, rotationSpeed * Time.deltaTime);
             return true;
        }
        return false;

    }
    public override void PersueWaypoint()
    {
        state = GuideState.Guiding;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y += 0.3f;
        Vector3 pos = Vector3.MoveTowards(guide.transform.position, wayPointPos, movementSpeed * Time.deltaTime);
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        guide.transform.position = pos;

        Vector3 dir = (guide.transform.position - wayPointPos).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        guide.transform.rotation = Quaternion.Slerp(guide.transform.rotation, rot, rotationSpeed * Time.deltaTime);


        if (wayPointPos.Equals(guide.transform.position))
        {
            WaypointRached();
            //wayPointIndex %= (wayPoints.Count);
        }
    }
    public override void GuidanceComplete() {
    
    }
}
