using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MicWriteManager : MonoBehaviour
{

    public GameObject StartButtonPanel;
    public GameObject StopButtonPanel;

    public void TurnOffStartButton()
    {
        StartButtonPanel.SetActive(false);
        StopButtonPanel.SetActive(true);
    }

    public void TurnOffStopButton()
    {
        StopButtonPanel.SetActive(false);
        StartButtonPanel.SetActive(true);
    }



}
