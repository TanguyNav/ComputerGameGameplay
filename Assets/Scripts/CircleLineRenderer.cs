using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CircleLineRenderer : MonoBehaviour
{
    public int segments;
    public float radius;
    LineRenderer line;
   
    void Start ()
    {
        line = gameObject.GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        CreatePoints ();
    }

    void CreatePoints ()
    {
        float x;
        float y;
        float z = 0f;
   
        float angle = 0f;
   
        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
               
            line.SetPosition(i,new Vector3(x,y,z) );
               
            angle += (360f / segments);
        }
    }
}
