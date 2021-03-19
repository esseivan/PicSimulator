using System;

namespace PicSimulatorLib
{
    public partial class Simulator
    {
        public class FrequencyChangeEventArgs : EventArgs
        {
            public int Frequency;

            public FrequencyChangeEventArgs(int frequency)
            {
                Frequency = frequency;
            }
        }
    }
}
