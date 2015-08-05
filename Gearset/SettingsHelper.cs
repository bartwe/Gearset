using System;
using System.Collections.Generic;
using System.IO;
#if WINDOWS
using System.Runtime.Serialization.Formatters.Binary;

#endif

namespace Gearset {
    /// <summary>
    /// Used to save data to/from a file.
    /// </summary>
    [Obsolete("This class needs redesign to be usable again", true)]
    class SettingsHelper {
        /// <summary>
        /// The place where all config values get saved.
        /// </summary>
        Dictionary<String, object> _configuration;

        /// <summary>
        /// The filename of the configuration file, initialized
        /// by default with "$username$.default.DebugConsoleConfig"
        /// </summary>
        public String CurrentSettingsName;

        internal SettingsHelper() {
            CurrentSettingsName = "default";
            Load();
        }

        /// <summary>
        /// Sets a specified configuration value, or overwrite the current
        /// if present.
        /// </summary>
        /// <param name="key">The name of the configuration value, as a rule of thumb
        /// use the following format for keys "filename.variableName" (without
        /// the .cs extension)</param>
        /// <param name="value">The value, only serializable objects should be used here.</param>
        internal void Set(String key, object value) {
            if (_configuration.ContainsKey(key)) {
                _configuration[key] = value;
            }
            else {
                _configuration.Add(key, value);
            }
        }

        /// <summary>
        /// Retrieves a configuratio value.
        /// </summary>
        /// <param name="key">The name of the configuration value</param>
        /// <param name="defaultValue">This valus is returned if the key is not found</param>
        /// <returns>The prevoiuly stored configuration value.</returns>
        internal object Get(String key, object defaultValue) {
            Object value;
            if (_configuration.TryGetValue(key, out value)) {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves the current state of the configuration.
        /// </summary>
        internal void Save() {
#if WINDOWS
            try {
                using (var file = new FileStream("gearset.config", FileMode.Create)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(file, _configuration);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Gearset settings could not be saved: " + e.Message);
            }
#endif
        }

        /// <summary>
        /// Loads a saved configuration
        /// </summary>
        internal void Load() {
#if WINDOWS
            try {
                if (File.Exists("gearset.config")) {
                    using (var file = new FileStream("gearset.config", FileMode.Open)) {
                        var formatter = new BinaryFormatter();
                        _configuration = (Dictionary<String, object>)formatter.Deserialize(file);
                    }
                }
                else {
                    Console.WriteLine("No settings file found, using a fresh set.");
                    _configuration = new Dictionary<string, object>();
                }
            }
            catch {
                Console.WriteLine("Problem while reading settings, using a fresh set.");
                _configuration = new Dictionary<string, object>();
            }
#endif
        }
    }
}
