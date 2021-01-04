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
        public Dictionary<string, short> RegistersName
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

        #region Utils

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
                    short regDest = Convert.ToInt16(regDestStr, 16);
                    if (RegistersName.ContainsKey(regName))
                        throw new InvalidOperationException($"Key already exists : '{regName}'");
                    RegistersName[regName] = regDest;
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
