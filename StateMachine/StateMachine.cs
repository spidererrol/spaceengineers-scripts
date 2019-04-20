using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class StateMachine<IType>
        {
            private bool active;
            private bool paused;
            private IType state;
            private DateTime reftime;
            private int delay;
            private int everycount;
            private readonly IType idlestate;
            private readonly int every;

            public StateMachine(int everynth = 0, int offset = 0, IType idlevalue = default(IType))
            {
                active = false;
                paused = false;
                state = default(IType);
                idlestate = default(IType);
                delay = 0;
                everycount = 0 - offset;
                every = everynth;
                idlestate = idlevalue;
                if (everynth > 0)
                {
                    if (offset < 0 || offset > everynth - 1)
                        throw new Exception("Offset must be between 0 and " + (everynth - 1).ToString());
                }
                else if (everynth < 0)
                    throw new Exception("everynth must be > 0");
                else if (offset != 0)
                    throw new Exception("Offset is not applicable without everynth");
            }

            public StateMachine(IType startvalue, IType idlevalue = default(IType), int everynth = 0, int offset = 0) : this(everynth, offset, idlevalue)
            {
                state = startvalue;
                active = true;
            }

            public bool Active => active;
            public bool Paused => paused;
            public IType Peek => (everycount == 0 && active) ? state : idlestate;

            public IType Step()
            {
                if (delay > 0)
                {
                    if (delay >= (DateTime.Now - reftime).TotalSeconds)
                        return idlestate;
                    else
                        delay = 0;
                }
                if (every > 0)
                {
                    IType ret;
                    if (everycount == 0)
                        ret = state;
                    else
                        ret = idlestate;
                    everycount++;
                    everycount %= every;
                    return ret;
                }
                return state;
            }

            public void Next(IType newstate, int delay = 0)
            {
                if (delay > 0)
                {
                    reftime = DateTime.Now;
                    this.delay = delay;
                }

                state = newstate;
                active = true;
                paused = false;
            }
            public void Start(IType startstate) => Next(startstate);

            public void Pause()
            {
                if (active)
                {
                    paused = true;
                    active = false;
                }
            }

            public bool Resume()
            {
                if (paused)
                {
                    active = true;
                    paused = false;
                }
                return active;
            }

            public void Stop()
            {
                active = paused = false;
            }
        }
    }
}