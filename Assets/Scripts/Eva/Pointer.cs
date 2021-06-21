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
        Vector3 wayPoinPos = wayPoints[wayPointIndex].transform.position;
        wayPoinPos.y += 0.3f;
        Vector3 pos = AR_Camera.transform.position + AR_Camera.transform.forward * 1.5f;
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        guide.transform.position = pos;

        Vector3 dir = (guide.transform.position - wayPoinPos).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        guide.transform.rotation = Quaternion.Slerp(guide.transform.rotation, rot, rotationSpeed * Time.deltaTime);

        float distance = Vector2.Distance(new Vector2(wayPoinPos.x, wayPoinPos.z), new Vector2(AR_Camera.transform.position.x, AR_Camera.transform.position.z));
        if (distance <=0.2f)
        {
            WaypointRached();
            //wayPointIndex %= (wayPoints.Count);
        }
    }

}
