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

        #region Config

        /// <summary>
        /// MCU name
        /// </summary>
        public abstract string mcuName { get; }
        /// <summary>
        /// Reset vector
        /// </summary>
        public virtual short ResetVector => 0;
        /// <summary>
        /// Interrupt vector
        /// </summary>
        public virtual short InterruptVector => 4;
        /// <summary>
        /// Number of bits in address
        /// </summary>
        public abstract byte addrBits { get; }
        /// <summary>
        /// Number of bits in bank
        /// </summary>
        public abstract byte bankBits { get; }
        /// <summary>
        /// Amount of registers per bank
        /// </summary>
        public abstract short bankLength { get; }
        /// <summary>
        /// Amount of banks
        /// </summary>
        public abstract byte bankCount { get; }

        #endregion

        #region HW

        /// <summary>
        /// Data memory (registers)
        /// </summary>
        public Register[] Data => data;
        /// <summary>
        /// Status register
        /// </summary>
        public StatusRegister Status => (StatusRegister)GetRegister("STATUS");
        /// <summary>
        /// Name to register links
        /// </summary>
        public Dictionary<string, short> RegistersNames => registersNames;
        /// <summary>
        /// Program memory
        /// </summary>
        public InstructionSet Program => program;
        /// <summary>
        /// IOs
        /// </summary>
        public IO[] GPIOs => gpios;
        /// <summary>
        /// Work register
        /// </summary>
        public abstract byte WReg { get; set; }
        /// <summary>
        /// Program counter
        /// </summary>
        public short PC
        {
            get
            {
                return (short)(PCL + PCLATH << 8);
            }
            set
            {
                byte valL = (byte)(value & 0xFF);
                byte valH = (byte)(value >> 8);
                PCLATH = valH;
                PCL = valL;
            }
        }
        /// <summary>
        /// Program counter Low byte
        /// </summary>
        public abstract byte PCL { get; set; }
        /// <summary>
        /// Program counter LAT High byte
        /// </summary>
        public abstract byte PCLATH { get; set; }
        /// <summary>
        /// Stack memory
        /// </summary>
        public short[] Stack => stack;
        /// <summary>
        /// Next instruction to be executed
        /// </summary>
        public Instruction NextInstruction => program[PC];

        /// <summary>
        /// Increment the program counter
        /// </summary>
        public void IncrementPC() => PC++;

        /// <summary>
        /// User ID registers
        /// </summary>
        public abstract short[] UserID { get; }

        /// <summary>
        /// Configuration words registers
        /// </summary>
        public abstract short[] ConfigurationWords { get; }

        /// <summary>
        /// Device ID Register
        /// </summary>
        public abstract short DeviceID { get; }

        /// <summary>
        /// Configuration memory registers
        /// </summary>
        public short[] ConfigurationMemory => configurationMemory;

        /// <summary>
        /// Selected bank
        /// </summary>
        public abstract byte SelectedBank { get; set; }

        #endregion

        #region Privates

        /// <summary>
        /// Data memory (registers)
        /// </summary>
        protected Register[] data;
        /// <summary>
        /// Name to register links
        /// </summary>
        protected Dictionary<string, short> registersNames = new Dictionary<string, short>();
        /// <summary>
        /// Program memory
        /// </summary>
        protected InstructionSet program = new InstructionSet();
        /// <summary>
        /// IOs
        /// </summary>
        protected IO[] gpios;
        /// <summary>
        /// Stack memory
        /// </summary>
        protected short[] stack;

        /// <summary>
        /// Selected bank, from 1 to bankCount
        /// </summary>
        protected byte selectedBank = 0;

        /// <summary>
        /// All configuration memory registers
        /// </summary>
        protected short[] configurationMemory;

        #endregion

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
                    if (registersNames.ContainsKey(regName))
                        throw new InvalidOperationException($"Key already exists : '{regName}'");
                    registersNames[regName] = regDest;
                    if (regName == "STATUS")
                    {
                        data[regDest] = GenerateStatusRegister(data[regDest]);
                    }
                }
                else if (line.StartsWith(microchip_decoder_register))
                {
                    thisLine = true;
                }
            }
        }

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

        protected virtual Register GenerateStatusRegister(Register baseRegister)
        {
            return StatusRegister.CopyFrom(baseRegister);
        }

        #endregion

        #region Decoding

        public Dictionary<long, Instruction> Decode(string file) => _decode(HexFileDecoder.GetInstructions(file));

        public Dictionary<long, Instruction> Decode(Dictionary<long, short> data) => _decode(HexFileDecoder.GetInstructions(data));

        protected abstract Dictionary<long, Instruction> _decode(Dictionary<long, Instruction> data);

        #endregion

        #region Addressing

        /// <summary>
        /// Get a register from the name
        /// </summary>
        public Register GetRegister(string name)
        {
            name = name.ToUpper();
            if (registersNames.ContainsKey(name))
                return data[registersNames[name]];
            return null;
        }

        /// <summary>
        /// Get a register from relative address and selected bank
        /// </summary>
        public Register GetRegister(short addrRel)
        {
            return data[GetAbsAddr(addrRel, selectedBank)];
        }

        /// <summary>
        /// Get a register from relative address and specified bank
        /// </summary>
        public Register GetRegister(short addrRel, byte bank)
        {
            return data[GetAbsAddr(addrRel, bank)];
        }

        /// <summary>
        /// Return a register from absolute address
        /// </summary>
        public Register GetRegisterAbs(int addrAbs)
        {
            return data[addrAbs];
        }

        /// <summary>
        /// Get the bank from the address
        /// </summary>
        public int GetBank(short absAddr)
        {
            return (absAddr >> addrBits) & (int)(Math.Pow(2, bankBits) - 1);
        }

        /// <summary>
        /// Get the absolute address from relative address and bank
        /// </summary>
        public int GetAbsAddr(short relAddr, byte bank)
        {
            return (int)((bank >> addrBits) + relAddr);
        }

        /// <summary>
        /// Get the bank relative address from the absolute address
        /// </summary>
        public int GetRelAddr(int absAddr)
        {
            return absAddr & (int)(Math.Pow(2, addrBits) - 1);
        }

        #endregion
    }
}
