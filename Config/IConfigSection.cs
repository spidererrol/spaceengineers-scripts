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
        public class ConfigSectionKey
        {
            protected readonly IConfigSection parent;
            protected readonly string key;

            public IConfigSection Section => parent;
            public string Key => key;

            public ConfigSectionKey(IConfigSection configSection, string keyname)
            {
                parent = configSection;
                key = keyname;
            }

            public ConfigSectionKey Comment(string comment)
            {
                parent.SetComment(key, comment);
                return this;
            }
            public string Comment() => parent.GetComment(key);
            public ConfigSectionKey SetComment(string comment) => Comment(comment);
            public string GetComment() => Comment();

            public string Get(string defvalue) => parent.Get(key, defvalue);
            public int Get(int defvalue) => parent.Get(key, defvalue);
            public float Get(float defvalue) => parent.Get(key, defvalue);
            public bool Get(bool defvalue) => parent.Get(key, defvalue);

            public ConfigSectionKey Get(ref string value)
            {
                parent.Get(key, ref value);
                return this;
            }
            public ConfigSectionKey Get(ref float value)
            {
                parent.Get(key, ref value);
                return this;
            }
            public ConfigSectionKey Get(ref int value)
            {
                parent.Get(key, ref value);
                return this;
            }
            public ConfigSectionKey Get(ref bool value)
            {
                parent.Get(key, ref value);
                return this;
            }

            public MyIniValue Get() => parent.Get(key);

            public ConfigSectionKey Set(string value)
            {
                parent.Set(key, value);
                return this;
            }
            public ConfigSectionKey Set(bool value)
            {
                parent.Set(key, value);
                return this;
            }
            public ConfigSectionKey Set(int value)
            {
                parent.Set(key, value);
                return this;
            }
            public ConfigSectionKey Set(float value)
            {
                parent.Set(key, value);
                return this;
            }

            public ConfigSectionKey Save()
            {
                parent.Save();
                return this;
            }
            public ConfigSectionKey Save(IMyTerminalBlock block)
            {
                parent.Save(block);
                return this;
            }
        }

        public interface IConfigSection
        {
            bool IsReadOnly();
            bool ContainsKey(string key);
            void Default(string key, bool value);
            void Default(string key, float value);
            void Default(string key, int value);
            void Default(string key, string value);
            void Delete(string key);
            MyIniValue Get(string key);
            bool Get(string key, bool defaultvalue);
            float Get(string key, float defaultvalue);
            int Get(string key, int defaultvalue);
            void Get(string key, ref bool value);
            void Get(string key, ref float value);
            void Get(string key, ref int value);
            void Get(string key, ref string value);
            string Get(string key, string defaultvalue);
            bool GetBool(string key);
            string GetComment(string key);
            float GetFloat(string key);
            int GetInt(string key);
            List<string> GetKeys();
            string GetString(string key);
            ConfigSectionKey Key(string key);
            void Save();
            void Save(IMyTerminalBlock block);
            void Set(string key, bool value);
            void Set(string key, float value);
            void Set(string key, int value);
            void Set(string key, string value);
            void SetComment(string key, string comment);
        }

        public class DictConfigSection : IConfigSection
        {
            protected readonly IDictionary<string, string> dict;

            public DictConfigSection(IDictionary<string, string> config)
            {
                dict = config;
            }
            public DictConfigSection(string[,] config)
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                for (int x = config.GetLowerBound(0); x < config.GetUpperBound(0); x++)
                {
                    d.Add(config[x, 0], config[x, 1]);
                }
                dict = d;
            }

            public bool IsReadOnly() => true;
            public bool ContainsKey(string key) => dict.ContainsKey(key);
            public void Default(string key, bool value) => Get(key, value);
            public void Default(string key, float value) => Get(key, value);
            public void Default(string key, int value) => Get(key, value);
            public void Default(string key, string value) => Get(key, value);
            public void Delete(string key)
            {
                throw new Exception("Attempt to delete a key from a read-only section");
            }

            public MyIniValue Get(string key)
            {
                MyIniKey myIniKey = new MyIniKey("(readonly)", key);
                return new MyIniValue(myIniKey, GetString(key));
            }
            public bool Get(string key, bool defaultvalue) => bool.Parse(Get(key, defaultvalue.ToString()));
            public float Get(string key, float defaultvalue) => float.Parse(Get(key, defaultvalue.ToString()));
            public int Get(string key, int defaultvalue) => int.Parse(Get(key, defaultvalue.ToString()));
            public void Get(string key, ref bool value) => value = Get(key, value);
            public void Get(string key, ref float value) => value = Get(key, value);
            public void Get(string key, ref int value) => value = Get(key, value);
            public void Get(string key, ref string value) => value = Get(key, value);
            public string Get(string key, string defaultvalue)
            {
                if (ContainsKey(key))
                    return key;
                else
                    return defaultvalue;
            }
            public bool GetBool(string key) => bool.Parse(GetString(key));
            public string GetComment(string key) => null;
            public float GetFloat(string key) => float.Parse(GetString(key));
            public int GetInt(string key) => int.Parse(GetString(key));
            public List<string> GetKeys() => new List<string>(dict.Keys);
            public string GetString(string key) => dict[key];
            public ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);
            public void Save() { }
            public void Save(IMyTerminalBlock block) { }
            public void Set(string key, bool value)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public void Set(string key, float value)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public void Set(string key, int value)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public void Set(string key, string value)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public void SetComment(string key, string comment)
            {
                throw new Exception("Attempt to set read-only config");
            }

        }

        public class LayeredConfigSection : IConfigSection
        {
            public class AccessConfigSection
            {
                public IConfigSection configSection;
                public bool writable;

                public AccessConfigSection(IConfigSection cs, bool write)
                {
                    configSection = cs;
                    writable = write;
                }
            }

            protected readonly List<AccessConfigSection> accessConfigSections;
            protected List<IConfigSection> configSections => accessConfigSections.ConvertAll(a => a.configSection);

            public bool IsReadOnly() => !accessConfigSections.Any(b => b.writable);

            public LayeredConfigSection()
            {
                accessConfigSections = new List<AccessConfigSection>();
            }
            public LayeredConfigSection Add(IConfigSection configSection, bool writable)
            {
                AccessConfigSection acs = new AccessConfigSection(configSection, writable);
                accessConfigSections.Insert(0, acs);
                return this;
            }
            public LayeredConfigSection Add(IConfigSection configSection) => Add(configSection, !configSection.IsReadOnly());
            public LayeredConfigSection Add(IDictionary<string, string> dict) => Add(new DictConfigSection(dict), false);
            public LayeredConfigSection Add(string[,] array) => Add(new DictConfigSection(array), false);

            protected IConfigSection FirstWritable() => accessConfigSections.FindAll(a => a.writable).First().configSection;
            protected IConfigSection FirstWithKey(string key) => accessConfigSections.FindAll(a => a.configSection.ContainsKey(key)).First().configSection;

            public bool ContainsKey(string key)
            {
                foreach (IConfigSection configSection in configSections)
                {
                    if (configSection.ContainsKey(key))
                        return true;
                }
                return false;
            }

            public void Default(string key, bool value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public void Default(string key, float value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public void Default(string key, int value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public void Default(string key, string value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }

            public void Delete(string key)
            {
                if (accessConfigSections.Any(a => !a.writable && a.configSection.ContainsKey(key)))
                    throw new Exception("Unable to remove key from read-only layers");
                accessConfigSections.FindAll(a => a.writable && a.configSection.ContainsKey(key)).ForEach(a => a.configSection.Delete(key));
            }

            public MyIniValue Get(string key) => FirstWithKey(key).Get(key);
            public bool Get(string key, bool defaultvalue)
            {
                Default(key, defaultvalue);
                return FirstWithKey(key).GetBool(key);
            }
            public float Get(string key, float defaultvalue)
            {
                Default(key, defaultvalue);
                return FirstWithKey(key).GetFloat(key);
            }
            public int Get(string key, int defaultvalue)
            {
                Default(key, defaultvalue);
                return FirstWithKey(key).GetInt(key);
            }
            public string Get(string key, string defaultvalue)
            {
                Default(key, defaultvalue);
                return FirstWithKey(key).GetString(key);
            }

            public void Get(string key, ref bool value) => value = Get(key, value);
            public void Get(string key, ref float value) => value = Get(key, value);
            public void Get(string key, ref int value) => value = Get(key, value);
            public void Get(string key, ref string value) => value = Get(key, value);

            public bool GetBool(string key) => FirstWithKey(key).GetBool(key);
            public string GetComment(string key) => FirstWithKey(key).GetComment(key);
            public float GetFloat(string key) => FirstWithKey(key).GetFloat(key);
            public int GetInt(string key) => FirstWithKey(key).GetInt(key);
            public string GetString(string key) => FirstWithKey(key).GetString(key);

            public List<string> GetKeys()
            {
                ISet<string> results = new HashSet<string>();
                foreach (IConfigSection configSection in configSections)
                {
                    foreach (string key in configSection.GetKeys())
                        results.Add(key);
                }
                return results.ToList();
            }

            public ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);

            public void Save() => accessConfigSections.FindAll(a => a.writable).ForEach(a => a.configSection.Save());
            public void Save(IMyTerminalBlock block) => accessConfigSections.FindAll(a => a.writable).ForEach(a => a.configSection.Save(block));

            public void Set(string key, bool value) => FirstWritable().Set(key, value);
            public void Set(string key, float value) => FirstWritable().Set(key, value);
            public void Set(string key, int value) => FirstWritable().Set(key, value);
            public void Set(string key, string value) => FirstWritable().Set(key, value);

            public void SetComment(string key, string comment) => accessConfigSections.FindAll(a => a.writable && a.configSection.ContainsKey(key)).ForEach(a => a.configSection.SetComment(key, comment));
        }

        public abstract class BaseConfigSection : IConfigSection
        {
            protected IConfigSection section;

            public BaseConfigSection(IConfigSection configSection)
            {
                section = configSection;
                ApplyDefaults();
            }
            public BaseConfigSection(IMyProgrammableBlock Me, string SectionName) : this(Config.Section(Me, SectionName)) { }

            public abstract void ApplyDefaults();

            // IConfigSection interface:
            public bool ContainsKey(string key) => section.ContainsKey(key);
            public void Default(string key, bool value) => section.Default(key, value);
            public void Default(string key, float value) => section.Default(key, value);
            public void Default(string key, int value) => section.Default(key, value);
            public void Default(string key, string value) => section.Default(key, value);
            public void Delete(string key) => section.Delete(key);
            public MyIniValue Get(string key) => section.Get(key);
            public bool Get(string key, bool defaultvalue) => section.Get(key, defaultvalue);
            public float Get(string key, float defaultvalue) => section.Get(key, defaultvalue);
            public int Get(string key, int defaultvalue) => section.Get(key, defaultvalue);
            public void Get(string key, ref bool value) => section.Get(key, ref value);
            public void Get(string key, ref float value) => section.Get(key, ref value);
            public void Get(string key, ref int value) => section.Get(key, ref value);
            public void Get(string key, ref string value) => section.Get(key, ref value);
            public string Get(string key, string defaultvalue) => section.Get(key, defaultvalue);
            public bool GetBool(string key) => section.GetBool(key);
            public string GetComment(string key) => section.GetComment(key);
            public float GetFloat(string key) => section.GetFloat(key);
            public int GetInt(string key) => section.GetInt(key);
            public List<string> GetKeys() => section.GetKeys();
            public string GetString(string key) => section.GetString(key);
            public bool IsReadOnly() => section.IsReadOnly();
            public ConfigSectionKey Key(string key) => section.Key(key);
            public void Save() => section.Save();
            public void Save(IMyTerminalBlock block) => section.Save(block);
            public void Set(string key, bool value) => section.Set(key, value);
            public void Set(string key, float value) => section.Set(key, value);
            public void Set(string key, int value) => section.Set(key, value);
            public void Set(string key, string value) => section.Set(key, value);
            public void SetComment(string key, string comment) => section.SetComment(key, comment);
        }
    }
}