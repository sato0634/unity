using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSC : MonoBehaviour
{
    public float speed;
    public float amp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time;
        this.transform.position = new Vector3(amp * Mathf.Cos( speed * t), 0 , amp* Mathf.Sin( speed * t ));      
           // this.transform.position = new Vector3(amp * t*t-3-speed*t, 0, 1);
    }
}
