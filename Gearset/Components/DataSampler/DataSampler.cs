using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    /// Keeps the history of a function.
    /// </summary>
    public sealed class DataSampler {
        public FixedLengthQueue<float> Values;
        int _elapsedFrames;

        public DataSampler(String name) {
            Name = name;
            SampleRate = 1;
            Values = new FixedLengthQueue<float>(GearsetResources.Console.Settings.DataSamplerConfig.DefaultHistoryLength);
        }

        public DataSampler(String name, int historyLength, int sampleRate, Func<float, float> function) {
            Name = name;
            Function2 = function;
            SampleRate = sampleRate;
            Values = new FixedLengthQueue<float>(historyLength);
        }

        public DataSampler(String name, int historyLength, int sampleRate, Func<float> function) {
            Name = name;
            Function = function;
            SampleRate = sampleRate;
            Values = new FixedLengthQueue<float>(historyLength);
        }

        public String Name { get; private set; }
        public int SampleRate { get; set; }
        public Func<float> Function { get; private set; }
        public Func<float, float> Function2 { get; private set; }

        public void Update(GameTime gameTime) {
            if (SampleRate == 0 || (Function == null && Function2 == null))
                return;

            _elapsedFrames++;

            if (_elapsedFrames >= SampleRate) {
                if (Function != null)
                    Values.Enqueue(Function());
                else
                    Values.Enqueue(Function2((float)gameTime.ElapsedGameTime.TotalSeconds));
                _elapsedFrames = 0;
            }
        }

        /// <summary>
        /// Takes a sample from the bound function.
        /// </summary>
        public void TakeSample() {
            if (Function != null)
                Values.Enqueue(Function());
        }

        /// <summary>
        /// Inserts a sample. This method must be used with samplers that are
        /// not bound to a function.
        /// </summary>
        /// <param name="value"></param>
        public void InsertSample(float value) {
            Values.Enqueue(value);
        }
    }
}
