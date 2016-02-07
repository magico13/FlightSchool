using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MagiCore;

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
            PopulateFromSourceNode(new Dictionary<string, string>(), template.sourceNode);
        }

        public void SetTeacher(ProtoCrewMember teacher)
        {
            Teacher = teacher;
        }
        public void SetTeacher(string teacher)
        {
            Teacher = HighLogic.CurrentGame.CrewRoster[teacher];
        }

        public void AddStudent(ProtoCrewMember student)
        {
            if (seatMax <= 0 || Students.Count < seatMax)
            {
                if (!Students.Contains(student))
                    Students.Add(student);
            }
        }
        public void AddStudent(string student)
        {
            AddStudent(HighLogic.CurrentGame.CrewRoster[student]);
        }
        
        public void RemoveStudent(ProtoCrewMember student)
        {
            if (Students.Contains(student))
            {
                Students.Remove(student);
                if (Started)
                {
                    UnityEngine.Debug.Log("[FS] Kerbal removed from in-progress class!");
                    //TODO: Assign partial rewards, based on what the REWARD nodes think
                }
            }
        }
        public void RemoveStudent(string student)
        {
            RemoveStudent(HighLogic.CurrentGame.CrewRoster[student]);
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

            //fire an event
        }

        public bool StartCourse()
        {
            //set all the kerbals to unavailable and begin tracking time
            if (Started)
                return true;

            //ensure we have more than the minimum number of students and not more than the maximum number
            int studentCount = Students.Count;
            if (seatMax > 0 && studentCount > seatMax)
                return false;
            if (seatMin > 0 && studentCount < seatMin)
                return false;

            //attempt to take the required funds
            double cost = CalculateCost();
            if (Funding.Instance.Funds < cost)
            {
                return false;
            }
            Funding.Instance.AddFunds(-cost, TransactionReasons.None);

            Started = true;
            Teacher.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            foreach (ProtoCrewMember student in Students)
                student.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            return true;
            //fire an event
        }

        public double CalculateCost(Dictionary<string, string> addlVariables = null)
        {
            if (addlVariables != null)
            {
                foreach (KeyValuePair<string, string> kvp in addlVariables)
                {
                    if (!Variables.ContainsKey(kvp.Key))
                        Variables.Add(kvp.Key, kvp.Value);
                }
            }
            if (Teacher != null)
            {
                Variables.Add("TeachLvl", Teacher.experienceLevel.ToString());
                int classID = -1;
                if (Teacher.trait == "Pilot")
                    classID = 0;
                else if (Teacher.trait == "Engineer")
                    classID = 1;
                else if (Teacher.trait == "Scientist")
                    classID = 2;
                Variables.Add("TeachClass", classID.ToString());
            }
            Variables.Add("FilledSeats", Students.Count.ToString());

            //recalculate the baseCost, seatCost, and teacherCost
            costBase = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costBase", "0"), Variables);
            costSeat = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costSeat", "0"), Variables);
            if (Teacher != null)
                costTeacher = 0;
            else
                costTeacher = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costTeacher", "0"), Variables);

            return Math.Max(costBase + costTeacher + (costSeat * Students.Count), 0.0);
        }
    }
}
