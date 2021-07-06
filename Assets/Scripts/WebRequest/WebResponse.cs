using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebResponse
{
    public string JSONResponse { get; set; }
    public UnityWebRequest.Result result { get; set; }
    public string error_message { get; set; }
    public WebResponse(string JSONResponse, UnityWebRequest.Result result)
    {
        this.result = result;
        this.JSONResponse = JSONResponse;
    }
}
