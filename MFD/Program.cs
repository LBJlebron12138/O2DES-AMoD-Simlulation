using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Traffic;
using System.Xml;
using O2DESNet;
using System.IO;
using System.Diagnostics;

namespace O2DESNet.AMoD
{
    partial class Program
    {
        static void Main(string[] args)
        {


            bool? crossHatch = null;
            Console.Write("Cross-Hatch at Junctions? ");
            var readLine = Console.ReadLine().ToUpper();
            if (readLine == "Y") crossHatch = true;
            else if (readLine == "N") crossHatch = false;
            else throw new Exception("Wrong Input!");


            Console.Write("Min. Random Seed: ");
            var seedMin = Convert.ToInt32(Console.ReadLine());
            Console.Write("Max. Random Seed: ");
            var seedMax = Convert.ToInt32(Console.ReadLine());

            var nw = NetWork(crossHatch.Value);//加载地图
           
            var AllCPs = nw.ControlPoints.Values.ToList();
            Console.WriteLine(nw.RoutingTablesFile);

            Stopwatch stopwatch = new Stopwatch();
           
            for (int seed = seedMin; seed <= seedMax; seed++)
            {
                Console.WriteLine("Random Seed: {0}", seed);
               
                    
                    AMoD_Module.Statics statics;
                    statics = GetPopulation(nw);
                    var sim = new Simulator(new AMoD_Module(statics, seed: seed));

                    sim.State.Display = true;

                    stopwatch.Restart();
                    sim.Run(TimeSpan.FromDays(1));
                    

                    stopwatch.Stop();


                    Output(sim, string.Format("{0}_{1}", crossHatch, seed), stopwatch);
               
            }
        }

        static double VehicleLength =4.5;
        static double Clearence = 4;
        //static double MaxSpeed = 16.7;
        static double Deceleration = 1.55;
        static void ConfigPath(O2DESNet.Traffic.Path.Statics path)
        {
            path.Capacity = Math.Max(1, (int)Math.Floor(path.Length / (VehicleLength + Clearence)));
            if (path.Length > VehicleLength + Clearence) path.SpeedByDensity = SpeedByDensity;
        }


        static double SpeedByDensity(double densityPerLane,double freespeed)
        {
            var safetyDistance = Math.Max(0, 1 / densityPerLane - VehicleLength);
            var speed = Math.Min(freespeed, Math.Sqrt(2 * Deceleration * safetyDistance));
            if (speed == 0) throw new Exception();
            return speed;
        }


