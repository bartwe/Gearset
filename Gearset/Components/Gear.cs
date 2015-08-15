using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    /// A Gear's methods will be called by the GearConsole
    /// pretty much like a DrawableGameComponent gets called
    /// by XNA.
    /// </summary>
    public abstract class Gear {
        protected GearConfig GearConfig;

        public Gear(GearConfig config) {
            GearConfig = config;

            config.EnabledChanged += config_EnabledChanged;
            config.VisibleChanged += config_VisibleChanged;

            Children = new List<Gear>();
        }

        public List<Gear> Children { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Gear"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get { return GearConfig.Enabled; } }

        //set { gearConfig.Enabled = value; OnEnabledChanged(); } }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Gear"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public bool Visible { get { return GearConfig.Visible; } }

        //set { gearConfig.Visible = value; OnVisibleChanged(); } }

        /// <summary>
        /// Gets the game.
        /// </summary>
        public Game Game { get { return GearsetResources.Game; } }

        void config_VisibleChanged(object sender, BooleanChangedEventArgs e) {
            OnVisibleChanged();
        }

        void config_EnabledChanged(object sender, BooleanChangedEventArgs e) {
            OnEnabledChanged();
        }

        /// <summary>
        /// Called every frame so that the component can get updated.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime) {}

        /// <summary>
        /// Called several times every frame, one for each render pass.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) {}

        /// <summary>
        /// Called for every component when the game resolution changes.
        /// So that it can adjust things like drawing positions.
        /// </summary>
        public virtual void OnResolutionChanged() {}

        /// <summary>
        /// sealed override to implement functionality when the value
        /// of Enabled changes.
        /// </summary>
        protected virtual void OnEnabledChanged() {}

        /// <summary>
        /// sealed override to implement functionality when the value
        /// of Visible changes.
        /// </summary>
        protected virtual void OnVisibleChanged() {}
    }
}
