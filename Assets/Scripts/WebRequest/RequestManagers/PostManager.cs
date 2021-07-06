using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Text;
//code taken from https://www.youtube.com/watch?v=Bp4h4pmcw3c&t=67s
//https://github.com/herbou/Unity_UploadImagesToServer

//{"project_id":"P-049c6d21-5e01-4bef-be47-e3cfbb2a2e19"}
[Serializable]
public enum ImageType
{
	PNG,
	JPG
}

public class PostManager : RequestManager
{
	public ResponseEvent PostProjectResponse;
    public ResponseEvent PostTaskResponse;
    public ResponseEvent PostImageResponse;
	public IEnumerator PostProject( string project_name,string project_owner) {
		WWWForm form = new WWWForm();

       // form.AddField("myField", "myData");
	   form.AddField("project_name", project_name);
	   form.AddField("project_owner", project_owner);
		//string json = JsonUtility.ToJson(center_position);
	 	//form.AddField("center_position", json);

        using (UnityWebRequest www = UnityWebRequest.Post(baseURL+"/api/projects", form))
        {
            yield return www.SendWebRequest();
			WebResponse answer;
            if (www.result != UnityWebRequest.Result.Success)
            {
				//  Debug.Log(www.error);
				answer = new WebResponse(""+www.error, www.result);
			}
            else
            {
				answer = new WebResponse(www.downloadHandler.text, www.result);
			}

			try{
				PostProjectResponse.Invoke(answer);
			}catch (Exception e) {
				Debug.Log(e.ToString());
			}
			
		}
    }
	public	IEnumerator PostTask(string project_id, string provider) {
		WWWForm form = new WWWForm();
       // form.AddField("myField", "myData");
	   form.AddField("project_id", project_id);
	   form.AddField("provider", provider);

        using (UnityWebRequest www = UnityWebRequest.Post(baseURL+"/api/tasks", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
				     Debug.Log(www.downloadHandler.text);
 
            // Or retrieve results as binary data
           		 byte[] results = www.downloadHandler.data;
                Debug.Log("Form upload complete!");
            }
        }
		Destroy(this.gameObject);
	}
}
