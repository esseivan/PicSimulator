using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicSimulatorLib;

namespace PicSimulatorLib
{
    public class HexFileDecoder
    {
        public static Dictionary<long, short> Decode(string filepath)
        {
            string[] textContent = File.ReadAllLines(filepath);

            Dictionary<long, short> data = new Dictionary<long, short>();

            long addrOffset = 0;
            bool eof = false;
            foreach (string line in textContent)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                byte index = 0;
                if (!line[index++].Equals(':'))
                    throw new Exception("Invalid file format");

                if (eof)
                    throw new Exception("Invalid file format");

                byte dataCount = Convert.ToByte(line.Substring(index, 2), 16);
                index += 2;
                short addr = Convert.ToInt16(line.Substring(index, 4), 16);
                index += 4;
                byte type = Convert.ToByte(line.Substring(index, 2), 16);
                index += 2;
                string dataString = line.Substring(index, dataCount * 2);
                short[] dataBytes = DecodeData(dataString, dataCount);
                index += (byte)(2 * dataCount);
                byte checksum = Convert.ToByte(line.Substring(index, 2), 16);
                index += 2;
                if (line.Length > index)
                    throw new Exception("Invalid file format");

                // Checksum verification
                int calculatedChecksum = GetChecksum(line);
                if (checksum != calculatedChecksum)
                    throw new Exception($"Invalid checksum at line '{line}'");

                short dataShort;
                switch (type)
                {
                    case 00:
                        // data
                        for (int i = 0; i < dataCount / 2; i++)
                        {
                            data.Add(addrOffset + addr + i, dataBytes[i]);
                        }
                        break;

                    case 01:
                        // EOF
                        eof = true;
                        break;

                    case 02:
                        // Linear address
                        // data MUST be only 2 bytes long
                        if (dataCount != 2)
                            throw new Exception("Invalid file format");
                        dataShort = Convert.ToInt16(dataString, 16);
                        addrOffset = dataShort << 4;
                        break;

                    case 04:
                        // Extended address
                        // data MUST be only 2 bytes long
                        if (dataCount != 2)
                            throw new Exception("Invalid file format");
                        dataShort = Convert.ToInt16(dataString, 16);
                        addrOffset = dataShort << 16;
                        break;

                    default:
                        throw new Exception("Invalid file format");
                }
            }

            return data;
        }

        public static Dictionary<long, Instruction> GetInstructions(string filepath)
        {
            return GetInstructions(Decode(filepath));
        }

        public static Dictionary<long, Instruction> GetInstructions(Dictionary<long, short> data)
        {
            return data.ToDictionary((x) => x.Key, (x) => new Instruction(x.Value));
        }

        private static short[] DecodeData(string dataString, byte dataCount)
        {
            short[] output = new short[dataCount / 2];
            if (dataString.Length != 2 * dataCount)
                throw new Exception("Invalid file format");

            for (int i = 0; i < dataCount / 2; i++)
            {
                int j = i;
                short value = Convert.ToByte(dataString.Substring((2 * j) * 2, 2), 16);
                value += (short)(0x100 * Convert.ToByte(dataString.Substring(((2 * j) + 1) * 2, 2), 16));
                output[i] = value;
            }
            return output;
        }

        private static byte GetChecksum(string line)
        {
            line = line.Substring(1, line.Length - 3);
            byte chk = 0;
            for (int i = 0; i < line.Length / 2; i++)
            {
                chk -= Convert.ToByte(line.Substring(i * 2, 2), 16);
            }
            return chk;
        }
    }
}
