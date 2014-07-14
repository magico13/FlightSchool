using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlightSchool
{
    public class FS_GUI
    {
        public bool showMainGUI;
        public Rect mainWindowRect = new Rect(0, 0, Screen.width / 4, 1);

        public void SetGUIPositions(GUI.WindowFunction OnWindow)
        {
            if (showMainGUI) mainWindowRect = GUILayout.Window(8940, mainWindowRect, DrawMainGUI, "Flight School", HighLogic.Skin.window);
        }

        public void DrawGUIs(int windowID)
        {
            if (showMainGUI) DrawMainGUI(windowID);
        }

        public void hideAll()
        {
            showMainGUI = false;
        }

        public void reset()
        {
            hideAll();
            mainWindowRect = new Rect(0, 0, Screen.width / 4, 1);
        }

        private void DrawMainGUI(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Hello world!");

            GUILayout.EndVertical();

            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }
    }
}
