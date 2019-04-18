using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSingleton : MonoBehaviour
{
    public static WarningSingleton m_Singleton;

    [HideInInspector]
    public bool m_DataInWarning = false;
    // Start is called before the first frame update
    void Awake()
    {
        m_Singleton = this;
    }

    public void BiometricInWarning()
    {
        m_DataInWarning = true;
        FindObjectOfType<OutputErrorData>().OutputErrorText("Warning in Biometrics");
    }

    public void BiometricInNominal()
    {
        m_DataInWarning = false;
    }
}
