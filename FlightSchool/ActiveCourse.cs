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
            sourceNode = template.sourceNode;
            PopulateFromSourceNode(new Dictionary<string, string>(), template.sourceNode);
        }

        public ActiveCourse(ConfigNode node)
        {
            FromConfigNode(node);
        }

        public ConfigNode AsConfigNode()
        {
            //save the source node, the variables, the teacher name, the student names, Started/Completed and the elapsed time
            ConfigNode node = new ConfigNode("ACTIVE_COURSE");
            node.AddValue("id", id);
            node.AddValue("name", name);
            node.AddValue("elapsedTime", elapsedTime);
            node.AddValue("Started", Started);
            node.AddValue("Completed", Completed);
            node.AddValue("teacher", Teacher != null ? Teacher.name : "NA");
            ConfigNode studentNode = new ConfigNode("STUDENTS");
            foreach (ProtoCrewMember student in Students)
                studentNode.AddValue("student", student.name);
            node.AddNode("STUDENTS", studentNode);

            ConfigNode variableNode = new ConfigNode("VARIABLES");
            foreach (KeyValuePair<string, string> kvp in Variables)
            {
                variableNode.AddValue(kvp.Key, kvp.Value);
            }
            node.AddNode("VARIABLES", variableNode);
            node.AddNode("SOURCE_NODE", sourceNode);

            return node;
        }

        public void FromConfigNode(ConfigNode node)
        {
            double.TryParse(ConfigNodeUtils.GetValueOrDefault(node, "elapsedTime", "0"), out elapsedTime);
            bool.TryParse(ConfigNodeUtils.GetValueOrDefault(node, "Started", "true"), out Started);
            bool.TryParse(ConfigNodeUtils.GetValueOrDefault(node, "Completed", "false"), out Completed);

            string teacherName = ConfigNodeUtils.GetValueOrDefault(node, "teacher", "NA");
            if (teacherName != "NA" && HighLogic.CurrentGame.CrewRoster.Exists(teacherName))
            {
                Teacher = HighLogic.CurrentGame.CrewRoster[teacherName];
            }

            //load students
            ConfigNode studentNode = node.GetNode("STUDENTS");
            if (studentNode != null)
            {
                Students.Clear();
                foreach (ConfigNode.Value val in studentNode.values)
                {
                    if (HighLogic.CurrentGame.CrewRoster.Exists(val.value))
                    {
                        Students.Add(HighLogic.CurrentGame.CrewRoster[val.value]);
                    }
                }
            }

            //load variables
            ConfigNode variableNode = node.GetNode("VARIABLES");
            if (variableNode != null)
            {
                foreach (ConfigNode.Value val in variableNode.values)
                {
                    Utilities.AddOrReplaceInDictionary(Variables, val.name, val.value);
                }
            }

            sourceNode = node.GetNode("SOURCE_NODE");

            PopulateFromSourceNode(Variables, sourceNode);
        }

        public bool MeetsTeacherReqs(ProtoCrewMember teacher)
        {
            return (teacher.rosterStatus == ProtoCrewMember.RosterStatus.Available && teacher.experienceLevel >= teachMinLevel && (teachClasses.Length == 0 || teachClasses.Contains(teacher.trait)) && !Students.Contains(teacher));
        }

        public void SetTeacher(ProtoCrewMember teacher)
        {
            if (MeetsTeacherReqs(teacher))
                Teacher = teacher;
        }
        public void SetTeacher(string teacher)
        {
            SetTeacher(HighLogic.CurrentGame.CrewRoster[teacher]);
        }

        public bool MeetsStudentReqs(ProtoCrewMember student)
        {
            return (student.type == (ProtoCrewMember.KerbalType.Crew) && (seatMax <= 0 || Students.Count < seatMax) && student.rosterStatus == ProtoCrewMember.RosterStatus.Available && student.experienceLevel >= minLevel &&
                student.experienceLevel <= maxLevel && (classes.Length == 0 || classes.Contains(student.trait)) && !Students.Contains(student) && student != Teacher);
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

        public void CompleteCourse()
        {

            //assign rewards to all kerbals and set them to free
            if (Completed)
            {
                //assign rewards
            }

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
                    Utilities.AddOrReplaceInDictionary<string, string>(Variables, kvp.Key, kvp.Value);
                }
            }
            if (Teacher != null)
            {
                Utilities.AddOrReplaceInDictionary<string, string>(Variables, "TeachLvl", Teacher.experienceLevel.ToString());
                
               // Variables.Add("TeachLvl", Teacher.experienceLevel.ToString());
                int classID = -1;
                if (Teacher.trait == "Pilot")
                    classID = 0;
                else if (Teacher.trait == "Engineer")
                    classID = 1;
                else if (Teacher.trait == "Scientist")
                    classID = 2;
                Utilities.AddOrReplaceInDictionary<string, string>(Variables, "TeachClass", classID.ToString());
                //Variables.Add("TeachClass", classID.ToString());
            }
            Utilities.AddOrReplaceInDictionary<string, string>(Variables, "FilledSeats", Students.Count.ToString());
           // UnityEngine.Debug.Log("FilledSeats: " + Variables["FilledSeats"]);
            //Variables.Add("FilledSeats", Students.Count.ToString());


           // UnityEngine.Debug.Log(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costBase", "0"));
            //recalculate the baseCost, seatCost, and teacherCost
            costBase = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costBase", "0"), Variables);
            costSeat = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costSeat", "0"), Variables);
            if (Teacher != null)
                costTeacher = 0;
            else
                costTeacher = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(sourceNode, "costTeacher", "0"), Variables);

            double cost = costBase + costTeacher + (costSeat * Students.Count);
          //  UnityEngine.Debug.Log("Course cost: " + cost);
            return Math.Max(cost, 0.0);
        }
    }
}
