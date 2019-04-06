using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Recieves and parses telemetry data from a server. 
/// </summary>
public class JSONParse : MonoBehaviour {

    public string m_JSONString;

    [Header("Server URLS")]
    public string url = "";
    public string switchUrl = "";
    public const int OBJECTID_LENGTH = 47;

    private OutputErrorData m_OutputErrorData;

    [Header("Display Text")]
    public Text bioText;

    public GameObject[] bubbles;
    public SuitDataElement[] m_SuitDataUIElements;

    public Text timeLeftText;

    public LineGraph lineGraph; 

    [Space(10)]

    // Expected ranges for telemetry data 
    [Header("Ranges")]

    [Space(5)]
    public float m_HeartRateLow;
    public float m_HeartRateHigh;

    [Space(5)]
    public float m_P_SuitLow;
    public float m_P_SuitHigh;

    [Space(5)]
    public float m_P_SubLow;
    public float m_P_SubHigh;

    [Space(5)]
    public float m_T_SubLow;
    public float m_T_SubHigh;

    [Space(5)]
    public float m_V_FanLow;
    public float m_V_FanHigh;

    [Space(5)]
    public float m_P_O2Low;
    public float m_P_O2High;

    [Space(5)]
    public float m_Rate_O2Low;
    public float m_Rate_O2High;

    [Space(5)]
    public float m_Cap_BatteryLow;
    public float m_Cap_BatteryHigh;

    [Space(5)]
    public float m_P_H2O_GLow;
    public float m_P_H2O_GHigh;

    [Space(5)]
    public float m_P_H2O_LLow;
    public float m_P_H2O_LHigh;

    [Space(5)]
    public float m_P_SOPLow;
    public float m_P_SOPHigh;

    [Space(5)]
    public float m_Rate_SOPLow;
    public float m_Rate_SOPHigh;

    void Start ()
    {
        m_OutputErrorData = FindObjectOfType<OutputErrorData>();
        InvokeRepeating("UpdateSystemData", 1, 5);
        InvokeRepeating("UpdateSystemSwitchData", 2, 3);
    }

    private void UpdateSystemData()
    {
        StartCoroutine(RunWWW());
        
    }

    private void UpdateSystemSwitchData()
    {
        StartCoroutine(RunSwitchWWW());
    }
    
    IEnumerator RunWWW()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            string json = ""; 
            if (www.isNetworkError)
            {
                bioText.text = "NETWORK ERROR Not connected to server :(\n"; 
                bioText.text += www.error; 
                
            } else if (www.isHttpError)
            {
                bioText.text = "HTTP ERROR Not connected to server :( :( :(";
                bioText.text += www.error; 
            } else 
            {
                // We are connected to the server 

                // Use line below only if the JSON comes in with brackets around it 
                //json = RemoveBrackets(www.downloadHandler.text);
                json = www.downloadHandler.text;
                //Debug.Log("Connected to biometrics server");
            }
            
