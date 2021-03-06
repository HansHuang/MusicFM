using System.Windows;

namespace MahApps.Metro.Controls
{
    /// <summary>
    /// Event arguments created for the RangeSlider's SelectionChanged event.
    /// <see cref="RangeSlider"/>
    /// </summary>
    public class RangeSelectionChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The value of the new range's beginning.
        /// </summary>
        public long NewRangeStart { get; set; }
        /// <summary>
        /// The value of the new range's ending.
        /// </summary>
        public long NewRangeStop { get; set; }

        internal RangeSelectionChangedEventArgs(long newRangeStart, long newRangeStop)
        {
            NewRangeStart = newRangeStart;
            NewRangeStop = newRangeStop;
        }

        internal RangeSelectionChangedEventArgs(RangeSlider slider)
            : this(slider.RangeStartSelected, slider.RangeStopSelected)
        { }
    }
}