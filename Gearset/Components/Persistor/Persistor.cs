using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Gearset.Components.Persistor {
    /// <summary>
    /// Handles persistance of object members. Can store and load data
    /// from/to an object as when it is being initialized. With this
    /// component variables can be edited, stored and reloaded.
    /// This class is not threadsafe as it uses the same StringBuilder
    /// </summary>
    class Persistor {
        /// <summary>
        /// One instance for all calls to String.Split. Avoids
        /// garbage generation.
        /// </summary>
        static readonly char[] CommaDelimiter = { ',' };

        /// <summary>
        /// The data stored by the dictionary it maps the an id
        /// with a dictionary of stored values for that id.
        /// </summary>
        ValueCollection _data;

        /// <summary>
        /// Constructor, will load everything.
        /// </summary>
        public Persistor() {
            Load("default");
        }

        public string CurrentSettingsName { get; set; }

        /// <summary>
        /// Loads a pre-saved Persistor data set.
        /// <see>Save</see>
        /// </summary>
        void Load(String settingsName) {
            CurrentSettingsName = settingsName;
            var formatter = new BinaryFormatter();
            try {
                var file = new FileStream(settingsName + ".PersistorData", FileMode.Open);
                _data = (ValueCollection)formatter.Deserialize(file);
                file.Close();
            }
            catch {
                GearsetResources.Console.Log("Gearset", "No persistor data file found, creating a fresh data set.");
                _data = new ValueCollection();
            }
        }

        /// <summary>
        /// Saves the current state of the configuration with a settings name.
        /// This allows to create several settings, useful for example to debug
        /// different components of your game.
        /// </summary>
        /// <param name="settingsName"></param>
        public void Save(String settingsName) {
#if WINDOWS
            CurrentSettingsName = settingsName;
            //try
            //{
            var formatter = new BinaryFormatter();
            var file = new FileStream(settingsName + ".PersistorData", FileMode.Create);
            //XmlSerializer serializer = new XmlSerializer(configuration.GetType());
            //serializer.Serialize(file, configuration);
            formatter.Serialize(file, _data);
            file.Close();
            //}
            //catch (Exception e)
            //{
            //    GearsetResources.Console.Log("Gearset", "The DebugConsole configuration could not be saved: " + e.Message);
            //}
#endif
        }

        /// <summary>
        /// Initializes fields and properties of the object o, with
        /// data of the specified category and of the specified id.
        /// </summary>
        /// <param name="o">Object to get its fields initialized.</param>
        /// <param name="type">The category this object belongs, all objects
        /// of the same category will get the same values.</param>
        /// <param name="id">The id of the object which identifies it inside
        /// its category, it </param>
        public void InitializeObject(IPersistent o) {
            var t = o.GetType();
            var ids = o.Ids.Split(CommaDelimiter, StringSplitOptions.RemoveEmptyEntries);

            // Hey, lock here, there might be other threads
            // trying to initialize their values also.
            lock (_data) {
                foreach (var id in ids) {
                    if (!_data.ContainsKey(id)) // Do we have something for this id?
                        continue;

                    // Iterate over what we have for this id
                    foreach (var pair in _data[id]) {
                        // Does o has a member we can write to?
                        var members = t.GetMember(pair.Key);
                        foreach (var info in members) {
                            if (info is FieldInfo) {
                                TrySetValue(o, (FieldInfo)info, _data[id][info.Name]);
                            }
                            else if (info is FieldInfo) {
                                TrySetValue(o, (PropertyInfo)info, _data[id][info.Name]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of the field defined by info on the object o
        /// to the value value. If the fields aren't of the same type
        /// it will be silently ignored.
        /// </summary>
        void TrySetValue(IPersistent o, FieldInfo info, Object value) {
            if (value.GetType() == info.FieldType)
                info.SetValue(o, value);
        }

        /// <summary>
        /// Sets the value of the property defined by info on the object o
        /// to the value value. If the fields aren't of the same type
        /// it will be silently ignored.
        /// </summary>
        void TrySetValue(IPersistent o, PropertyInfo info, Object value) {
            if (value.GetType() == info.PropertyType)
                info.SetValue(o, value, null);
        }

        /// <summary>
        /// Call this method to save a value that will be later initialized
        /// to the object o.
        /// </summary>
        /// <param name="id">The id to which the value will be saved.</param>
        /// <param name="info">The field or property info to be saved.</param>
        /// <param name="value">The value to save for the specified field.</param>
        public void SaveValue<T>(String id, String memberName, T value) {
            lock (_data) {
                Dictionary<string, object> idData;
                if (_data.ContainsKey(id))
                    idData = _data[id];
                else {
                    idData = new Dictionary<string, object>();
                    _data.Add(id, idData);
                }

                idData[memberName] = value;
            }
        }
    }
}
