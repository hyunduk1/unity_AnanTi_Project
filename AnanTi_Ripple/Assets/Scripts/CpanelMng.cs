using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Klak.Ndi
{
    public class CpanelMng : MonoBehaviour
    {
        // Start is called before the first frame update
        public NdiReceiver _ndiRecv;
        void Start()
        {
            // NdiReceiver.

            _ndiRecv.GetComponent<NdiReceiver>().ndiName = CConfigMng.Instance._strNdiName;
        }

        // Update is called once per frame
        void Update()
        {
           
        }
    }
}
