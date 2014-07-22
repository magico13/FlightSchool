using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void OnDestroy()
        {
           /* if (StageRecoveryWrapper.StageRecoveryAvailable)
            {
                StageRecoveryWrapper.RemoveRecoverySuccessEvent(success);
                StageRecoveryWrapper.RemoveRecoveryFailureEvent(success);
            }*/
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
           // FS_PersistenceModuleSaver.ApplyScenarioToSave();
            FSGS.gui.showMainGUI = true;
            if (StageRecoveryWrapper.StageRecoveryAvailable)
            {
                StageRecoveryWrapper.AddRecoverySuccessEvent(success);
                StageRecoveryWrapper.AddRecoveryFailureEvent(success);
            }
            
        }

        void success(Vessel v, Dictionary<string, int> parts)
        {
            FSGS.Log("Got " + v.vesselName);
            for (int i=0; i< parts.Count; i++)
            {
                FSGS.Log("Got " + parts.Keys.ElementAt(i) + "x" + parts.Values.ElementAt(i));
            }
        }

        public void FixedUpdate()
        {
            
        }

    }
}
