using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof (Ant))]
public class FOVEditor : Editor
{
    private void OnSceneGUI()
    {
        Ant ant = (Ant)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(ant.transform.position, Vector3.forward, Vector3.up, 360, ant.RangeOfVision);
        Vector3 viewAngleA = ant.DirFromAngle(-ant.AngleOfVision/ 2,false);
        Vector3 viewAngleB = ant.DirFromAngle(ant.AngleOfVision/ 2,false);
        Handles.DrawLine(ant.transform.position, ant.transform.position + viewAngleA* ant.RangeOfVision);
        Handles.DrawLine(ant.transform.position, ant.transform.position + viewAngleB * ant.RangeOfVision);
    }
}
