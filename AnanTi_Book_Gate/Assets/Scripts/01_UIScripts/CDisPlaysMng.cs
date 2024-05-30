using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDisPlaysMng : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < CConfigMng.Instance._nDisPlayDevices; i++)
        {
            Display.displays[i].Activate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
