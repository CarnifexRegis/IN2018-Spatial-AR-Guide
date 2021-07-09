using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poster : MonoBehaviour
{

    public GameObject guide;
    public GameObject canvas;
    void Update()
    {
        gameObject.transform.position = guide.transform.position + new Vector3(1.2f,0.1f,0.0f);
        gameObject.transform.rotation = guide.transform.rotation;
    }
    public void SetCanvasActive(bool active) {
        canvas.SetActive(active);
    }
}
