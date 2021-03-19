using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Threading.Timer;

namespace PicSimulatorLib
{
    public partial class Simulator : IDisposable
    {
        private MCU _mcu;
        public MCU MCU => _mcu;

        public Status status = Status.Idle;

        private double desiredFrequency = 1;
        private double interval = 1;

        public bool Running => status == Status.AutoRun;

        private Thread threadRun;
        private ThreadStart ts;

        private int tickCount = 0;
        private Timer frequencyTimer;

        private int savedFrequency = 0;
        public int Frequency => savedFrequency;

        public event EventHandler<FrequencyChangeEventArgs> OnFrequencyChanged;
        public event EventHandler OnPaused;

        public enum Status
        {
            Idle,
            AutoRun,
        }

        public Simulator(MCU mcu, int frequency)
        {
            _mcu = mcu;
            SetFrequency(frequency);
            Reset();
            ts = new ThreadStart(ThreadRun);
            TimerCallback tc = new TimerCallback(FrequencyCalibration);
            frequencyTimer = new Timer(tc);
        }

        private void FrequencyCalibration(object o)
        {
            // Change to 10% of error
            double desiredInterval = 1d / desiredFrequency;
            double realInterval = 1d / tickCount;
            double difference = realInterval - desiredInterval;
            double change = difference * 0.2;

            savedFrequency = tickCount;
            OnFrequencyChanged?.Invoke(this, new FrequencyChangeEventArgs(savedFrequency));

            interval -= change;
            tickCount = 0;
        }

        private void ThreadRun()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                // Process
                Step();

                tickCount++;

                if (!Running)
                    break;

                // Wait additionnal time for specified frequency
                // Pros : Precision ;; Cons : CPU usage
                while (sw.ElapsedTicks / interval < Stopwatch.Frequency) ;

                sw.Restart();
            }

