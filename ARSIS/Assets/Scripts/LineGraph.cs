using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//store data
//have set width but change step to have it fit on graph
//link with warning system to start showing graph data

[RequireComponent(typeof(LineRenderer))]
public class LineGraph : MonoBehaviour {

    public GameObject m_PointObject; //object that spawns at point position
    public Transform m_GraphOrigin;
    public LineData m_Input; //temporary for sending data into graph

    public float m_Width = 4.0f; //horizontal width of graph in Unity units
    public float m_Height = 2.0f; //vertical height of graph in Unity units
    public float m_StepLength = 1.0f;

    public float m_MaxRange = 50.0f; //max value of data that is used

    private LineRenderer m_LineRenderer;

    private List<LineData> m_LineData;
    private float m_Step;
    private int m_PointCount = 0; //current number of points created

    void Start ()
    {
        m_LineData = new List<LineData>();
        m_LineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Input.m_DataValue = 86.0f;
            AddLineDataPoint(m_Input);
        }
	}

    public void AddLineDataPoint(LineData lineDataPoint) //send in min and max for data value
    {
        lineDataPoint.m_Time = Time.time; //set xval of point
        GameObject point = Instantiate(m_PointObject);

        Debug.Log(lineDataPoint.m_DataValue);

        //map datapoint to the scale of the graph
        float mappedXDataPoint = Mathf.Repeat(Time.time, m_Width);
        float mappedYDataPoint = Map(lineDataPoint.m_DataValue, 60.0f, m_MaxRange, 0, m_Height);
        Vector3 offset = new Vector3(mappedXDataPoint, mappedYDataPoint, 0);

        point.transform.parent = m_GraphOrigin.transform;
        point.transform.localPosition = Vector3.zero + offset;

        m_Step += m_StepLength; //move to next step(possibly switch to Time.time)
        m_PointCount++; //point count is int tracking indices for line renderer

        m_LineData.Add(lineDataPoint);

        SetLineRendererPosition(point.transform.position);
    }

    private void SetLineRendererPosition(Vector3 originlocalPosition)
    {
        Vector3 localLineRendererPosition = transform.InverseTransformPoint(originlocalPosition); //take global position, and transform it to local space of line renderer
        m_LineRenderer.positionCount = m_PointCount;
        m_LineRenderer.SetPosition(m_PointCount - 1, localLineRendererPosition); //point count - 1 because 0 based index for line renderer
    }

    private float Map(float value, float startMin, float startMax, float endMin, float endMax)
    {
        float diff = (value - startMin) / (startMax - startMin);

        float newValue = (endMin * (1 - diff)) + (endMax * diff);

        return newValue;
    }
}

//class that holds data for points including location, and data type
[System.Serializable]
public struct LineData
{
    public float m_Time; //step or x value this data point is on
    public float m_DataValue;
}
