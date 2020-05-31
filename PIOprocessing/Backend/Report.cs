using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PIOprocessing {
    
    
    public struct Spot
    {
        public string Action;
        public string AggPos;
        public string CllPos;
        public string BoardType;
        public string BoardSubtype;
    }
    
    public class Report
    {
        public static bool useRelativeBetsizes = true;
        protected string filePath;
        protected Spot spot;
        protected bool resolvedSpot;
        protected int stacksize;
        public int Stacksize { get { return stacksize; } set { stacksize = value; DeterminePotsize(); } }
        protected int potsize;
        public int Potsize { get { return potsize; } }


        public Spot Spot { get { return spot; } }
        public bool ResolvedSpot { get { return resolvedSpot; } }

        protected List<string[]> data;
        protected string[] headers;
        protected List<int> freqColumns;
        protected List<string> freqLabels;
        protected List<Hand> hands;
        public List<Hand> Hands
        {
            get
            {
                if(!isLoaded)
                {
                    loadData();
                    isLoaded = true;
                }
                return hands;
            }
        }

        private SortedDictionary<HandCategory,HashSet<HandType>> handCategories;

        public SortedDictionary<HandCategory,HashSet<HandType>> HandCategories
        {
            get
            {
                if (!isLoaded)
                {
                    loadData();
                }
                return handCategories;
            }
        }

        protected SortedDictionary<HandType,HandGroup> handTypes;
        public SortedDictionary<HandType,HandGroup> HandTypes
        {
            get
            {
                if(!isLoaded)
                {
                    loadData();
                }
                return handTypes;
            }
        }

        public HandGroup GetHandGroup(HandType type)
        {
            if(!isLoaded)
            {
                loadData();
            }
            return handTypes[type];
        }
        protected bool isLoaded;
        public string FilePath { get {return filePath;}}
        public List<string[]> Data
        {
            get
            {
                if(!isLoaded)
                {
                    loadData();
                }
            return data;
            }
        }

        public Report(string filePath)
        {
            this.filePath = filePath;
            isLoaded = false;
            data = new List<string[]>();
            hands = new List<Hand>();
            freqColumns = new List<int>();
            handCategories = new SortedDictionary<HandCategory, HashSet<HandType>>();
            handTypes = new SortedDictionary<HandType, HandGroup>();
            resolvedSpot = determineSpot();
        }


        public string GetReportDirectory() {
            return (new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath))).Name;
        }

        public void GenerateHandsFile(string directoryPath) {
            if(!isLoaded)
            {
               loadData();
            }
            OutputFile file = new OutputFile(directoryPath + "\\" + GetReportDirectory().Replace(" ", "_") + ".csv");
            // write column headers: Flop, Hand, Weight, Category, Type, Strengthlbl, Strengthorder, Frequencylabels, 
            string[] outputHeaders = new string[7 + freqColumns.Count];
            outputHeaders[0] = "Flop";
            outputHeaders[1] = "Hand";
            outputHeaders[2] = "Weight";
            outputHeaders[3] = "Strength Category";
            outputHeaders[4] = "Strength Type";
            outputHeaders[5] = "Strength label";
            outputHeaders[6] = "Strength order";
            int index = 7;
            foreach(int freqColumn in freqColumns) {
                outputHeaders[index] = headers[freqColumn];
                index++;
            }

            file.writeCsvLine(outputHeaders);

            foreach(Hand hand in Hands) {
                string[] outputLine = new string[7 + hand.Frequencies.Count];
                outputLine[0] = hand.FlopText;
                outputLine[1] = hand.HoleCardsText;
                outputLine[2] = hand.Weight.ToString();
                outputLine[3] = hand.Strength.Category.ToString();
                outputLine[4] = hand.Strength.Type.ToString();
                outputLine[5] = hand.Strength.StrengthLabel;
                outputLine[6] = hand.Strength.StrengthOrder.ToString();
                index = 7;
                foreach(KeyValuePair<string,float> frequency in hand.Frequencies) {
                    outputLine[index] = frequency.Value.ToString();
                    index++;
                }
                file.writeCsvLine(outputLine);
            }

            // write data
        }

        private void DeterminePotsize()
        {
            // Console.WriteLine("Starting stacks: " + Properties.Settings.Default.StartingStacks);
            potsize = (Properties.Settings.Default.StartingStacks - stacksize) * 2;
            if(spot.AggPos != "BB" && spot.CllPos != "BB")
            {
                potsize += Properties.Settings.Default.BigBlind;
            }
            if (spot.AggPos != "SB" && spot.CllPos != "SB")
            {
                potsize += Properties.Settings.Default.SmallBlind;
            }
            // Console.WriteLine($"{GetReportDirectory()} potsize: {potsize}, stacksize: {stacksize}");
        }

        private bool determineSpot()
        {
            string folderName = GetReportDirectory();
            string[] splitName = folderName.Split('_');
            if(splitName.Length < 4)
            {
                // foldername incorrect!
                return false;
            }
            string[] positions = splitName[2].Split("vs".ToCharArray());
            string boardSubtype = "";
            for (int i = 4; i < splitName.Length; i++)
            {
                boardSubtype += splitName[i];
            }
            spot = new Spot { Action = splitName[1], AggPos = positions[0], CllPos = positions[2], BoardType = splitName[3], BoardSubtype = boardSubtype };
            return true;
        }

        private string getFrequencyLabel(string frequencyHeader)
        {
            if (Properties.Settings.Default.RelativeBetsize && potsize != 0) { 
                // NumberFormatInfo setPrecision = new NumberFormatInfo();
                // setPrecision.NumberDecimalDigits = 2;
                string[] splitLabel = frequencyHeader.Split(' ');
                if (splitLabel.Length > 2 && !splitLabel[1].EndsWith("%"))
                {
                    double betPercentage = (double.Parse(splitLabel[1]) / (double)potsize) * 100;
                    if((int)betPercentage < 2)
                    {
                        return frequencyHeader;
                    }
                    // int betPercentage = (int)(double.Parse(splitLabel[1]) / potsize) * 100;
                    return $"Bet {(int)betPercentage}%";
                }
                else
                {
                    if (splitLabel[0] == "CHECK")
                        return "Check";
                    else
                        return frequencyHeader;
                }
            } else
            {
                return frequencyHeader;
            }

        }
        
        // Reads in the data from the CSV file and parses it to the Data collection
        protected void loadData()
        {
            DeterminePotsize();
            freqLabels = new List<string>();
            using (var file = new FileStream (@filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var reader = new StreamReader(file))
            {
                staticTimer.start("ReportReading");
                if (!reader.EndOfStream) {
                    headers = reader.ReadLine().Split(',');
                    for(int i=0; i < headers.Length; i++) {
                        if(headers[i].EndsWith("Freq")) {
                            freqColumns.Add(i);
                            freqLabels.Add(getFrequencyLabel(headers[i]));
                            
                        }
                    }
                }
                staticTimer.stop("ReportReading");

                int rownumber = 2;
                while (!reader.EndOfStream)
                {
                    staticTimer.start("ReportReading");
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    data.Add(values);
                    Dictionary<string,float> frequencies = new Dictionary<string, float>();

                    int i = 0;
                    foreach(int freqColumn in freqColumns) {
                        frequencies.Add(freqLabels[i],float.Parse(values[freqColumn]));
                        i++;
                    }
                    Hand hand = new Hand(values[Mappings.ReportColumns["Flop"]],values[Mappings.ReportColumns["Hand"]],float.Parse(values[Mappings.ReportColumns["Weight"]]),frequencies,rownumber++);
                    hands.Add(hand);
                    staticTimer.stop("ReportReading");
                    HandGroup group;
                    if(handTypes.TryGetValue(hand.Strength.Type,out group)) {
                        group.addHand(hand);
                    } else {
                        HandGroup newGroup = new HandGroup(spot,hand.Strength.Category,hand.Strength.Type);
                        newGroup.addHand(hand);
                        handTypes.Add(hand.Strength.Type,newGroup);
                    }

                    HashSet<HandType> typeSet;
                    if(handCategories.TryGetValue(hand.Strength.Category,out typeSet))
                    {
                        typeSet.Add(hand.Strength.Type);
                    } else {
                        HashSet<HandType> newSet = new HashSet<HandType>();
                        newSet.Add(hand.Strength.Type);
                        handCategories.Add(hand.Strength.Category, newSet);
                    }
                }
            }
            staticTimer.log("ReportReading");
            staticTimer.log("HandStrength");

            isLoaded = true;
        }
    }
}