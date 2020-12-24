using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class RegisterListPopulate
    {

        public static RegisterList Populate(string filepath)
        {
            string data = File.ReadAllText(filepath).Replace("\r", "");
            string[] lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();

            if (lines.Length == 0)
                throw new FormatException("Invalid file format");

            RegisterList rl = new RegisterList();

            foreach (string line in lines)
            {
                string[] items = line.Split(',');
                ImportItem item = new ImportItem(items);

                // Partition fill
                bool nameProcessed = false;
                if (item.name.StartsWith("__"))
                {
                    string name_val_str = item.name.Substring(2, item.name.Length - 2);
                    if (int.TryParse(name_val_str, out int count))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            Register r = new Register()
                            {
                                UnimplementedMask = item.uMask,
                                WritableMask = item.wMask,
                                ReadableMask = item.rMask,
                                SyncToAddr = item.syncToAddr,
                            };

                            rl.Add(item.GetFullAddr(), r, "RAM" + item.GetFullAddr());
                        }
                        nameProcessed = true;
                    }
                }

                // Classic single register
                if (!nameProcessed)
                {
                    Register r = new Register()
                    {
                        UnimplementedMask = item.uMask,
                        WritableMask = item.wMask,
                        ReadableMask = item.rMask,
                        SyncToAddr = item.syncToAddr,
                    };

                    rl.Add(item.GetFullAddr(), r, item.name);
                }
            }

            return rl;
        }

        public class ImportItem
        {
            public byte bank;
            public int addr;
            public string name;
            public byte porVal;
            public byte rMask;
            public byte wMask;
            public byte uMask;
            public int syncToAddr = -1;

            public int GetFullAddr()
            {
                return bank << 7 + addr;
            }

            public ImportItem(IEnumerable<string> items)
            {
                if (items.Count() != 8)
                    throw new ArgumentException("Invalid items count. Required '8'", "items");

                bank = byte.Parse(items.ElementAt(0));
                addr = GetValue(items.ElementAt(1));
                name = items.ElementAt(2);

                porVal = (byte)GetValue(items.ElementAt(3));
                rMask = (byte)GetValue(items.ElementAt(4));
                wMask = (byte)GetValue(items.ElementAt(5));
                uMask = (byte)GetValue(items.ElementAt(6));
                if (!string.IsNullOrEmpty(items.ElementAt(7)))
                    syncToAddr = GetValue(items.ElementAt(7));
            }

            /// <summary>
            /// Return the value from hex or decimal string
            /// </summary>
            public int GetValue(string item)
            {
                if (item.StartsWith("0x"))
                    return Convert.ToInt16(item.Substring(2, item.Length - 2), 16);
                else
                    return Convert.ToInt16(item, 10);
            }
        }
    }
}
