using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadboardTerminalManager : MonoBehaviour
{
    [System.Serializable]
    public struct PowerRail
    {
        public Transform terminalStart;
        public Transform terminalEnd;
    }

    [System.Serializable]
    public struct TerminalStrip
    {
        public Transform terminalStart;
        public Transform terminalEnd;
    }

    public GameObject terminalPrefab; // Prefab for the terminals

    public PowerRail topPowerRail;
    public PowerRail bottomPowerRail;   

    public int powerRailGroups = 5; // Number of groups
    public int terminalsPerPRGroup = 5; // Number of terminals per group
    
    public TerminalStrip topPowerStrip;
    public TerminalStrip bottomPowerStrip;   

    public int powerStripLength = 30; // Number of groups
    public int terminalsPerRow = 5; // Number of terminals per group

    public Transform terminalParent;

    private char currentLabel = 'j';

    public float[] terminalsA = new float[30];
    public float[] terminalsB = new float[30];

    public float[] powerRails = new float[4];


    void Start()
    {
        PopulatePowerRail(topPowerRail);
        PopulatePowerRail(bottomPowerRail);
        PopulateTerminalStrip(topPowerStrip);
        PopulateTerminalStrip(bottomPowerStrip);
    }

    void PopulatePowerRail(PowerRail pr)
    {
        float totalWidth = Vector3.Distance(pr.terminalStart.position, pr.terminalEnd.position);

        float terminalSpacing = totalWidth/((terminalsPerPRGroup - 1) * powerRailGroups + 2 * (powerRailGroups - 1));

        for (int line = 0; line < 2; line++)
        {
            Vector3 lineOffset = new Vector3(0f, 0f, -terminalSpacing * line);
            Vector3 currentPos = pr.terminalStart.localPosition + lineOffset;

            for (int group = 0; group < powerRailGroups; group++)
            {
                for (int terminal = 0; terminal < terminalsPerPRGroup; terminal++)
                {
                    GameObject terminalGO = Instantiate(terminalPrefab, currentPos, Quaternion.identity, transform);
                    terminalGO.GetComponent<Pin>().PinNumber = (line == 0) ? "-" : "+";
                    Debug.Log(terminal);
                    terminalGO.transform.parent = terminalParent;
                    terminalGO.transform.localPosition = currentPos;
                    currentPos += new Vector3(terminalSpacing, 0, 0);
                }

                currentPos += new Vector3(terminalSpacing, 0, 0);
            }
        }
    }

    void PopulateTerminalStrip(TerminalStrip ts)
    {
        float totalWidth = Vector3.Distance(ts.terminalStart.position, ts.terminalEnd.position);

        float terminalSpacing = totalWidth/((powerStripLength - 1));

        for (int line = 0; line < terminalsPerRow; line++)
        {
            Vector3 lineOffset = new Vector3(0f, 0f, -terminalSpacing * line);
            Vector3 currentPos = ts.terminalStart.localPosition + lineOffset;

            for (int terminal = 0; terminal < powerStripLength; terminal++)
            {
                GameObject terminalGO = Instantiate(terminalPrefab, currentPos, Quaternion.identity, transform);
                terminalGO.transform.parent = terminalParent;
                terminalGO.transform.localPosition = currentPos;
                currentPos += new Vector3(terminalSpacing, 0, 0);

                // +1 since breadboard is 1-indexing
                terminalGO.GetComponent<Pin>().PinNumber =  $"{terminal+1}{currentLabel}";
                
            }
            if (--currentLabel < 'a') currentLabel = 'j';
        }
    }
}