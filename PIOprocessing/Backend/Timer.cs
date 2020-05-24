using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace PIOprocessing
{
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