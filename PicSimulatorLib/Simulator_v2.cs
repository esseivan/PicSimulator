using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicSimulatorLib
{
    public class Simulator_v2 : IDisposable
    {
        private Thread thdClock;
        private Thread thdSimulator;
        private Thread thdConstraints;
        private Thread thdFrequency;

        private MCU _mcu;
        public MCU MCU => _mcu;

        private double desiredFrequency;

        private bool tick = false;
        private int tickCount = 0;
        private int tickOffset = 0;
        // 0:unset ; 1:incrementing ; 2:decrementing ; 3:OK
        private int offsetMode = 0;
        private bool pauseOffset = false;

        private bool autoRun = false;
        private bool exit = false;

        public bool Running => autoRun;

        public event EventHandler<FrequencyChangeEventArgs> OnFrequencyChanged;

        public Simulator_v2(MCU mcu, double frequency)
        {
            this._mcu = mcu;
            SetFrequency(frequency);

            thdClock = new Thread(new ThreadStart(ThreadClockRun));
            thdSimulator = new Thread(new ThreadStart(ThreadSimulatorRun));
            thdConstraints = new Thread(new ThreadStart(ThreadConstraintsRun));
            thdFrequency = new Thread(new ThreadStart(ThreadFrequencyRun));

            autoRun = false;
            thdClock.Start();
            thdSimulator.Start();
            //thdConstraints.Start();
            thdFrequency.Start();
        }

        public void AutoRun()
        {
            autoRun = true;
        }

        public void Stop()
        {
            autoRun = false;
        }

        public void SetFrequency(double frequency)
        {
            this.desiredFrequency = frequency;
            offsetMode = 0;
        }

        public void ThreadClockRun()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!exit)
            {
                // Paused
                while (!autoRun && !exit) ;

                // Exit condition
                if (exit)
                    return;

                tick = true;

                sw.Restart();

                tickCount++;

                // Wait next tick
                long compareTo = (long)(Stopwatch.Frequency / desiredFrequency) - tickOffset;
                while (sw.ElapsedTicks < compareTo) ;
            }
        }

        public void ThreadSimulatorRun()
        {
            while (true)
            {
                // Paused
                while ((!autoRun || !tick) && !exit) ;

                // Exit condition
                if (exit)
                    return;

                tick = false;

                // work
                //tickCount++;
            }
        }

        public void ThreadConstraintsRun()
        {

        }

        public void ThreadFrequencyRun()
        {
            while (true)
            {
                // Paused
                while (!autoRun && !exit) ;

                // Exit condition
                if (exit)
                    return;

                // Determine offset
                if (offsetMode != 3)
                {
                    pauseOffset = !pauseOffset;
                    if (offsetMode == 0)
                    {
                        pauseOffset = false;
                        tickOffset = 0;
                        if (tickCount < desiredFrequency)
                            offsetMode = 1;
                        else if (tickCount > desiredFrequency)
                            offsetMode = 2;
                    }
                    else if (!pauseOffset)
                    {
                        if (offsetMode == 1)
                        {
                            if (tickCount < desiredFrequency)
                                tickOffset++;
                            else
                                offsetMode++;
                        }
                        else if (offsetMode == 2)
                        {
                            if (tickCount > desiredFrequency)
                                tickOffset--;
                            else
                            {
                                offsetMode++;
                                Console.WriteLine("Final offset : " + tickOffset);
                            }
                        }
                        Console.WriteLine("Offset is : " + tickOffset);
                    }
                }

                OnFrequencyChanged?.Invoke(this, new FrequencyChangeEventArgs(tickCount));

                tickCount = 0;

                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            exit = true;

            Console.Write("Waiting for clock thread...");
            while (thdClock.IsAlive) ;
            Console.Write("Waiting for simulator thread...");
            while (thdSimulator.IsAlive) ;
            Console.Write("Waiting for constraints thread...");
            //while (thdConstraints.IsAlive) ;
            Console.Write("Waiting for frequency calibration thread...");
            while (thdFrequency.IsAlive) ;
            Console.Write("Waiting complete");
        }

        public class FrequencyChangeEventArgs : EventArgs
        {
            public int Frequency;

            public FrequencyChangeEventArgs(int frequency)
            {
                Frequency = frequency;
            }
        }
    }
}
