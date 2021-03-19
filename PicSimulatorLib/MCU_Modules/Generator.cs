using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Generator
    {
        public enum MCUReference
        {
            PIC16F1827,
            PIC16F18334,
        }

        public static MCU GenerateSimulator(MCUReference reference)
        {
            Type mcuType;
            try
            {
                string typeName = $"PicSimulatorLib.MCU_{reference}";
                mcuType = Type.GetType(typeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (mcuType == null)
                throw new ArgumentException("MCU not recognised");

            if (!mcuType.IsSubclassOf(typeof(MCU)))
                throw new ArgumentException("Type not supported");

            var constructor = mcuType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new ArgumentException("Type not supported");

            MCU mcu = constructor.Invoke(null) as MCU;

            return mcu;
        }
    }
}
