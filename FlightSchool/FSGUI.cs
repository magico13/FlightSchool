using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace FlightSchool
{
    public class FSGUI
    {
        public bool showMain = true;
        public Rect MainGUIPos = new Rect(200, 200, 750, 500);
        public void SetGUIPositions(GUI.WindowFunction OnWindow)
        {
            if (showMain) MainGUIPos = GUILayout.Window(7349, MainGUIPos, DrawMainGUI, "Flight School");
        }

        public void DrawGUIs(int windowID)
        {
            if (showMain) DrawMainGUI(windowID);
        }

        int offeredActiveToolbar = 0;
        Vector2 offeredActiveScroll = new Vector2();
        ActiveCourse selectedCourse = null;
        Vector2 currentStudentList = new Vector2();
        Vector2 availableKerbalList = new Vector2();
        double totalCost = 0;
        protected void DrawMainGUI(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(250)); //offered/active list
            int oldStatus = offeredActiveToolbar;
            offeredActiveToolbar = GUILayout.Toolbar(offeredActiveToolbar, new string[] { "Offered", "Active" });
            if (offeredActiveToolbar != oldStatus)
            {
                selectedCourse = null;
            }
            offeredActiveScroll = GUILayout.BeginScrollView(offeredActiveScroll);
            if (offeredActiveToolbar == 0) //offered list
            {
                foreach (CourseTemplate template in FlightSchool.Instance.OfferedCourses)
                {
                    if (GUILayout.Button(template.id + " - " + template.name))
                    {
                        selectedCourse = new ActiveCourse(template);
                        totalCost = selectedCourse.CalculateCost();
                    }
                }
            }
            else //active list
            {
                foreach (ActiveCourse course in FlightSchool.Instance.ActiveCourses)
                {
                    if (GUILayout.Button(course.id + " - " + course.name)) //show percent complete?
                    {
                        selectedCourse = course;
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(); //Selected info
            if (offeredActiveToolbar == 0)
            {
                if (selectedCourse != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(selectedCourse.id + " - " + selectedCourse.name);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Label(selectedCourse.description);

                    GUILayout.Label("Course length: " + MagiCore.Utilities.GetFormattedTime(selectedCourse.time, true));

                    //select the teacher
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Teacher: " + (selectedCourse.Teacher != null ? (selectedCourse.Teacher.name+" ("+selectedCourse.Teacher.trait+" "+selectedCourse.Teacher.experienceLevel+")" ) : "Guest Lecturer"), GUILayout.ExpandWidth(false)))
                    {
                        //select a teacher
                        List<ProtoCrewMember> validTeachers = new List<ProtoCrewMember>();
                        foreach (ProtoCrewMember pcm in HighLogic.CurrentGame.CrewRoster.Crew)
                        {
                            if (selectedCourse.MeetsTeacherReqs(pcm))
                                validTeachers.Add(pcm);
                        }

                        DialogOption[] options = new DialogOption[validTeachers.Count+1];
                        for (int i=0; i< validTeachers.Count; i++)
                        {
                            ProtoCrewMember pcm = validTeachers[i];
                            options[i] = new DialogOption(pcm.name + ": " + pcm.trait + " " + pcm.experienceLevel, () => { selectedCourse.SetTeacher(pcm); totalCost = selectedCourse.CalculateCost(); });
                        }
                        options[validTeachers.Count] = new DialogOption("Guest Lecturer", () => { selectedCourse.Teacher = null; totalCost = selectedCourse.CalculateCost(); });
                        MultiOptionDialog diag = new MultiOptionDialog("Select a kerbal to lead this course:", "Select Teacher", null, options);
                        PopupDialog.SpawnPopupDialog(diag, false, GUI.skin);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    //select the kerbals. Two lists, the current Students and the available ones
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical(GUILayout.Width(250));
                    GUILayout.Label("Enrolled:");
                    currentStudentList = GUILayout.BeginScrollView(currentStudentList);
                    for (int i = 0; i < selectedCourse.Students.Count; i++ )
                    {
                        ProtoCrewMember student = selectedCourse.Students[i];
                        if (GUILayout.Button(student.name+": "+student.trait+" "+student.experienceLevel))
                        {
                            selectedCourse.Students.RemoveAt(i);
                            i--;
                            totalCost = selectedCourse.CalculateCost();
                        }

                    }
                    GUILayout.EndScrollView();
                    if (selectedCourse.seatMax > 0)
                    {
                        GUILayout.Label(selectedCourse.seatMax - selectedCourse.Students.Count + " remaining seat(s).");
                    }
                    if (selectedCourse.seatMin > 0)
                    {
                        GUILayout.Label(Math.Max(0, selectedCourse.seatMin - selectedCourse.Students.Count) + " student(s) required.");
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(GUILayout.Width(250));
                    GUILayout.Label("Available:");
                    availableKerbalList = GUILayout.BeginScrollView(availableKerbalList);
                    for (int i = 0; i < HighLogic.CurrentGame.CrewRoster.Count; i++)
                    {
                        ProtoCrewMember student = HighLogic.CurrentGame.CrewRoster[i];
                        if (selectedCourse.MeetsStudentReqs(student))
                        {
                            if (GUILayout.Button(student.name + ": " + student.trait + " " + student.experienceLevel))
                            {
                                selectedCourse.AddStudent(student);
                                totalCost = selectedCourse.CalculateCost();
                            }
                        }

                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    //double totalCost = selectedCourse.costBase + (selectedCourse.costTeacher * (selectedCourse.Teacher != null ? 0 : 1)) + (selectedCourse.Students.Count * selectedCourse.costSeat);


                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Start Course: √"+Math.Ceiling(totalCost), GUILayout.ExpandWidth(false)))
                    {
                        selectedCourse.StartCourse();
                        FlightSchool.Instance.ActiveCourses.Add(selectedCourse);
                        selectedCourse = null;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                }
            }
            else
            {
                //An active course has been selected
                if (selectedCourse != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(selectedCourse.id + " - " + selectedCourse.name);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Label(selectedCourse.description);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Teacher: " + (selectedCourse.Teacher != null ? (selectedCourse.Teacher.name + " (" + selectedCourse.Teacher.trait + " " + selectedCourse.Teacher.experienceLevel + ")") : "Guest Lecturer"), GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Time remaining: "+MagiCore.Utilities.GetFormattedTime(selectedCourse.time-selectedCourse.elapsedTime, true));
                    GUILayout.Label(Math.Round(100*selectedCourse.elapsedTime/selectedCourse.time, 1) + "% complete");

                    //scroll list of all students
                    GUILayout.Label("Students:");
                    currentStudentList = GUILayout.BeginScrollView(currentStudentList);
                    for (int i = 0; i < selectedCourse.Students.Count; i++ )
                    {
                        ProtoCrewMember student = selectedCourse.Students[i];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(student.name+": "+student.trait+" "+student.experienceLevel);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                        {
                            DialogOption[] options = new DialogOption[2];
                            options[0] = new DialogOption("Yes", () => {selectedCourse.Students.Remove(student); student.rosterStatus = ProtoCrewMember.RosterStatus.Available;});
                            options[1] = new DialogOption("No", ()=>{});

                            MultiOptionDialog diag = new MultiOptionDialog("Are you sure you want "+student.name+ " to drop this course?", "Drop Course?", null, options);
                            PopupDialog.SpawnPopupDialog(diag, false, GUI.skin);

                            i--;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Cancel Course", GUILayout.ExpandWidth(false)))
                    {
                        DialogOption[] options = new DialogOption[2];
                        options[0] = new DialogOption("Yes", () => { selectedCourse.CompleteCourse(); FlightSchool.Instance.ActiveCourses.Remove(selectedCourse); selectedCourse = null; });
                        options[1] = new DialogOption("No", ()=>{});

                        MultiOptionDialog diag = new MultiOptionDialog("Are you sure you want to cancel this course?", "Cancel Course?", null, options);
                        PopupDialog.SpawnPopupDialog(diag, false, GUI.skin);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }
    }
}
