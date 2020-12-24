using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulator.MCU_Modules
{
    public class RegisterList
    {
        private Dictionary<string, int> names = new Dictionary<string, int>();
        private Dictionary<int, Register> registers = new Dictionary<int, Register>();

        public Register this[int key] { get => registers[key]; set => registers[key] = value; }
        public Register this[string key] { get => registers[names[key]]; set => registers[names[key]] = value; }

        public ICollection<int> Indexes => registers.Keys;
        public ICollection<Register> Registers => registers.Values;
        public ICollection<string> Names => names.Keys;

        public void Add(int index, Register value, string name)
        {
            registers[index] = value;
            names[name] = index;
        }
        public void Clear()
        {
            names.Clear();
            registers.Clear();
        }
        public bool Contains(KeyValuePair<string, Register> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(int index)
        {
            return registers.ContainsKey(index);
        }

        public bool ContainsKey(string name)
        {
            return names.ContainsKey(name);
        }

        public bool Remove(int index)
        {
            if (!ContainsKey(index))
                return false;
            var delNames = names.Where((x) => x.Value == index);
            foreach (var item in delNames)
            {
                names.Remove(item.Key);
            }
            registers.Remove(index);
            return true;
        }

        public bool Remove(string name)
        {
            if (!ContainsKey(name))
                return false;
            int index = names[name];
            registers.Remove(index);
            names.Remove(name);
            return true;
        }
    }
}
