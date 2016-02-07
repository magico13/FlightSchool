using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSchool
{
    public class ActiveCourse : CourseTemplate
    {
        public ProtoCrewMember Teacher = null;
        public List<ProtoCrewMember> Students = new List<ProtoCrewMember>();

        public double elapsedTime = 0;
        public bool Started = false, Completed = false;


        public ActiveCourse(CourseTemplate template)
        {

        }


        public bool ProgressTime(double dT)
        {
            if (!Started)
                return false;
            if (!Completed)
            { 
                elapsedTime += dT;
                Completed = elapsedTime >= time;
                if (Completed) //we finished the course!
                {
                    CompleteCourse();
                }
            }
            return Completed;
        }

        protected void CompleteCourse()
        {
            //assign rewards to all kerbals and set them to free
            Teacher.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            foreach (ProtoCrewMember student in Students)
                student.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        }

        public void StartCourse()
        {
            //set all the kerbals to unavailable and begin tracking time
            if (Started)
                return;
            Started = true;
            Teacher.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            foreach (ProtoCrewMember student in Students)
                student.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
        }
    }
}
