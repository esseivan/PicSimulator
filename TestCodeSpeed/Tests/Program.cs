using System;
using System.Diagnostics;

namespace Tests
{
    class Program
    {
        static void testSpeed()
        {
            Int32 counter = 0;
            Int32 count = 100000000;

            Stopwatch sw = Stopwatch.StartNew();
            while(counter < count)
            {
                counter++;
            }
            sw.Stop();

            double microSec = ((double)sw.ElapsedTicks / Stopwatch.Frequency) * Math.Pow(10,6);
            Console.WriteLine($@"Execution time for count : {count} instructions took : 
{microSec}µs ≈ {sw.ElapsedMilliseconds}ms ≈ {sw.ElapsedMilliseconds / 1000}s");

            double frequency = count / (microSec / Math.Pow(10,6));
            Console.WriteLine($"Estimated frequency is : {EsseivaN.Tools.Tools.DecimalToEngineer(frequency, 3, true)}Hz");
        }

        static void Main(string[] args)
        {
            string t;
            do
            {
                Console.WriteLine("Testing...");

                testSpeed();

                Console.WriteLine("Done");

                t = Console.ReadLine();
            } while (!string.IsNullOrEmpty(t));

            Console.WriteLine("Closing...");
        }
    }
}
