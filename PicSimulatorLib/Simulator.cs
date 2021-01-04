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

        public Status status = Status.Idle;

        public enum Status
        {
            Idle,
            AutoRun,
        }

        public Simulator(MCU mcu)
        {
            _mcu = mcu;
            Reset();
        }

        public void Reset()
        {
            if (_mcu.NextInstruction != null)
                _mcu.NextInstruction.IsNext = false;
            _mcu.PC = _mcu.Settings.resetVector;
            _mcu.NextInstruction.IsNext = true;
        }

        public void Run()
        {

        }

        public void Stop()
        {

        }

        public void Step()
        {
            if (status != Status.Idle)
                return;

            ProcessInstruction(_mcu.NextInstruction);
        }

        private void ProcessInstruction(Instruction ins)
        {
            // Check for interrupt


            byte f = (byte)ins.Parameter1;
            short k_short = ins.Parameter1;
            byte dTemp = (byte)ins.Parameter2;
            byte b = (byte)ins.Parameter2;
            byte value = 0, value2 = 0, value3 = 0;
            bool carry;
            byte k = f;
            bool d = dTemp == 1;
            bool incrementPC = true;
            switch (ins.Code)
            {
                case Instruction.InstructionCode.ADDWF:
                    value = (byte)(_mcu.wreg + _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ADDWFC:
                    value = (byte)(_mcu.wreg + _mcu.Data[f].Value + (_mcu.Status.Value & 1));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ANDWF:
                    value = (byte)(_mcu.wreg & _mcu.Data[f].Value);
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
                    Register reg = _mcu.Data[_mcu.Bank, value];
                    reg.SetValue(0);
                    break;

                case Instruction.InstructionCode.CLRW:
                    _mcu.wreg = 0;
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
                    value = (byte)(_mcu.wreg | _mcu.Data[f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVF:
                    value = _mcu.Data[f].Value;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVWF:
                    _mcu.Data[f].Value = _mcu.wreg;
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
                    carry = _mcu.wreg <= _mcu.Data[f].Value;
                    value = (byte)(_mcu.Data[f].Value - _mcu.wreg);
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SUBWFB:
                    carry = _mcu.wreg <= _mcu.Data[f].Value;
                    value = (byte)(_mcu.Data[f].Value - _mcu.wreg - (_mcu.Status.C ? 0 : 1));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SWAPF:
                    value = (byte)((_mcu.Data[f].Value << 4) | (_mcu.Data[f].Value >> 4));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.XORWF:
                    value = (byte)(_mcu.wreg ^ _mcu.Data[f].Value);
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
                    value = (byte)(_mcu.wreg + k);
                    _mcu.Status.C = value < _mcu.wreg;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.ANDLW:
                    value = (byte)(_mcu.wreg & k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.IORLW:
                    value = (byte)(_mcu.wreg | k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.MOVLW:
                    _mcu.wreg = k;
                    break;

                case Instruction.InstructionCode.SUBLW:
                    carry = _mcu.wreg <= k;
                    value = (byte)(_mcu.wreg - k);
                    _mcu.Status.C = carry;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.XORLW:
                    value = (byte)(_mcu.wreg ^ k);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.BRA:
                    _mcu.PC += k;
                    break;

                case Instruction.InstructionCode.BRW:
                    _mcu.PC += _mcu.wreg;
                    break;

                case Instruction.InstructionCode.CLRWDT:
                    throw new NotImplementedException(ins.ToString());

                case Instruction.InstructionCode.NOP:
                    break;

                case Instruction.InstructionCode.OPTION:
                    throw new NotImplementedException(ins.ToString());

                case Instruction.InstructionCode.RESET:
                    throw new NotImplementedException(ins.ToString());

                case Instruction.InstructionCode.SLEEP:
                    throw new NotImplementedException(ins.ToString());

                case Instruction.InstructionCode.TRIS:
                    throw new NotImplementedException(ins.ToString());

                case Instruction.InstructionCode.ADDFSR:
                    throw new NotImplementedException(ins.ToString());
                case Instruction.InstructionCode.MOVIW:
                    throw new NotImplementedException(ins.ToString());
                case Instruction.InstructionCode.MOVIW2:
                    throw new NotImplementedException(ins.ToString());
                case Instruction.InstructionCode.MOVWI:
                    throw new NotImplementedException(ins.ToString());
                case Instruction.InstructionCode.MOVWI2:
                    throw new NotImplementedException(ins.ToString());

                // Unable to process with current informations
                case Instruction.InstructionCode.MOVLB:
                //_mcu.Bank = k;
                case Instruction.InstructionCode.MOVLP:
                //_mcu.PCLATH = k;
                case Instruction.InstructionCode.CALL:
                case Instruction.InstructionCode.CALLW:
                case Instruction.InstructionCode.GOTO:
                case Instruction.InstructionCode.RETFIE:
                case Instruction.InstructionCode.RETLW:
                case Instruction.InstructionCode.RETURN:
                default:
                    incrementPC = false;
                    _mcu.DelegateInstruction(ins);
                    break;
            }

            if (incrementPC)
                _mcu.IncrementPC();
            ins.IsNext = false;
            _mcu.NextInstruction.IsNext = true;
        }

        private void SetDestination(byte value, bool d = true, byte f = 0, bool checkZ = true)
        {
            if (checkZ)
                _mcu.Status.Z = value == 0;

            if (d)
                _mcu.Data[f].Value = value;
            else
                _mcu.wreg = value;
        }
    }
}
