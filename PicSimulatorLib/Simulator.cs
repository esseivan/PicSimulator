using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Simulator
    {
        private MCU _mcu;
        public MCU MCU => _mcu;

        public Status status = Status.Stopped;

        public enum Status
        {
            Stopped,
            Started,
        }

        public Simulator(MCU mcu)
        {
            _mcu = mcu;
        }

        public void Start()
        {
            _mcu.PC = _mcu.ResetVector;
        }

        public void Step()
        {
            if (status == Status.Stopped)
                throw new Exception("Simulation not started");

            ProcessInstruction();
        }

        private void ProcessInstruction()
        {
            Instruction ins = _mcu.NextInstruction;

            // Check for interrupt

            byte f = (byte)ins.Parameter1;
            byte dTemp = (byte)ins.Parameter2;
            byte b = (byte)ins.Parameter2;
            byte value = 0, value2 = 0, value3 = 0;
            bool carry;
            byte k = f;
            if (dTemp < 0 || dTemp > 1)
                throw new ArgumentOutOfRangeException("d");
            bool d = dTemp == 1;
            switch (ins.Code)
            {
                case Instruction.InstructionCode.ADDWF:
                    value = (byte)(_mcu.WReg + _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ADDWFC:
                    value = (byte)(_mcu.WReg + _mcu.Data[f].Value + (_mcu.Status.Value & 1));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ANDWF:
                    value = (byte)(_mcu.WReg & _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ASRF:
                    carry = (_mcu.Data[f].Value & 0x01) > 0;
                    value2 = (byte)(_mcu.Data[f].Value & 0x80);
                    value = (byte)((_mcu.Data[f].Value >> 1) | value2);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.LSLF:
                    carry = (_mcu.Data[f].Value & 0x80) > 0;
                    value = (byte)(_mcu.Data[f].Value << 1);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.LSRF:
                    carry = (_mcu.Data[f].Value & 0x01) > 0;
                    value = (byte)(_mcu.Data[f].Value >> 1);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.CLRF:
                    value = (byte)(_mcu.Data[f].Value & 0x7F);
                    Register reg = _mcu.GetRegister(value, _mcu.SelectedBank);
                    reg.SetValue(0);
                    break;

                case Instruction.InstructionCode.CLRW:
                    _mcu.WReg = 0;
                    break;

                case Instruction.InstructionCode.COMF:
                    value = (byte)(~_mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.DECF:
                    value = _mcu.Data[f].Value--;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.INCF:
                    value = _mcu.Data[f].Value++;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.IORWF:
                    value = (byte)(_mcu.WReg | _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVF:
                    value = _mcu.Data[f].Value;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVWF:
                    _mcu.Data[f].Value = _mcu.WReg;
                    break;

                case Instruction.InstructionCode.RLF:
                    carry = (_mcu.Data[f].Value & 0x80) > 0;
                    value = (byte)((_mcu.Data[f].Value << 1) | (_mcu.Status.C ? 0x01 : 0));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.RRF:
                    carry = (_mcu.Data[f].Value & 0x01) > 0;
                    value = (byte)((_mcu.Data[f].Value >> 1) | (_mcu.Status.C ? 0x80 : 0));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SUBWF:
                    carry = _mcu.WReg <= _mcu.Data[f].Value;
                    value = (byte)(_mcu.Data[f].Value - _mcu.WReg);
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SUBWFB:
                    carry = _mcu.WReg <= _mcu.Data[f].Value;
                    value = (byte)(_mcu.Data[f].Value - _mcu.WReg - (_mcu.Status.C ? 0 : 1));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SWAPF:
                    value = (byte)((_mcu.Data[f].Value << 4) | (_mcu.Data[f].Value >> 4));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.XORWF:
                    value = (byte)(_mcu.WReg ^ _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.DECFSZ:
                    value = _mcu.Data[f].Value--;
                    SetDestination(value, d, f, false);
                    if (value == 0) // Skip if zero
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.INCFSZ:
                    value = _mcu.Data[f].Value++;
                    SetDestination(value, d, f, false);
                    if (value == 0) // Skip if zero
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.BCF:
                    value = (byte)(_mcu.Data[f].Value | (1 << b));
                    SetDestination(value, d, f, false);
                    break;

                case Instruction.InstructionCode.BSF:
                    value = (byte)(_mcu.Data[f].Value & (0xff - (1 << b)));
                    SetDestination(value, d, f, false);
                    break;

                case Instruction.InstructionCode.BTFSC:
                    value = (byte)(_mcu.Data[f].Value & (0xFF - (1 << b)));
                    if (value == 0)
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.BTFSS:
                    value = (byte)(_mcu.Data[f].Value & (0xFF - (1 << b)));
                    if (value != 0)
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.ADDLW:
                    value = (byte)(_mcu.WReg + k);
                    _mcu.Status.C = value < _mcu.WReg;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.ANDLW:
                    value = (byte)(_mcu.WReg & k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.IORLW:
                    value = (byte)(_mcu.WReg | k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.MOVLB:
                    _mcu.SelectedBank = k;
                    break;

                case Instruction.InstructionCode.MOVLP:
                    _mcu.PCLATH = k;
                    break;

                case Instruction.InstructionCode.MOVLW:
                    _mcu.WReg = k;
                    break;

                case Instruction.InstructionCode.SUBLW:
                    carry = _mcu.WReg <= k;
                    value = (byte)(_mcu.WReg - k);
                    _mcu.Status.C = carry;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.XORLW:
                    value = (byte)(_mcu.WReg ^ k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.BRA:
                    break;

                case Instruction.InstructionCode.BRW:
                    break;

                case Instruction.InstructionCode.CALL:
                    break;
                case Instruction.InstructionCode.CALLW:
                    break;
                case Instruction.InstructionCode.GOTO:
                    break;
                case Instruction.InstructionCode.RETFIE:
                    break;
                case Instruction.InstructionCode.RETLW:
                    break;
                case Instruction.InstructionCode.RETURN:
                    break;
                case Instruction.InstructionCode.CLRWDT:
                    break;
                case Instruction.InstructionCode.NOP:
                    break;
                case Instruction.InstructionCode.OPTION:
                    break;
                case Instruction.InstructionCode.RESET:
                    break;
                case Instruction.InstructionCode.SLEEP:
                    break;
                case Instruction.InstructionCode.TRIS:
                    break;
                case Instruction.InstructionCode.ADDFSR:
                    break;
                case Instruction.InstructionCode.MOVIW:
                    break;
                case Instruction.InstructionCode.MOVIW2:
                    break;
                case Instruction.InstructionCode.MOVWI:
                    break;
                case Instruction.InstructionCode.MOVWI2:
                    break;
                default:
                    break;
            }

            _mcu.IncrementPC();
        }

        private void SetDestination(byte value, bool d = true, byte f = 0, bool checkZ = true)
        {
            if (checkZ)
                _mcu.Status.Z = value == 0;

            if (d)
                _mcu.Data[f].Value = value;
            else
                _mcu.WReg = value;
        }
    }
}
