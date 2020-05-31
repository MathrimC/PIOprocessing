using System.Collections.Generic;
using System.IO;
using System;

namespace PIOprocessing {
    public class Hand
    {
        protected int csvRowNumber;
        public int CsvRowNumber { get { return csvRowNumber; } }

        public Comparison<Card> CardComp = new Comparison<Card>((x, y) => x.RankNumber - y.RankNumber);
        protected Card[] flopCards;
        public Card[] FlopCards {get {return flopCards;}}
        protected string flopText;
        public string FlopText {get { return flopText;}}
        protected Card[] holeCards;
        public Card[] HoleCards {get {return holeCards;}}
        protected string holeCardsText;
        public string HoleCardsText {get {return holeCardsText;}}
        protected float weight;
        public float Weight {get {return weight;}}
        protected Dictionary<string,float> frequencies;
        public Dictionary<string,float> Frequencies {get {return frequencies;}}
        protected HandStrength strength;
        public HandStrength Strength {
            get {
                if(!strengthCalculated){
                    staticTimer.start("HandStrength");
                    HandStrengthCalculator calc = new HandStrengthCalculator (this);
                    strength = calc.HandStrength;
                    strengthCalculated = true;
                    staticTimer.stop("HandStrength");
                }
                return strength;
            }
        }

        protected bool strengthCalculated;

        
        public Hand(string flop, string hand, float weight, Dictionary<string,float> frequencies, int rownumber) {
            flopCards = parseCards(flop,3);
            Array.Sort(flopCards,CardComp);
            holeCards = parseCards(hand,2);
            Array.Sort(holeCards,CardComp);
            this.weight = weight;
            this.frequencies = frequencies;
            strengthCalculated = false;
            flopText = flop;
            holeCardsText = hand;
            csvRowNumber = rownumber;
        }

        public bool isSuited() {
            if(holeCards[0].Suit == holeCards[1].Suit) {
                return true;
            } else {
                return false;
            }
        }

        public bool isPocketPair() {
            if(HoleCards[0].RankNumber == HoleCards[1].RankNumber) {
                return true;
            } else {
                return false;
            }
        }

        public string GetRanksLabel() {
            return holeCards[0].Rank.ToString() + holeCards[1].Rank.ToString();
        }

        public int[] GetRanks()
        {
            return new int[5] { flopCards[0].RankNumber, flopCards[1].RankNumber, flopCards[2].RankNumber, holeCards[0].RankNumber, holeCards[1].RankNumber };
        }

        public string GetText() {
            string handText = "Flop: ";
            foreach(Card flopCard in flopCards) {
                handText += flopCard.GetText();
            }
            handText += ",holecards: ";
            foreach(Card holeCard in holeCards) {
                handText += holeCard.GetText();
            }
            handText += ",weight: " + weight + ",frequencies: ";
            foreach(KeyValuePair<string,float> frequency in frequencies) {
                handText += frequency.ToString() + ", ";
            }
            return handText;
        }


        private Card[] parseCards(string cardString, int cardCount) {
            Card[] cards = new Card[cardCount];
            int cardNumber = 0;
            int position = 0;
            while(position < cardString.Length-1 || cardNumber < cardCount) {
                char rank = cardString[position];
                char suit = cardString[position+1];
                cards[cardNumber] = new Card(rank,suit);
                cardNumber++;
                position += 2;
                while(position < cardString.Length && cardString[position] == ' ') {
                    position++;
                }
            }
            return cards;
        }
    }

    public struct Card {
        public char Rank;
        public char Suit;
        public int RankNumber;

        public Card(char rank, char suit) {
            Rank = rank;
            Suit = suit;
            if(!Mappings.Ranks.TryGetValue(rank, out RankNumber)) {
                throw new ArgumentException("Unknown card rank");
            };
            if(Array.IndexOf(Mappings.Suits,Suit) < 0) {
                throw new ArgumentException("Unknown card suit");
            }
            
        }



        public string GetText() {
            char[] cardText = {Rank,Suit};
            return new string(cardText);
        }
    }

    public struct HandStrength {
        public HandCategory Category;
        public HandType Type;
        public HandSubtype Subtype;
        public string StrengthLabel;
        public int StrengthOrder;

    }
}