            OnPaused?.Invoke(this, EventArgs.Empty);
        }

        public void SetFrequency(double frequency)
        {
            desiredFrequency = frequency;
            if (frequency <= 0 || double.IsNaN(frequency) || double.IsInfinity(frequency))
                throw new ArgumentOutOfRangeException("frequency");

            interval = 1d / desiredFrequency;
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
            status = Status.AutoRun;
            threadRun = new Thread(ts);
            threadRun.Start();
            frequencyTimer.Change(1000, 1000);
        }

        public void Stop()
        {
            frequencyTimer.Change(Timeout.Infinite, Timeout.Infinite);
            status = Status.Idle;
            _mcu.NextInstruction.IsNext = true;
        }

        public void Step()
        {
            ProcessInstruction(_mcu.NextInstruction);
        }

        private void ProcessInstruction(Instruction ins)
        {

            // Check for interrupt


            byte f = (byte)ins.Parameter1;
            byte b = (byte)ins.Parameter2;
            bool d = b == 1;
            bool carry;
            byte value;
            bool incrementPC = true;
            switch (ins.Code)
            {
                case Instruction.InstructionCode.ADDWF:
                    value = (byte)(_mcu.wreg + _mcu.Data[_mcu.Bank, f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ADDWFC:
                    value = (byte)(_mcu.wreg + _mcu.Data[_mcu.Bank, f].Value + (_mcu.Status.Value & 1));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ANDWF:
                    value = (byte)(_mcu.wreg & _mcu.Data[_mcu.Bank, f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.ASRF:
                    carry = (_mcu.Data[_mcu.Bank, f].Value & 0x01) > 0;
                    byte value2 = (byte)(_mcu.Data[_mcu.Bank, f].Value & 0x80);
                    value = (byte)((_mcu.Data[_mcu.Bank, f].Value >> 1) | value2);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.LSLF:
                    carry = (_mcu.Data[_mcu.Bank, f].Value & 0x80) > 0;
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value << 1);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.LSRF:
                    carry = (_mcu.Data[_mcu.Bank, f].Value & 0x01) > 0;
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value >> 1);
                    _mcu.Status.C = carry; // Carry flag
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.CLRF:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value & 0x7F);
                    Register reg = _mcu.Data[_mcu.Bank, value];
                    reg.SetValue(0);
                    break;

                case Instruction.InstructionCode.CLRW:
                    _mcu.wreg = 0;
                    break;

                case Instruction.InstructionCode.COMF:
                    value = (byte)(~_mcu.Data[_mcu.Bank, f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.DECF:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value - 1);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.INCF:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value + 1);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.IORWF:
                    value = (byte)(_mcu.wreg | _mcu.Data[_mcu.Bank, f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVF:
                    value = _mcu.Data[_mcu.Bank, f].Value;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.MOVWF:
                    _mcu.Data[_mcu.Bank, f].Value = _mcu.wreg;
                    break;

                case Instruction.InstructionCode.RLF:
                    carry = (_mcu.Data[_mcu.Bank, f].Value & 0x80) > 0;
                    value = (byte)((_mcu.Data[_mcu.Bank, f].Value << 1) | (_mcu.Status.C ? 0x01 : 0));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.RRF:
                    carry = (_mcu.Data[_mcu.Bank, f].Value & 0x01) > 0;
                    value = (byte)((_mcu.Data[_mcu.Bank, f].Value >> 1) | (_mcu.Status.C ? 0x80 : 0));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SUBWF:
                    carry = _mcu.wreg <= _mcu.Data[_mcu.Bank, f].Value;
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value - _mcu.wreg);
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SUBWFB:
                    carry = _mcu.wreg <= _mcu.Data[_mcu.Bank, f].Value;
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value - _mcu.wreg - (_mcu.Status.C ? 0 : 1));
                    _mcu.Status.C = carry;
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.SWAPF:
                    value = (byte)((_mcu.Data[_mcu.Bank, f].Value << 4) | (_mcu.Data[_mcu.Bank, f].Value >> 4));
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.XORWF:
                    value = (byte)(_mcu.wreg ^ _mcu.Data[_mcu.Bank, f].Value);
                    SetDestination(value, d, f, true);
                    break;

                case Instruction.InstructionCode.DECFSZ:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value - 1);
                    SetDestination(value, d, f, false);
                    if (value == 0) // Skip if zero
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.INCFSZ:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value + 1);
                    SetDestination(value, d, f, false);
                    if (value == 0) // Skip if zero
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.BCF:
                    _mcu.Data[_mcu.Bank, f].Value &= (byte)(0xff - (1 << b));
                    break;

                case Instruction.InstructionCode.BSF:
                    var regt = _mcu.Data[_mcu.Bank, f];
                    _mcu.Data[_mcu.Bank, f].Value |= (byte)(1 << b);
                    break;

                case Instruction.InstructionCode.BTFSC:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value & (0xFF - (1 << b)));
                    if (value == 0)
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.BTFSS:
                    value = (byte)(_mcu.Data[_mcu.Bank, f].Value & (1 << b));
                    if (value != 0)
                        _mcu.IncrementPC();
                    break;

                case Instruction.InstructionCode.ADDLW:
                    value = (byte)(_mcu.wreg + f);
                    _mcu.Status.C = value < _mcu.wreg;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.ANDLW:
                    value = (byte)(_mcu.wreg & f);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.IORLW:
                    value = (byte)(_mcu.wreg | f);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.MOVLW:
                    _mcu.wreg = f;
                    break;

                case Instruction.InstructionCode.SUBLW:
                    carry = _mcu.wreg <= f;
                    value = (byte)(_mcu.wreg - f);
                    _mcu.Status.C = carry;
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.XORLW:
                    value = (byte)(_mcu.wreg ^ f);
                    SetDestination(value, false, f, true);
                    break;

                case Instruction.InstructionCode.BRA:
                    _mcu.PC += f;
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
            if (status != Status.AutoRun)
                _mcu.NextInstruction.IsNext = true;

            // Check for Breakpoint
            if (_mcu.NextInstruction.BreakPointSet && status == Status.AutoRun)
            {
                Stop();
                return;
            }
        }

        private void SetDestination(byte value, bool d = true, byte f = 0, bool checkZ = true)
        {
            if (checkZ)
                _mcu.Status.Z = value == 0;

            if (d)
                _mcu.Data[_mcu.Bank, f].Value = value;
            else
                _mcu.wreg = value;
        }

        public void Dispose()
        {
            status = Status.Idle;
            threadRun?.Join();
        }
    }
}
