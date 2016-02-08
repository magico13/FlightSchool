using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSchool
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
    class ScenarioFlightSchool : ScenarioModule
    {
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            ConfigNode FSData = node.GetNode("FlightSchoolData");

            //load all the active courses
            FlightSchool.Instance.ActiveCourses.Clear();
            foreach (ConfigNode courseNode in FSData.GetNodes("ACTIVE_COURSE"))
            {
                try
                {
                    FlightSchool.Instance.ActiveCourses.Add(new ActiveCourse(courseNode));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
            //load all the XP data and assign any forced XP to kerbals
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            ConfigNode FSData = new ConfigNode("FlightSchoolData");
            //save all the active courses
            foreach (ActiveCourse course in FlightSchool.Instance.ActiveCourses)
            {
                ConfigNode courseNode = course.AsConfigNode();
                FSData.AddNode("ACTIVE_COURSE", courseNode);
            }
            node.AddNode("FlightSchoolData", FSData);


            //save all the kerbal data
        }
    }
}
