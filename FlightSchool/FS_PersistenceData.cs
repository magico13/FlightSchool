using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FlightSchool
{
    public class FS_PersistenceModuleSaver
    {
        public static void ApplyScenarioToSave()
        {
            //Code for saving to the persistence.sfs
            ProtoScenarioModule scenario = HighLogic.CurrentGame.scenarios.Find(s => s.moduleName == typeof(FS_PersistenceData).Name);
            if (scenario == null)
            {
                try
                {
                    FlightSchool.FSGS.Log("Adding InternalModule scenario to game '" + HighLogic.CurrentGame.Title + "'");
                    HighLogic.CurrentGame.AddProtoScenarioModule(typeof(FS_PersistenceData), new GameScenes[] { GameScenes.SPACECENTER });
                    // the game will add this scenario to the appropriate persistent file on save from now on
                }
                catch (ArgumentException ae)
                {
                    Debug.LogException(ae);
                }
                catch
                {
                    FlightSchool.FSGS.Log("Unknown failure while adding scenario.");
                }
            }
            else
            {
                if (!scenario.targetScenes.Contains(GameScenes.SPACECENTER))
                    scenario.targetScenes.Add(GameScenes.SPACECENTER);
            }
            //End code for persistence.sfs
        }
    }

    class FS_PersistenceData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }
    }
}
