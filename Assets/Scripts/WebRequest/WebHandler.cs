using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

//using SimpleJSON;
//using System.Text.Json;
//https://github.com/Bunny83/SimpleJSON
//Json parser needed for dictionaries

//uses Newtonsoft.jason because .NET  2.0 does not suppor dictionaries
//https://docs.microsoft.com/de-de/visualstudio/gamedev/unity/unity-scripting-upgrade

// save image locations to json file

//Note for json parsing if a null object is sent the client recieves a message "null" not an empt value
public class WebHandler : MonoBehaviour
{
    [SerializeField]
    protected string baseURL = "http://192.168.178.35:8000";
    [SerializeField]
    protected PostManager postManager;
    [SerializeField]
    protected PutManager putManager;
    [SerializeField]
    protected DeleteManager deleteManager;
    [SerializeField]
    protected GetManager getManager;

    [SerializeField]
    protected Text errorView;

    public StringEvent projectIDChanged;



    // Start is called before the first frame update
    protected virtual void Start()
    {
        postManager.setBaseURL(baseURL);
        putManager.setBaseURL(baseURL);
        deleteManager.setBaseURL(baseURL);
        getManager.setBaseURL(baseURL);

    }

    // Update is called once per frame
    void Update(){
    }

    public void OnPostProjectResponse(WebResponse webResponse) {

       // errorView.text += "postproject answer recieved";
        if (webResponse == null) {
            Debug.Log("faulty response");
            return;
        }
      
            // Debug.Log("Succesffuly posted project and recieved callback"+reponse.JSONResponse);
        Debug.Log("PostProject result: "+webResponse.result);
        Debug.Log("PostProject response"+webResponse.JSONResponse);
        if (webResponse.result != UnityWebRequest.Result.Success)
        {
            errorView.text += "postproject failed";
        }
        else
        {
            if (webResponse.JSONResponse == null || webResponse.JSONResponse == "null")
            {
                Debug.Log("Empty JSONResponse");
                return;
            }
            try
            {
                //Dictionary<string, string> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(webResponse.JSONResponse);
                
               // var N = JSON.Parse(webResponse.JSONResponse);
            }
            catch (Exception e)
            {
                Debug.Log("ErrorParsingJSON:" + e.ToString());
            }
    
        }
     
    }
    public void OnPostGetAnchors(WebResponse webResponse)
    {
        
    }


}

