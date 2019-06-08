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
    partial class Program : MyGridProgram
    {
        // AutomatedAssembler.cs
        // Automated Assembler v2.2
        #region mdk macros
        // Deployed: $MDK_DATETIME$
        #endregion mdk macros

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        const string default_component_maintype = "MyObjectBuilder_BlueprintDefinition";
        private const string DebugTag = "[AutomatedAssembler-Debug]";

        // Map item names to what assemblers need to see:
        Dictionary<string, string> item2definition = new Dictionary<string, string>
        {
            { "advancedpowercell","MyObjectBuilder_BlueprintDefinition/AdvancedPowerCell" },
            { "paintgunmag","MyObjectBuilder_BlueprintDefinition/Blueprint_PaintGunMag" },
            { "explosionfuel","MyObjectBuilder_BlueprintDefinition/bpExplosionFuel" },
            { "implosionfuel","MyObjectBuilder_BlueprintDefinition/bpImplosionFuel" },
            { "sncnc40mmbullet","MyObjectBuilder_BlueprintDefinition/bpSNCNC40mmBullet" },
            { "sncncpulseenergy","MyObjectBuilder_BlueprintDefinition/bpSNCNCPulseEnergy" },
            { "bulletproofglass","MyObjectBuilder_BlueprintDefinition/BulletproofGlass" },
            { "canvas","MyObjectBuilder_BlueprintDefinition/Canvas" },
            { "capacitormodule","MyObjectBuilder_BlueprintDefinition/CapacitorModule" },
            { "computer","MyObjectBuilder_BlueprintDefinition/ComputerComponent" },
            { "construction","MyObjectBuilder_BlueprintDefinition/ConstructionComponent" },
            { "detector","MyObjectBuilder_BlueprintDefinition/DetectorComponent" },
            { "display","MyObjectBuilder_BlueprintDefinition/Display" },
            { "explosives","MyObjectBuilder_BlueprintDefinition/ExplosivesComponent" },
            { "girder","MyObjectBuilder_BlueprintDefinition/GirderComponent" },
            { "gravitygenerator","MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent" },
            { "hslasteel","MyObjectBuilder_BlueprintDefinition/hslaSteel" },
            { "interiorplate","MyObjectBuilder_BlueprintDefinition/InteriorPlate" },
            { "largetube","MyObjectBuilder_BlueprintDefinition/LargeTube" },
            { "medical","MyObjectBuilder_BlueprintDefinition/MedicalComponent" },
            { "metalgrid","MyObjectBuilder_BlueprintDefinition/MetalGrid" },
            { "missile200mm","MyObjectBuilder_BlueprintDefinition/Missile200mm" },
            { "motor","MyObjectBuilder_BlueprintDefinition/MotorComponent" },
            { "nato_25x184mmmagazine","MyObjectBuilder_BlueprintDefinition/NATO_25x184mmMagazine" },
            { "nato_5p56x45mmmagazine","MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine" },
            { "parachute","MyObjectBuilder_BlueprintDefinition/Parachute" },
            { "powercell","MyObjectBuilder_BlueprintDefinition/PowerCell" },
            { "productioncontrol","MyObjectBuilder_BlueprintDefinition/productioncontrolcomponent" },
            { "radiocommunication","MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent" },
            { "reactor","MyObjectBuilder_BlueprintDefinition/ReactorComponent" },
            { "shield","MyObjectBuilder_BlueprintDefinition/ShieldComponent" },
            { "smalltube","MyObjectBuilder_BlueprintDefinition/SmallTube" },
            { "solarcell","MyObjectBuilder_BlueprintDefinition/SolarCell" },
            { "steelplate","MyObjectBuilder_BlueprintDefinition/SteelPlate" },
            { "superconductor","MyObjectBuilder_BlueprintDefinition/Superconductor" },
            { "thrust","MyObjectBuilder_BlueprintDefinition/ThrustComponent" },
        };
        public MyDefinitionId ItemToDefinitionId(string item)
        {
            if (item2definition.ContainsKey(item.ToLower()))
                return MyDefinitionId.Parse(item2definition[item.ToLower()]);
            else
                return MyDefinitionId.Parse(item);
        }

        public bool HaveItemToDefinitionId(string item) => item2definition.ContainsKey(item.ToLower());

        // Ingredients (optional)
        public IDictionary<string, IList<KeyValuePair<string, float>>> ingredients = new Dictionary<string, IList<KeyValuePair<string, float>>>();

        // Quotas
        Dictionary<string, int> quotas = new Dictionary<string, int> {
            {"BulletproofGlass",12000},
            {"Canvas",300 },
            {"Computer",6500},
            {"Construction",50000},
            {"Detector",400},
            {"Display",500},
            {"Explosives",500},
            {"Girder",3500},
            {"GravityGenerator",250},
            {"InteriorPlate",55000},
            {"LargeTube",6000},
            {"Medical",120},
            {"MetalGrid",15500},
            {"Motor",16000},
            {"PowerCell",2800},
            {"RadioCommunication",250},
            {"Reactor",10000},
            {"SmallTube",26000},
            {"SolarCell",2800},
            {"SteelPlate",300000},
            {"Superconductor",3000},
            {"Thrust",16000},
        };

        // The tag to use to find the assembler & LCD.
        string TAG = "[AUTOMATIC]";

        // This is for mapping imported to the values I use:
        Dictionary<string, string> disp2real = new Dictionary<string, string> {
            { "Bulletp. Glass","BulletproofGlass" },
            { "GravGen","GravityGenerator" },
            { "Radio-comm","RadioCommunication" },
            { "Thruster","Thrust" },
        };

        // You don't need to edit anything below this line.

        //System.Text.RegularExpressions.Regex reEndComponent = new System.Text.RegularExpressions.Regex(@"Component$");

        Queue<string> statusmsgs = new Queue<string>();
        MultiSurface statussurfaces;
        ConsoleSurface console;

        void StatusInit()
        {
            statusmsgs.Clear();
            if (statussurfaces != null)
                statussurfaces.ClearText();
        }

        void FindStatusLCD()
        {
            List<IMyTextSurfaceProvider> providers = GetBlocks.ByName<IMyTextSurfaceProvider>(TAG);
            List<IMyTextSurface> surfaces = GetBlocks.ByName<IMyTextSurface>(TAG);
            MultiSurface.ISurfaceFilter
                surfaceFilter = MultiSurface.ShowOnScreenFilter(ParseConfigSurface);
            statussurfaces = new MultiSurface(providers, surfaces, surfaceFilter);

            statussurfaces.ClearOnWrite();
            statussurfaces.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;

            while (statusmsgs.Any())
                statussurfaces.WriteLine(statusmsgs.Dequeue());
        }

        private bool EnableDebug = true;

        void Status(string msg)
        {
            statusmsgs.Enqueue(msg);
            while (statussurfaces != null && statusmsgs.Count > 0)
                statussurfaces.WriteLine(statusmsgs.Dequeue());
            console.Echo((EnableDebug ? "S:" : "") + msg);
        }

        void DebugInit()
        {
            console = ConsoleSurface.EasyConsole(this);
            console.Add(GetBlocks.ByName<IMyTextPanel>(DebugTag));
        }

        void Debug(string msg)
        {
            if (EnableDebug)
                console.Echo("D:" + msg);
        }

        void SetQuota(string item, int qty)
        {
            Debug("SetQuota(" + item + "," + qty + ")");
            item = item.Trim('"', '\'', ' ');
            if (quotas.ContainsKey(item))
            {
                Debug("Quota[" + item + "] = " + quotas[item] + " => " + qty);
                quotas[item] = qty;
            }
            else
            {
                List<string> qkeys = new List<string>(quotas.Keys);
                foreach (string qkey in qkeys)
                {
                    if (qkey.ToLower() == item.ToLower())
                    {
                        Debug("quota[" + qkey + "] = " + qty);
                        quotas[qkey] = qty;
                        return;
                    }
                }
                List<IMyAssembler> assemblers = GetBlocks.ByName<IMyAssembler>(TAG, false);
                if (assemblers.Count < 1)
                {
                    Status("No " + TAG + " assemblers found");
                    return;
                }

                //IDictionary<string, IList<KeyValuePair<string, float>>> reqs = assemblers[0].GetValue<IDictionary<string, IList<KeyValuePair<string, float>>>>("ReadComponentBlueprint");

                if (item2definition.ContainsKey(item.ToLower()))
                {
                    if (item.ToLower() == item)
                    {
                        System.Globalization.TextInfo textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
                        item = textInfo.ToTitleCase(item);
                    }

                    Status("Adding quota for: " + item);
                    quotas.Add(item, qty);
                }
                else
                {
                    Status("Unknown item " + item + " - skipping quota");
                }
            }
        }

        void SetQuota(string item, string qty) => SetQuota(item, HUnit(qty));

        class LinkOrder<IMyType> : IComparer<IMyType> where IMyType : IMyTerminalBlock
        {
            public int Compare(IMyType l, IMyType r)
            {
                string[] ls = l.CustomName.Split(' ');
                string[] rs = r.CustomName.Split(' ');
                int li = int.Parse(ls[ls.Length - 1]);
                int ri = int.Parse(rs[rs.Length - 1]);
                return li.CompareTo(ri);
            }

        }

        int HUnit(string hunit) => (int)Utility.HUnitToDouble(hunit);

        string HUnit(int number) => Utility.DoubleToHUnit(number);

        // [---] thing n / x
        System.Text.RegularExpressions.Regex LCDCompLine = new System.Text.RegularExpressions.Regex(@"\s*\[[^\]]+\]\s*(.*?)\s+(?:\d+\.\d+|\d+)\s*[kmg]?\s*/\s*((?:\d+\.\d+|\d+)\s*[kmg]?)\s*", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        void ImportLCD(string LCDname)
        {
            Debug("Import LCD: " + LCDname);
            List<IMyTextPanel> panels = GetBlocks.ByName<IMyTextPanel>(LCDname);
            string combined = "";
            panels.Sort(new LinkOrder<IMyTextPanel>());
            for (int i = 0; i < panels.Count; i++)
            {
                combined = string.Concat(combined, panels[i].GetText());
            }
            string[] lines = combined.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                System.Text.RegularExpressions.MatchCollection matches = LCDCompLine.Matches(line);
                for (int m = 0; m < matches.Count; m++)
                {
                    System.Text.RegularExpressions.Match match = matches[m];
                    if (!match.Success)
                        continue;
                    Debug(" Searching for " + match.Groups[1].Value);
                    string comp = FindComponent(match.Groups[1].Value);
                    if (comp == null)
                    {
                        Debug("  No match");
                        continue;
                    }
                    Debug("  =" + comp);

                    int quota = HUnit(match.Groups[2].Value);
                    SetQuota(comp, quota);
                }
            }
        }

        private string FindComponent(string description)
        {
            string dns = description.Replace(" ", "");
            if (quotas.ContainsKey(dns))
                return dns.ToLower();
            if (disp2real.ContainsKey(description))
                return disp2real[description].ToLower();
            if (item2definition.ContainsKey(dns.ToLower()))
                return dns.ToLower();
            if (description.EndsWith(".."))
            { // Probably cut off, need a more complex search:
                description = description.Substring(0, description.Length - 2);
                dns = description.Replace(" ", "");
                Debug("Searching using '" + description + "'");
                HashSet<string> hits = new HashSet<string>();
                quotas.Keys.ToList().FindAll(i => i.StartsWith(dns)).ForEach(i => hits.Add(i.ToLower()));
                disp2real.Keys.ToList().FindAll(i => i.StartsWith(description)).ConvertAll(i => disp2real[i]).ForEach(i => hits.Add(i.ToLower()));
                item2definition.Keys.ToList().FindAll(i => i.StartsWith(dns.ToLower())).ForEach(i => hits.Add(i.ToLower()));
                if (hits.Count == 1)
                    return hits.First();
                Debug("Multiple hits - cannot determine which to match (" + Utility.List2String(hits.ToList()) + ")");
            }
            return null;
        }

        void ReadCommandsPB()
        {
            ReadCommands(Me.CustomData);
        }

        void ReadCommandsLCD()
        {
            //if (statuslcd != null)
            //    ReadCommands(statuslcd.CustomData);
            foreach (IMyTextPanel lcd in GetBlocks.ByName<IMyTextPanel>(TAG))
            {
                ReadCommands(lcd.CustomData);
            }
        }

        List<string> ParseCommand(string command, string commandline, int maxparts = 0) => ParseCommand(commandline.Trim().Remove(0, command.Length).TrimStart(), maxparts);

        /// <summary>
        /// Parse a commandline 
        /// </summary>
        /// <param name="commandline">Command line to parse</param>
        /// <param name="maxparts">Maximum number of parts to parse. You will get the remaining as an extra parameter.</param>
        /// <returns>List of parsed components.</returns>
        List<string> ParseCommand(string commandline, int maxparts = 0)
        {
            List<string> parts = new List<string>();

            Debug("Parse: " + commandline);

            while (commandline.Length > 0)
            {
                if (maxparts > 0 && parts.Count == maxparts)
                {
                    parts.Add(commandline.Trim());
                    Debug("Adding tail");
                    return parts;
                }
                char sepChar;
                int until;
                if (commandline[0] == '"' || commandline[0] == '\'')
                {
                    sepChar = commandline[0];
                    commandline = commandline.Remove(0, 1);
                    until = commandline.IndexOf(sepChar);
                }
                else
                {
                    char[] seps = { ' ', ',' };
                    until = int.MaxValue;
                    foreach (char sep in seps)
                    {
                        int tryuntil = commandline.IndexOf(sep);
                        if (tryuntil > -1 && tryuntil < until)
                            until = tryuntil;
                    }
                    if (until == int.MaxValue)
                        until = -1;
                }
                if (until > 0)
                {
                    parts.Add(commandline.Substring(0, until));
                    commandline = commandline.Remove(0, until + 1).TrimStart();
                }
                else
                {
                    if (commandline.Trim().Length > 0)
                        parts.Add(commandline.Trim());
                    break;
                }
            }
            return parts;
        }

        IList<ktype> SortedKeys<ktype, vtype>(IDictionary<ktype, vtype> dictionary)
        {
            List<ktype> sortedKeys = dictionary.Keys.ToList();
            sortedKeys.Sort();
            return sortedKeys;
        }

        private bool addlearned = true;

        void ReadCommands(string commandstext)
        {
            bool showquotas = false;
            bool showcomponents = false;
            bool showrecipies = false;

            if (commandstext == null || commandstext.Length == 0)
                return;
            string[] commands = commandstext.Split('\n');
            for (int i = 0; i < commands.Length; i++)
            {
                Debug("Command line " + i);
                string commandline = commands[i].Trim();
                if (commandline.StartsWith("show "))
                    commandline = commandline.Replace("show ", "show");
                string[] command = commandline.Split(' ');
                //Debug("$" + commandline);
                if (command[0].StartsWith("//") || command[0].StartsWith("#")) // Comments
                    continue;
                switch (command[0].ToLower())
                {
                    case "":
                        // Ignore blank lines.
                        break;
                    case "addlearned":
                        addlearned = true;
                        break;
                    case "noaddlearned":
                        addlearned = false;
                        break;
                    case "debug":
                        EnableDebug = true;
                        break;
                    case "no":
                        switch (command[1].ToLower())
                        {
                            case "addlearned":
                                addlearned = false;
                                break;
                            case "debug":
                                EnableDebug = false;
                                break;
                            default:
                                Status("ERROR - Unknown command: " + commandline);
                                break;
                        }
                        break;
                    case "quota":
                        if (command[1] == "*")
                        {
                            List<string> quotakeys = new List<string>(quotas.Keys);
                            for (int qi = 0; qi < quotakeys.Count; qi++)
                            {
                                SetQuota(quotakeys[qi], command[2]);
                            }
                        }
                        else
                        {
                            SetQuota(command[1], command[2]);
                        }
                        break;
                    case "showquotas":
                        showquotas = true;
                        break;
                    case "alias":
                        List<string> aliasparts = ParseCommand(command[0], commandline);
                        if (aliasparts.Count != 2)
                        {
                            Status("Invalid alias line: " + commandline);
                            continue;
                        }
                        if (disp2real.ContainsKey(aliasparts[0]))
                            disp2real[aliasparts[0]] = aliasparts[1];
                        else
                            disp2real.Add(aliasparts[0], aliasparts[1]);
                        break;
                    case "component":
                    case "components":
                        List<string> parts = ParseCommand(command[0], commandline);
                        if (parts.Count != 2)
                        {
                            Status("Invalid component line: " + commandline);
                            continue;
                        }
                        RegisterComponent(parts, true);
                        break;
                    case "recipe":
                        List<string> recipeparts = ParseCommand(command[0], commandline);
                        if (recipeparts.Count < 2)
                        {
                            Status("Unable to parse: " + commandline);
                            continue;
                        }
                        RegisterRecipe(recipeparts, true);
                        break;
                    case "showcomponents":
                        showcomponents = true;
                        break;
                    case "showrecipes":
                    case "showrecipies":
                        showrecipies = true;
                        break;
                    case "import":
                        switch (command[1].ToLower())
                        {
                            case "lcd":
                            case "lcds":
                                string search = commandline.Substring(command[0].Length + command[1].Length + 2);
                                ImportLCD(search);
                                break;
                            default:
                                Status("I don't know how to import type '" + command[1] + "'");
                                break;
                        }
                        break;
                    case "begin":
                        switch (command[1].ToLower())
                        {
                            case "alias":
                                i++;
                                commandline = commands[i];
                                command = commandline.Split(' ');
                                while (command[0].ToLower() != "end")
                                {
                                    List<string> aliasparts2 = ParseCommand(commandline);
                                    if (aliasparts2.Count != 2)
                                    {
                                        Status("Invalid alias line: " + commandline);
                                        continue;
                                    }
                                    if (disp2real.ContainsKey(aliasparts2[0]))
                                        disp2real[aliasparts2[0]] = aliasparts2[1];
                                    else
                                        disp2real.Add(aliasparts2[0], aliasparts2[1]);
                                    i++;
                                    commandline = commands[i];
                                    command = commandline.Split(' ');
                                }
                                break;
                            case "quota":
                                i++;
                                commandline = commands[i];
                                command = commandline.Split(' ');
                                while (command[0].ToLower() != "end")
                                {
                                    SetQuota(command[0], command[1]);
                                    i++;
                                    commandline = commands[i];
                                    command = commandline.Split(' ');
                                }
                                break;
                            case "component":
                            case "components":
                                i++;
                                commandline = commands[i];
                                while (commandline.ToLower() != "end " + command[1].ToLower())
                                {
                                    List<string> parts2 = ParseCommand(commandline);
                                    if (parts2.Count != 2)
                                    {
                                        Status("Invalid component line: " + commandline);
                                        i++;
                                        commandline = commands[i];
                                        continue;
                                    }
                                    RegisterComponent(parts2, true);
                                    i++;
                                    commandline = commands[i];
                                }
                                break;
                            case "recipe":
                            case "recipes":
                            case "recipies":
                                i++;
                                commandline = commands[i];
                                while (commandline.ToLower() != "end " + command[1].ToLower())
                                {
                                    List<string> recipiesparts = ParseCommand(commandline);
                                    if (recipiesparts.Count < 2)
                                    {
                                        Status("Unable to parse: " + commandline);
                                        continue;
                                    }
                                    RegisterRecipe(recipiesparts, true);
                                }
                                break;
                            default:
                                Status("FATAL! Unknown 'begin' type: " + command[1]);
                                throw new Exception("Unknown 'begin' type: " + command[1]);
                        }
                        break;
                    case "tag":
                        Status("Setting tag to: " + commandline.Substring(4));
                        TAG = commandline.Substring(4);
                        break;
                    case "surface":
                        // Ignoring as this is managed elsewhere (but it is a valid line).
                        break;
                    default:
                        Status("Unknown command:" + command[0]);
                        break;
                }
            }
            if (showquotas)
            {
                Status("BEGIN QUOTA");
                foreach (string v in SortedKeys(quotas))
                {
                    Status(v + " " + quotas[v]);
                }
                Status("END QUOTA");
            }
            if (showcomponents)
            {
                Status("BEGIN COMPONENTS");
                foreach (string comp in SortedKeys(item2definition))
                {
                    Status("\"" + comp + "\" \"" + item2definition[comp] + "\"");
                }
                Status("END COMPONENTS");
            }
            if (showrecipies)
            {
                Status("BEGIN RECIPIES");
                foreach (string comp in SortedKeys(ingredients))
                {
                    IList<KeyValuePair<string, float>> bits = ingredients[comp];
                    List<string> ings = new List<string>();
                    foreach (KeyValuePair<string, float> ing in bits)
                    {
                        ings.Add("\"" + ing.Key + ":" + ing.Value.ToString() + "\""); // Currently this has to be in one set of quotes.
                    }
                    Status("\"" + comp + "\" " + string.Join(",", ings.GetEnumerator()));
                }
                Status("END RECIPIES");
            }
        }
        bool ParseConfigSurface(IMyTextSurfaceProvider surfaceProvider, string name_or_id)
        {
            string commandstext = ((IMyTerminalBlock)surfaceProvider).CustomData;
            if (commandstext == null || commandstext.Length == 0)
                return false;
            string[] commands = commandstext.Split('\n');
            foreach (string line in commands)
            {
                if (!line.ToLower().StartsWith("surface "))
                    continue;
                string rest;
                rest = line.Remove(0, 8).TrimStart();
                if (!rest.StartsWith(name_or_id + " "))
                    continue;
                rest = rest.Trim();
                bool ret;
                if (!bool.TryParse(rest, out ret))
                    return false;
                return ret;
            }
            return false;
        }

        List<KeyValuePair<string, int>> GetQueue(IMyAssembler assembler)
        {
            List<MyProductionItem> items = new List<MyProductionItem>();
            assembler.GetQueue(items);
            return items.ConvertAll(i => new KeyValuePair<string, int>(i.BlueprintId.ToString(), (int)i.Amount));
        }
        //void SetQueue(IMyAssembler assembler, List<KeyValuePair<string, int>> newQueue = null)
        //{
        //    assembler.SetValue<List<KeyValuePair<string, int>>>("Queue", newQueue);
        //}

        //string currentitem = "";

        string SimplifyComponent(string key)
        {
            string newkey = key;
            int pos = key.IndexOf('/');
            if (pos >= 0)
                newkey = key.Substring(pos + 1);
            string oldkey = "";
            while (oldkey != newkey)
            {
                oldkey = newkey;
                newkey = newkey.Replace("Component", "").Replace("component", "").Replace("Blueprint", "").Replace("blueprint", "");
                if (newkey.ToLower().StartsWith("bp"))
                    newkey = newkey.Substring(2);
                if (newkey.ToLower().EndsWith("bp"))
                    newkey = newkey.Substring(newkey.Length - 3);
            }
            return newkey;
        }
        string MakeComponent(string descr)
        {
            if (descr.Contains('/'))
                return descr;
            return default_component_maintype + "/" + descr;
        }

        void RegisterComponent(string name, string definition, bool config = false)
        {
            string lname = name.ToLower();
            if (item2definition.ContainsKey(lname))
                if (config)
                    item2definition.Remove(lname);
                else
                    return;
            Debug((config ? "Add" : "Learned") + " Component: " + name);
            item2definition.Add(lname, MakeComponent(definition));
            if (addlearned && !config)
            {
                if (Me.CustomData.Length > 0 && !Me.CustomData.EndsWith("\n"))
                    Me.CustomData += "\n";
                Me.CustomData += "component \"" + name + "\" \"" + definition + "\"\n";
            }
        }
        void RegisterComponent(List<string> parts, bool config = false)
        {
            if (parts.Count != 2)
                throw new Exception("2 parts are required");
            RegisterComponent(parts[0], parts[1], config);
        }

        void RegisterRecipe(string name, List<KeyValuePair<string, float>> definition, bool config = false)
        {
            if (ingredients.ContainsKey(name))
                if (config)
                    ingredients.Remove(name);
                else
                    return;
            Debug((config ? "Add" : "Learned") + " Recipe: " + name);
            ingredients.Add(name, definition);
            if (addlearned && !config)
            {
                if (Me.CustomData.Length > 0 && !Me.CustomData.EndsWith("\n"))
                    Me.CustomData += "\n";
                IList<string> ings = definition.ConvertAll(d => d.Key + ":" + d.Value);
                Me.CustomData += "recipe \"" + name + "\" " + Utility.List2String(ings) + "\n";
            }
        }

        void RegisterRecipe(List<string> parts, bool config = false) => RegisterRecipe(parts[0], parts.GetRange(1, parts.Count - 1), config);
        void RegisterRecipe(string item, List<string> parts, bool config = false)
        {
            if (item2definition.ContainsKey(item.ToLower()))
                item = item2definition[item.ToLower()];
            else
            {
                Status("Recipe for unknown component: " + item);
                return;
            }
            //Debug("Recipe: " + item);
            List<KeyValuePair<string, float>> ingots = new List<KeyValuePair<string, float>>();
            //Debug(parts.Count + " ingredients");
            foreach (string ingredient in parts)
            {
                string iq = ingredient.Trim();
                int pos = iq.IndexOf('/');
                if (pos >= 0)
                    iq = iq.Substring(pos + 1);
                string[] iqs = iq.Split(new char[] { ':', '=' }, 2);
                if (iqs.Length != 2)
                {
                    Status("No ingredient quantity for " + ingredient + " in " + item);
                    continue;
                }
                string ingot = iqs[0];
                float qty = float.Parse(iqs[1]);
                //Debug("Adding " + ingot + " = " + qty);
                ingots.Add(new KeyValuePair<string, float>(ingot, qty));
            }
            RegisterRecipe(item, ingots, config);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblers"></param>
        /// <exception cref="Exception">This will throw an exception if PB extended API is missing or not working</exception>
        void ComponentDiscoveryPB(IList<IMyAssembler> assemblers)
        {
            Debug("Get reqs (PB)");
            IDictionary<string, IList<KeyValuePair<string, float>>> Itemreqs = assemblers[0].GetValue<IDictionary<string, IList<KeyValuePair<string, float>>>>("AssemblableItems");
            Debug("Extract component reqs");
            List<string> Itemreqkeys = new List<string>(Itemreqs.Keys);
            Itemreqkeys.Sort();
            foreach (string key in Itemreqkeys)
            {
                if (key.StartsWith("Component/") || key.StartsWith(default_component_maintype + "/"))
                {
                    string blueprint = default_component_maintype + "/" + key.Substring(key.IndexOf('/'));
                    string newkey = SimplifyComponent(key);
                    if (item2definition.ContainsKey(newkey) && ingredients.ContainsKey(blueprint))
                        continue;
                    Debug("+" + newkey);
                    List<KeyValuePair<string, float>> newreqs = new List<KeyValuePair<string, float>>();
                    for (int ri = 0; ri < Itemreqs[key].Count; ri++)
                    {
                        KeyValuePair<string, float> kv = Itemreqs[key][ri];
                        string newrkey = kv.Key.Replace("MyObjectBuilder_Ingot/", "");
                        newreqs.Add(new KeyValuePair<string, float>(newrkey, kv.Value));
                    }
                    RegisterComponent(newkey, key);
                    RegisterRecipe(blueprint, newreqs);
                }
            }
        }

        void ComponentDiscovery()
        {
            Debug("Detecting active blueprints");

            // I want to scan as many assemblers as I can, even ones I don't control!
            List<IMyAssembler> assemblers = GetBlocks.ByType<IMyAssembler>(false);
            foreach (IMyAssembler ass in assemblers)
            {
                List<MyProductionItem> items = new List<MyProductionItem>();
                ass.GetQueue(items);
                foreach (MyProductionItem item in items)
                {
                    string name = SimplifyComponent(item.BlueprintId.SubtypeName);
                    Debug("Detected: " + name);
                    string definition = item.BlueprintId.TypeId + "/" + item.BlueprintId.SubtypeId;
                    if (!item2definition.ContainsKey(name.ToLower()) && !item2definition.ContainsValue(definition))
                    {
                        RegisterComponent(name, definition);
                    }
                }
            }
        }

        void Main(string arg)
        {
            GetBlocks.ClearCache();
            DebugInit();
            StatusInit();
            Status(DateTime.Now.ToShortTimeString());
            ReadCommandsPB();
            FindStatusLCD();
            ReadCommandsLCD();

            Debug("Find assemblers");
            List<IMyAssembler> assemblers = GetBlocks.ByName<IMyAssembler>(TAG, false);

            Debug("Detecting");

            try
            {
                Debug("Discovery PB");
                ComponentDiscoveryPB(assemblers);
            }
            catch (Exception)
            {
                Debug("PB extensions not available");
                // Do nothing, I just don't want to abort.
            }
            Debug("General discovery");
            ComponentDiscovery();

            Debug("Running");

            Dictionary<string, double> stock = new Dictionary<string, double>();
            Dictionary<string, double> istock = new Dictionary<string, double>();

            Debug("Get all blocks");
            IList<IMyTerminalBlock> allblocks = GetBlocks.Everything;

            Debug("Iterate all blocks");
            foreach (IMyTerminalBlock block in allblocks)
            {
                int icount = block.InventoryCount;
                for (int i = 0; i < icount; i++)
                {
                    IMyInventory inv = block.GetInventory(i);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    inv.GetItems(items);
                    foreach (MyInventoryItem thing in items)
                    {
                        string mainType = thing.Type.TypeId.ToString();
                        mainType = mainType.Substring(mainType.LastIndexOf('_') + 1);
                        if (mainType != "Component" && mainType != "Ingot")
                            continue;
                        string subType = thing.Type.SubtypeId.ToString();
                        double amount = (double)thing.Amount;
                        //Debug("(" + subType + ")");
                        if (mainType == "Component")
                        {
                            //if (!block.CustomName.Contains("[STOCK]"))
                            //    continue;
                            if (stock.ContainsKey(subType))
                            {
                                stock[subType] += amount;
                            }
                            else
                            {
                                stock.Add(subType, amount);
                            }
                        }
                        else
                        {
                            if (istock.ContainsKey(subType))
                            {
                                istock[subType] += amount;
                            }
                            else
                            {
                                istock.Add(subType, amount);
                            }
                        }
                    }
                }
            }

            Debug(Utility.List2String(new List<string>(stock.Keys)));
            Debug(Utility.List2String(new List<string>(istock.Keys)));

            if (assemblers.Count < 1)
            {
                Status("No " + TAG + " assemblers found");
                return;
            }

            Debug("Assemblers:");
            assemblers.ForEach(a => Debug(" " + a.CustomName));

            Debug("Process quotas");
            List<string> quotaKeys = new List<string>(quotas.Keys);
            Dictionary<string, double> level = new Dictionary<string, double>();
            Dictionary<string, int> need = new Dictionary<string, int>();
            foreach (string qKey in quotaKeys)
            {
                int qVal = quotas[qKey];
                if (qVal < 1)
                    continue;
                double sVal;
                if (stock.ContainsKey(qKey))
                {
                    sVal = stock[qKey];
                }
                else
                {
                    sVal = 0;
                }
                double fraction = sVal / qVal;
                if (fraction >= 1.0)
                    continue;
                string component = FindComponent(qKey);
                if (!item2definition.ContainsKey(component))
                {
                    Status("Error: I do not know how to create " + qKey + ", please add a 'component " + component + " ...' line in CustomData");
                    continue;
                }
                string blueprint = item2definition[component];
                level.Add(blueprint, fraction);
                need.Add(blueprint, qVal - (int)Math.Floor(sVal));
                if (ingredients.ContainsKey(blueprint))
                {
                    Debug("Checking ingredients for " + qKey);
                    IList<KeyValuePair<string, float>> itemreqs = ingredients[blueprint];
                    Debug(itemreqs.Count + " ingredients");
                    bool skipitem = false;
                    for (int r = 0; r < itemreqs.Count; r++)
                    {
                        KeyValuePair<string, float> kv = itemreqs[r];
                        string key = kv.Key;
                        float val = kv.Value;
                        if (!istock.ContainsKey(key))
                        {
                            Status("Missing " + key + " for " + qKey);
                            skipitem = true;
                            continue;
                        }
                        if (istock[key] < val)
                        {
                            Status("Not enough " + key + " for " + qKey);
                            skipitem = true;
                            continue;
                        }
                        Debug("I have enough " + key + " for " + qKey);
                    }
                    if (skipitem)
                    {
                        Debug("Not enough resources to make " + qKey);
                        level.Remove(blueprint);
                        need.Remove(blueprint);
                        continue;
                    }
                }
                else if (ingredients.Any()) // Only emit this warning if there are some ingredients known.
                {
                    Status("Warning: I do not know what materials " + qKey + " requires (continuing without checking)");
                }
            }
            Debug("Sort");
            //List<KeyValuePair<string,double>> sortedlevel = level.ToList();
            //sortedlevel.Sort((a, b) => a.Value.CompareTo(b.Value));
            List<KeyValuePair<string, int>> sortedneed = need.ToList();
            sortedneed.Sort((a, b) => level[a.Key].CompareTo(level[b.Key]));

            Debug("Stock check");
            List<string> stockKeys = new List<string>(stock.Keys);
            foreach (string stockItem in stockKeys)
            {
                if (quotas.ContainsKey(stockItem))
                    continue;
                Status("NO QUOTA FOR: " + stockItem);
            }

            if (!level.Any())
            {
                Status("Nothing to do");
                return;
            }

            Debug("Needs:");
            foreach (KeyValuePair<string, int> nv in need)
            {
                // Can't do this as a need.ForEach(...) for some reason.
                Debug(" " + SimplifyComponent(nv.Key) + " = " + nv.Value.ToString());
            }

            // I used to check the single builditem for ingredients here but I think that is now
            // obsolete.

            Debug("Schedule assemblers");
            foreach (IMyAssembler ass in assemblers)
            {
                Status("[" + ass.CustomName + "]");
                List<KeyValuePair<string, int>> assqueue = GetQueue(ass);
                ISet<string> inQueue = new HashSet<string>();
                for (int q = 0; q < assqueue.Count; q++)
                {
                    KeyValuePair<string, int> item = assqueue[q];
                    inQueue.Add(item.Key);
                    if (!need.ContainsKey(item.Key))
                    {
                        Status(string.Format("-{0}={1}", SimplifyComponent(item.Key), item.Value));
                        ass.RemoveQueueItem(q, (double)item.Value);
                    }
                }

                foreach (KeyValuePair<string, int> nv in sortedneed)
                {
                    if (inQueue.Contains(nv.Key))
                        continue;
                    MyDefinitionId def = ItemToDefinitionId(nv.Key);
                    double dBuildCount = nv.Value;
                    //dBuildCount /= assemblers.Count;
                    dBuildCount *= 0.1;
                    Status(string.Format("+{0}={1}", SimplifyComponent(nv.Key), nv.Value));
                    ass.AddQueueItem(def, Math.Ceiling(dBuildCount));
                    inQueue.Add(nv.Key);
                }

                Status(inQueue.Count + " jobs in queue");
            }
            Debug("Complete");
        }
    }
}