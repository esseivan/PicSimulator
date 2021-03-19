using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public abstract class MCU
    {
        /// <summary>
        /// Default Microchip proc's files path
        /// </summary>
        public static string MicrochipProcPath = @"C:\Program Files (x86)\Microchip\MPLABX";

        public MCUSettings Settings => settings;
        protected MCUSettings settings;
        public abstract string MCU_Name { get; }

        #region Memory
        protected InstructionSet program;
        protected MemoryMap data;
        protected short[] configuration;
        protected StatusRegister status;
        public InstructionSet Program => program;
        public MemoryMap Data => data;
        public short[] Configuration => configuration;
        public Dictionary<string, long> RegistersName
        {
            get => data.names;
            protected set => data.names = value;
        }
        public virtual Instruction NextInstruction => program[PC];
        public virtual StatusRegister Status => status;
        public abstract byte Bank { get; }
        #endregion

        #region PC
        public abstract short PC { get; set; }
        public virtual void OffsetPC(short offset) => PC += offset;
        public virtual void IncrementPC() => PC++;
        #endregion

        #region Stack
        protected short[] stack;
        public short[] Stack => stack;
        protected abstract byte StackPtr { get; set; }
        public virtual short PopStack()
        {
            if (StackPtr == 0)
                throw new InvalidOperationException("Stack is empty");
            return stack[--StackPtr];
        }

        /// <summary>
        /// Push (PC + 1) to the stack
        /// </summary>
        public virtual void PushStack()
        {
            stack[StackPtr++] = (short)(PC + 1);
            if (StackPtr >= settings.stackCount)
                StackOverflow();
        }

        /// <summary>
        /// Push the specified value to the stack
        /// </summary>
        public virtual void PushStack(short val)
        {
            stack[StackPtr++] = val;
            if (StackPtr >= settings.stackCount)
                StackOverflow();
        }

        public virtual void StackOverflow()
        {
            throw new Exception("Stack Overflow");
        }
        public abstract void Call(short addr);
        #endregion

        #region WREG
        public abstract byte wreg { get; set; }
        #endregion

        #region IOs

        protected IO[] ios;

        public IO[] IOs => ios;

        #endregion

        protected Dictionary<string, List<Constraint>> Constraints = new Dictionary<string, List<Constraint>>();
        protected Dictionary<string, List<string>> AffectConstraints = new Dictionary<string, List<string>>();

        public MCU()
        {
            Register.ApplyConstraints = ApplyConstraints;
        }

        #region Utils

        public byte ApplyConstraints(byte value, Register o)
        {
            // If contraint output
            if (Constraints.ContainsKey(o.Name))
            {
                foreach (var item in Constraints[o.Name])
                {
                    value = item.action(value);
                }
            }

            // If constraint input
            if (AffectConstraints.ContainsKey(o.Name))
            {
                foreach (var item in AffectConstraints[o.name])
                {
                    data[item].ConstraintWork();
                }
            }

            return value;
        }

        public abstract void DelegateInstruction(Instruction ins);

        #region Hidden

        private const string microchip_decoder_register = @"// Register: ";
        public void DefineRegistersNameFromMicrochipFile(string chipName)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            string filePath = GetMicrochipFilePath(chipName);

            string content = File.ReadAllText(filePath).Replace("\r", "");
            string[] lines = content.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            bool thisLine = false;
            foreach (string line in lines)
            {
                if (thisLine)
                {
                    thisLine = false;
                    string[] datas = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string regName = datas[0].ToUpper();
                    string regDestStr = datas[2].Substring(0, datas[2].Length - 1);
                    long regDest = Convert.ToInt32(regDestStr, 16);
                    if (RegistersName.ContainsKey(regName))
                        throw new InvalidOperationException($"Key already exists : '{regName}'");
                    RegistersName[regName] = regDest;
                    data[regDest].name = regName;
                }
                else if (line.StartsWith(microchip_decoder_register))
                {
                    thisLine = true;
                }
            }
            SetStatusRegister();
        }
        public abstract void SetStatusRegister();

        public string GetMicrochipFilePath(string chipName)
        {
            if (!Directory.Exists(MicrochipProcPath))
                throw new DirectoryNotFoundException($"Directory '{MicrochipProcPath}' not found");

            DirectoryInfo di = new DirectoryInfo(MicrochipProcPath);
            var files = di.GetFiles($"{chipName}.inc", SearchOption.AllDirectories);
            if (files.Length == 0)
                return string.Empty;
            if (files.Length == 1)
                return files[0].FullName;

            var list = files.Select((x) => x.FullName).ToList();
            list.Sort();
            return list[0];
        }

        protected virtual StatusRegister GenerateStatusRegister(Register baseRegister)
        {
            return StatusRegister.CopyFrom(baseRegister);
        }

        #endregion

        #region Decoding

        public Dictionary<long, Instruction> Decode(string file) => _decode(HexFileDecoder.GetInstructions(file));

        public Dictionary<long, Instruction> Decode(Dictionary<long, short> data) => _decode(HexFileDecoder.GetInstructions(data));

        protected abstract Dictionary<long, Instruction> _decode(Dictionary<long, Instruction> data);

        #endregion

        #region Sync

        /// <summary>
        /// Set a bank relative sync
        /// </summary>
        public void SetBankRegisterSync(short relAddr)
        {
            for (byte i = 1; i < settings.bankCount; i++)
            {
                data[i, relAddr].SyncToAddr = relAddr;
            }
        }

        /// <summary>
        /// Apply syncs addresses
        /// </summary>
        public void ApplySyncs()
        {
            var toSync = data.registers.Where((x) => x.SyncToAddr != Register.SyncToAddr_None);
            foreach (var item in toSync)
            {
                item.SyncRegister = data[item.SyncToAddr];
                if (item == item.SyncRegister)
                    throw new InvalidOperationException("Cannot sync to itself");
                item.SyncToAddr = Register.SyncToAddr_None;
            }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Get the bank from the address
        /// </summary>
        public int GetBank(short absAddr)
        {
            return (absAddr >> settings.addrBits) & (int)(Math.Pow(2, settings.bankBits) - 1);
        }

        /// <summary>
        /// Get the absolute address from relative address and bank
        /// </summary>
        public int GetAbsAddr(short relAddr, byte bank)
        {
            return (int)((bank >> settings.addrBits) + relAddr);
        }

        /// <summary>
        /// Get the bank relative address from the absolute address
        /// </summary>
        public int GetRelAddr(int absAddr)
        {
            return absAddr & (int)(Math.Pow(2, settings.addrBits) - 1);
        }

        #endregion

        #endregion

    }
}
