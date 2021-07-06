using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class GetManager : RequestManager
{
  //  string baseURL = "http://192.168.178.35:8000";
  void Start() {
     //   StartCoroutine(GetAllTask());
      //  StartCoroutine(GetAllProjects());
    }
 
    public IEnumerator GetAllTask() {
        UnityWebRequest www = UnityWebRequest.Get(baseURL+"/api/tasks");
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
 
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
        public IEnumerator GetAllProjects() {
        UnityWebRequest www = UnityWebRequest.Get(baseURL+"/api/projects");
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
 
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }


    public IEnumerator GetAllImages(string project_id, string file_name) {
        UnityWebRequest www = UnityWebRequest.Get(baseURL+"/api/projects/"+project_id+"/images/"+file_name);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
 
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }




    
}
