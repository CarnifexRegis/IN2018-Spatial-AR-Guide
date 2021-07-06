using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

// needed to deliver response string
[Serializable]
public class ResponseEvent : UnityEvent<WebResponse>
{

}