            if (!json.Equals(""))
            {
                SuitData jsonObject = JsonUtility.FromJson<SuitData>(json);
                LineData data = new LineData();

                data.m_DataValue = jsonObject.v_fan;
                data.m_Time = Time.time; 

                //lineGraph.AddLineDataPoint(data);
                Debug.Log("Parsing");
                UpdateUI(jsonObject);

                /*
                bubbles[1].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_suit);
                bubbles[2].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_sub);
                bubbles[3].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.t_sub);
                bubbles[4].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.v_fan);
                bubbles[5].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_o2);
                bubbles[6].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.cap_battery);
                bubbles[7].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_h2o_g);
                bubbles[8].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_h2o_l);
                bubbles[9].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.p_sop);
                bubbles[10].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.rate_sop);

                bubbles[12].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.t_battery);
                bubbles[13].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.t_oxygen); 
                bubbles[14].GetComponent<EnvelopeChameleon>().setCurrentValue(jsonObject.t_water);
                */
                // Get the lesser time between oxygen and battery 
                string identifier = "";
                string lesserTime = getLesserTime(jsonObject.t_oxygen, jsonObject.t_battery, out identifier);
                
                // Display Time Left 
                timeLeftText.text = "Time Left: " + lesserTime + " (" + identifier + ")";
            } else
            {
                Debug.Log("no data recieved from the server"); 
            }
            
        }
    }

    private void UpdateUI(SuitData data)
    {
        m_SuitDataUIElements[0].SetData("Internal Suit Pressure", data.t_water.ToString());
        m_SuitDataUIElements[1].SetData("Time Life Battery", data.t_battery.ToString());
        m_SuitDataUIElements[2].SetData("Time Life Oxygen", data.t_oxygen.ToString());
        m_SuitDataUIElements[3].SetData("Time Life Water", data.t_water.ToString());
        m_SuitDataUIElements[4].SetData("SUB Pressure", data.p_sub.ToString());
        m_SuitDataUIElements[5].SetData("SUB Tempurature", data.t_sub.ToString());
        m_SuitDataUIElements[6].SetData("Fan Tachometer", data.v_fan.ToString());
        m_SuitDataUIElements[7].SetData("Extravehicular Activity Rate", data.p_o2.ToString());
        m_SuitDataUIElements[8].SetData("Oxygen Pressure", data.p_o2.ToString());
        m_SuitDataUIElements[9].SetData("Oxygen Rate", data.rate_o2.ToString());
        m_SuitDataUIElements[10].SetData("Battery Capacity", data.cap_battery.ToString());
        m_SuitDataUIElements[11].SetData("H20 Gas Pressure", data.p_h2o_g.ToString());
        m_SuitDataUIElements[12].SetData("H20 Liquid Pressure", data.p_h2o_l.ToString());
        m_SuitDataUIElements[13].SetData("SOP Pressure", data.p_sop.ToString());
        m_SuitDataUIElements[14].SetData("SOP Rate", data.rate_sop.ToString());
    }

    private void UpdateSwitchUI(SuitDataSwitch switchData)
    {
        m_SuitDataUIElements[15].SetData("Battery Amp High", switchData.battery_amp_high.ToString());
        m_SuitDataUIElements[16].SetData("Battery VDC Low", switchData.battery_vdc_low.ToString());
        m_SuitDataUIElements[17].SetData("Suit Pressure Low", switchData.p_suit_low.ToString());
        m_SuitDataUIElements[18].SetData("SOP On", switchData.p_sop_on.ToString());
        m_SuitDataUIElements[19].SetData("Suit Pressure Emergency", switchData.p_suit_emergency.ToString());
        m_SuitDataUIElements[20].SetData("Suit Pressure High", switchData.p_suit_high.ToString());
        m_SuitDataUIElements[21].SetData("O2 Use High", switchData.o2_use_high.ToString());
        m_SuitDataUIElements[22].SetData("SOP Pressure Low", switchData.p_suit_low.ToString());
        m_SuitDataUIElements[23].SetData("Fan Failure", switchData.fan_error.ToString());
        m_SuitDataUIElements[24].SetData("CO2 High", switchData.co2_high.ToString());
        m_SuitDataUIElements[25].SetData("Vehicle Power Present", switchData.vehicle_power.ToString());
        m_SuitDataUIElements[26].SetData("H20 Is Off", switchData.h2o_off.ToString());
        m_SuitDataUIElements[27].SetData("O2 is Off", switchData.o2_off.ToString());
    }

    private string getLesserTime(string strOxygen, string strBattery, out string identifier)
    {
        string[] strOxygenParsed = strOxygen.Split(':');
        string[] strBatteryParsed = strBattery.Split(':'); 

        for (int i = 0; i < strOxygenParsed.Length; i++)
        {
            int intOxygen = int.MaxValue; 
            int intBattery = int.MaxValue;

            bool oxygenOk = int.TryParse(strOxygenParsed[i], out intOxygen);
            bool batteryOk = int.TryParse(strBatteryParsed[i], out intBattery);

            if (!oxygenOk)
            {
                identifier = "Error";
                return "Incorrect Oxygen Value";
            }

            if (intOxygen == 0)
            {
                MusicManager.m_Instance.PlaySong(MusicManager.m_Instance.m_Skyfall);
                identifier = "Oxygen is out";
                return strOxygen;
            }

            if (!batteryOk)
            {
                identifier = "Error";
                return "Incorrect Battery Value"; 
            }

            if (intOxygen > intBattery)
            {
                identifier = "Battery"; 
                return strBattery; 
            } else if (intOxygen < intBattery)
            {
                identifier = "Oxygen"; 
                return strOxygen; 
            } 
        }

        // The two strings are equal 
        identifier = "Battery and Oxygen"; 
        return strBattery; 
    }

    IEnumerator RunSwitchWWW()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(switchUrl))
        {
            yield return www.SendWebRequest();

            string json = "";
            if (www.isNetworkError)
            {
                bioText.text = "NETWORK ERROR Not connected to server :(\n";
                bioText.text += www.error;

            }
            else if (www.isHttpError)
            {
                bioText.text = "HTTP ERROR Not connected to server :( :( :(";
                bioText.text += www.error;
            }
            else
            {
                // We are connected to the server 

                // Use line below only if the JSON comes in with brackets around it 
                //json = RemoveBrackets(www.downloadHandler.text);  
                json = www.downloadHandler.text;

            }

            SuitDataSwitch jsonObject = JsonUtility.FromJson<SuitDataSwitch>(json);

            CheckSuitSwitches(jsonObject);
            UpdateSwitchUI(jsonObject);
        }
    }

    private void CheckAllRanges(SuitData ndt)
    {
        if (CheckValueRange(ndt.heart_bpm, m_HeartRateLow, m_HeartRateLow)) Debug.Log("Heart Rate going hardcore");
        if (CheckValueRange(ndt.p_suit, m_P_SuitLow, m_P_SuitHigh)) Debug.Log("Suit Pressure Bad, fix it");
        if (CheckValueRange(ndt.p_sub, m_P_SubLow, m_P_SubHigh)) Debug.Log("Outside Pressure Crazy, the world is ending");
        if (CheckValueRange(ndt.t_sub, m_T_SubLow, m_T_SubHigh)) Debug.Log("Outside Temperature is Crazy, the universe is probably breaking");
        if (CheckValueRange(ndt.v_fan, m_V_FanLow, m_V_FanHigh)) Debug.Log("Fan Rotation not right, get working on spinning that out");
        if (CheckValueRange(ndt.p_o2, m_P_O2Low, m_P_O2High)) Debug.Log("O2 pressure not right, breathe drastically and heavily");
        if (CheckValueRange(ndt.rate_o2, m_Rate_O2Low, m_Rate_O2High)) Debug.Log("O2 rate is wrong, the flow ain't flowing");
        if (CheckValueRange(ndt.cap_battery, m_Cap_BatteryLow, m_Cap_BatteryHigh)) Debug.Log("Battery is low, lights are about to go out");
        if (CheckValueRange(ndt.p_h2o_g, m_P_H2O_GLow, m_P_H2O_GHigh)) Debug.Log("Gas of H2O pressure is bad, that's probably not good");
        if (CheckValueRange(ndt.p_h2o_l, m_P_H2O_LLow, m_P_H2O_LHigh)) Debug.Log("Liquid of H2O pressure is bad, woops...");
        if (CheckValueRange(ndt.p_sop, m_P_SOPLow, m_P_SOPHigh)) Debug.Log("SOP is bad, thats no good");
        if (CheckValueRange(ndt.rate_sop, m_Rate_SOPLow, m_Rate_SOPHigh)) Debug.Log("Rate of SOP is bad, sorry about that");
    }

    private void CheckSuitSwitches(SuitDataSwitch ndts)
    {
        m_OutputErrorData.ClearText();

        if (ndts.h2o_off == "true") m_OutputErrorData.OutputErrorText("H2O IS OFF");
        if (ndts.sspe == "true") m_OutputErrorData.OutputErrorText("SUIT P EMERG");
        if (ndts.fan_error == "true") m_OutputErrorData.OutputErrorText("FAN SW OFF");
        if (ndts.vent_error == "true") m_OutputErrorData.OutputErrorText("NO VENT FLOW"); // Add vent rpms 
        if (ndts.vehicle_power == "true") m_OutputErrorData.OutputErrorText("VEHICLE POWER AVAIL");
        if (ndts.o2_off == "true") m_OutputErrorData.OutputErrorText("O2 IS OFF");
        if (ndts.p_sop_on == "true") m_OutputErrorData.OutputErrorText("SECONDARY OXYGEN TANK ON"); 
    }

    private string CleanUpJSON(string json)
    {
        string newJson = json.Remove(1,OBJECTID_LENGTH);

        return newJson;
    }

    private string RemoveBrackets(string json)
    {
        string newJson = json;
        newJson = newJson.Remove(newJson.Length - 1);
        newJson = newJson.Remove(0, 1);

        return newJson;
    }

    private bool CheckValueRange(float value, float lowRange, float highRange)
    {
        bool b = false;
        if (value < highRange && value > lowRange)
        {
            b = true;
           // Debug.Log("Value is good. You good my dude, or dudette");
        }

        return b;
    }
}


