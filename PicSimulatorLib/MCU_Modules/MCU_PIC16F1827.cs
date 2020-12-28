﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class MCU_PIC16F1827 : MCU_Simulator
    {
        protected new byte addrBits => 7;
        protected new byte bankBits => 5;
        protected new short bankLength => 128;
        protected new byte bankCount => 32;

        public MCU_PIC16F1827()
        {
            registers = new Register[4096];
            program = new byte[4096];
            gpios = new IO[16];

            Populate();
        }

        public void Populate()
        {
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = new Register();
            }
        }

        public Dictionary<string, Register> PopulateRegisters()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "MCU_Modules", "Generators", "PIC16F1827.csv");
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Register map not found for PIC16F1827 ({0})", path));

            Dictionary<string, Register> registers = new Dictionary<string, Register>();

            string data = File.ReadAllText(path).Replace("\r", "");
            string[] lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] items = line.Split(',');
                ImportItem item = new ImportItem(items);

                if (item.name.StartsWith("__"))
                {
                    string name_val_str = item.name.Substring(2, item.name.Length - 2);
                    int count = int.Parse(name_val_str);

                    for (int i = 0; i < count; i++)
                    {
                        Register r = new Register()
                        {
                            UnimplementedMask = item.uMask,
                            WritableMask = item.wMask,
                            ReadableMask = item.rMask,
                            SyncToAddr = item.syncToAddr,
                        };

                        registers.Add(item.name, r);
                    }

                }

                string name = items[0];
                bool nameProcessed = false;
                if (name.StartsWith("__"))
                {
                    string name_val_str = name.Substring(2, name.Length - 2);
                    if (int.TryParse(name_val_str, out int name_val))
                    {
                        nameProcessed = true;

                        // ...

                    }
                }
                if (!nameProcessed)
                {
                    // ...
                }

            }

            return registers;
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

            public ImportItem(IEnumerable<string> items)
            {
                if (items.Count() != 8)
                    throw new ArgumentException("Not enough items", "items");

                bank = byte.Parse(items.ElementAt(0));
                addr = GetValue(items.ElementAt(1));
                name = items.ElementAt(2);

                porVal = (byte)GetValue(items.ElementAt(3));
                rMask = (byte)GetValue(items.ElementAt(4));
                wMask = (byte)GetValue(items.ElementAt(5));
                uMask = (byte)GetValue(items.ElementAt(6));
                syncToAddr = GetValue(items.ElementAt(7));
            }

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
