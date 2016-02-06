using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MagiCore;

namespace FlightSchool
{
    public enum ExpireMode {IMMEDIATE, RECOVERY}; //expire as soon as time runs out, or on recovery
    public enum RepeatMode {NEVER, EXPIRED, ALWAYS}; //is the course repeatable?

    public class Reward
    {
        string CourseID = "";
        int XP = 0;
        ConfigNode FlightLog = new ConfigNode();

        ConfigNode MiscRewards = new ConfigNode();

        ConfigNode[] Expiry = { };
    }

    public class Expiry
    {
        public ExpireMode expireType = ExpireMode.IMMEDIATE;
        public double timeOut = 0;
    }

    public class CourseTemplate
    {
        public ConfigNode sourceNode = new ConfigNode();

        public string id = "";
        public string name = "";

        public bool Available = true; //Whether the course is currently being offered

        public string[] activePreReqs = { }; //prereqs that must not be expired
        public string[] preReqs = { }; //prereqs that must be taken, but can be expired
        public string[] conflicts = { }; //course IDs that cannot be taken while this course is not expired

        public double time = 0; //how much time the course takes (in seconds)
        public bool required = false; //whether the course is required for the kerbal to be usable
        public RepeatMode repeatable = RepeatMode.NEVER; //whether the course can be repeated

        public string[] classes = { }; //which classes can take this course (empty == all)
        public int minLevel = 0; //minimum kerbal level required to take the course
        public int maxLevel = 5; //max kerbal level allowed to take the course

        public int seatMax = -1; //maximum number of kerbals allowed in the course at once
        public int seatMin = 0; //minimum number of kerbals required to start the course

        public double costBase = 0; //base cost of the class
        public double costSeat = 0; //cost per seat

        public double costTeacher = 0; //cost of hiring a teacher rather than using one of our own kerbals
        public string[] teachClasses = { }; //which classes are valid teachers (empty == all)
        public int teachMinLevel = 0; //minimum level for the teacher

        public ConfigNode[] Rewards = { }; //The list of rewards for completing the course
       // public ConfigNode[] Expiry = { }; //The list of ways that course experience can be lost

        public CourseTemplate(ConfigNode source)
        {
            sourceNode = source;
        }

        public CourseTemplate()
        {

        }

        /// <summary>
        /// Populates all fields from the provided ConfigNode, parsing all formulas
        /// </summary>
        /// <param name="variables"></param>
        public void PopulateFromSourceNode(Dictionary<string, string> variables, ConfigNode source = null)
        {
            if (source == null)
                source = sourceNode;

            id = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "id"), variables);
            name = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "name"), variables);

            bool.TryParse(MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "Available", "true"), variables), out Available);

            List<string> tmpList = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "activePreReqs"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            activePreReqs = tmpList.ToArray();

            tmpList = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "preReqs"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            preReqs = tmpList.ToArray();

            tmpList = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "conflicts"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            conflicts = tmpList.ToArray();

            time = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "time", "0"), variables);
            bool.TryParse(MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "required", "false"), variables), out required);

            string repeatStr = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "required", "false"), variables).Trim();
            switch (repeatStr)
            {
                case "NEVER": repeatable = RepeatMode.NEVER; break;
                case "EXPIRED": repeatable = RepeatMode.EXPIRED; break;
                case "ALWAYS": repeatable = RepeatMode.ALWAYS; break;
                default: repeatable = RepeatMode.NEVER; break;
            }

            tmpList = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "classes"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            classes = tmpList.ToArray();

            minLevel = (int)(MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "minLevel", "0"), variables));
            maxLevel = (int)(MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "maxLevel", "5"), variables));

            seatMax = (int)(MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "seatMax", "-1"), variables));
            seatMin = (int)(MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "seatMin", "0"), variables));

            costBase = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "costBase", "0"), variables);
            costSeat = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "costSeat", "0"), variables);

            costTeacher = MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "costTeacher", "0"), variables);
            tmpList = MathParsing.ReplaceMathVariables(ConfigNodeUtils.GetValueOrDefault(source, "teachClasses"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            teachClasses = tmpList.ToArray();

            teachMinLevel = (int)(MathParsing.ParseMath(ConfigNodeUtils.GetValueOrDefault(source, "teachMinLevel", "0"), variables));

            //get the REWARD nodes and replace any variables in there too
            Rewards = source.GetNodes("REWARD");
            foreach (ConfigNode node in Rewards)
                ConfigNodeUtils.ReplaceValuesInNode(node, variables);

          /*  Expiry = source.GetNodes("EXPIRY");
            foreach (ConfigNode node in Expiry)
                ConfigNodeUtils.ReplaceValuesInNode(node, variables);*/
        }
    }
}
