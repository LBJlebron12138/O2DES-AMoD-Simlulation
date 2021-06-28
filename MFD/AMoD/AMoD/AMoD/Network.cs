using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Traffic;
using System.Xml;
using System.Text.RegularExpressions;

namespace O2DESNet.AMoD
{
    partial class Program
    {

        static PathMover.Statics NetWork(bool crossHatchAtJunctions = false)
        {
            var NW = new PathMover.Statics();


            XmlDocument networkdata = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create("network_berlin_fix.xml", settings);
            networkdata.Load(reader);
            //Population Person = new Population();
            XmlNode network = networkdata.SelectSingleNode("network");
            XmlNode nodes = network.SelectSingleNode("nodes");//节点
            XmlNode links = network.SelectSingleNode("links");//路段
            XmlNodeList node = nodes.ChildNodes;
            XmlNodeList link = links.ChildNodes;

            List<ControlPoint.Statics> CPS = new List<ControlPoint.Statics>();
            //int j = 0;
            foreach (XmlNode n in node)
            {
                XmlElement element_node = (XmlElement)n;
                var id_node = element_node.GetAttribute("id").ToString();
                var node_X = element_node.GetAttribute("x").ToString();
                var X = Convert.ToDouble(node_X);
                var node_Y = element_node.GetAttribute("y").ToString();
                var Y = Convert.ToDouble(node_Y);
                var cp = NW.CreatControlPoint(tag: string.Format("{0}", id_node));
                cp.X = X;
                cp.Y = Y;
                CPS.Add(cp);
                //j++;
                //if (j == 150)
                //    break;
            }
            foreach (string str in System.IO.File.ReadAllLines("node.txt", Encoding.Default))
            {

                var st = Regex.Split(str, ",");
                var T = Convert.ToDouble(st[0]);
                var T_1 = (int)T;
                var T_F = T_1.ToString();

                var CP = (from p in CPS
                          where p.Tag == T_F
                          select p).ToList();
                var cp = CP.First();
                int tag = Convert.ToInt32(st[3]);
                cp.AreaTag = tag;

            }




            List<Path.Statics> path_temp = new List<Path.Statics>();
            foreach (XmlNode ln in link)
            {
                XmlElement element_link = (XmlElement)ln;
                // XmlNode attributes = ln.FirstChild;
                // XmlNodeList attribute = attributes.ChildNodes;
                var id = element_link.GetAttribute("id").ToString();
                var cp_in = element_link.GetAttribute("from").ToString();
                var cp_out = element_link.GetAttribute("to").ToString();
                var length = element_link.GetAttribute("length").ToString();
                var freespeed = element_link.GetAttribute("freespeed").ToString();
                var freespeed_double = Convert.ToDouble(freespeed);
                // var capacity = element_link.GetAttribute("capacity").ToString();
                var permlanes = element_link.GetAttribute("permlanes").ToString();
                var lanes = Convert.ToDouble(permlanes);
                var Lanes = Convert.ToInt32(lanes);
                var length_double = Convert.ToDouble(length);
                
                var capacity = (int)Math.Floor(length_double * lanes / 8.5);
                //var capacity_int = Convert.ToInt32(Convert.ToDouble(capacity));
                var IN_Point_list = (from p in CPS
                                     where p.Tag == cp_in
                                     select p).ToList();

                var OUT_Point_list = (from c in CPS
                                      where c.Tag == cp_out
                                      select c).ToList();

                // if (IN_Point_list.Count != 0 || OUT_Point_list.Count != 0)
                //{
                var IN_Point = IN_Point_list.First();
                var OUT_Point = OUT_Point_list.First();
                Path.Statics path;
                path = NW.CreatePath(
                    tag: string.Format("{0}", id),
                    length: length_double,
                    capacity: capacity,
                    start: IN_Point,
                    end: OUT_Point,
                    crossHatched: crossHatchAtJunctions,
                    freespeed:freespeed_double,
                    Lanes:Lanes,
                    areatag:IN_Point.AreaTag
                    
                    );
                path.SpeedByDensity = SpeedByDensity;
                //path_temp.Add(path);
                //  }



            }


            NW.RoutingTablesFile = "Troutingtable_line.txt";
            NW.OutputRoutingTables();

            // NW.RoutingTableBuilder();
            return NW;
        }
    }
}



            /*
           static PathMover.Statics Network(bool crossHatchAtJunctions = true)
            {
                var sf = new PathMover.Statics();
                string path_load = "***";
                XmlDocument networkdata = new XmlDocument();
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.DtdProcessing = DtdProcessing.Parse;
                XmlReader reader = XmlReader.Create(path_load, settings);
                networkdata.Load(reader);
                //Population Person = new Population();

                XmlNode Nodes = networkdata.SelectSingleNode("nodes");//根节点
                XmlNodeList node = Nodes.ChildNodes;//根节点的子节点
                foreach(var nd in node)
                {

                }


                //return network;
            }




            /*
            static void Main()
            {
                XmlDocument plandata = new XmlDocument();
                plandata.Load("D:\\AMoD_data\\population.xml");
                Population Person = new Population();
                XmlNode population = plandata.SelectSingleNode("population");//根节点
                XmlNodeList person = population.ChildNodes;//根节点的子节点
                foreach (XmlNode p in person)
                {
                    XmlElement element_person = (XmlElement)p;
                    Person.Id = element_person.GetAttribute("id").ToString();
                    XmlNode t = p.FirstChild;
                    XmlNode person_plandata = t.FirstChild;
                    XmlElement element_person_plandata = (XmlElement)person_plandata;
                    Person.Link = element_person_plandata.GetAttributeNode("link").ToString();
                    Person.Time = element_person_plandata.GetAttributeNode("end_time").ToString();
                    Console.WriteLine("{0}_{1}_{2}", Person.Id, Person.Link, Person.Time);

                }

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
            */


        
 

