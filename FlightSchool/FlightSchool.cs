using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP;

namespace FlightSchool
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class FlightSchool : MonoBehaviour
    {
        public void Awake()
        {

        }
        
        private void OnDraw()
        {
            
        }

        private void OnWindow(int windowID)
        {
            
        }

        public void Start()
        {
            Debug.Log("[FS] Success!");
            
        }

        public void FixedUpdate()
        {
            
        }

    }
}
