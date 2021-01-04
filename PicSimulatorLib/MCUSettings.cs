using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class MCUSettings
    {
        public short dataMemoryCount,
            programMemoryCount,
            configurationMemoryCount;

        public byte bankCount,
            addrBits,
            bankBits,
            stackCount;

        public short bankLength,
            resetVector,
            interruptVector;
    }
}
