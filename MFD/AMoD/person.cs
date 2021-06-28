using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using O2DESNet.Traffic;
using System.Xml;

namespace O2DESNet.AMoD
{
    public class Person : Component<Person.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public string Tag { get; set; }
            public DateTime RequestTimeStamp { get; set; }
            public ControlPoint.Statics RequestPostion { get; set; } = new ControlPoint.Statics();
            public ControlPoint.Statics RequestTarget { get; set; } = new ControlPoint.Statics();
        }
        #endregion

        #region Dynamics
        /**********************************************************/
        /* All dynamic properties shall have only public getter,  */
        /* where setter should remain as private.                 */
        /**********************************************************/
        //public HourCounter HourCounter { get; private set; }
        public DateTime PickupTimeStamp { get; private set; }
        public DateTime DropoffTimeStamp { get;private set; }
        public TimeSpan WaitTime { get;private set; }
        
        

        #endregion

        #region Events
        private abstract class InternalEvent : Event { internal Person This { get; set; } } // event adapter 

        private class GetPickupTimeStampEvent : InternalEvent
        {
            public override void Invoke()
            {
                This.PickupTimeStamp = ClockTime;
            }
        }

        private class GetDropoffTimeStampEvent : InternalEvent
        {
            public override void Invoke()
            {
                This.DropoffTimeStamp = ClockTime;
            }
        }

        private class GetWaitTimeEvent : InternalEvent
        {
            public override void Invoke()
            {
                This.WaitTime = ClockTime - This.Config.RequestTimeStamp;
            }
        }


        /**********************************************************/
        /* All internal events shall be private,                  */
        /* and inherite from InternalEvent as defined above       */
        /**********************************************************/
        //private class InitEvent : InternalEvent
        //{
        //    public override void Invoke()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        #endregion

        #region Input Events - Getters
        /***************************************************************/
        /* Methods returning an InternalEvent as O2DESNet.Event,       */
        /* with parameters for the objects to be passed in.            */
        /* Note that the InternalEvent shall always carry This = this. */
        /***************************************************************/
        //public Event Input(TLoad load) { return new InternalEvent { This = this, Load = load }; }
        public Event GetPickupTimeStamp() { return new GetPickupTimeStampEvent { This = this }; }
        public Event GetDropoffTimeStamp() { return new GetDropoffTimeStampEvent { This = this }; }
        public Event GetWaitTime() { return new GetWaitTimeEvent{ This = this }; }
        #endregion

        #region Output Events - Reference to Getters
        /***********************************************************************/
        /* List of functions that maps outgoing objects to an external event.  */
        /* Note that the mapping is specified only in external structure.      */
        /***********************************************************************/
        //public List<Func<TLoad, Event>> OnOutput { get; private set; } = new List<Func<TLoad, Event>>();
        #endregion

        public Person() : base(new Statics()) { Name = "person"; }
        public Person(Statics config, int seed, string tag = null) : base(config, seed, tag)
        {
            Name = "Person";
        }

        public override void WarmedUp(DateTime clockTime)
        {
            base.WarmedUp(clockTime);
        }

        public override void WriteToConsole(DateTime? clockTime = null)
        {
            base.WriteToConsole(clockTime);
        }
    }
}
