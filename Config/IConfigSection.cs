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
            void Default(string key, long value);
            void Default(string key, Color value);
            void Default(string key, string value);
            void Delete(string key);
            MyIniValue Get(string key);
            bool Get(string key, bool defaultvalue);
            float Get(string key, float defaultvalue);
            int Get(string key, int defaultvalue);
            long Get(string key, long defaultvalue);
            Color Get(string key, Color defaultvalue);
            void Get(string key, ref bool value);
            void Get(string key, ref float value);
            void Get(string key, ref int value);
            void Get(string key, ref long value);
            void Get(string key, ref Color value);
            void Get(string key, ref string value);
            string Get(string key, string defaultvalue);
            bool GetBool(string key);
            string GetComment(string key);
            float GetFloat(string key);
            int GetInt(string key);
            long GetInt64(string key);
            long GetLong(string key);
            Color GetColor(string key);
            List<string> GetKeys();
            string GetString(string key);
            ConfigSectionKey Key(string key);
            void Save();
            void Save(IMyTerminalBlock block);
            void Set(string key, bool value);
            void Set(string key, float value);
            void Set(string key, int value);
            void Set(string key, string value);
            void Set(string key, long value);
            void Set(string key, Color value);
            void SetComment(string key, string comment);
        }

        public abstract class AConfigSection : IConfigSection
        {
            public abstract bool ContainsKey(string key);
            public abstract void Delete(string key);
            public abstract MyIniValue Get(string key);
            public abstract List<string> GetKeys();
            public abstract void Save(IMyTerminalBlock block);
            public abstract void Save();
            public abstract void Set(string key, string value);
            public abstract void SetComment(string key, string comment);
            public abstract string GetComment(string key);

            public abstract bool IsReadOnly();
            public abstract bool Get(string key, bool defaultvalue);
            public abstract float Get(string key, float defaultvalue);
            public abstract int Get(string key, int defaultvalue);
            public abstract string Get(string key, string defaultvalue);
            public abstract long Get(string key, long defaultvalue);
            public abstract Color Get(string key, Color defaultvalue);

            public virtual ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);
            public virtual void Default(string key, string value)
            {
                if (!ContainsKey(key))
                    Set(key, value);
            }
            public virtual void Default(string key, bool value) => Default(key, value.ToString());
            public virtual void Default(string key, float value) => Default(key, value.ToString());
            public virtual void Default(string key, int value) => Default(key, value.ToString());
            public virtual void Default(string key, long value) => Default(key, value.ToString());
            public virtual void Get(string key, ref bool value) => value = Get(key, value);
            public virtual void Get(string key, ref float value) => value = Get(key, value);
            public virtual void Get(string key, ref int value) => value = Get(key, value);
            public virtual void Get(string key, ref string value) => value = Get(key, value);
            public virtual void Get(string key, ref long value) => value = Get(key, value);
            public virtual void Get(string key, ref Color value) => value = Get(key, value);
            public virtual bool GetBool(string key) => Get(key).ToBoolean();
            public virtual float GetFloat(string key) => Get(key).ToSingle();
            public virtual int GetInt(string key) => Get(key).ToInt32();
            public virtual long GetInt64(string key) => Get(key).ToInt64();
            public virtual long GetLong(string key) => Get(key).ToInt64();
            public virtual Color GetColor(string key) {
                // Need to parse using WebColor.
                throw new Exception("Not Implemented");
            }
            public virtual string GetString(string key) => Get(key).ToString();
            public virtual void Set(string key, bool value) => Set(key, value.ToString());
            public virtual void Set(string key, float value) => Set(key, value.ToString());
            public virtual void Set(string key, int value) => Set(key, value.ToString());
            public virtual void Set(string key, long value) => Set(key, value.ToString());

            public void Default(string key, Color value) {
                // Need to parse using WebColor.
                throw new Exception("Not Implemented");
            }

            public void Set(string key, Color value) {
                // Need to parse using WebColor.
                throw new Exception("Not Implemented");
            }
        }

        public abstract class RWConfigSection : AConfigSection
        {
            public override bool IsReadOnly() => false;

            public override bool Get(string key, bool defaultvalue)
            {
                Default(key, defaultvalue);
                return GetBool(key);
            }
            public override float Get(string key, float defaultvalue)
            {
                Default(key, defaultvalue);
                return GetFloat(key);
            }
            public override int Get(string key, int defaultvalue)
            {
                Default(key, defaultvalue);
                return GetInt(key);
            }
            public override long Get(string key, long defaultvalue) {
                Default(key, defaultvalue);
                return GetLong(key);
            }
            public override Color Get(string key, Color defaultvalue) {
                Default(key, defaultvalue);
                return GetColor(key);
            }
            public override string Get(string key, string defaultvalue)
            {
                Default(key, defaultvalue);
                return GetString(key);
            }
        }

        public abstract class ROConfigSection : AConfigSection
        {
            public override bool IsReadOnly() => true;

            public override bool Get(string key, bool defaultvalue)
            {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetBool(key);
            }
            public override float Get(string key, float defaultvalue)
            {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetFloat(key);
            }
            public override int Get(string key, int defaultvalue)
            {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetInt(key);
            }
            public override string Get(string key, string defaultvalue)
            {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetString(key);
            }
            public override long Get(string key, long defaultvalue) {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetLong(key);
            }
            public override Color Get(string key, Color defaultvalue) {
                if (!ContainsKey(key))
                    return defaultvalue;
                return GetColor(key);
            }

            public override void Delete(string key)
            {
                throw new Exception("Attempt to delete a key from a read-only section");
            }
            public override void Set(string key, string value)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public override void SetComment(string key, string comment)
            {
                throw new Exception("Attempt to set read-only config");
            }
            public override void Save(IMyTerminalBlock block) { }
            public override void Save() { }

        }

        public class DictConfigSection : ROConfigSection
        {
            protected readonly IDictionary<string, string> dict;

            public DictConfigSection()
            {
                dict = new Dictionary<string, string>();
            }
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

            public override bool ContainsKey(string key) => dict.ContainsKey(key);

            public override MyIniValue Get(string key)
            {
                MyIniKey myIniKey = new MyIniKey("(readonly)", key);
                return new MyIniValue(myIniKey, GetString(key));
            }

            public override string GetComment(string key) => null;

            public override List<string> GetKeys() => new List<string>(dict.Keys);

            public override ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);
        }

        public class LayeredConfigSection : RWConfigSection
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

            public override bool IsReadOnly() => !accessConfigSections.Any(b => b.writable);

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

            public override bool ContainsKey(string key)
            {
                foreach (IConfigSection configSection in configSections)
                {
                    if (configSection.ContainsKey(key))
                        return true;
                }
                return false;
            }

            public override void Default(string key, bool value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public override void Default(string key, float value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public override void Default(string key, int value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }
            public override void Default(string key, string value)
            {
                if (!ContainsKey(key))
                    FirstWritable().Default(key, value);
            }

            public override void Delete(string key)
            {
                if (accessConfigSections.Any(a => !a.writable && a.configSection.ContainsKey(key)))
                    throw new Exception("Unable to remove key from read-only layers");
                accessConfigSections.FindAll(a => a.writable && a.configSection.ContainsKey(key)).ForEach(a => a.configSection.Delete(key));
            }

            public override MyIniValue Get(string key) => FirstWithKey(key).Get(key);

            public override bool GetBool(string key) => FirstWithKey(key).GetBool(key);
            public override string GetComment(string key) => FirstWithKey(key).GetComment(key);
            public override float GetFloat(string key) => FirstWithKey(key).GetFloat(key);
            public override int GetInt(string key) => FirstWithKey(key).GetInt(key);
            public override string GetString(string key) => FirstWithKey(key).GetString(key);

            public override List<string> GetKeys()
            {
                ISet<string> results = new HashSet<string>();
                foreach (IConfigSection configSection in configSections)
                {
                    foreach (string key in configSection.GetKeys())
                        results.Add(key);
                }
                return results.ToList();
            }

            public override void Save() => accessConfigSections.FindAll(a => a.writable).ForEach(a => a.configSection.Save());
            public override void Save(IMyTerminalBlock block) => accessConfigSections.FindAll(a => a.writable).ForEach(a => a.configSection.Save(block));

            public override void Set(string key, bool value) => FirstWritable().Set(key, value);
            public override void Set(string key, float value) => FirstWritable().Set(key, value);
            public override void Set(string key, int value) => FirstWritable().Set(key, value);
            public override void Set(string key, string value) => FirstWritable().Set(key, value);

            public override void SetComment(string key, string comment) => accessConfigSections.FindAll(a => a.writable && a.configSection.ContainsKey(key)).ForEach(a => a.configSection.SetComment(key, comment));
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
            public virtual bool ContainsKey(string key) => section.ContainsKey(key);
            public virtual void Default(string key, bool value) => section.Default(key, value);
            public virtual void Default(string key, float value) => section.Default(key, value);
            public virtual void Default(string key, int value) => section.Default(key, value);
            public virtual void Default(string key, string value) => section.Default(key, value);
            public virtual void Default(string key, long value) => section.Default(key, value);
            public virtual void Default(string key, Color value) => section.Default(key, value);
            public virtual void Delete(string key) => section.Delete(key);
            public virtual MyIniValue Get(string key) => section.Get(key);
            public virtual bool Get(string key, bool defaultvalue) => section.Get(key, defaultvalue);
            public virtual float Get(string key, float defaultvalue) => section.Get(key, defaultvalue);
            public virtual int Get(string key, int defaultvalue) => section.Get(key, defaultvalue);
            public virtual void Get(string key, ref bool value) => section.Get(key, ref value);
            public virtual void Get(string key, ref float value) => section.Get(key, ref value);
            public virtual void Get(string key, ref int value) => section.Get(key, ref value);
            public virtual void Get(string key, ref string value) => section.Get(key, ref value);
            public virtual string Get(string key, string defaultvalue) => section.Get(key, defaultvalue);
            public virtual long Get(string key, long defaultvalue) => section.Get(key, defaultvalue);
            public virtual Color Get(string key, Color defaultvalue) => section.Get(key, defaultvalue);
            public virtual void Get(string key, ref long value) => section.Get(key, ref value);
            public virtual void Get(string key, ref Color value) => section.Get(key, ref value);
            public virtual bool GetBool(string key) => section.GetBool(key);
            public virtual Color GetColor(string key) => section.GetColor(key);
            public virtual string GetComment(string key) => section.GetComment(key);
            public virtual float GetFloat(string key) => section.GetFloat(key);
            public virtual int GetInt(string key) => section.GetInt(key);
            public virtual long GetInt64(string key) => section.GetInt64(key);
            public virtual List<string> GetKeys() => section.GetKeys();
            public virtual long GetLong(string key) => section.GetLong(key);
            public virtual string GetString(string key) => section.GetString(key);
            public virtual bool IsReadOnly() => section.IsReadOnly();
            public virtual ConfigSectionKey Key(string key) => section.Key(key);
            public virtual void Save() => section.Save();
            public virtual void Save(IMyTerminalBlock block) => section.Save(block);
            public virtual void Set(string key, bool value) => section.Set(key, value);
            public virtual void Set(string key, float value) => section.Set(key, value);
            public virtual void Set(string key, int value) => section.Set(key, value);
            public virtual void Set(string key, string value) => section.Set(key, value);
            public virtual void Set(string key, long value) => section.Set(key, value);
            public virtual void Set(string key, Color value) => section.Set(key, value);
            public virtual void SetComment(string key, string comment) => section.SetComment(key, comment);
        }

        public abstract class KeyPrefixConfigSection : BaseConfigSection
        {
            private readonly string prefix;
            public KeyPrefixConfigSection(string prefix, IConfigSection configSection) : base(configSection) { this.prefix = prefix; }
            public KeyPrefixConfigSection(string prefix, IMyProgrammableBlock Me, string SectionName) : base(Me, SectionName) { this.prefix = prefix; }

            // All of these just call the base with the prefix prepended to the key:
            public override bool ContainsKey(string key) => base.ContainsKey(prefix + key);
            public override void Default(string key, bool value) => base.Default(prefix + key, value);
            public override void Default(string key, float value) => base.Default(prefix + key, value);
            public override void Default(string key, int value) => base.Default(prefix + key, value);
            public override void Default(string key, string value) => base.Default(prefix + key, value);
            public override void Delete(string key) => base.Delete(prefix + key);
            public override MyIniValue Get(string key) => base.Get(prefix + key);
            public override bool Get(string key, bool defaultvalue) => base.Get(prefix + key, defaultvalue);
            public override float Get(string key, float defaultvalue) => base.Get(prefix + key, defaultvalue);
            public override int Get(string key, int defaultvalue) => base.Get(prefix + key, defaultvalue);
            public override void Get(string key, ref bool value) => base.Get(prefix + key, ref value);
            public override void Get(string key, ref float value) => base.Get(prefix + key, ref value);
            public override void Get(string key, ref int value) => base.Get(prefix + key, ref value);
            public override void Get(string key, ref string value) => base.Get(prefix + key, ref value);
            public override string Get(string key, string defaultvalue) => base.Get(prefix + key, defaultvalue);
            public override bool GetBool(string key) => base.GetBool(prefix + key);
            public override string GetComment(string key) => base.GetComment(prefix + key);
            public override float GetFloat(string key) => base.GetFloat(prefix + key);
            public override int GetInt(string key) => base.GetInt(prefix + key);
            public override string GetString(string key) => base.GetString(prefix + key);
            public override ConfigSectionKey Key(string key) => base.Key(key);
            public override void Set(string key, bool value) => base.Set(prefix + key, value);
            public override void Set(string key, float value) => base.Set(prefix + key, value);
            public override void Set(string key, int value) => base.Set(prefix + key, value);
            public override void Set(string key, string value) => base.Set(prefix + key, value);
            public override void SetComment(string key, string comment) => base.SetComment(prefix + key, comment);

            // This only returns keys which start with the prefix and then strips the prefix off:
            public override List<string> GetKeys() => base.GetKeys().FindAll(s => s.StartsWith(prefix)).ConvertAll(s => s.Remove(0, prefix.Length));
        }
    }
}