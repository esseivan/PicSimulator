using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    /// <summary>
    /// List of registers accessible from an index (address) or a name
    /// </summary>
    public class RegisterList
    {
        private Dictionary<string, int> namesToIndexes = new Dictionary<string, int>();
        private Dictionary<int, Register> indexesToRegisters = new Dictionary<int, Register>();

        public Register this[int key] { get => indexesToRegisters[key]; set => indexesToRegisters[key] = value; }
        public Register this[string key] { get => indexesToRegisters[namesToIndexes[key]]; set => indexesToRegisters[namesToIndexes[key]] = value; }

        public ICollection<int> Indexes => indexesToRegisters.Keys;
        public ICollection<Register> Registers => indexesToRegisters.Values;
        public ICollection<string> Names => namesToIndexes.Keys;

        public void Add(int index, Register value, string name)
        {
            if (ContainsKey(index))
                throw new ArgumentException($"Duplicate entry for 'index' : '{index}'");
            if (ContainsKey(name))
                throw new ArgumentException($"Duplicate entry for 'name' : '{name}'");

            indexesToRegisters[index] = value;
            namesToIndexes[name] = index;
        }

        public Register GetRegister(int index)
        {
            if (ContainsKey(index))
                return this[index];
            return null;
        }

        public Register GetRegister(string name)
        {
            if (ContainsKey(name))
                return this[name];
            return null;
        }

        public void SetRegister(int index, Register register)
        {
            this[index] = register;
        }

        public void SetRegister(string name, Register register)
        {
            if (!ContainsKey(name))
                throw new KeyNotFoundException($"'{name}' is not present in the register list. You need to manually add it first");
            this[name] = register;
        }

        public void Clear()
        {
            namesToIndexes.Clear();
            indexesToRegisters.Clear();
        }

        public bool ContainsKey(int index)
        {
            return indexesToRegisters.ContainsKey(index);
        }

        public bool ContainsKey(string name)
        {
            return namesToIndexes.ContainsKey(name);
        }

        public bool Remove(int index)
        {
            if (!ContainsKey(index))
                return false;
            var delNames = namesToIndexes.Where((x) => x.Value == index);
            if (delNames.Count() > 0)
            {
                if (delNames.Count() > 1)
                    throw new NotImplementedException();
                else
                    namesToIndexes.Remove(delNames.First().Key);
            }
            indexesToRegisters.Remove(index);
            return true;
        }

        public bool Remove(string name)
        {
            if (!ContainsKey(name))
                return false;
            int index = namesToIndexes[name];
            indexesToRegisters.Remove(index);
            namesToIndexes.Remove(name);
            return true;
        }

        public void ApplySyncs()
        {
            var toSync = Registers.Where((x) => x.SyncToAddr != Register.SyncToAddr_None).ToArray();
            foreach (var item in toSync)
            {
                Register sync = GetRegister(item.SyncToAddr);
                if (sync == null) continue;
                item.SyncToAddr = Register.SyncToAddr_None;
                item.SyncRegister = sync;
            }
        }
    }
}