//////////////////////// All telemetry variables are defined here /////////////////////////////////
[System.Serializable]
public class SuitData
{
    public string create_date = "";
    public int heart_bpm = 0;
    public float p_suit = 0;
    public float p_sub = 0;
    public int t_sub = 0;
    public int v_fan = 0;  // fan speed 
    public int p_o2 = 0;
    public float rate_o2 = 0.0f;
    public int cap_battery = 0;
    public int p_h2o_g = 0;
    public int p_h2o_l = 0;    // if the delta between _g and _l is more than 3, then water quantity low 
    public int p_sop = 0;
    public float rate_sop = 0.0f;
    public string t_battery = "";
    public string t_oxygen = "";
    public string t_water = "";
}

[System.Serializable]
public class SuitDataSwitch
{
    //TODO make enum Warning, Nominal, VeryBad

    public string create_date = "";
    public string sspe = "";  // SUIT P EMERG    - out of oxygen or regulator is not working 
    public string fan_error = ""; // FAN SW OFF   - 
    public string vent_error = ""; // NO VENT FLOW  - <v_fan> rpm  
    public string vehicle_power = ""; // VEHICLE POWER AVAIL   - you should switch to save suit power 
    public string h2o_off = ""; // H20 IS OFF  - for cooling inside the suit 
    public string o2_off = "";  // O2 IS OFF
    public string o2_use_high = "";  // O2 IS OFF
    public string co2_high = "";  // O2 IS OFF
    public string battery_amp_high = "";
    public string battery_vdc_low = "";
    public string p_suit_low = "";
    public string p_sop_on = "";
    public string p_suit_emergency = "";
    public string p_suit_high = "";


    // BAT VDC LOW / VAT VDC XX.X - if battery is under 15 V 

    // public bool p_sop = false; // SOP P LOW   SOP P <p_sop>  SOP RATE <rate_sop>  
    // triggered when O2 rate is greater than 10.2 psi/min -  O2 USE HIGH O2 RATE <rate_O2> 

    // public bool low_voltage = false;   // BAT VDC LOW    BAT VDC <t_battery> V 

    // water gas pressure - WATER QUANTITY LOW 
}
