﻿using System;
using System.Collections.Generic;
using System.Linq;
using O2DESNet.SVGRenderer;
using System.Xml.Linq;

namespace O2DESNet.Traffic
{
    public class Path : Component<Path.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public int Index { get; internal set; }
            public string Tag { get; internal set; }
            public PathMover.Statics PathMover { get; internal set; }
            internal Statics() { }

            public double Length { get; set; }
            public int Capacity { get; set; }
            /// <summary>
            /// Function map vehicle density in # vehicles per meter,  to the speed in meters per second
            /// </summary>
            public Func<double,double, double> SpeedByDensity { get; set; }
            public ControlPoint.Statics Start { get; internal set; }
            public ControlPoint.Statics End { get; internal set; }
            public bool CrossHatched { get; set; } = false;

            public double Freespeed { get; set; }
            public double Lanes { get; set; }
            public int AreaTag { get; set; }

            #region SVG Output
            /// <summary>
            /// SVG - Path Description
            /// </summary>
            public string D { get; set; }
            /// <summary>
            /// SVG - X coordinate for translation
            /// </summary>
            public double X { get; set; } = 0;
            /// <summary>
            /// SVG - Y coordinate for translation
            /// </summary>
            public double Y { get; set; } = 0;
            /// <summary>
            /// SVG - Rotate degree for transformation
            /// </summary>
            public double Rotate { get; set; } = 0;

            public Group SVG()
            {
                string name = "path#" + PathMover.Index + "_" + Index;
                var g = new Group(name, new O2DESNet.SVGRenderer.Path(LineStyle, D, new XAttribute("id", name + "_d")));
                var label = new Text(LabelStyle, string.Format("PATH{0}", Index), new XAttribute("transform", "translate(-10 -4)"));
                g.Add(new PathMarker(name + "_marker", name + "_d", 0.333, new Use("arrow"), label)); // forwards & bi-directional                
                if (X != 0 || Y != 0 || Rotate != 0) g.Add(new XAttribute("transform", string.Format("translate({0} {1}) rotate({2})", X, Y, Rotate)));
                return g;
            }

            public static CSS LineStyle = new CSS("pm_path", new XAttribute("stroke", "black"), new XAttribute("stroke-dasharray", "3,3"), new XAttribute("fill", "none"), new XAttribute("stroke-width", "0.5"));
            public static CSS LabelStyle = new CSS("pm_path_label", new XAttribute("text-anchor", "start"), new XAttribute("font-family", "Verdana"), new XAttribute("font-size", "4px"), new XAttribute("fill", "black"));

