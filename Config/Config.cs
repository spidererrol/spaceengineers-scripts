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
        /// <summary>
        /// Handle storing configuration on an <see cref="IMyTerminalBlock"/> in <see cref="MyIni"/> format.
        /// </summary>
        class Config : MyIni
        {
            public class ConfigSectionKey
            {
                protected readonly ConfigSection parent;
                protected readonly string key;

                public ConfigSection Section => parent;
                public string Key => key;

                public ConfigSectionKey(ConfigSection configSection, string keyname)
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

            /// <summary>
            /// This represents a single section within a <see cref="IngameScript.Config"/>.
            /// </summary>
            public class ConfigSection
            {
                private readonly Config parent;
                private readonly string section;

                /// <summary>
                /// Constructor - get a section from an existing <see cref="IngameScript.Config"/>
                /// </summary>
                /// <param name="myParent">Parent <see cref="IngameScript.Config"/></param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(Config myParent, string mySection)
                {
                    parent = myParent;
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section directly from a block.
                /// </summary>
                /// <param name="start">An <see cref="IMyTerminalBlock"/> to get the config from.</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(IMyTerminalBlock start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section from an ini string.
                /// </summary>
                /// <param name="start">String form of configuration</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(string start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section from an existing <see cref="MyIni"/>
                /// </summary>
                /// <remarks>
                /// The supplied MyIni will be copied and updates to this ConfigSection will NOT update the MyIni.
                /// </remarks>
                /// <param name="start"><see cref="MyIni"/> to initialise the configuration</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(MyIni start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }

                public ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);

                /// <returns>Returns the <see cref="IngameScript.Config"/> that this section is part of.</returns>
                public Config Config => parent;

                /// <summary>
                /// Check if a specific key exists in this section.
                /// </summary>
                /// <param name="key">Key to check for</param>
                /// <returns>Wether or not the key exists</returns>
                public bool ContainsKey(string key) => parent.ContainsKey(section, key);

                /// <summary>
                /// Set a key to a specified value.
                /// </summary>
                /// <param name="key">Name of key to set.</param>
                /// <param name="value">Value to set.</param>
                public void Set(string key, string value) => parent.Set(section, key, value);
                public void Set(string key, bool value) => parent.Set(section, key, value);
                public void Set(string key, int value) => parent.Set(section, key, value);
                public void Set(string key, float value) => parent.Set(section, key, value);

                public void SetComment(string key, string comment) => parent.SetComment(section, key, comment);
                public string GetComment(string key) => parent.GetComment(section, key);

                /// <summary>
                /// Retrieve a key, setting it to a default if it is missing.
                /// </summary>
                /// <param name="key">Key to retrieve</param>
                /// <param name="defaultvalue">Default value if key is missing.</param>
                /// <returns>value of key or default value.</returns>
                public string Get(string key, string defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToString();
                }
                public int Get(string key, int defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToInt32();
                }
                public float Get(string key, float defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToSingle();
                }
                public bool Get(string key, bool defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToBoolean(defaultvalue);
                }
                public MyIniValue Get(string key) => parent.Get(section, key);
                public string GetString(string key) => Get(key).ToString();
                public int GetInt(string key) => Get(key).ToInt32();
                public float GetFloat(string key) => Get(key).ToSingle();
                public bool GetBool(string key) => Get(key).ToBoolean();

                /// <summary>
                /// Shorthand for <c>value = section.Get(key,value);</c>
                /// </summary>
                /// <param name="key">key to update</param>
                /// <param name="value">value to use as default and to update</param>
                public void Get(string key, ref string value) => value = Get(key, value);
                public void Get(string key, ref int value) => value = Get(key, value);
                public void Get(string key, ref float value) => value = Get(key, value);
                public void Get(string key, ref bool value) => value = Get(key, value);

                /// <summary>
                /// Set key to a default value if it has not been set already.
                /// </summary>
                /// <param name="key">key to set</param>
                /// <param name="value">default value to use</param>
                public void Default(string key, string value) => Get(key, value);
                public void Default(string key, int value) => Get(key, value);
                public void Default(string key, float value) => Get(key, value);
                public void Default(string key, bool value) => Get(key, value);

                /// <summary>
                /// Save ENTIRE config to block.
                /// </summary>
                /// <seealso cref="Config.Save(IMyTerminalBlock)"/>
                /// <param name="block">block to save configuration to.</param>
                public void Save(IMyTerminalBlock block) => parent.Save(block);
                /// <summary>
                /// Save ENTIRE config to the block used to create the config.
                /// </summary>
                /// <seealso cref="Config.Save()"/>
                /// <exception cref="NoBlockSpecified">Thrown if this config was not created using an <see cref="IMyTerminalBlock"/></exception>
                public void Save() => parent.Save();

            }

            private readonly IMyTerminalBlock termBlock;
            private string original;

            /// <summary>
            /// Create a <see cref="ConfigSection"/> directly from a config source.
            /// </summary>
            /// <param name="start">A <see cref="IMyTerminalBlock"/>, <see cref="MyIni"/>, or string</param>
            /// <param name="section">The name of the section to use.</param>
            /// <returns>A <see cref="ConfigSection"/></returns>
            public static ConfigSection Section(IMyTerminalBlock start, string section) => new Config(start).Section(section);
            public static ConfigSection Section(MyIni start, string section) => new Config(start).Section(section);
            public static ConfigSection Section(string start, string section) => new Config(start).Section(section);

            /// <summary>
            /// Parse configuration from a terminal block.
            /// Additionally block is stored to be used with <see cref="Save()"/> later on.
            /// </summary>
            /// <param name="block">The <see cref="IMyTerminalBlock"/> to retrieve the config from.</param>
            public Config(IMyTerminalBlock block) : base()
            {
                termBlock = block;
                Load(block);
            }

            /// <summary>
            /// Parse configuration from a string.
            /// </summary>
            /// <param name="defaultini">string containing config.</param>
            public Config(string defaultini) : base()
            {
                Load(defaultini);
            }
            /// <summary>
            /// Parse configuration from an existing <see cref="MyIni"/>
            /// </summary>
            /// <remarks>
            /// The <see cref="MyIni"/> will be copied and will not recieve updates.
            /// </remarks>
            /// <param name="defaultini"><see cref="MyIni"/> to base config on.</param>
            public Config(MyIni defaultini) : base()
            {
                Load(defaultini);
            }
            /// <summary>
            /// Create an empty configuration.
            /// </summary>
            /// <remarks>
            /// You can then use <see cref="Load(IMyTerminalBlock)"/> to load a config if desired.
            /// </remarks>
            public Config() : base() { }

            /// <summary>
            /// Load configuration from a block. If there is no configuration on the block then
            /// first saves any existing configuration to the block.
            /// </summary>
            /// <remarks>
            /// Will clear any existing configuration first.
            /// </remarks>
            /// <param name="block"></param>
            public void Load(IMyTerminalBlock block)
            {
                if (block.CustomData.Length == 0)
                {
                    block.CustomData = this.ToString();
                }
                this.Clear();
                this.TryParse(block.CustomData);
                original = this.ToString();
            }
            /// <summary>
            /// Load configuration from an existing <see cref="MyIni"/>
            /// </summary>
            /// <remarks>
            /// Will copy the config from the <see cref="MyIni"/>. Updates will not be sent to the original
            /// ini.
            /// </remarks>
            /// <param name="defaultini"><see cref="MyIni"/> to load from.</param>
            public void Load(MyIni defaultini)
            {
                this.Clear();
                this.TryParse(defaultini.ToString());
                original = this.ToString();
            }
            /// <summary>
            /// Load configuration from a string.
            /// </summary>
            /// <param name="defaultini">string containing config.</param>
            public void Load(string defaultini)
            {
                this.Clear();
                this.TryParse(defaultini);
                original = this.ToString();
            }

            /// <summary>
            /// Save the current configuration to the given block.
            /// </summary>
            /// <param name="block"></param>
            public void Save(IMyTerminalBlock block)
            {
                string output = this.ToString();
                if (output != original)
                {
                    block.CustomData = output;
                    original = output;
                }
            }
            public class NoBlockSpecified : Exception
            {
                public NoBlockSpecified(string msg) : base(msg) { }
            }
            /// <summary>
            /// Save to the block that this config was created with.
            /// </summary>
            /// <exception cref="NoBlockSpecified">Thrown if this config was not created using an <see cref="IMyTerminalBlock"/></exception>
            public void Save()
            {
                if (termBlock != null)
                    Save(termBlock);
                else
                    throw new NoBlockSpecified("Trying to save without having a block to save to!");
            }

            /// <summary>
            /// Retrieve (create) a <see cref="ConfigSection"/>.
            /// </summary>
            /// <param name="section">Name of section to connect to</param>
            /// <returns>Object representing the specified section.</returns>
            public ConfigSection Section(string section)
            {
                return new ConfigSection(this, section);
            }

        }
    }
}
