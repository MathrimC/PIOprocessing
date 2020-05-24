using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIOprocessing.Models
{
    class HandModel
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public int StrengthOrder { get; set; }
        public string StrengthLabel { get; set; }
        public string Flop { get; set; }
        public string Holecards { get; set; }
        public float Weight { get; set; }
        
        public List<float> Frequencies { get; set; }

        public HandModel(Hand hand)
        {
            Category = hand.Strength.Category.ToString();
            Type = hand.Strength.Type.ToString();
            StrengthOrder = hand.Strength.StrengthOrder;
            StrengthLabel = hand.Strength.StrengthLabel;
            Flop = hand.FlopText;
            Holecards = hand.HoleCardsText;
            Weight = hand.Weight;
            Frequencies = new List<float>();
            //int index = 0;
            foreach(KeyValuePair<string,float> frequencySpec in hand.Frequencies)
            {
                Frequencies.Add(frequencySpec.Value);
                //index++;
            }
        }
    }
}
