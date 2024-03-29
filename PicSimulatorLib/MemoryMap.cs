﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class MemoryMap
    {
        public Register this[byte bank, short address]
        {
            get => registers[(bank << settings.addrBits) + address];
            set => registers[(bank << settings.addrBits) + address] = value;
        }
        public Register this[long address]
        {
            get => registers[address];
            set => registers[address] = value;
        }

        public Register this[string name]
        {
            get => this[names[name]];
            set => this[names[name]] = value;
        }

        public readonly Register[] registers;

        public readonly MCUSettings settings;

        public Dictionary<string, long> names;

        public int Count => registers.Length;

        public MemoryMap(MCUSettings settings)
        {
            this.settings = settings;
            names = new Dictionary<string, long>();
            registers = new Register[settings.dataMemoryCount];
            for (int i = 0; i < settings.dataMemoryCount; i++)
            {
                registers[i] = new Register();
            }
        }

        public void SetNames(IDictionary<string, long> data)
        {
            names = new Dictionary<string, long>(data);
        }

        public string GetRegisterName(Register register)
        {
            var name = names.Where((x) => this[x.Value] == register);
            if (name.Count() == 0)
                return string.Empty;
            return name.First().Key;
        }
    }
}
