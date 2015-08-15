using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Gearset.Components.Data {
    /// <summary>
    /// Class that takes a DataSampler and plots its values to the screen.
    /// </summary>
    public sealed class Plotter : Gear {
        /// <summary>
        /// Current plots.
        /// </summary>
#if WINDOWS
        readonly ObservableCollection<Plot> _plots;

        public ObservableCollection<Plot> Plots { get { return _plots; } }
#else
        private List<Plot> plots;
        public List<Plot> Plots { get { return plots; } }
#endif

        /// <summary>
        /// Keep a reference to the last plot added so we can position the next one.
        /// </summary>
        Plot _lastPlotAdded;

        /// <summary>
        /// If a plot is beign removed, the change of visibility to false will not
        /// imply that the plot should be added to the hidden list.
        /// </summary>
        Plot _plotBeignRemoved;

        readonly InternalLineDrawer _lines;
        readonly InternalLabeler _labels;

        /// <summary>
        /// Gets or sets the config object.
        /// </summary>
        public PlotterConfig Config { get { return GearsetResources.Console.Settings.PlotterConfig; } }

        public Plotter()
            : base(GearsetResources.Console.Settings.PlotterConfig) {
#if WINDOWS
            _plots = new ObservableCollection<Plot>();
#else
            plots = new List<Plot>();
#endif
            Config.Cleared += Config_Cleared;
            // Create the line drawer
            _lines = new InternalLineDrawer();
            Children.Add(_lines);
            _labels = new InternalLabeler();
            Children.Add(_labels);
        }

        void Config_Cleared(object sender, EventArgs e) {
            Clear();
        }

        /// <summary>
        /// Shows a plot of the sampler with the specified name if the sampler does
        /// not exist it is created.
        /// </summary>
        public void ShowPlot(String samplerName) {
            // Check if the plot exist already.
            foreach (var p in _plots) {
                if (p.Sampler.Name == samplerName)
                    return;
            }
            var sampler = GearsetResources.Console.DataSamplerManager.GetSampler(samplerName);
            if (sampler == null)
                return;

            var position = GetNextPosition();
            var plot = new Plot(sampler, position, Config.DefaultSize);
            _plots.Add(plot);

            // Hide all plots that are beign hidden.
            plot.Visible = !Config.HiddenPlots.Contains(samplerName);

            plot.VisibleChanged += plot_VisibleChanged;

            _lastPlotAdded = plot;
        }

        void plot_VisibleChanged(object sender, EventArgs e) {
            var plot = sender as Plot;

            if (!plot.Visible) {
                // Disabled
                _labels.HideLabel(plot.TitleLabelName);
                _labels.HideLabel(plot.MinLabelName);
                _labels.HideLabel(plot.MaxLabelName);
                if (!Config.HiddenPlots.Contains(plot.Sampler.Name) && _plotBeignRemoved != plot)
                    Config.HiddenPlots.Add(plot.Sampler.Name);
            }
            else {
                // Enabled
                if (Config.HiddenPlots.Contains(plot.Sampler.Name))
                    Config.HiddenPlots.Remove(plot.Sampler.Name);
            }
        }

        /// <summary>
        /// Calculates a position for a new plot.
        /// </summary>
        Vector2 GetNextPosition() {
            var padding = new Vector2(3, 15);
            var screenSize = new Vector2(GearsetResources.Device.Viewport.Width, GearsetResources.Device.Viewport.Height);

            Plot lastPlotAdded;
            if (_plots.Count == 0)
                return new Vector2(padding.X, screenSize.Y - padding.Y - Config.DefaultSize.Y);
            lastPlotAdded = _plots[_plots.Count - 1];

            var result = lastPlotAdded.Position + (padding + lastPlotAdded.Size) * Vector2.UnitX;
            if (result.X + Config.DefaultSize.X > GearsetResources.Device.Viewport.Width) {
                result = new Vector2(padding.X, lastPlotAdded.Position.Y - padding.Y - Config.DefaultSize.Y);
            }
            return result;
        }

        public sealed override void Update(GameTime gameTime) {}

        public sealed override void Draw(GameTime gameTime) {
            // Just to make sure we're only doing this one per frame.
            if (GearsetResources.CurrentRenderPass != RenderPass.BasicEffectPass)
                return;

            foreach (var plot in _plots) {
                if (!plot.Visible) continue;
                var count = plot.Sampler.Values.Capacity;
                float max, min, actualmin, actualmax;
                var position = plot.Position;
                var size = plot.Size;
                GetLimits(plot, out actualmin, out actualmax);


                min = plot.Min = plot.Min + (actualmin - plot.Min) * 0.3f;
                max = plot.Max = plot.Max + (actualmax - plot.Max) * 0.3f;

                // Draw the background
                GearsetResources.Console.SolidBoxDrawer.ShowGradientBoxOnce(position, position + size, new Color(56, 56, 56, 150), new Color(16, 16, 16, 127));

                // Draw the border
                plot.DrawBorderLines(Color.Gray, _lines);
                //if (plot.TitleBar.IsMouseOver)
                //    plot.TitleBar.DrawBorderLines(Color.White, lines);
                if (plot.ScaleNob.IsMouseOver)
                    plot.ScaleNob.DrawBorderLines(Color.White, _lines);
                _labels.ShowLabel(plot.TitleLabelName, position + new Vector2(0, -12), plot.Sampler.Name);

                if (min != max) {
                    // Draw zero
                    if (min < 0 && max > 0) {
                        var normalValue = (0 - min) / (max - min);
                        var yoffset = new Vector2 { X = 0, Y = size.Y * (1 - normalValue) };
                        _lines.ShowLineOnce(position + yoffset, position + new Vector2(size.X, 0) + yoffset, new Color(230, 0, 0, 220));
                    }

                    var previousPoint = Vector2.Zero;
                    var pixelOffset = Vector2.UnitY;
                    var i = 0;
                    foreach (var value in plot.Sampler.Values) {
                        var normalValue = (value - min) / (max - min);
                        var point = new Vector2 {
                            X = position.X + i / ((float)count - 1) * size.X,
                            Y = position.Y + (size.Y - 1f) * MathHelper.Clamp((1 - normalValue), 0, 1)
                        };

                        if (i != 0) {
                            _lines.ShowLineOnce(previousPoint, point, new Color(138, 198, 49));
                            _lines.ShowLineOnce(previousPoint + pixelOffset, point + pixelOffset, new Color(138, 198, 49));
                        }

                        i++;
                        previousPoint = point;
                    }

                    // Show the min/max labels.
                    _labels.ShowLabel(plot.MinLabelName, position + new Vector2(2, size.Y - 12), actualmin.ToString(), Color.White);
                    _labels.ShowLabel(plot.MaxLabelName, position + new Vector2(2, 0), actualmax.ToString(), Color.White);
                }
                else if (plot.Sampler.Values.Count > 0) {
                    _lines.ShowLineOnce(new Vector2(position.X, position.Y + size.Y * .5f), new Vector2(position.X + size.X, position.Y + size.Y * .5f), new Color(138, 198, 49));
                    _lines.ShowLineOnce(new Vector2(position.X, position.Y + size.Y * .5f + 1), new Vector2(position.X + size.X, position.Y + size.Y * .5f + 1), new Color(138, 198, 49));
                    _labels.ShowLabel(plot.MinLabelName, position + new Vector2(2, size.Y * .5f - 12), actualmin.ToString(), Color.White);
                }
                else {
                    plot.DrawCrossLines(Color.Gray);
                }
            }
        }

        // TODO: move this to the DataSampler class.
        static void GetLimits(Plot plot, out float min, out float max) {
            max = float.MinValue;
            min = float.MaxValue;
            foreach (var value in plot.Sampler.Values) {
                if (value > max)
                    max = value;
                if (value < min)
                    min = value;
            }
            max = (float)Math.Ceiling(max);
            min = (float)Math.Floor(min);
        }

        /// <summary>
        /// Removes a plot, if ShowPlot is called again for this plot, it will be shown
        /// again
        /// </summary>
        /// <param name="name">Name of the plot to remove.</param>
        public void RemovePlot(String name) {
            _plotBeignRemoved = null;
            foreach (var plot in _plots) {
                if (plot.Sampler.Name == name) {
                    _plotBeignRemoved = plot;
                    break;
                }
            }
            if (_plotBeignRemoved != null) {
                _plotBeignRemoved.Visible = false;
                _plots.Remove(_plotBeignRemoved);
                _plotBeignRemoved = null;
            }
        }

        /// <summary>
        /// Removes all plots, if ShowPlot is called again, plots will be shown
        /// again.
        /// </summary>
        public void Clear() {
            HideAll();
            _plots.Clear();
        }

        /// <summary>
        /// Hides all plots, data will still be captured.
        /// </summary>
        public void HideAll() {
            foreach (var plot in _plots) {
                plot.Visible = false;
            }
        }

        /// <summary>
        /// Hides all plots, data will still be captured.
        /// </summary>
        public void ShowAll() {
            foreach (var plot in _plots) {
                plot.Visible = true;
            }
        }

        /// <summary>
        /// Resets the positions of all overlaid plots.
        /// </summary>
        public void ResetPositions() {
            var plotsAux = new List<Plot>();
            foreach (var plot in _plots)
                plotsAux.Add(plot);
            _plots.Clear();
            foreach (var plot in plotsAux) {
                plot.Position = GetNextPosition();
                plot.Size = Config.DefaultSize;
                _plots.Add(plot);
            }
            plotsAux.Clear();
        }
    }
}
