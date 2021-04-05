using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript {
    partial class Program : MyGridProgram {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        /* Block Renaming ("//" comment this line to enable):

        public Program() {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save() {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }
            
        private static readonly System.Text.RegularExpressions.Regex rePosMarks = new System.Text.RegularExpressions.Regex(@"\s*<.*>");
        private static readonly System.Text.RegularExpressions.Regex reTagMarks = new System.Text.RegularExpressions.Regex(@"\s*\[.*\]");
        private static readonly System.Text.RegularExpressions.Regex reAddPos = new System.Text.RegularExpressions.Regex(@"!POS");

        public class BlockName {
            private IMyTerminalBlock terminalBlock;
            private string currentName;
            private bool modified => terminalBlock.CustomName != currentName;
            public string Name {
                get {
                    return currentName;
                }
                set {
                    currentName = value;
                }
            }

            public BlockName(IMyTerminalBlock block) {
                terminalBlock = block;
                currentName = block.CustomName;
            }

            public void ApplyPos(bool clear = false) {
                bool wantPos = Contains(reAddPos);
                bool hasPos = Contains(rePosMarks);
                if (wantPos && hasPos)
                    return;
                if (hasPos && (wantPos || clear)) {
                    Remove(reAddPos);
                    Remove(rePosMarks);
                }
                if (wantPos && !hasPos) {
                    Name += " <" + terminalBlock.Position.ToString() + ">";
                    terminalBlock.ShowOnHUD = true;
                }
            }
            public void ApplyPos(string arg) => ApplyPos(arg == "ClearPOS");

            public void ClearPos() {
                Remove(rePosMarks);
            }

            public void ApplyArg(string arg) {
                if (arg == "ClearPOS")
                    ClearPos();
                if (arg == "ClearHUD")
                    terminalBlock.ShowOnHUD = false;
            }

            public void ClearTags() => Remove(reTagMarks);

            public bool Contains(string contains) => Name.Contains(contains);
            public bool Contains(System.Text.RegularExpressions.Regex contains) => contains.Match(Name).Success;
            public void Replace(string match, string replace) => Name = Name.Replace(match, replace);
            public void Replace(System.Text.RegularExpressions.Regex match, string replace) => Name = match.Replace(Name, replace);
            public void Remove(string match) => Replace(match, "");
            public void Remove(System.Text.RegularExpressions.Regex match) => Replace(match, "");

            public void autoShowOnHUD() {
                if (Name.Contains("[EDC:"))
                    terminalBlock.ShowOnHUD = false;
                else
                    terminalBlock.ShowOnHUD = true;
            }

            public void Save() {
                if (!modified)
                    return;
                terminalBlock.CustomName = currentName;
            }

        }

        public void Main(string argument, UpdateType updateSource) {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            console.Echo(argument);
            List<IMyAirVent> vents = GetBlocks.ByType<IMyAirVent>();
            foreach (IMyAirVent vent in vents) {
                BlockName name = new BlockName(vent);
                name.ApplyPos(argument);
                name.autoShowOnHUD();

                name.ApplyArg(argument);
                name.Save();
            }
            List<IMyDoor> doors = GetBlocks.ByType<IMyDoor>();
            foreach (IMyDoor door in doors) {
                BlockName name = new BlockName(door);
                name.ApplyPos(argument);
                name.autoShowOnHUD();


                name.ApplyArg(argument);
                name.Save();
            }
            List<IMyTextPanel> lcds = GetBlocks.ByType<IMyTextPanel>();
            foreach (IMyTextPanel lcd in lcds) {
                BlockName name = new BlockName(lcd);
                name.ApplyPos(argument);
                name.autoShowOnHUD();


                name.ApplyArg(argument);
                name.Save();
            }
            console.ClearScreen();
            console.Echo("Finished: " + DateTime.Now);
        }

        // End Block renaming */
        /*
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */
        /* Solar panel controller:

        public Program() {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public DoorStatus doorStatus = DoorStatus.Closed; // I don't actually care if Closed or Opened, I'm interesting in the "ing" versions.

        public void speedPositive(IMyMotorAdvancedStator hinge) {
            if (hinge.TargetVelocityRPM == 0)
                hinge.TargetVelocityRPM = 1;
            hinge.TargetVelocityRPM = Math.Abs(hinge.TargetVelocityRPM);
        }

        public void speedNegative(IMyMotorAdvancedStator hinge) {
            if (hinge.TargetVelocityRPM == 0)
                hinge.TargetVelocityRPM = -1;
            hinge.TargetVelocityRPM = 0-Math.Abs(hinge.TargetVelocityRPM);
        }

        public int tick = 0;

        public void Main(string argument, UpdateType updateSource) {
            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            if (updateSource.HasFlag(UpdateType.Terminal) || updateSource.HasFlag(UpdateType.Trigger)) {
                // User clicked something interesting.
                console.Echo("User request: " + argument);
                switch (argument.ToLower()) {
                    case "open":
                        doorStatus = DoorStatus.Opening;
                        break;
                    case "close":
                        doorStatus = DoorStatus.Closing;
                        break;
                    case "toggle":
                        //TODO: Detect position of hinges and prefer that.
                        switch (doorStatus) {
                            case DoorStatus.Closed:
                            case DoorStatus.Closing:
                                doorStatus = DoorStatus.Opening;
                                break;
                            case DoorStatus.Open:
                            case DoorStatus.Opening:
                                doorStatus = DoorStatus.Closing;
                                break;
                        }
                        break;
                }
            }
            console.Echo(doorStatus.ToString());
            if (doorStatus == DoorStatus.Closed || doorStatus == DoorStatus.Open) {
                return;
            }
            IMyMotorAdvancedStator right = GetBlocks.FirstByName<IMyMotorAdvancedStator>("Hinge Solar Right");
            IMyMotorAdvancedStator left = GetBlocks.FirstByName<IMyMotorAdvancedStator>("Hinge Solar Left");

            const float fullAngle = 1.5f;

            // console.Echo("Left: " + left.Angle);
            // console.Echo("Right: " + right.Angle);

            left.RotorLock = false;
            right.RotorLock = false;

            if (doorStatus == DoorStatus.Opening) {
                // Left first, -ve
                // Right second, +ve
                speedNegative(left);
                if (left.Angle > 0) {
                    speedNegative(right); // Don't open this yet.
                } else {
                    speedPositive(right); // Start opening at 0 deg.
                }
                if (left.Angle <= -fullAngle && right.Angle >= fullAngle) {
                    if (tick > 10) {
                        doorStatus = DoorStatus.Open;
                        left.RotorLock = true;
                        right.RotorLock = true;
                    } else {
                        tick++;
                    }
                } else {
                    tick = 0;
                }
            } else {
                // Right first, -ve
                // Left second, +ve
                speedNegative(right);
                if (right.Angle > 0) {
                    speedNegative(left);
                } else {
                    speedPositive(left);
                }
                if (right.Angle <= -fullAngle && left.Angle >= fullAngle) {
                    if (tick > 10) {
                        doorStatus = DoorStatus.Closed;
                        left.RotorLock = true;
                        right.RotorLock = true;
                    } else {
                        tick++;
                    }
                } else {
                    tick = 0;
                }
            }
        }

        // End Solar panel controller */
        /*
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */
        /* Drill trailer controller:

        public Program() {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public static void ApplyAction1<MyType>(MyType block, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock {
            if (filter == null || filter.Invoke(block)) {
                block.ApplyAction(action);
            }
        }

        public static void ApplyAction<MyType>(IList<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock {
            foreach (MyType block in blocks) {
                ApplyAction1(block, action, filter);
            }
        }

        public static void TurnOn<MyType>(IList<MyType> blocks) where MyType : IMyFunctionalBlock => ApplyAction(blocks, "OnOff_On", b => !b.Enabled);
        public static void TurnOff<MyType>(IList<MyType> blocks) where MyType : IMyFunctionalBlock => ApplyAction(blocks, "OnOff_Off", b => b.Enabled);

        public static double Rad2Deg(double rad) => (180 * rad) / Math.PI;
        public static double Deg2Rad(double deg) => (Math.PI * deg) / 180;

        public static bool AngleLess(double rad, double less) => Rad2Deg(rad) < less;
        public static bool AngleMore(double rad, double more) => Rad2Deg(rad) > more;
        public static bool AngleEq(double rad, double value) => Rad2Deg(rad) == value;
        public static bool AngleEq<MyType>(MyType block, double value) where MyType : IMyMotorStator => Rad2Deg(block.Angle) == value;
        public static bool AngleLess<MyType>(MyType block, double less) where MyType : IMyMotorStator => Rad2Deg(block.Angle) < less;
        public static bool AngleMore<MyType>(MyType block, double more) where MyType : IMyMotorStator => Rad2Deg(block.Angle) > more;

        public static void SetVelocityPositive<MyType>(IList<MyType> blocks) where MyType : IMyMotorStator {
            foreach (MyType block in blocks) {
                if (block.TargetVelocityRPM < 0)
                    block.TargetVelocityRPM = Math.Abs(block.TargetVelocityRPM);
            }
        }

        public static void SetVelocityNegative<MyType>(IList<MyType> blocks) where MyType : IMyMotorStator {
            foreach (MyType block in blocks) {
                if (block.TargetVelocityRPM > 0)
                    block.TargetVelocityRPM = 0 - Math.Abs(block.TargetVelocityRPM);
            }
        }

        public static void UnlockGears(IList<IMyLandingGear> gears) {
            foreach (IMyLandingGear gear in gears) {
                if (gear.AutoLock)
                    gear.AutoLock = false;
                if (gear.IsLocked)
                    gear.Unlock();
            }
        }

        public static void LockGears(IList<IMyLandingGear> gears) {
            foreach (IMyLandingGear gear in gears) {
                if (!gear.AutoLock)
                    gear.AutoLock = true;
                if (!gear.IsLocked)
                    gear.Lock();
            }
        }

        public static bool IsRetracting(IMyPistonBase piston) => piston.Velocity < 0;
        public static bool IsExtending(IMyPistonBase piston) => piston.Velocity > 0;

        public enum Request {
            Idle,       // Do nothing.
            Drive,      // Pack up everything ready to drive.
            Wind,       // Generic request for Wind power.
            Drive2Wind, // Deploy & lock outriggers to generate power.
            Drill2Wind, // Retract drill but leave outriggers deployed.
            Drill,      // Deploy everything and start drilling.
        };

        public Request prevrequest = Request.Idle;
        public Request request = Request.Idle;

        public int tick = 0;

        public string notice;

        public void Main(string argument, UpdateType updateSource) {
            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            //console.Echo("Drill Control");
            // if (notice != null && notice.Length > 0) console.Echo(notice); // debugging
            if (request != Request.Idle)
                prevrequest = request;
            if (updateSource.HasFlag(UpdateType.Terminal) || updateSource.HasFlag(UpdateType.Trigger)) {
                // User clicked something interesting.
                console.Echo("=>" + argument);
                switch (argument.ToLower()) {
                    case "drive":
                        request = Request.Drive;
                        break;
                    case "wind":
                        request = Request.Wind;
                        break;
                    case "drill":
                        request = Request.Drill;
                        break;
                    default:
                        console.Echo("ERROR: What means " + argument + "?");
                        break;
                }
            }

            console.Echo(request.ToString());
            if (request == Request.Idle)
                return;

            // Hinge Outrigger * 1, Angle > 80      => Outrigger deployed
            // Hinge Outrigger * 2, Angle < 10      => Outrigger deployed
            // Hinge Outrigger * 1, Angle < -90     => Outrigger retracted
            // Hinge Outrigger * 2, Angle > 90      => Outrigger retracted

            // Rotor Outrigger Left, Angle > 28     => Outrigger deployed
            // Rotor Outrigger Left, Angle < 1      => Outrigger retracted
            // Rotor Outrigger Right, Angle < -28   => Outrigger deployed
            // Rotor Outrigger Right, Angle > -1    => Outrigger retracted

            // Drill Hinge, Angle < -85             => Drill Stowed
            // Drill Hinge, Angle > -1              => Drill Deployed

            // Drill Piston -, CurrentPosition < 1  => Drill Retracted
            // Drill Piston +, CurrentPosition > 1.9=> Drill Retracted
            // Drill Piston -, CurrentPosition > 1.9=> Drill Finished
            // Drill Piston +, CurrentPosition < 1  => Drill Finished

            List<IMyMotorAdvancedStator> drillHinges = GetBlocks.ByName<IMyMotorAdvancedStator>("Drill Hinge", false);

            List<IMyPistonBase> drillPistonM = GetBlocks.ByName<IMyPistonBase>("Drill Piston -", false); // 0 when stowed
            List<IMyPistonBase> drillPistonP = GetBlocks.ByName<IMyPistonBase>("Drill Piston +", false); // 0 when deployed

            List<IMyMotorAdvancedStator> oLeft1 = GetBlocks.ByType<IMyMotorAdvancedStator>(false).FindAll(h => h.CustomName.StartsWith("Hinge Outrigger Left")).FindAll(h => h.CustomName.EndsWith("1"));
            List<IMyMotorAdvancedStator> oLeft2 = GetBlocks.ByType<IMyMotorAdvancedStator>(false).FindAll(h => h.CustomName.StartsWith("Hinge Outrigger Left")).FindAll(h => h.CustomName.EndsWith("2"));
            List<IMyMotorAdvancedStator> oRight1 = GetBlocks.ByType<IMyMotorAdvancedStator>(false).FindAll(h => h.CustomName.StartsWith("Hinge Outrigger Right")).FindAll(h => h.CustomName.EndsWith("1"));
            List<IMyMotorAdvancedStator> oRight2 = GetBlocks.ByType<IMyMotorAdvancedStator>(false).FindAll(h => h.CustomName.StartsWith("Hinge Outrigger Right")).FindAll(h => h.CustomName.EndsWith("2"));

            List<IMyMotorAdvancedStator> o1 = oLeft1.Concat(oRight1).ToList();
            List<IMyMotorAdvancedStator> o2 = oLeft2.Concat(oRight2).ToList();

            List<IMyMotorAdvancedStator> oLeftRotor = GetBlocks.ByName<IMyMotorAdvancedStator>("Rotor Outrigger Left", false);
            List<IMyMotorAdvancedStator> oRightRotor = GetBlocks.ByName<IMyMotorAdvancedStator>("Rotor Outrigger Right", false);

            List<IMyMotorAdvancedStator> oRotor = oLeftRotor.Concat(oRightRotor).ToList();

            List<IMyReflectorLight> reflectorLights = GetBlocks.ByName<IMyReflectorLight>("Drill Rotating Light", false);
            reflectorLights = reflectorLights.Concat(GetBlocks.ByName<IMyReflectorLight>("Rotating Light Outrigger Left", false)).ToList();
            reflectorLights = reflectorLights.Concat(GetBlocks.ByName<IMyReflectorLight>("Rotating Light Outrigger Right", false)).ToList();

            List<IMyShipDrill> drill = GetBlocks.ByType<IMyShipDrill>(false);

            List<IMyLandingGear> oGear = GetBlocks.ByName<IMyLandingGear>("Landing Gear Outrigger Left", false);
            oGear = oGear.Concat(GetBlocks.ByName<IMyLandingGear>("Landing Gear Outrigger Right", false)).ToList();

            List<IMyLandingGear> drillGear = GetBlocks.ByName<IMyLandingGear>("Landing Gear Drill", false);

            List<IMyFunctionalBlock> drillExt = GetBlocks.ByName<IMyFunctionalBlock>("Drill Extending", false);
            List<IMyFunctionalBlock> drillRet = GetBlocks.ByName<IMyFunctionalBlock>("Drill Retracting", false);

            switch (request) {
                case Request.Drive:
                    // Ensure lights are running:
                    TurnOn(reflectorLights);
                    // Ensure drill is off and retracted.
                    TurnOff(drill);
                    TurnOff(drillExt);
                    // Ensure drill is retracted:
                    if (drillPistonM.FindAll(d => d.CurrentPosition > 0.0f).Count > 0 || drillPistonP.FindAll(d => d.CurrentPosition < 2.0f).Count > 0) {
                        TurnOn(drillRet);
                        if (drillPistonM.FindAll(p => IsExtending(p)).Count > 0)
                            ApplyAction(drillPistonM, "Retract");
                        if (drillPistonP.FindAll(p => IsRetracting(p)).Count > 0)
                            ApplyAction(drillPistonP, "Extend");
                        return;
                    }
                    // Ensure drill is stowed:
                    if (drillHinges.FindAll(h => AngleMore(h, -85)).Count > 0) {
                        TurnOff(drillRet);
                        SetVelocityNegative(drillHinges);
                        return;
                    }
                    // Ensure Outriggers are stowed:
                    UnlockGears(oGear);
                    if (o1.FindAll(h => AngleMore(h, 70)).Count > 0) {
                        SetVelocityNegative(o1);
                        SetVelocityPositive(o2);
                        return;
                    }
                    if (oLeftRotor.FindAll(h => AngleMore(h, 0)).Count > 0 || oRightRotor.FindAll(h => AngleLess(h, 0)).Count > 0) {
                        SetVelocityPositive(oRightRotor);
                        SetVelocityNegative(oLeftRotor);
                        return;
                    }
                    if (o1.FindAll(h => AngleMore(h, -90)).Count > 0)
                        return;
                    LockGears(drillGear);
                    TurnOff(reflectorLights);
                    request = Request.Idle;
                    break;
                case Request.Wind:
                    // I could be either in drive or drill...
                    if (o1.FindAll(h => AngleMore(h, -90)).Count == 0) {
                        notice = "o1 was in drive";
                        request = Request.Drive2Wind;
                        return;
                    }
                    if (drillPistonM.FindAll(d => d.CurrentPosition > 0.0f).Count > 0 || drillPistonP.FindAll(d => d.CurrentPosition < 2.0f).Count > 0) {
                        notice = "drill extended";
                        request = Request.Drill2Wind;
                        return;
                    }
                    if (prevrequest == Request.Drill) {
                        notice = "prev was drill";
                        request = Request.Drill2Wind;
                        return;
                    }
                    if (prevrequest == Request.Drive) {
                        notice = "prev was drive";
                        request = Request.Drive2Wind;
                        return;
                    }
                    // Maybe I'm already in Wind:
                    if (o1.FindAll(h => AngleEq(h, -90)).Count == 0 && drillPistonM.FindAll(d => d.CurrentPosition > 0.0f).Count == 0 || drillPistonP.FindAll(d => d.CurrentPosition < 2.0f).Count == 0) {
                        request = Request.Idle;
                        return;
                    }
                    console.Echo(notice = "Unable to determine current state. Idling...");
                    console.Echo("Switch to drive or drill mode and let it complete to clear the error.");
                    break;
                case Request.Drive2Wind:
                    TurnOn(reflectorLights);
                    UnlockGears(drillGear); // Unlock drill
                    if (o1.FindAll(h => AngleMore(h, -90)).Count == 0) {
                        // Start stage 1:
                        SetVelocityPositive(o1);
                        SetVelocityNegative(o2);
                        return;
                    }
                    if (o1.FindAll(h => AngleLess(h, 0)).Count > 0) {
                        // Wait until stage 1 has progressed far enough.
                        return;
                    }
                    if (o1.FindAll(h => AngleLess(h, 85)).Count > 0) { // Note: doesn't quite reach 90!
                        // While legs are still deploying:
                        SetVelocityNegative(oRightRotor);
                        SetVelocityPositive(oLeftRotor);
                        return;
                    }
                    LockGears(oGear);
                    TurnOff(reflectorLights);
                    request = Request.Idle;
                    break;
                case Request.Drill2Wind:
                    // Ensure lights are running:
                    TurnOn(reflectorLights);
                    // Ensure drill is off and retracted.
                    TurnOff(drillExt);
                    TurnOff(drill);
                    // Ensure drill is retracted:
                    if (drillPistonM.FindAll(d => d.CurrentPosition > 0.0f).Count > 0 || drillPistonP.FindAll(d => d.CurrentPosition < 2.0f).Count > 0) {
                        TurnOn(drillRet);
                        if (drillPistonM.FindAll(p => IsExtending(p)).Count > 0)
                            ApplyAction(drillPistonM, "Retract");
                        if (drillPistonP.FindAll(p => IsRetracting(p)).Count > 0)
                            ApplyAction(drillPistonP, "Extend");
                        return;
                    }
                    // Ensure drill is stowed:
                    if (drillHinges.FindAll(h => AngleMore(h, -85)).Count > 0) {
                        TurnOff(drillRet);
                        SetVelocityNegative(drillHinges);
                        return;
                    }
                    TurnOff(reflectorLights);
                    request = Request.Idle;
                    break;
                case Request.Drill:
                    TurnOn(reflectorLights);
                    UnlockGears(drillGear); // Unlock drill
                    if (o1.FindAll(h => AngleMore(h, -90)).Count == 0) {
                        // Start stage 1:
                        SetVelocityPositive(o1);
                        SetVelocityNegative(o2);
                        return;
                    }
                    if (o1.FindAll(h => AngleLess(h, 0)).Count > 0) {
                        // Wait until stage 1 has progressed far enough.
                        return;
                    }
                    if (o1.FindAll(h => AngleLess(h, 85)).Count > 0) { // Note: doesn't quite reach 90!
                        // While legs are still deploying:
                        SetVelocityNegative(oRightRotor);
                        SetVelocityPositive(oLeftRotor);
                        return;
                    }
                    LockGears(oGear);

                    if (drillHinges.FindAll(h => AngleLess(h, -88)).Count > 0) {
                        SetVelocityPositive(drillHinges);
                        return;
                    }
                    if (drillHinges.FindAll(h => AngleLess(h, -1)).Count > 0) {
                        // Wait for drill to deploy:
                        return;
                    }

                    TurnOn(drill);
                    TurnOn(drillExt);

                    // First retract each P segment:
                    if (drillPistonP.FindAll(p => p.CurrentPosition > 0.0f && p.CurrentPosition < 2.0f).Count > 0) {
                        // A P is in progress:
                        return;
                    }
                    if (drillPistonP.FindAll(d => d.CurrentPosition > 0.0f).Count > 0) {
                        // Retract the first available:
                        ApplyAction1(drillPistonP.FindAll(d => d.CurrentPosition > 0.0f).First(),"Retract");
                        return;
                    }

                    // Now extend each M segment:
                    if (drillPistonM.FindAll(p => p.CurrentPosition > 0.0f && p.CurrentPosition < 2.0f).Count > 0) {
                        // A M is in progress:
                        return;
                    }
                    if (drillPistonM.FindAll(d => d.CurrentPosition < 2.0f).Count > 0) {
                        // Extend the first available:
                        ApplyAction1(drillPistonM.FindAll(d => d.CurrentPosition < 2.0f).First(), "Extend");
                        return;
                    }

                    // This should never hit, but leave it in as a safety:
                    if (drillPistonM.FindAll(d => d.CurrentPosition < 2.0f).Count > 0 || drillPistonP.FindAll(d => d.CurrentPosition > 0.0f).Count > 0) {
                        // Wait for full extension
                        return;
                    }
                    TurnOff(drillExt);
                    request = Request.Wind; // Retract & Stow
                    break;
            }
        }

        // End Drill trailer controller */

        /* Lift Controller:

        public Program() {

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        const double scanrange = 10;

        public void Main(string argument, UpdateType updateSource) {
            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            IMyPistonBase myPiston = GetBlocks.FirstByName<IMyPistonBase>("Piston Lift");
            IMyCameraBlock cam = GetBlocks.FirstByName<IMyCameraBlock>("Camera Lift", false);
            console.ClearText();
            if (!cam.CanScan(scanrange)) {
                console.Echo("Charging...");
                cam.EnableRaycast = true;
                return;
            } else {
                cam.EnableRaycast = false;
            }

            MyDetectedEntityInfo det = cam.Raycast(scanrange);
            if (det.IsEmpty()) {
                myPiston.MaxLimit = myPiston.HighestPosition;
                console.Echo("Unlimited");
            } else {
                double dist = Vector3D.Distance(cam.GetPosition(), det.HitPosition.Value);
                myPiston.MaxLimit = myPiston.MinLimit + 1 + (float)dist;
                console.Echo("Limited at " + myPiston.MaxLimit.ToString("N2") + " (" + dist.ToString("N2") + ")");
            }
        }

        // End Lift Controller */

        //* Drill Tower Controller:

        const float DRILL_DRILL_SPEED = 0.1f; // How fast to move drill piston when drilling
        const float DRILL_GRIND_SPEED = 0.5f; // How fast to move drill when grinding
        const float DRILL_FAST_SPEED = 1.0f; // How fast to move drill piston "disconnected" manouvers

        const float OUTRIGGER_EXTEND_SPEED = 1.0f; // How fast to extend the outriggers.
        const float OUTRIGGER_RETRACT_SPEED = 1.0f; // How fast to retract the outriggers.

        const float WASTE_PISTON_SPEED = 1.0f; // How fast to extend/retract the waste pistons.
        const float WASTE_HINGE_SPEED = 0.5f; // How fast to move the waste hinges (note they all run at the same speed)
        const float WASTE_THRESHOLD_DEG = 45.0f; // How far to open "Hinge Ejector 1" before extending pistons.

        const float ORE_FILL_THRESHOLD = 0.9f;   // When cargos reach this fraction full of ore then suspend drilling

        // This is actually used as a constant, but that causes warnings so don't make it one:
        bool RETRACT_ON_FULL = true;       // If Ore is "full" then retract the drill head (otherwise leave down for faster resume)

        const float fudgeFactor = 0.1f; // When checking limit stops, allow this much fudging

        public Program() {

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public enum State {
            //
            // NOTE: These need to be lower case to ensure TryParse works (I think).
            //
            idle,       // Don't try to do anything.
            deploy,     // Deploy on ground ready for disconnect.
            drill,      // Drill down to get ores.
            retract,    // Retract drill
            drive,      // Ready drill for movement (ie retract outriggers etc).
            stop,       // Emergency stop - (try to) immediately stop anything that moves
        };
        public State currentState = State.idle;
        public State prevState = State.idle;

        public MyShipConnectorStatus prevConnectionStatus = MyShipConnectorStatus.Unconnected;
        public IMyShipConnector ShipConnector => GetBlocks.FirstByName<IMyShipConnector>("Drill Tower", true);
        public IMyPistonBase DrillPiston => GetBlocks.FirstByName<IMyPistonBase>("Drills");
        public IMySensorBlock DrillRetractedSensor => GetBlocks.FirstByName<IMySensorBlock>("Drills Retracted");
        //public IMySensorBlock DrillTopSensor => GetBlocks.FirstByName<IMySensorBlock>("Drills Top"); // Gone - not needed any more.
        public IMyProjector Projector => GetBlocks.FirstByType<IMyProjector>();
        public IMyShipMergeBlock PistonMerge => GetBlocks.FirstByName<IMyShipMergeBlock>("Piston");
        public IMyShipMergeBlock BaseMerge => GetBlocks.FirstByName<IMyShipMergeBlock>("Base");
        public IMyConveyorSorter WasteSorter => GetBlocks.FirstByName<IMyConveyorSorter>("Stone Ejector");
        // No need to play with Ores Sorter as it is handled by a sensor.

        public List<IMyLandingGear> OutriggerGears => GetBlocks.ByName<IMyLandingGear>("Outrigger");
        public List<IMyPistonBase> OutriggerPistons => GetBlocks.ByName<IMyPistonBase>("Outrigger");
        public List<IMyLandingGear> RearGears => GetBlocks.ByType<IMyLandingGear>(b => b.CubeGrid == Me.CubeGrid); // These need to be exact grid not IsSameConstructAs
        public List<IMyShipDrill> Drills => GetBlocks.ByType<IMyShipDrill>();
        public List<IMyShipGrinder> Grinders => GetBlocks.ByType<IMyShipGrinder>();
        public List<IMyShipWelder> Welders => GetBlocks.ByType<IMyShipWelder>();
        public List<IMyMotorStator> WasteHinges => GetBlocks.ByName<IMyMotorStator>("Hinge Ejector");
        public List<IMyPistonBase> WastePistons => GetBlocks.ByName<IMyPistonBase>("Piston Ejector");


        public IMyLandingGear ConnectedGear(IMyPistonBase piston) => GetBlocks.FirstByType<IMyLandingGear>(b => b.CubeGrid == piston.TopGrid);

        public ConsoleSurface console;

        public static double Rad2Deg(double rad) => (180 * rad) / Math.PI;
        public static double Deg2Rad(double deg) => (Math.PI * deg) / 180;

        public static void Off(IMyFunctionalBlock b) {
            if (b.Enabled)
                b.ApplyAction("OnOff_Off");
        }
        public static void On(IMyFunctionalBlock b) {
            if (!b.Enabled)
                b.ApplyAction("OnOff_On");
        }
        public static void Velocity(IMyPistonBase b, float v) {
            if (b.Velocity != v)
                b.Velocity = v;
        }
        public static void Retract(IMyPistonBase b, float v) => Velocity(b, -Math.Abs(v));
        public static void Extend(IMyPistonBase b, float v) => Velocity(b, Math.Abs(v));

        public static float AngleDeg(IMyMotorStator b) => (float)(Rad2Deg(b.Angle));
        public static void Velocity(IMyMotorStator b, float rpm) {
            if (b.TargetVelocityRPM != rpm)
                b.TargetVelocityRPM = rpm;
        }
        // Note: Hinges are reversed (0 is fully open, 90 is fully closed), these functions are adjusted for that.
        public static void Close(IMyMotorStator b, float rpm) => Velocity(b, Math.Abs(rpm));
        public static void Open(IMyMotorStator b, float rpm) => Velocity(b, -Math.Abs(rpm));
        public static void HingeLock(IMyMotorStator b) => b.RotorLock = true;
        public static void HingeUnLock(IMyMotorStator b) => b.RotorLock = false;

        public static void Off<IType>(List<IType> bs) where IType : IMyFunctionalBlock => bs.ForEach(b => Off(b));
        public static void On<IType>(List<IType> bs) where IType : IMyFunctionalBlock => bs.ForEach(b => On(b));
        public static void Retract<IType>(List<IType> bs, float v) where IType : IMyPistonBase => bs.ForEach(b => Retract(b, v));
        public static void Extend<IType>(List<IType> bs, float v) where IType : IMyPistonBase => bs.ForEach(b => Extend(b, v));
        public static void Close<IType>(List<IType> bs, float rpm) where IType : IMyMotorStator => bs.ForEach(b => Close(b, rpm));
        public static void Open<IType>(List<IType> bs, float rpm) where IType : IMyMotorStator => bs.ForEach(b => Open(b, rpm));
        public static void HingeLock<IType>(List<IType> bs) where IType : IMyMotorStator => bs.ForEach(b => HingeLock(b));
        public static void HingeUnLock<IType>(List<IType> bs) where IType : IMyMotorStator => bs.ForEach(b => HingeUnLock(b));

        public static bool GEish(float l, float r) {
            return l >= r - fudgeFactor;
        }
        public static bool LEish(float l, float r) {
            return l <= r + fudgeFactor;
        }

        public void Transition() {
            //TODO: figure out which states need a safe transition (if any) and handle them. Change prevState when done.
            if (prevState == State.drill && DrillRetractedSensor.IsActive && currentState != State.stop) {
                Stop();
            }
            prevState = State.idle;
        }

        public void CheckEvents() {
            if (ShipConnector.Status == MyShipConnectorStatus.Connected && prevConnectionStatus != MyShipConnectorStatus.Connected) {
                // Freshly connected!
                if (prevState == State.drive)
                    prevState = State.idle;
                currentState = State.drive;
                console.Echo("Connected - selecting 'drive' mode!");
            }
            prevConnectionStatus = ShipConnector.Status;
        }

        public void Deploy() {
            int landed = 0;
            foreach (IMyPistonBase pist in OutriggerPistons) {
                IMyLandingGear gear = ConnectedGear(pist);
                gear.AutoLock = false;
                if (gear.LockMode == LandingGearMode.Locked) {
                    landed++;
                    console.Echo(gear.CustomName + " is landed");
                    continue;
                }

                if (gear.LockMode == LandingGearMode.ReadyToLock) {
                    console.Echo("Locking " + gear.CustomName);
                    pist.MaxLimit = pist.CurrentPosition;
                    gear.Lock();
                } else {
                    pist.MaxLimit = pist.HighestPosition;
                    if (pist.Velocity != OUTRIGGER_EXTEND_SPEED)
                        console.Echo("Extending " + gear.CustomName);
                    pist.Velocity = OUTRIGGER_EXTEND_SPEED;
                }
            }
            if (landed < OutriggerPistons.Count)
                return;
            if (RearGears.Count != 2) {
                console.Echo("I found " + RearGears.Count + " rear gears!");
                currentState = State.idle;
                return;
            }
            foreach (IMyLandingGear gear in RearGears) {
                gear.Unlock();
            }
            ShipConnector.Disconnect();
            currentState = State.idle;
        }
        public static bool IsConnected(IMyShipMergeBlock b) => b.IsConnected; // There are reports of this not working so if it fails I'll switch to comparing CubeGrids.

        public void Drill() {
            if (GEish(DrillPiston.CurrentPosition, DrillPiston.MaxLimit) && !IsConnected(BaseMerge) && Projector.RemainingBlocks > 0) {
                // We are fully extended, not merged to base, and incomplete
                if (RETRACT_ON_FULL) {
                    currentState = State.retract;
                } else {
                    currentState = State.idle;
                }
                return;
            }
            if (GEish(AngleDeg(WasteHinges.First()), 90)) {
                HingeUnLock(WasteHinges);
                Open(WasteHinges, WASTE_HINGE_SPEED);
                // Note: Allow to continue, I can start drilling whilst still extending this!
            }
            if (GEish(AngleDeg(WasteHinges.First()), 0) && LEish(AngleDeg(WasteHinges.First()), WASTE_THRESHOLD_DEG)) {
                Extend(WastePistons, WASTE_PISTON_SPEED);
            }
            if (LEish(AngleDeg(WasteHinges.First()), 0)) {
                HingeLock(WasteHinges);
                On(WasteSorter);
            }
            Off(Grinders);
            On(Drills);
            On(Welders);
            On(Projector);
            if (LEish(DrillPiston.CurrentPosition, DrillPiston.MinLimit) && !IsConnected(PistonMerge)) {
                On(PistonMerge);
                // Unfortunatly these won't both merge at once.
                Off(BaseMerge);
            }
            if (LEish(DrillPiston.CurrentPosition, DrillPiston.MinLimit) && IsConnected(PistonMerge)) {
                Extend(DrillPiston, DRILL_DRILL_SPEED);
            }
            if (GEish(DrillPiston.CurrentPosition, DrillPiston.MaxLimit) && IsConnected(PistonMerge)) {
                On(BaseMerge);
                if (!IsConnected(BaseMerge))
                    return;
                Off(PistonMerge);
            }
            if (GEish(DrillPiston.CurrentPosition, DrillPiston.MaxLimit) && !IsConnected(PistonMerge)) {
                Retract(DrillPiston, DRILL_FAST_SPEED);
            }
        }
        public void Retract() {
            if (DrillRetractedSensor.IsActive) {
                if (!GEish(AngleDeg(WasteHinges.First()), 90)) {
                    Off(WasteSorter);
                    HingeUnLock(WasteHinges);
                    Retract(WastePistons, WASTE_PISTON_SPEED);
                    Close(WasteHinges, WASTE_HINGE_SPEED);
                    return;
                }
                HingeLock(WasteHinges);
                if (LEish(DrillPiston.CurrentPosition, DrillPiston.MinLimit)) {
                    Grinders.ForEach(b => Off(b));
                    Welders.ForEach(b => Off(b));
                    Drills.ForEach(b => Off(b));
                    currentState = State.idle;
                    return;
                }
                Retract(DrillPiston, DRILL_GRIND_SPEED);
            }
            Off(Projector);
            Off(Welders);
            On(Grinders);
            if (GEish(DrillPiston.CurrentPosition, DrillPiston.MaxLimit) && IsConnected(BaseMerge)) {
                On(PistonMerge);
                if (!IsConnected(PistonMerge)) {
                    return; // Wait
                }
                Off(BaseMerge);
            }
            if (GEish(DrillPiston.CurrentPosition, DrillPiston.MaxLimit) && !IsConnected(BaseMerge)) {
                Retract(DrillPiston, DRILL_GRIND_SPEED);
            }
            if (LEish(DrillPiston.CurrentPosition, DrillPiston.MinLimit) && IsConnected(PistonMerge)) {
                On(BaseMerge);
                //Danger: BaseMerge won't connect in this position until PistonMerge releases, so I just have to pray!
                Off(PistonMerge);
            }
            if (LEish(DrillPiston.CurrentPosition, DrillPiston.MinLimit) && !IsConnected(PistonMerge)) {
                Extend(DrillPiston, DRILL_FAST_SPEED); // Extending detached so I can go fast.
            }
        }
        public void Drive() {
            if (!DrillRetractedSensor.IsActive) {
                Retract();
                currentState = State.drive;
                return;
            }
            if (ShipConnector.Status == MyShipConnectorStatus.Connectable)
                ShipConnector.Connect();
            RearGears.FindAll(g => g.LockMode == LandingGearMode.ReadyToLock).ForEach(g => g.Lock());
            bool glocked = RearGears.Any(g => g.LockMode == LandingGearMode.Locked);
            if (ShipConnector.Status != MyShipConnectorStatus.Connected && !glocked) {
                console.Echo("Not connected to a hauler! Refusing to enter drive mode!");
                currentState = State.idle;
                return;
            }
            OutriggerGears.ForEach(g => g.Unlock());
            if (OutriggerGears.Any(g => g.LockMode == LandingGearMode.Locked)) {
                console.Echo("Failed to unlock outriggers! Aborting drive mode!");
                currentState = State.idle;
                return;
            }
            OutriggerPistons.ForEach(p => p.Velocity = -OUTRIGGER_RETRACT_SPEED);
            OutriggerPistons.ForEach(p => p.MaxLimit = p.HighestPosition);
            console.Echo("Ready to drive");
            currentState = State.idle;
            return;
        }
        public void Stop() {
            OutriggerPistons.ForEach(p => p.Velocity = 0);
            WastePistons.ForEach(p => p.Velocity = 0);
            DrillPiston.Velocity = 0;
            WasteHinges.ForEach(h => h.TargetVelocityRPM = 0);
            Grinders.ForEach(b => Off(b));
            Welders.ForEach(b => Off(b));
            Drills.ForEach(b => Off(b));
            Off(WasteSorter);
            currentState = State.idle;
            return;
        }

        //TODO: Button controls
        //TODO: Ore Cargo Capacity "[Ore" or ",Ore"
        public void Main(string argument, UpdateType updateSource) {
            console = ConsoleSurface.EasyConsole(this,"[Drill Control]");
            State savePrevState = currentState;
            if (argument.ToLower().Trim() != "") {
                if (!Enum.TryParse(argument.ToLower().Trim(), out currentState)) {
                    currentState = savePrevState; // Paranoia check
                    console.Echo("ERROR: No such command '" + argument + "'");
                    return;
                }
            }
            if (currentState != savePrevState) {
                prevState = savePrevState;
            }
            if (currentState != prevState) {
                console.Echo(currentState.ToString());
            }

            CheckEvents();

            if (currentState == State.idle)
                return; // Nothing to do.
            if (prevState != State.idle && currentState != prevState) {
                Transition();
                return;
            }

            switch (currentState) {
                case State.deploy:
                    Deploy();
                    break;
                case State.drill:
                    Drill();
                    break;
                case State.drive:
                    Drive();
                    break;
                case State.retract:
                    Retract();
                    break;
                case State.stop:
                    Stop();
                    break;
                default:
                    console.Echo("ERROR: Got into an unknown state: " + currentState);
                    return;
            }
        }

        // End Drill Tower Controller */

        /* Temp:

        // End Temp */

    }
}
