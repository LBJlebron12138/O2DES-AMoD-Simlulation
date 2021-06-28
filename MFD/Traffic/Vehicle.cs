using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Traffic
{
    public class Vehicle : Component<Vehicle.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public double Speed { get; set; }
        }
        #endregion

        #region Dynamics
        public List<ControlPoint.Statics> Targets { get; private set; } = new List<ControlPoint.Statics>();
        public Dictionary<int, double> Milage { get; private set; } = new Dictionary<int, double> { { 0, 0 } };
        public Dictionary<int, List<TimeSpan>> Duration { get; private set; } = new Dictionary<int, List<TimeSpan>>();
        public int CurrentPhase { get; private set; } = 0;
        /// <summary>
        /// The time when the current phase started
        /// </summary>
        public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
        public bool Idle { get; private set; } = true;
        public bool Match { get; private set; } = false;
        public ControlPoint.Statics At { get; private set; } = null;

        
       
        #endregion

        #region Events
        private abstract class InternalEvent : Event { internal Vehicle This { get; set; } } // event adapter   
        
        private class UPdataidle:InternalEvent
        {
            public override void Invoke()
            {
                if (This.Idle == true)
                    This.Idle = false;
                else
                    This.Idle = true;
            }
        }

        private class UPdatamatch : InternalEvent
        {
            public override void Invoke()
            {
                if (This.Match == true)
                    This.Match = false;
                else
                    This.Match = true;
            }
        }

        private class UPdataAt : InternalEvent
        {
            internal ControlPoint.Statics At { get; set; }
            public override void Invoke()
            {
                This.At = At;
            }
        }




        private class AddTargetEvent : InternalEvent
        {
            internal ControlPoint.Statics Target { get; set; }
            public override void Invoke()
            {
                This.Targets.Add(Target);
            }
        }
        private class SetTargetsEvent : InternalEvent
        {
            internal List<ControlPoint.Statics> Targets { get; set; }
            public override void Invoke()
            {
                This.Targets = new List<ControlPoint.Statics>(Targets);
            }
        }

        private class RemoveTargetEvent : InternalEvent { public override void Invoke() { This.Targets.RemoveAt(0); } }

        private class IncreaseMilageEvent : InternalEvent
        {
            internal double Meters { get; set; }
            public override void Invoke()
            {
                This.Milage[0] += Meters;
                if (!This.Milage.ContainsKey(This.CurrentPhase)) This.Milage.Add(This.CurrentPhase, 0);
                else This.Milage[This.CurrentPhase] += Meters;
            }
        }
        private class UpdPhaseEvent : InternalEvent
        {
            internal int Phase { get; set; }
            public override void Invoke()
            {
                if (Phase == This.CurrentPhase) return;
                if (This.CurrentPhase > 0) This.Duration[This.CurrentPhase].Add(ClockTime - This.TimeStamp); // do not count if current state is 0

                if (!This.Milage.ContainsKey(Phase)) This.Milage.Add(Phase, 0);
                if (!This.Duration.ContainsKey(Phase) && Phase > 0) This.Duration.Add(Phase, new List<TimeSpan>());
                This.CurrentPhase = Phase;
                This.TimeStamp = ClockTime;
            }
        }

        
        #endregion

        #region Input Events - Getters
        public Event AddTarget(ControlPoint.Statics target) { return new AddTargetEvent { This = this, Target = target }; }
        public Event SetTargets(List<ControlPoint.Statics> targets) { return new SetTargetsEvent { This = this, Targets = targets }; }
        public Event RemoveTarget() { return new RemoveTargetEvent { This = this }; }
        public Event CalcMilage(double meters)
        {
            return new IncreaseMilageEvent { This = this, Meters = meters };
        }
        public Event UpdPhase(int phase) { return new UpdPhaseEvent { This = this, Phase = phase }; }
        public Event UpdIdle(){ return new UPdataidle { This=this}; }
        public Event UpdMatch() { return new UPdatamatch { This = this }; }
        public Event UpdAt(ControlPoint.Statics At) { return new UPdataAt { This = this, At = At }; }//更新车辆所在节点位置

        #endregion

        public Vehicle() : base(new Statics()) { Name = "Veh"; }
        public Vehicle(Statics category, int seed, string tag = null) : base(category, seed, tag) { Name = "Veh"; }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var phase in Milage.Keys.ToList()) Milage[phase] = 0;
            foreach (var phase in Duration.Keys.ToList()) Duration[phase] = new List<TimeSpan>();
            TimeStamp = clockTime;
        }
    }
}
