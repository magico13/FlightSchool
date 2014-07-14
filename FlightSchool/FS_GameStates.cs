using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlightSchool
{
    public class FS_GameStates
    {
        public FS_GUI gui = new FS_GUI();


        public void Log(String message)
        {
            Debug.Log("[FS] " + message);
        }

    }
}
