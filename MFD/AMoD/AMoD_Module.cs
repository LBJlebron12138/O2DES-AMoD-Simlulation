using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using O2DESNet.Traffic;
using System.Threading;
using SA;

namespace O2DESNet.AMoD
{
    public class AMoD_Module : Component<AMoD_Module.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public PathMover.Statics PathMover { get; set; }
            // public List<ControlPoint.Statics> RequestPoints { get; set; }
            // public List<ControlPoint.Statics> RequestTarget { get; set; }
            public List<ControlPoint.Statics> Origins { get; set; }//空车生成起点

            public Vehicle.Statics VehicleCategory { get; set; }

            public int NPersons { get; set; } = 1;
            //public Queue<Person> RequestSaver { get; set; } = new Queue<Person>();
            public List<Person> RequestSaver { get; set; } = new List<Person>();
            public Dictionary<int, double> AreaPathLegth { get; set; } = new Dictionary<int, double>();
          
         }
        #endregion

        #region Dynamics
        public PathMover PathMover { get; private set; } 
        public Dictionary<Vehicle, Person> Matcher { get; private set; } = new Dictionary<Vehicle,Person>();

        //public List<DateTime> RequestTime { get; private set; } = new List<DateTime>;
        //public Dictionary<Vehicle,ControlPoint.Statics > Vehicles_At { get; private set; } = new Dictionary<Vehicle, ControlPoint.Statics>();
        //public List<KeyValuePair<Vehicle, ControlPoint.Statics>> Vehicles_At { get; private set; } = new List<KeyValuePair<Vehicle, ControlPoint.Statics>>();
        public List<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
       



        public Dictionary<int, Tuple<List<DateTime>,List<Path>,List<double>, List<double>,List<double>>> Area { get; private set; } = new Dictionary<int, Tuple<List<DateTime>,List<Path>,List<double>, List<double>, List<double>>>();


        public List<Vehicle> Completed { get; private set; } = new List<Vehicle>();
        
        //public int CompletedPickup { get; private set; } = 0;
        public int fuck = 0;

        



        #endregion



        #region Events
        private abstract class InternalEvent : Event { internal AMoD_Module This { get; set; } } // event adapter 

        
        private class StartEvent:InternalEvent
        {
            

            public override void Invoke()
            {
                //Parallel.ForEach(This.Config.RequestSaver, preson =>
                //{
                //    Schedule(new AddBatch { This = This, preson = preson }, preson.Config.RequestTimeStamp);
                //});
                foreach (var preson in This.Config.RequestSaver)
                {

                    Schedule(new Depart { This = This, preson = preson }, preson.Config.RequestTimeStamp);
                    // Schedule(new MatchtestEvent { This = This, person = preson }, preson.Config.RequestTimeStamp);

                }
            }
        }
        //beta_1
        private class Depart : InternalEvent
        {
            internal Person preson { get; set; }
           
            public override void Invoke()
            {
                var vehicle = new Vehicle(This.Config.VehicleCategory, DefaultRS.Next());
                This.Vehicles.Add(vehicle);
                This.Config.RequestSaver.Remove(preson);
                var At = preson.Config.RequestPostion;
                
                var target = preson.Config.RequestTarget;
                if (!(This.PathMover.Paths[At.PathTo(target)].Vacancy < 1))
                {
                    Execute(vehicle.SetTargets(new List<ControlPoint.Statics> { target }));
                    Execute(This.PathMover.CallToDepart(vehicle, At));
                }
                // Execute(new MatchtestEvent { This = This, person = preson });


            }
        }

        private class FinishEvent : InternalEvent
        {
            internal Vehicle Vehicle { get; set; }
            internal ControlPoint.Statics At { get; set; }
            public override void Invoke()
            {
                This.Completed.Add(Vehicle);
                This.Vehicles.Remove(Vehicle);
                
            }
        }



        
        
        
        private class ChangeRecordEvent:InternalEvent
        {
            public override void Invoke()
            {



                if (This.fuck == 0)
                {
                    Schedule(new ChangeRecordEvent { This = This }, ClockTime.Add(TimeSpan.FromHours(7)));
                    This.fuck++;
                }
                else
                { 

                 foreach (var Area in This.Area)
                 {
                    double Density_Sum = 0;
                    double Time_Sum = 0;
                    double Sum_Time_mult_QuantityofFlow = 0;
                    foreach (var path in Area.Value.Item2)
                    {
                            double time = 0;
                            if (path.Occupancy == 0)
                                time = path.Config.Length / path.Config.Freespeed;
                            else
                                time= path.Config.Length /  path.Config.SpeedByDensity(path.Occupancy / (path.Config.Length * path.Config.Lanes), path.Config.Freespeed);
                        double Time_mult_QuantityofFlow = time*(path.Vehicle_Passed / 60);
                        Density_Sum += path.Occupancy / path.Config.Lanes;
                        Time_Sum += time;
                        Sum_Time_mult_QuantityofFlow += Time_mult_QuantityofFlow;
                        Execute(path.SpeedRecord());
                    }
                    double Density = Density_Sum / This.Config.AreaPathLegth[Area.Key];
                    double Speed = This.Config.AreaPathLegth[Area.Key] / Time_Sum;
                    double QuantityofFlow = Sum_Time_mult_QuantityofFlow / Time_Sum;
                    This.Area[Area.Key].Item1.Add(ClockTime);
                    This.Area[Area.Key].Item3.Add(Density);
                    This.Area[Area.Key].Item4.Add(Speed);
                    This.Area[Area.Key].Item5.Add(QuantityofFlow);
                 }
                   Schedule(new ChangeRecordEvent { This = This }, ClockTime.Add(TimeSpan.FromMinutes(1)));
                }

                
                

            }
        }
        private class DeadLockEvent :InternalEvent
        {
            public override void Invoke()
            {
                This.fuck++;
                Schedule(new DeadLockEvent { This = This }, ClockTime.Add(TimeSpan.FromMinutes(10)));
            }
        }

        //beta_2




        //beta_5





        #endregion

        #region Input Events - Getters

        #endregion

        #region Output Events - Reference to Getters

        #endregion

        public AMoD_Module(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "AMoD_Module";
            PathMover = new PathMover(config.PathMover, DefaultRS.Next());
            //for (int i = 0; i < this.Config.NPersons; i++) RequestSaver.Enqueue(); 
            PathMover.OnArrive.Add((veh, cp) => new FinishEvent { This = this, Vehicle = veh,At=cp });
            //PathMover.OnArrive.Add((veh, cp) => new DropoffEvent { This = this, Matchvehicle = veh ,At=cp});
            InitEvents.AddRange(PathMover.InitEvents);
            PathMover.OnDeadlock.Add(pm => new DeadLockEvent { This = this });

            InitEvents.Add(new StartEvent { This = this });
            InitEvents.Add(new ChangeRecordEvent { This = this });
            //InitEvents.Add(new DeadLockEvent { This = this });
            foreach (var path in PathMover.Paths.Values)
            {
                if (this.Area.ContainsKey(path.Config.AreaTag))
                    this.Area[path.Config.AreaTag].Item2.Add(path);
                else
                {
                    List<Path> paths = new List<Path>();
                    List<double> D = new List<double>();
                    List<double> S = new List<double>();
                    List<double> Q = new List<double>();
                    List<DateTime> T = new List<DateTime>();
                    this.Area.Add(path.Config.AreaTag, new Tuple<List<DateTime>,List<Path>,List<double>,List<double>, List<double>>(T,paths,D,S,Q));
                    this.Area[path.Config.AreaTag].Item2.Add(path);
                }
            }
            



        }

        public override void WarmedUp(DateTime clockTime)
        {
            PathMover.WarmedUp(clockTime);
            
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            Console.WriteLine(clockTime);
            

            Console.WriteLine();

            //Console.WriteLine("10\t{}\t{}\t{}\t{}", Area[10].Item2, Area[10].Item3, Area[10].Item3);
            foreach (var veh in Vehicles) if (veh.Targets.Count > 0) Console.WriteLine("{0}:\t Target CP{1}", veh, veh.Targets.First().Index);
        }
    }
}
