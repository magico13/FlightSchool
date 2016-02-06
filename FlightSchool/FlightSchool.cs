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
        List<CourseTemplate> CourseTemplates = new List<CourseTemplate>();
        List<CourseTemplate> OfferedCourses = new List<CourseTemplate>();
        public void Start()
        {
            FindAllCourseConfigs(); //find all applicable configs
            GenerateOfferedCourses(); //turn the configs into offered courses

        }

        public void FixedUpdate()
        {
            
        }

        public void FindAllCourseConfigs()
        {
            CourseTemplates.Clear();
            //find all configs and save them
        }

        public void GenerateOfferedCourses() //somehow provide some variable options here?
        {
            //convert the saved configs to course offerings
            foreach (CourseTemplate template in CourseTemplates)
            {
                CourseTemplate duplicate = new CourseTemplate(template.sourceNode); //creates a duplicate so the initial template is preserved
                duplicate.PopulateFromSourceNode(null);
                if (duplicate.Available)
                    OfferedCourses.Add(duplicate);
            }

            //fire an event to let other mods add available courses
        }

    }
}
