using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuitDataElement : MonoBehaviour
{
    public string m_DataTitle;
    public string m_DataValue;
    public string m_DataUnitName;

    private Text m_BiometricTitle;
    private Text m_BiometricValue;
    // Start is called before the first frame update
    void Start()
    {
        m_BiometricTitle = transform.GetChild(5).gameObject.GetComponent<Text>(); //title is 6th child element in biometric data
        m_BiometricValue = transform.GetChild(4).gameObject.GetComponent<Text>(); //value is 5th child element in biometric data

        m_BiometricTitle.text = m_DataTitle;
        m_BiometricValue.text = m_DataValue;
    }

    public void SetData(string dataTitle, string dataValue)
    {
        if (m_BiometricTitle == null || m_BiometricValue == null) return;

        m_BiometricTitle.text = dataTitle;
        m_BiometricValue.text = dataValue;
    }
}
