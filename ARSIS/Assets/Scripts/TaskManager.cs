using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using System;
using UnityEngine.Networking;

/// <summary>
/// Manages all tasks. Tasks are currently hard-coded in; however, we are working on a solution to pull them from a server. 
/// </summary>
public class TaskManager : MonoBehaviour {

    // Singleton 
    public static TaskManager S;

    // List of all steps organized by procedure 
    public List<Procedure> allProcedures = new List<Procedure>(); 

    // Lists of individual steps 
    public Procedure disableAlarm = new Procedure("Disable Alarm", false, 24);
    public Procedure reroutPower = new Procedure("Reroute Power", false, 30);
    public Procedure lightSwitch = new Procedure("Light Switch", false, 20);
    public Procedure superNova = new Procedure("Supernova", false, 11); 

    // Static Array of images for each task 
    public Texture2D[] images; 

    // Web Connection 
    public string url = "";
    private OutputErrorData m_OutputErrorData;

    // For testing 
    //public GameObject cube; 

    void Start () {
        S = this; 
        
        populateProcedures();

        allProcedures.Add(superNova);
        allProcedures.Add(disableAlarm);
        allProcedures.Add(reroutPower);
        allProcedures.Add(lightSwitch);
      
        
        for (int i = 0; i < allProcedures.Count; i++)
        {
            VoiceManager.S.addProcedureCommand(allProcedures[i].procedure_title); 

        }


        m_OutputErrorData = FindObjectOfType<OutputErrorData>();
        InvokeRepeating("UpdateSystemData", 1, 5);
    }

    public string getStep(int task, int step)
    {
        string retval; 
        try
        {
            retval = allProcedures[task].getStep(step - 1).text; 
        } catch (ArgumentOutOfRangeException)
        {
            Debug.Log("Argument out of range"); 
            retval = ""; 
        } catch (NullReferenceException)
        {
            Debug.Log("Null reference exception for task " + task + " step " + step); 
            retval = ""; 
        }
        return retval; 
    }

    public int getProcedureIndexByName(string name)
    {
        for (int i = 0; i < allProcedures.Count; i++)
        {
            if (allProcedures[i].procedure_title.Equals(name) )
            {
                return i; 
            }
        }
        return -1; 
    }

    public Texture2D getPic(int task, int step)
    {
        Texture2D retval;
        try
        {
            retval = allProcedures[task].getStep(step - 1).getImage();
        }
        catch (ArgumentOutOfRangeException)
        {
            retval = null;
        }
        return retval;
    }

    public string getWarning(int task, int step)
    {
        string retval; 
        try
        {
            retval = allProcedures[task].getStep(step - 1).warning; 
        } catch (ArgumentOutOfRangeException)
        {
            retval = ""; 
        }
        return retval; 
    }

    // Server Connection Stuffs 
    private void UpdateSystemData()
    {
        StartCoroutine(RunWWW());
    }

