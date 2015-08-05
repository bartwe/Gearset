using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    /// Keeps values of functions so they can be plotted.
    /// </summary>
    public class DataSamplerManager : Gear {
        readonly Dictionary<string, DataSampler> _samplers;

        public DataSamplerConfig Config { get { return GearsetSettings.Instance.DataSamplerConfig; } }

#if WINDOWS
        public ObservableCollection<DataSampler> ObservableSamplers;
        public ReadOnlyObservableCollection<DataSampler> Samplers { get; private set; }
#else
        public List<DataSampler> observableSamplers;
        public List<DataSampler> Samplers { get; private set; }
#endif

        public DataSamplerManager()
            : base(GearsetSettings.Instance.DataSamplerConfig) {
            _samplers = new Dictionary<string, DataSampler>();
#if WINDOWS
            ObservableSamplers = new ObservableCollection<DataSampler>();
            Samplers = new ReadOnlyObservableCollection<DataSampler>(ObservableSamplers);
#else
            observableSamplers = new List<DataSampler>();
            Samplers = new List<DataSampler>(observableSamplers);
#endif
        }

        /// <summary>
        /// Adds an sampler which is bound to a <c>function</c> that will be sampled
        /// every <c>sampleRate</c> frames. <c>historyLength</c> values will be kept.
        /// </summary>
        public void AddSampler(String name, int historyLength, int sampleRate, Func<float> function) {
            var sampler = new DataSampler(name, historyLength, sampleRate, function);
            InsertSampler(name, sampler);
        }

        void InsertSampler(String name, DataSampler sampler) {
            _samplers.Add(name, sampler);
            ObservableSamplers.Add(sampler);
        }

        /// <summary>
        /// Adds an sampler which is bound to a <c>function</c> that will be sampled
        /// every <c>sampleRate</c> frames. <c>historyLength</c> values will be kept.
        /// </summary>
        public void AddSampler(String name, int historyLength, int sampleRate, Func<float, float> function) {
            var sampler = new DataSampler(name, historyLength, sampleRate, function);
            InsertSampler(name, sampler);
        }

        /// <summary>
        /// Adds a single sample to the sampler of the specified name, if the sampler does
        /// not exists it will be created. This function is intended to be used with sampler that
        /// are not bound to a function.
        /// </summary>
        public void AddSample(String name, float value) {
            DataSampler sampler;
            if (!_samplers.TryGetValue(name, out sampler)) {
                sampler = new DataSampler(name);
                InsertSampler(name, sampler);
            }
            sampler.InsertSample(value);
        }

        /// <summary>
        /// Adds a single sample to the sampler of the specified name, if the sampler does
        /// not exists it will be created with the specified historyLength. 
        /// This function is intended to be used with sampler that are not bound to a function.
        /// </summary>
        public void AddSample(string name, float value, int historyLength) {
            DataSampler sampler;
            if (!_samplers.TryGetValue(name, out sampler)) {
                sampler = new DataSampler(name, historyLength, 0, default(Func<float>));
                InsertSampler(name, sampler);
            }
            else {
                sampler.Values.Capacity = historyLength;
            }
            sampler.InsertSample(value);
        }

        internal DataSampler GetSampler(String name) {
            DataSampler sampler;
            if (!_samplers.TryGetValue(name, out sampler)) {
                AddSampler(name, Config.DefaultHistoryLength, 0, (Func<float>)null);
                return _samplers[name];
            }
            return sampler;
        }

        public override void Update(GameTime gameTime) {
            // Update all samplers.
            foreach (var sampler in _samplers.Values) {
                sampler.Update(gameTime);
            }
        }
    }
}