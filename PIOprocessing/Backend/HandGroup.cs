using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PIOprocessing {
    
    public enum HandGroupType {HandType}
    public class HandGroup
    {
        protected HandGroupType groupType;
        protected string spot;
        protected HandCategory category;
        protected HandType handType;
        // dictionary of grouped hands, indexed by handstrength order
        protected SortedDictionary<int,List<Hand>> groupedHands;

        // dictionary of hand strength labels, indexed by handstrength order
        protected SortedDictionary<int, string> strengthLabels;
        
        // dictionary of average frequencies, indexed by frequency label (graphline)
        protected Dictionary<string,FrequencyValues> groupedFrequencies;

        public HandGroup(List<Hand> reportHands, HandType type) {
            groupType = HandGroupType.HandType;
            handType = type;
            groupedHands = new SortedDictionary<int,List<Hand>>();
            strengthLabels = new SortedDictionary<int, string>();
            groupedFrequencies = new Dictionary<string,FrequencyValues>();
            filterHands(reportHands, type);
        }
        public HandGroup(HandType type) {
            groupType = HandGroupType.HandType;
            handType = type;
            groupedHands = new SortedDictionary<int,List<Hand>>();
            strengthLabels = new SortedDictionary<int, string>();
            groupedFrequencies = new Dictionary<string,FrequencyValues>();
        }

        public List<int> GetStrengthOrders()
        {
            return groupedHands.Keys.ToList();
        }
        public List<Hand> GetHands(int strengthOrder)
        {
            return groupedHands[strengthOrder];
        }

        public string GetStrengthLabel(int strengthOrder)
        {
            string label;
            if(!strengthLabels.TryGetValue(strengthOrder, out label))
            {
                label = "";
            }
            return label;
        }

        public void addHand(Hand reportHand) {
            
            // add the hand in the grouped hands dictionary (ordered by handstrengthorder)
            List<Hand> hands;
            if(groupedHands.TryGetValue(reportHand.Strength.StrengthOrder,out hands)) {
                hands.Add(reportHand);
            } else {
                List<Hand> newList = new List<Hand>();
                newList.Add(reportHand);
                groupedHands.Add(reportHand.Strength.StrengthOrder,newList);
                strengthLabels.Add(reportHand.Strength.StrengthOrder, reportHand.Strength.StrengthLabel);
            }

            // add the hand frequencies in the frequencyvalues dictionary (ordered by frequencylabel)
            Dictionary<string, float>.KeyCollection freqlabels = reportHand.Frequencies.Keys;
            foreach(string freqLabel in freqlabels ) {
                FrequencyValues frequencyValues;
                if(groupedFrequencies.TryGetValue(freqLabel, out frequencyValues)) {
                    frequencyValues.AddFrequency(reportHand);
                } else {
                    FrequencyValues newFreqValues = new FrequencyValues(freqLabel);
                    newFreqValues.AddFrequency(reportHand);
                    groupedFrequencies.Add(freqLabel,newFreqValues);
                }
            }
        }

        public FrequencyValues GetFrequencyValues(string frequencyLabel) {
            return groupedFrequencies[frequencyLabel];
        }

        public string[] GetFrequencyLabels() {
            string[] frequencyLabels = new string[groupedFrequencies.Count];
            groupedFrequencies.Keys.CopyTo(frequencyLabels,0);
            return frequencyLabels;
        }

        protected void filterHands(List<Hand> reportHands, HandType handType) {
            foreach (Hand reportHand in reportHands) {
                if(reportHand.Strength.Type == handType) {
                    addHand(reportHand);
                }
            }
        }

        protected void createFrequencyValues() {
            groupedFrequencies = new Dictionary<string,FrequencyValues>();

            // iterate over all the handgroups (one handgroup per handstrengthorder)
            foreach(KeyValuePair<int,List<Hand>> handGroup in groupedHands) {
                // iterate over all the hands in the handstrength group
                foreach(Hand hand in handGroup.Value)
                    // iterate over the different betting frequencies for the hand
                    foreach(KeyValuePair<string,float> frequency in hand.Frequencies ) {
                        FrequencyValues freqValues;
                        if(groupedFrequencies.TryGetValue(frequency.Key,out freqValues)) {
                            freqValues.AddFrequency(handGroup.Key,frequency.Value,hand.Weight);
                        } else {
                            FrequencyValues newFreqValues = new FrequencyValues(frequency.Key);
                            newFreqValues.AddFrequency(handGroup.Key,frequency.Value,hand.Weight);
                            groupedFrequencies.Add(frequency.Key,newFreqValues);
                        }
                    }

            }
        }
    }

    public class FrequencyValues {
        protected HandCategory category;
        protected HandType type;
        protected string label;
        
        // dictionaries of frequencies, indexed by the handstrength order (x-axis element)
        protected SortedDictionary<int,float> frequencyTotals;
        protected SortedDictionary<int,float> frequencyWeights;
        protected SortedDictionary<int,float> frequencies;
        public SortedDictionary<int,float> Frequencies
        {
            get
            {
                if(!averageCalculated) {
                    calculateAverage();
                }
                return frequencies;
            }
        }
        protected bool averageCalculated;

        public FrequencyValues(string frequencyLabel) {
            label = frequencyLabel;
            frequencyTotals = new SortedDictionary<int,float>();
            frequencyWeights = new SortedDictionary<int,float>();
            frequencies = new SortedDictionary<int,float>();
            averageCalculated = false;
        }

        public void AddFrequency(Hand hand) {
            AddFrequency(hand.Strength.StrengthOrder,hand.Frequencies[label],hand.Weight);
        }
        public void AddFrequency(int strengthorder, float frequency, float weight) {
            averageCalculated = false;
            float total;
            if(frequencyTotals.TryGetValue(strengthorder,out total)) {
                total += (frequency * weight);
                frequencyTotals[strengthorder] = total;
                frequencyWeights[strengthorder] += weight;
            } else {
                frequencyTotals.Add(strengthorder,frequency * weight);
                frequencyWeights.Add(strengthorder,weight);
            }
        }

        public float GetFrequency(int handStrengthOrder) {
            return frequencyTotals[handStrengthOrder] / frequencyWeights[handStrengthOrder];
        }

        public int[] GetHandStrengthOrders() {
            int[] handStrengthOrders = new int[frequencyTotals.Count];
            frequencyTotals.Keys.CopyTo(handStrengthOrders,0);
            return handStrengthOrders;
        }

        protected void calculateAverage() {
            frequencies.Clear();
            foreach(KeyValuePair<int,float> total in frequencyTotals) {
                frequencies.Add(total.Key,total.Value / frequencyWeights[total.Key]);
            }
            averageCalculated = true;
        }
    }
}