    IEnumerator RunWWW()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            string fromServer = "";
            if (www.isNetworkError)
            {
                Debug.Log("NETWORK ERROR Not connected to tasklist server :(");
            }
            else if (www.isHttpError)
            {
                Debug.Log("HTTP ERROR Not connected to tasklist server :(");
            }
            else
            {
                //Debug.Log("Connected to tasklist server"); 
                fromServer = www.downloadHandler.text;

                //Debug.Log(fromServer);

                Procedure jsonObject = JsonUtility.FromJson<Procedure>(fromServer);

                //Debug.Log(jsonObject.steps[0].image); 
                //string image = (String)jsonObject.steps[0].image;
                //Debug.Log(image); 

                //String b64String = (String)fromSocket["image"];
            }
        }
    }


     // Staticly creates all tasks 
    private void populateProcedures()
    {
        disableAlarm.addStep(0, new serStep("1. On the RIGHT side of the EVA kit, locate and use the PANEL ACCESS KEY to unlock the PANEL ACCESS DOOR LOCKS.", images[0], "CAUTION: The keys are on the tension-spring cable."));
        disableAlarm.addStep(1, new serStep("2. Carefully return keys to the side of the EVA kit.", null, null));
        disableAlarm.addStep(2, new serStep("3. Insert your fingers in the CENTER OPENING and secure the PANEL ACCESS DOOR in an OPEN position.", images[1], "WARNING: Door can accidentally close."));
        disableAlarm.addStep(3, new serStep("4. On your belt, use the BLUE CARABINEER to securely tether to the TEHTER CABLE inside the STORAGE.", images[2], "CAUTION: Notice the TETHER CABLE is adjustable."));
        disableAlarm.addStep(4, new serStep("5. Locate the E-STOP button and gently press down to temporarily disable the alarm.", images[3], ""));
        disableAlarm.addStep(5, new serStep("6. Locate the FUSIBLE DISCONNECT box and tether the BLUE CARABINEER to the TETHER CABLE.", null, ""));
        disableAlarm.addStep(6, new serStep("7. Remove the BLUE CARABINEER from the FUSIBLE DISCONNECT box and transfer it to STORAGE.", images[4], ""));
        disableAlarm.addStep(7, new serStep("8. Open the FUSIBLE DISCONNECT box and secure the lid in the open position.", images[5], "CAUTION: Pull the locking tab toward STORAGE with the index finger while lifting the cover with the thumb."));
        disableAlarm.addStep(8, new serStep("9.Locate the BLACK DISCONNECT and tether if to the TEHTER CABLE.", null, ""));
        disableAlarm.addStep(9, new serStep("10. Remove the DISCONNECT and place it in STORAGE.", images[6], "CAUTION: Pull up with the index and middle fingers while pushing down on the FUSE ACCESS PANEL with the thumb."));
        disableAlarm.addStep(10, new serStep("11. Tether the FUSE ACCESS OANEL to the TEHTER CABLE.", null, ""));
        disableAlarm.addStep(11, new serStep("12. Remove the FUSE ACCESS PANEL by pulling straight up.", images[7], ""));
        disableAlarm.addStep(12, new serStep("13. Place the FUSE ACCESS PANEL into STORAGE.", null, ""));
        disableAlarm.addStep(13, new serStep("14. Tether the ALARM FUSE to the TETHER CABLE.", null, ""));
        disableAlarm.addStep(14, new serStep("15. In STORAGE, locate the BLUE FUSE PULLER.", null, ""));
        disableAlarm.addStep(15, new serStep("16. Use the BLUE FUSE PULLER to remove ONLY the ALARM FUSE.", images[8], "CAUTION: Rock the ALARM FUSE with the FUSE PULLER when pulling up."));
        disableAlarm.addStep(16, new serStep("17. Return the ALARM FUSE and the FUSE PULLER to STORAGE.", null, ""));
        disableAlarm.addStep(17, new serStep("18. In STORAGE, locate the FUSE ACCESS PANEL and reinstall it into the FUSIBLE DISCONNECT box.", images[9], ""));
        disableAlarm.addStep(18, new serStep("19. Remove the FUSE ACCESS PANEL tether from the TETHER CABLE and stow inside.", null, "WARNING: All tethers are under spring tension and can retract quickly."));
        disableAlarm.addStep(19, new serStep("20. In STORAGE, locate the DISCONNECT and reinstall it into the FUSIBLE DISCONNECT box.", images[10], "CAUTION: The DISCONNECT must read 'ON' in the upper right corner to restore conductivity."));
        disableAlarm.addStep(20, new serStep("21. Remove the DISCONNECT tether form the TETHER CABLE.", null, "WARNING: All tethers are under spring tension and can retract quickly."));
        disableAlarm.addStep(21, new serStep("22. Close the FUSIBLE DISCONNECT box cover.", null, ""));
        disableAlarm.addStep(22, new serStep("23. In STORAGE, use the BLUE CARABINEER to clip and lock the FUSIBLE DISCONNECT box cover.", images[11], ""));
        disableAlarm.addStep(23, new serStep("24. Remove the BLUE CARABINEER's tether from the TETHER CABLE.", null, "WARNING: All tethers are under spring tension and can retract quickly."));
        
        reroutPower.addStep(0, new serStep("1. Locate the AUX. POWER INPUT.", null, ""));
        reroutPower.addStep(1, new serStep("2. Locate BATTER PACK and tether to TETHER CABLE.", null, ""));
        reroutPower.addStep(2, new serStep("3. Undo the BATTERY PACK LEADS from the AUX. POWER INPUT.", images[12], "CAUTION: Depress the red and black plastic hammers on the side of the AUX. POWER INPUT and pull the leads straight up."));
        reroutPower.addStep(3, new serStep("4. Remove BATTERY PACK from AUX.POWER INPUT.", null, ""));
        reroutPower.addStep(4, new serStep("5. Locate the ON/OFF switch on the back of the BATTERY PACK and switch it to the OFF position", images[13], ""));
        reroutPower.addStep(5, new serStep("6. Place the BATTERY PACK into STORAGE.", null, ""));
        reroutPower.addStep(6, new serStep("7. In STORAGE, find the replacement BATTER PACK.", null, ""));
        reroutPower.addStep(7, new serStep("8. Locate the ON/OFF switch on the back of the BATTERY PACK and switch it to the ON position.", null, ""));
        reroutPower.addStep(8, new serStep("9. Attach the replacement BATTER PACK onto the AUX. POWER INPUT by the Velcro.", null, ""));
        reroutPower.addStep(9, new serStep("10. Insert the BATTERY PACK leads back into the same colored ports.", null, "CAUTION: Depress the red and black plastic hammers on the side fo the AUX. POWER INPUT and push leads straight into their ports."));
        reroutPower.addStep(10, new serStep("11. Conduct a GENTLE PUSH TEST on the wires.", null, ""));
        reroutPower.addStep(11, new serStep("12. Remove the BATTERY PACK tether from the TETHER CABLE.", images[14], "WARNING: All tethers are under spring tension and can retract quickly."));
        reroutPower.addStep(12, new serStep("13. In STORAGE, locate the GRAY 220 VOLT PLUG.", null, ""));
        reroutPower.addStep(13, new serStep("14. Install it into the POWER OUT.", null, "CAUTION: Outlet and plug mate are stiff, ensure the full engagement of the plug into the outlet."));
        reroutPower.addStep(14, new serStep("15. Loacte the metal BUSS BAR and verify there are BLACK, GREEN, & WHITE BUSSES, each with 2 openings.", null, ""));
        reroutPower.addStep(15, new serStep("16. Insert the WHITE 220 VOLT LEAD into the LEFT WHITE BUSS opening and GENTLY TIGHTEN the thumbscrew.", images[15], "CAUTION: DO NOT over tighten the thumbscrew."));
        reroutPower.addStep(16, new serStep("17. Insert the GREEN 220 VOLT LEAD into the LEFT GREEN BUSS opening.", images[16], "CAUTION: DO NOT overtighten the thumbscrew."));
        reroutPower.addStep(17, new serStep("18. Insert the BLACK 220 VOLT LEAD into the LEFT BLACK BUSS opening.", images[17], "CAUTION: DO NOT over tighten the thumbscrew."));
        reroutPower.addStep(18, new serStep("19. Make sure the METAL LEADS are not sticking out the BACK of the BUSS BAR.", null, ""));
        reroutPower.addStep(19, new serStep("20. Cunduct a GENTLE PULL TEST on each cable.", null, ""));
        reroutPower.addStep(20, new serStep("21. In STORAGE, locate the 110 VOLT PLUG and install it in POWER IN.", images[18], "CAUTION: Lift cover with one hand while installing PLUG into the outlet with the other. The lid is spring-loaded."));
        reroutPower.addStep(21, new serStep("22. Insert the WHITE 110 VOLT PLUG LEAD into the RIGHT WHITE BUSS opening.", images[19], "CAUTION: DO NOT over tighten the thumbscrew."));
        reroutPower.addStep(22, new serStep("23. Insert the GREEN 110 VOLT PLUG LEAD into the RIGHT GREEN BUS opening.", images[20], "CAUTION: DO NOT over tighten the thumbscrew."));
        reroutPower.addStep(23, new serStep("24. Insert the BLACK 110 VOLT PLUB LEAD into the RIGHT BLACK BUSS opening.", images[21], "CAUTION: DO NOT over tighten the thumbscrew."));
        reroutPower.addStep(24, new serStep("25. Conduct a GENTLE PULL TEST on each cable.", images[22], ""));
        reroutPower.addStep(25, new serStep("26. In STORAGE, locate the E-STOP KEY.", images[23], ""));
        reroutPower.addStep(26, new serStep("27. Insert the KEY into the E-STOP and TURN to the RIGHT and button will pop up.", null, ""));
        reroutPower.addStep(27, new serStep("28. Remove the KEY and place it in STORAGE.", images[24], ""));
        reroutPower.addStep(28, new serStep("29. Locate the AUX. POWER SWITCH on the POWER IN box and switch it to the 'ON' position.", null, ""));
        reroutPower.addStep(29, new serStep("30. Can you please confirm YSE or NO that the SYSTEM GO indicator light is GREEN?", null, ""));

        lightSwitch.addStep(0, new serStep("1. Locate the Aux. Power Switch", images[25], ""));
        lightSwitch.addStep(1, new serStep("2. Flip Aux. power switch to OFF position", images[26], ""));
        lightSwitch.addStep(2, new serStep("3. Locate Power Outlet Lock Key tethered to Tether Ubolt", images[27], ""));
        lightSwitch.addStep(3, new serStep("4. Locate Power Outlet Lock", images[28], ""));
        lightSwitch.addStep(4, new serStep("5. Unlock Power Outlet Lock with Power Outlet Lock Key", images[29], ""));
        lightSwitch.addStep(5, new serStep("6. Retether Power Outlet Lock Key and Power Outlet Lock to Tether Ubolt", images[30], ""));
        lightSwitch.addStep(6, new serStep("7. Above Power Outlets, locate the Power Plug", images[31], ""));
        lightSwitch.addStep(7, new serStep("8. Locate the  Power Outlets inside the weather container.", images[32], ""));
        lightSwitch.addStep(8, new serStep("9. Install Power Plug into Power Outlet", images[33], ""));
        lightSwitch.addStep(9, new serStep("10. Close weather container", images[34], ""));
        lightSwitch.addStep(10, new serStep("11. Insert volt lead in the right silver buss opening and gently tighten the thumb screw", images[35], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(11, new serStep("12. Locate Wrench Tethered to Tether Ubolt", images[36], ""));
        lightSwitch.addStep(12, new serStep("13. Insert the volt lead in the right terminal block and tighten the nut using the wrench", images[37], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(13, new serStep("14. Insert the lead in the left silver buss opening and gently tighten the thumb screw", images[38], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(14, new serStep("15. Insert the lead in the Left Terminal Block and tighten the nut using the wrench", images[39], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(15, new serStep("16. Conduct a gentle pull test on each", images[40], ""));  
        lightSwitch.addStep(16, new serStep("17. Once secure retether Wrench to Tether Ubolt.", images[41], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(17, new serStep("18. Locate the AUX power switch and switch it to the ON position", images[42], "CAUTION: Ensure lead is completely inside the buss before tightening"));
        lightSwitch.addStep(18, new serStep("19. Conform that LED indicator light is GREEN", images[43], ""));
        lightSwitch.addStep(19, new serStep("Congratulations! You have completed the task.", images[44], ""));

        superNova.addStep(0, new serStep("Say: Adele, show path", null, ""));
        superNova.addStep(1, new serStep("Follow white line to find a key hanging on a command hook behind computer monitor.", images[52], ""));
        superNova.addStep(2, new serStep("Pick up the key, and take it with you", null, ""));
        superNova.addStep(3, new serStep("Follow white line to task board staging area", images[50], ""));
        superNova.addStep(4, new serStep("Press the green dot side of the switch on right front end of task board.", images[47], ""));
        superNova.addStep(5, new serStep("Use the key to unlock the lock over the grey safety cover on the front center of the task board and open the container.", images[46], ""));
        superNova.addStep(6, new serStep("Press the green dot side of switch inside the unlocked safety cover down.", images[48], ""));
        superNova.addStep(7, new serStep("Check that all four blue buttons on the board have lit up.", images[51], ""));
        superNova.addStep(8, new serStep("Close and lock the safety cover over the front center switch.", images[46], ""));
        superNova.addStep(9, new serStep("Return key by following the white line back to the white command hook behind the computer monitor.", images[52], ""));
        superNova.addStep(10, new serStep("Procedure Complete! Good Job!", images[44], ""));
    }
}

// Used for parsing JSON from server 
[System.Serializable]
public class Procedure
{
    public string procedure_title;
    public bool emergency;
    public int num_steps;
    public serStep[] steps;

    public Procedure(string title, bool emergency, int numSteps)
    {
        this.procedure_title = title;
        this.emergency = emergency;
        this.num_steps = numSteps;

        this.steps = new serStep[numSteps]; 
    }

    public serStep getStep(int num)
    {
        try
        {
            return steps[num];
        } catch (IndexOutOfRangeException)
        {
            return null;
        }
         
    }

    public void addStep(int index, serStep newStep)
    {
        steps[index] = newStep; 
    }
}

[System.Serializable]
public class serStep
{
    public string text;
    public string type;
    public string warning;
    public string image;

    private Texture2D imageTex = null; 

    public serStep(string content, string type, string warning, Texture2D image)
    {
        this.text = content;
        this.type = type;
        this.warning = warning;
        this.imageTex = image;
    }

    public serStep(string content, Texture2D image, string warning)
    {
        this.text = content;
        this.warning = warning;
        this.imageTex = image;
    }

    public Texture2D getImage()
    {
        if (this.imageTex == null)
        {
            // do magical stuff to convert the string to an texture
            return null; 
        } else
        {
            return imageTex; 
        }
    }
}
