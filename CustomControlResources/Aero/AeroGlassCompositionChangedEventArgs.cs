using System;

namespace CustomControlResources.Aero
{
    /// <summary>
    /// GlassAvailabilityChanged event arg
    /// </summary>
    public class AeroGlassCompositionChangedEventArgs : EventArgs
    {
        public AeroGlassCompositionChangedEventArgs(bool avialbility)
        {
            GlassAvailable = avialbility;
        }

        /// <summary>
        /// Aero effect avaliable or not
        /// </summary>
        public bool GlassAvailable { get; private set; }

    }
}
