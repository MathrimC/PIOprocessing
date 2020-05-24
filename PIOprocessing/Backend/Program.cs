using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace PIOprocessing
{
   
    class Program
    {
        /*
        static void Main(string[] args)
        {
            TestAverages();

        }
        */

        static string testOutputDir = "E:\\PIOprocessing\\TestOutput\\";
        static void TestAverages() {
            Timer totalTimer = new Timer("Total execution");
            Timer parseTimer = new Timer("Parsing CSV content");
            Timer strengthTimer = new Timer("Calculating handstrength");
            Timer outputTimer = new Timer("Writing output");
            totalTimer.start();
            ReportReader reader = new ReportReader("E:\\PIOprocessing\\Reports");
            outputTimer.start();
            OutputFile testOutput = new OutputFile("E:\\PIOprocessing\\test.csv");
            outputTimer.stop();
            foreach(Report report in reader.Reports) {
                // outputTimer.start();
                report.GenerateHandsFile(testOutputDir);
                // outputTimer.stop();
                string reportName = report.GetReportDirectory();
                foreach(KeyValuePair<HandType,HandGroup> handType in report.HandTypes) {

                    foreach(string freqLabel in handType.Value.GetFrequencyLabels()) {
                        FrequencyValues freqValues = handType.Value.GetFrequencyValues(freqLabel);
                        string[] outputLine = new string[freqValues.GetHandStrengthOrders().Length + 3];
                        outputLine[0] = reportName;
                        outputLine[1] = handType.Key.ToString();
                        outputLine[2] = freqLabel;
                        int index = 3;
                        foreach(int order in freqValues.GetHandStrengthOrders()) {
                            outputLine[index] = freqValues.GetFrequency(order).ToString();
                            index++;
                        }
                        testOutput.writeCsvLine(outputLine);

                    }
                }
            }
            totalTimer.stop();
            totalTimer.log();
        }
        static void Test() {
            Timer totalTimer = new Timer("Total execution");
            Timer parseTimer = new Timer("Parsing CSV content");
            Timer strengthTimer = new Timer("Calculating handstrength");
            Timer outputTimer = new Timer("Writing output");
            totalTimer.start();
            parseTimer.start();
            ReportReader reader = new ReportReader("E:\\PIOprocessing\\Reports");
            outputTimer.start();
            OutputFile testOutput = new OutputFile("E:\\PIOprocessing\\test.csv");
            outputTimer.stop();
            List<int> counts = new List<int>();
            foreach(Report report in reader.Reports) {
                foreach(Hand hand in report.Hands) {
                    strengthTimer.start();
                    HandStrength handStrength = hand.Strength;
                    strengthTimer.stop();
                    outputTimer.start();
                    string[] output = {hand.GetText(),handStrength.Category.ToString(),handStrength.Type.ToString(),handStrength.StrengthLabel.ToString()}; 
                    testOutput.writeCsvLine(output);
                    //Console.WriteLine($"{hand.GetText()}{handStrength.Category}");
                    outputTimer.stop();
                }
                counts.Add(report.Hands.Count);
                //Console.WriteLine(report.FilePath);
                //Console.WriteLine("Amount of rows: " + report.Data.Count);
                //Console.WriteLine("First hand test: " + report.Hands[0].GetText());
            }
            parseTimer.stop();
            int handcount = 0;
            foreach(int count in counts) {
                handcount += count;
            }
            Console.WriteLine($"{handcount} hand objects created from {reader.Reports.Count} report files");
            totalTimer.stop();
            parseTimer.logCalculated(strengthTimer.getElapsed() + outputTimer.getElapsed());
            strengthTimer.log();
            outputTimer.log();
            totalTimer.log();

        }

    }




}