            /// <summary>
            /// Including arrows, styles
            /// </summary>
            public static Definition SVGDefs
            {
                get
                {
                    return new Definition(
                        new O2DESNet.SVGRenderer.Path("M -5 -2 L 0 0 L -5 2", "black", new XAttribute("id", "arrow"), new XAttribute("stroke-width", "0.5")),
                        new Style(LineStyle, LabelStyle)
                        );
                }
            }
            #endregion
        }
        #endregion

        #region Dynamics
        public Dictionary<Vehicle, double> VehiclePositions { get; private set; } = new Dictionary<Vehicle, double>();//dij
        public Dictionary<Vehicle, DateTime> VehicleCompletionTimes { get; private set; } = new Dictionary<Vehicle, DateTime>();//tij
        public Queue<Vehicle> VehiclesCompleted { get; private set; } = new Queue<Vehicle>();//Bi
        public DateTime LastTimeStamp { get; private set; }//mi
        /// <summary>
        /// Record the availabilities of the following paths for the vehicle to exit to,
        /// </summary>
        public Dictionary<Statics, bool> ToEnter { get; private set; } = new Dictionary<Statics, bool>();//ei'
        /// <summary>
        /// Record if the paths are exitable at when the vehicle completes its target.
        /// </summary>
        public Dictionary<Statics, bool> ToArrive { get; private set; } = new Dictionary<Statics, bool>();//Xi'
        public double CurrentSpeed { get; private set; }
        public int Occupancy { get { return VehiclePositions.Count + VehiclesCompleted.Count; } }
        public int Vacancy { get { return Config.Capacity - Occupancy; } }
        public bool LockedByPaths { get; private set; } = false;//ki
        public HourCounter HC_AllVehicles { get; private set; } = new HourCounter();//
        public HourCounter HC_VehiclesTravelling { get; private set; } = new HourCounter();
        public HourCounter HC_VehiclesCompleted { get; private set; } = new HourCounter();
        #endregion
        public List<double> SpeedRecrod { get; private set; } = new List<double>();
        //public Dictionary<DateTime,int> Vehicle_Record { get; private set; } = new Dictionary<DateTime, int>();
        public double Vehicle_Passed { get; private set; } = 0;
        #region Events
        private abstract class EventOfPath : Event
        {
            internal Path This { get; set; }
            internal void UpdPositions()
            {
                var distance = This.CurrentSpeed * (ClockTime - This.LastTimeStamp).TotalSeconds;
                foreach (var veh in This.VehiclePositions.Keys.ToArray()) This.VehiclePositions[veh] += distance;
                This.LastTimeStamp = ClockTime;
            }
            protected void Count()
            {
                This.HC_AllVehicles.ObserveCount(This.Occupancy, ClockTime);
                This.HC_VehiclesTravelling.ObserveCount(This.VehiclePositions.Count, ClockTime);
                This.HC_VehiclesCompleted.ObserveCount(This.VehiclesCompleted.Count, ClockTime);
            }
        } // event adapter 

        // Alpha_1
        private class EnterEvent : EventOfPath
        {
            internal Vehicle Vehicle { get; set; }
            public override void Invoke()
            {
                if (This.Vacancy < 1) throw new Exception("There is no vacancy for incoming vehicle.");
                UpdPositions();
                This.VehiclePositions.Add(Vehicle, 0);
                This.VehicleCompletionTimes.Add(Vehicle, ClockTime);
                Execute(new UpdCompletionEvent { This = This });
                foreach (var evnt in This.OnVacancyChg) Execute(evnt(This));
                //Log("Start,{0},{1}", Vehicle, This);
            }
        }

        // Alpha_2
        private class UpdToExitEvent : EventOfPath
        {
            internal Path Path { get; set; } // the path after which vehicles complete the last targets
            internal bool ToExit { get; set; }
            public override void Invoke()
            {
                if (This.ToArrive.ContainsKey(Path.Config)) This.ToArrive[Path.Config] = ToExit;
                else This.ToArrive.Add(Path.Config, ToExit);
                Execute(new ExitEvent { This = This });
            }
        }

        // Alpha_3
        private class UpdToEnterEvent : EventOfPath
        {
            internal Path Path { get; set; } // the path which vehicles exit to
            internal bool ToEnter { get; set; }
            public override void Invoke()
            {
                if (This.ToEnter.ContainsKey(Path.Config)) This.ToEnter[Path.Config] = ToEnter;
                else This.ToEnter.Add(Path.Config, ToEnter);
                Execute(new ExitEvent { This = This });
            }
        }

        // Alpha_4
        private class ResetEvent : EventOfPath
        {
            public override void Invoke()
            {
                This.VehiclePositions = new Dictionary<Vehicle, double>();
                This.VehicleCompletionTimes = new Dictionary<Vehicle, DateTime>();
                This.VehiclesCompleted = new Queue<Vehicle>();
                This.LastTimeStamp = ClockTime;
                foreach (var path in This.ToEnter.Keys.ToList()) This.ToEnter[path] = true;
                This.LockedByPaths = false;
                Count();
            }
        }

        // Beta_1
        private class UpdCompletionEvent : EventOfPath
        {
            public override void Invoke()
            {
                Count();
                This.CurrentSpeed = This.Config.SpeedByDensity(This.Occupancy / (This.Config.Length*This.Config.Lanes),This.Config.Freespeed);
                foreach (var veh in This.VehiclePositions.Keys)
                {
                    var completionTime = ClockTime + TimeSpan.FromSeconds(Math.Max(0, (This.Config.Length - This.VehiclePositions[veh]) / This.CurrentSpeed));
                    Schedule(new AttemptToCompleteEvent { This = This, Vehicle = veh }, completionTime);
                    This.VehicleCompletionTimes[veh] = completionTime;
                }
            }
        }

        private class PathspeedRecord:EventOfPath
        {
            public override void Invoke()
            {
                This.Vehicle_Passed = 0;
                if (This.Occupancy == 0)
                This.SpeedRecrod.Add(This.Config.Freespeed);
                else
                This.SpeedRecrod.Add(This.VehiclePositions.Keys.Count*This.Config.SpeedByDensity(This.Occupancy / (This.Config.Length * This.Config.Lanes), This.Config.Freespeed)/ This.Occupancy);
            }
        }



        // Beta_2
        private class AttemptToCompleteEvent : EventOfPath
        {
            internal Vehicle Vehicle { get; set; }
            public override void Invoke()
            {
                if (!This.VehicleCompletionTimes.ContainsKey(Vehicle) || !This.VehicleCompletionTimes[Vehicle].Equals(ClockTime)) return;
                This.VehiclePositions.Remove(Vehicle);
                This.Vehicle_Passed++;
                This.VehicleCompletionTimes.Remove(Vehicle);
                This.VehiclesCompleted.Enqueue(Vehicle);
                UpdPositions();
                Execute(new ExitEvent { This = This });
            }
        }

        // Beta_3
        private class ExitEvent : EventOfPath
        {
            public override void Invoke()
            {
                if (This.VehiclesCompleted.Count == 0) return;

                var prevLockedByPaths = This.LockedByPaths;
                var vehicle = This.VehiclesCompleted.First();
                var target = vehicle.Targets.First();
                if (vehicle.Targets.Count > 1 && target.Equals(This.Config.End)) target = vehicle.Targets[1]; // if multiple targets exists, and the 1st target is to be reached, look at the 2nd target
                var exit = false;
                if (target == This.Config.End)  // target is reached at the End
                {
                    exit = This.ToArrive[This.Config];
                    This.LockedByPaths = false;
                }
                else
                {
                    var next = This.Config.End.PathTo(target);
                    exit = This.ToEnter[next];
                    This.LockedByPaths = !exit;

                    if (next.CrossHatched)
                    {
                        if (target == next.End)
                        {
                            exit &= This.ToArrive[next];
                        }
                        else
                        {
                            var next2 = next.End.PathTo(target);
                            exit &= This.ToEnter[next2];
                            This.LockedByPaths = !exit;
                            // both next and next^2 path should be available if the next is cross-hatched
                        }
                    }
                }
                if (exit)
                {
                    var veh = This.VehiclesCompleted.Dequeue();
                    //Log("Exit,{0},{1}", veh, This);
                    foreach (var evnt in This.OnExit) Execute(evnt(veh));
                    Execute(new ExitEvent { This = This });//exe Beta_3
                    Execute(new UpdCompletionEvent { This = This });//exe Beta_1
                    foreach (var evnt in This.OnVacancyChg) Execute(evnt(This));
                }
                if (!prevLockedByPaths && This.LockedByPaths || This.VehiclesCompleted.Count == 0)
                {
                    foreach (var evnt in This.OnLockedByPaths) Execute(evnt(This));
                }
            }
        }
        #endregion

        #region Input Events - Getters
        public Event Enter(Vehicle vehicle) { return new EnterEvent { This = this, Vehicle = vehicle }; }
        /// <summary>
        /// Update the state of the following paths, 
        /// if they are available for the vehicle to exit to.
        /// </summary>
        public Event UpdToEnter(Path path, bool toEnter) { return new UpdToEnterEvent { This = this, Path = path, ToEnter = toEnter }; }
        /// <summary>
        /// Update the state of the current or following paths (if it is cross-hatched), 
        /// if they are exitable at the end when vehicles reached the last target.
        /// </summary>
        public Event UpdToExit(Path path, bool toExit)
        {
            return new UpdToExitEvent
            {
                This = this,
                Path = path,
                ToExit = toExit
            };
        }
        /// <summary>
        /// Reset the Path by emptying all the Vehicles, and releasing all locks cause by other congested paths (the locks for arriving which is not caused by congestion remain).
        /// </summary>
        public Event Reset() { return new ResetEvent { This = this }; }
        public Event SpeedRecord() { return new PathspeedRecord { This = this }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Vehicle, Event>> OnExit { get; private set; } = new List<Func<Vehicle, Event>>();//Gamma_1
        public List<Func<Path, Event>> OnVacancyChg { get; private set; } = new List<Func<Path, Event>>();//Gamma_2
        public List<Func<Path, Event>> OnLockedByPaths { get; private set; } = new List<Func<Path, Event>>();//Gamma_3
        #endregion

        public Path(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Path";
           
        }

        public override void WarmedUp(DateTime clockTime)
        {
            HC_AllVehicles.WarmedUp(clockTime);
            HC_VehiclesTravelling.WarmedUp(clockTime);
            HC_VehiclesCompleted.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = default(DateTime?))
        {
            Console.Write("Path{0} (CP{1} -> CP{2})", Config.Index, Config.Start.Index, Config.End.Index);
            if (VehiclePositions.Count > 0)
            {
                Console.Write("\tTravelling: ");
                foreach (var veh in VehiclePositions.Keys) Console.Write("{0} ", veh);
            }
            if (VehiclesCompleted.Count > 0)
            {
                Console.Write("\tCompleted: ");
                foreach (var veh in VehiclesCompleted) Console.Write("{0} ", veh);
            }
            Console.WriteLine();
        }
    }
}
