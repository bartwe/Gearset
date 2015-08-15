namespace Gearset {
    /// <summary>
    /// Every DebugComponent will be called (Draw())
    /// for each of these modes, so they can draw
    /// when they need to.
    /// </summary>
    enum RenderPass {
        /// <summary>
        /// DebugComponents should draw only using
        /// spriteBatch here (without Begin() or End())
        /// </summary>
        SpriteBatchPass,

        /// <summary>
        /// DebugComponents should draw using the 
        /// common effect, and without changing any 
        /// parameters from it.
        /// </summary>
        BasicEffectPass,

        /// <summary>
        /// DebugComponents can draw whatever they want
        /// in this pass, they can change the params
        /// of the spritebatch and the basiceffect or even
        /// use their own effects.
        /// </summary>
        CustomPass,

        /// <summary>
        /// Similar to BasicEffectPass but with matrices
        /// work with screen space coordinates.
        /// </summary>
        ScreenSpacePass,

        /// <summary>
        /// Similar to ScreenSpacePass but with matrices
        /// passed by the game, so it's a transformed
        /// screen space.
        /// </summary>
        GameSpacePass
    }
}