        static AMoD_Module.Statics GetPopulation(PathMover.Statics pm)
        {

            var sts = new AMoD_Module.Statics();
            
            XmlDocument plandata = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create("rawPopulation.xml", settings);
            plandata.Load(reader);
            //Population Person = new Population();

            XmlNode population = plandata.SelectSingleNode("population");//根节点
            XmlNodeList person = population.ChildNodes;//根节点的子节点
           
            //List<DateTime> Temp = new List<DateTime>();
            int tag = 0;
            foreach (XmlNode p in person)
            {
                XmlNode plans = p.FirstChild;
                XmlNodeList plan = plans.ChildNodes;

                foreach(XmlNode node in plan)
                {
                    Person person_INnetwork = new Person();
                    if (node.LocalName == "leg")
                    {
                        XmlElement element_node = (XmlElement)node;
                        var mode = element_node.Attributes[0].Value;

                        if (mode =="av"|| mode == "pt" || mode == "ptslow")
                        {
                            var t = element_node.Attributes[1].Value;
                            string Tm = "0001/1/1 " + t;
                            try
                            {
                                var RT = Convert.ToDateTime(Tm);
                                person_INnetwork.Config.RequestTimeStamp = RT;//请求时间
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            XmlNode Lastnode = node.PreviousSibling;
                            XmlNode Nextnode = node.NextSibling;
                            XmlElement element_last = (XmlElement)Lastnode;
                            XmlElement element_next = (XmlElement)Nextnode;
                            string RL = element_last.GetAttribute("link").ToString();

                            string TL = element_next.GetAttribute("link").ToString();
                            var Paths = pm.Paths;
                            foreach (var path in Paths.Values)
                            {
                                if (path.Tag == RL)
                                    person_INnetwork.Config.RequestPostion = path.Start;//请求位置
                                else if (path.Tag == TL)
                                    person_INnetwork.Config.RequestTarget = path.Start;//请求目的地

                            }
                        }

                    }
            
                   
   

                   
                    if (person_INnetwork.Config.RequestPostion == person_INnetwork.Config.RequestTarget )
                        continue;
                    if (person_INnetwork.Config.RequestPostion.Tag == null || person_INnetwork.Config.RequestTarget.Tag == null)
                        continue;
                    
                    tag++;
                    
                    person_INnetwork.Tag = tag.ToString();//标签
                    DateTime ET = Convert.ToDateTime("0001/1/1 7:00:00");
                    DateTime BT = Convert.ToDateTime("0001/1/1 8:00:00");

                    if (person_INnetwork.Config.RequestTimeStamp.CompareTo(ET) > 0 && person_INnetwork.Config.RequestTimeStamp.CompareTo(BT) < 0)
                    {
                        
                        Person Clone1 = new Person();
                        Clone1.Config.Tag = "Clone1_" + person_INnetwork.Config.Tag;
                        Clone1.Config.RequestPostion = person_INnetwork.Config.RequestPostion.PathsIn[0].Start;
                        Clone1.Config.RequestTarget = person_INnetwork.Config.RequestTarget.PathsIn[0].Start;
                        Clone1.Config.RequestTimeStamp = person_INnetwork.Config.RequestTimeStamp.AddSeconds(15);

                        Person Clone2 = new Person();
                        Clone2.Config.Tag = "Clone2_" + person_INnetwork.Config.Tag;
                        Clone2.Config.RequestPostion = person_INnetwork.Config.RequestPostion.PathsOut[0].End;
                        Clone2.Config.RequestTarget = person_INnetwork.Config.RequestTarget.PathsIn[0].Start;
                        Clone2.Config.RequestTimeStamp = person_INnetwork.Config.RequestTimeStamp.AddSeconds(30);
                        
                        Person Clone3 = new Person();
                        Clone3.Config.Tag = "Clone3_" + person_INnetwork.Config.Tag;
                        Clone3.Config.RequestPostion = person_INnetwork.Config.RequestPostion.PathsIn[0].End;
                        Clone3.Config.RequestTarget = person_INnetwork.Config.RequestTarget.PathsOut[0].End;
                        Clone3.Config.RequestTimeStamp = person_INnetwork.Config.RequestTimeStamp.AddSeconds(45);

                        Person Clone4 = new Person();
                        Clone4.Config.Tag = "Clone4_" + person_INnetwork.Config.Tag;
                        Clone4.Config.RequestPostion = person_INnetwork.Config.RequestPostion.PathsIn[0].Start;
                        Clone4.Config.RequestTarget = person_INnetwork.Config.RequestTarget.PathsOut[0].End;
                        Clone4.Config.RequestTimeStamp = person_INnetwork.Config.RequestTimeStamp.AddSeconds(60);
                        

                        
                        if(!Clone1.Config.RequestPostion.Equals(Clone1.Config.RequestTarget))
                            sts.RequestSaver.Add(Clone1);
                        if (!Clone2.Config.RequestPostion.Equals(Clone2.Config.RequestTarget))
                            sts.RequestSaver.Add(Clone2);
                        if (!Clone3.Config.RequestPostion.Equals(Clone3.Config.RequestTarget))
                            sts.RequestSaver.Add(Clone3);
                        if (!Clone4.Config.RequestPostion.Equals(Clone4.Config.RequestTarget))
                            sts.RequestSaver.Add(Clone4);
                        sts.RequestSaver.Add(person_INnetwork);
                        
                    }
                    
                }

                


            }
            
            /*
            Random random = new Random(Seed:2);
            
            var CPS_8 = pm.ControlPoints.Values.Where(cp => cp.AreaTag == 8).ToList();
            var AllCPs = pm.ControlPoints.Values.ToList();
            for (var i = 0; i < 10000; i++)
            {
                Person person_INnetwork = new Person();
                person_INnetwork.Config.RequestPostion = CPS_8[random.Next(0, CPS_8.Count - 1)];
                person_INnetwork.Config.RequestTarget=AllCPs[random.Next(0, AllCPs.Count - 1)];
                DateTime ET = Convert.ToDateTime("0001/1/1 7:00:00");
                person_INnetwork.Config.RequestTimeStamp = ET;
                person_INnetwork.Config.Tag = Convert.ToString(i);
                if (!person_INnetwork.Config.RequestPostion.Equals(person_INnetwork.Config.RequestTarget))
                sts.RequestSaver.Add(person_INnetwork);
            }
            for (var i = 0; i < 10000; i++)
            {
                Person person_INnetwork = new Person();
                person_INnetwork.Config.RequestTarget = CPS_8[random.Next(0, CPS_8.Count - 1)];
                person_INnetwork.Config.RequestPostion = AllCPs[random.Next(0, AllCPs.Count - 1)];
                DateTime ET = Convert.ToDateTime("0001/1/1 7:00:00");
                person_INnetwork.Config.RequestTimeStamp = ET;
                person_INnetwork.Config.Tag = Convert.ToString(10000+i);
                if (!person_INnetwork.Config.RequestPostion.Equals(person_INnetwork.Config.RequestTarget))
                sts.RequestSaver.Add(person_INnetwork);
            }
            */
                        sts.RequestSaver.Sort((Person p1, Person p2) => p1.Config.RequestTimeStamp.CompareTo(p2.Config.RequestTimeStamp));//按时间重新排序
                                                                                                                          //var pm = ExamplePM(true);//加载地图

            var Ps = pm.Paths;
            foreach (var path in Ps.Values)
            {
                if (sts.AreaPathLegth.ContainsKey(path.AreaTag))
                    sts.AreaPathLegth[path.AreaTag] += path.Length;
                else
                {
                    sts.AreaPathLegth.Add(path.AreaTag, path.Length);
                }
            }
            //sts.Origins = Origins;// GetOrigins(pm),
            sts.PathMover = pm;
            sts.NPersons = 20000;
            sts.VehicleCategory = new Vehicle.Statics();
           
            return sts;

            

        }
        public class Population
        {
            public Population()
            { }
            private string person_id;
            public string Id
            {
                get { return person_id; }
                set { person_id = value; }
            }


            private string link;
            public string Link
            {
                get { return link; }
                set { link = value; }
            }

            private string time;
            public string Time
            {
                get { return time; }
                set { time = value; }
            }
        }
        static void Output(Simulator sim, string tag, Stopwatch stopwatch)
        {
            var state = (AMoD_Module)sim.Assembly;


            Console.WriteLine("{0}\t{1:F4}\t{2:F4}",


                stopwatch.Elapsed.TotalSeconds,
                state.Completed.Count,
                state.PathMover.TotalMilage
                );
             
               

          
           using (StreamWriter sw = new StreamWriter("SpeedRecord_night" + tag + ".csv", true))
            {
                
                foreach (var path in state.PathMover.Paths.Values)
                {
                    StringBuilder strValue = new StringBuilder();
                    strValue.Append(path.Config.Tag);
                    foreach (var a in path.SpeedRecrod)
                    {
                        strValue.Append(a);
                        strValue.Append(",");
                    }
                    sw.WriteLine(strValue);
                }

            }
            foreach (var R in state.Area)
            {
                using (StreamWriter sw = new StreamWriter("MFDRecord_" + R.Key + ".csv", true))
               {
                    for(var i=0; i<R.Value.Item1.Count; i++)
                    {
                        sw.WriteLine("{0},{1},{2},{3},{4}", R.Value.Item1[i], R.Value.Item2.Count, R.Value.Item3[i], R.Value.Item4[i], R.Value.Item5[i]);
                       
                    }
                }

            }
            /*using (StreamWriter sw = new StreamWriter("WaittimeRecord_" + tag + ".csv", true))
            {
                Dictionary<int, List<Person>> R = new Dictionary<int, List<Person>>();
                
                state.CompletedPickup.Sort((Person p1, Person p2) => p1.PickupTimeStamp.CompareTo(p2.PickupTimeStamp));
                foreach (var p in state.CompletedPickup)
                {
                    var H = p.PickupTimeStamp.Hour;
                    var M = p.PickupTimeStamp.Minute;
                    
                    int split = 20 * H + (M / 3);
                    if (R.ContainsKey(split))
                        R[split].Add(p);
                    else
                    { 
                        List<Person> newlist = new List<Person>();
                        R.Add(split, newlist);
                        R[split].Add(p);
                    }
                    
                }
                List<double> TS = new List<double>();
                foreach(var r in R)
                {
                   double alltime = 0;
                    foreach (var v in r.Value)
                    {
                        alltime += v.WaitTime.TotalMinutes;
                    }
                    var averagetime = alltime / r.Value.Count;
                    TS.Add(averagetime);
                }
                int j = 0;
                foreach (var i in TS)
                {
                    sw.WriteLine("{0},{1}", j, i);
                    j++;
                }
            }*/

        }
    }
}
