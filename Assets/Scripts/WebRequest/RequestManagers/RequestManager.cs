using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RequestManager : MonoBehaviour
{
    public string baseURL = "";

    public void setBaseURL(string baseURL){
        this.baseURL = baseURL;
    }

}
