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
        public static FS_GameStates FSGS = new FS_GameStates();

        public void Awake()
        {
            RenderingManager.AddToPostDrawQueue(0, OnDraw);
        }

        private void OnDraw()
        {
            FSGS.gui.SetGUIPositions(OnWindow);
        }

        private void OnWindow(int windowID)
        {
            FSGS.gui.DrawGUIs(windowID);
        }

        public void Start()
        {
            FS_PersistenceModuleSaver.ApplyScenarioToSave();
            FSGS.gui.showMainGUI = true;
            
        }

        public void FixedUpdate()
        {
            
        }

    }
}
