using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Timers;

namespace PIOprocessing
{
    class staticTimer
    {


        static Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>() {
            { "HandStrength", new Stopwatch() },
            { "ReportScanning", new Stopwatch() },
            { "ReportReading", new Stopwatch() },
            { "Total", new Stopwatch() } };

        public static void start(string timerName)
        {
            timers[timerName].Start();
        }

        public static void stop(string timerName)
        {
            timers[timerName].Stop();
        }
        public static void log(string timerName)
        {
            Console.WriteLine(timerName + " time: " + timers[timerName].Elapsed);
        }

        public static void reset()
        {
            foreach(Stopwatch sw in timers.Values)
            {
                sw.Reset();
            }
        }
    }
    class Timer
    {
        private Stopwatch sw;
        private string subject;
        
        public Timer(string subject) {
            sw = new Stopwatch();
            this.subject = subject;
        }

        public void start() {
            sw.Start();
        }

        public void stop() {
            sw.Stop();
        }

        public void log() {
            Console.WriteLine(subject + " time: " + sw.Elapsed);    
        }

        public void logCalculated(TimeSpan substract) {
            Console.WriteLine(subject + " time: " + (sw.Elapsed - substract));  
        }

        public TimeSpan getElapsed() {
            return sw.Elapsed;
        }
    }
}