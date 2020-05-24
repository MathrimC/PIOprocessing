using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

namespace PIOprocessing {
    
    public enum HandCategory {StraightFlush, Quads, FullHouse, Flush, Straight, ThreeOfAKind, TwoPair, Overpair, TopPair, Underpair, SecondPair, NogroupPP, ThirdPair, LowPair, NutAir, SecondNutAir, RestAir, Unknown}
    public enum HandType {Unknown, 
        NA_S_FD, NA_S_BDFD, NA_O_NFD, NA_O_BDNFD, NA_OESD, NA_OTHER,
        SA_S_FD, SA_S_BDFD, SA_O_NFD, SA_O_BDNFD, SA_OESD, SA_OTHER,
        RA_S_FD_OESD, RA_S_FD_GS, RA_S_FD, RA_S_BDFD, RA_O_FD, RA_OESD, RA_GS, RA_OTHER,
        TP_S_FD, TP_S_BDFD, TP_O_NFD, TP_O_BDNFD, TP_OESD, TP_OTHER,
        SP_S_FD, SP_S_BDFD, SP_O_NFD, SP_O_BDNFD, SP_OESD, SP_OTHER,
        TrP_S_FD, TrP_S_BDFD, TrP_O_NFD, TrP_O_BDNFD, TrP_OESD, TrP_OTHER,
        OP_FD, OP_BDFD, OP_OTHER,
        UP_FD, UP_BDFD, UP_OTHER,
        PP_FD, PP_BDFD, PP_OTHER,
        LP_FD, LP_BDFD, LP_OTHER,
        TwP_BDNFD, TwP,
        SET, TRIPS,
        FL_NUT, FL_2ND, FL_OTHER,
        ST_FD, ST_BDFD, ST_OTHER,
        FULLHOUSE,
        QUADS,
        SF
    }
    public enum HandSubtype {Unkown, OTHER,
        S_FD_OESD, S_FD_GS, S_FD, O_NFD, OESD, S_BDFD, O_BDNFD, GS,
        FD, BDFD}
    enum StraightType {None, Gutshot, OpenEnded, DoubleGutter, Straight}
    enum FlushType {None, SuitedBDFD, OffsuitBDFD, SuitedFD, OffsuitFD, Flush}
    enum StraightRank {Nut, Middle, Bottom}
    enum TwoPairType {TopTwo, TopBottom, BottomTwo}
    enum SetType {Top, Middle, Bottom}
    class HandStrengthCalculator
    {
        // the index is the three sorted flop ranks followed by the two sorted handranks
        // static Dictionary<int[], HandStrengthCalculator> ranksCache = new Dictionary<int[], HandStrengthCalculator>();


        protected Hand hand;
        protected HandStrength handStrength;
        public HandStrength HandStrength {
            get {
                if(!strengthCalculated) {
                    determineHandStrength();
                    strengthCalculated = true;
                }
                return handStrength;
            }
        }

        protected bool strengthCalculated;

        public int[] RankMatches { get { return rankMatches; } }
        public int MatchRank { get { return matchRank; } }
        public int KickerRank { get { return kickerRank; } }
        public int BoardMatches { get { return boardMatches; } }
        public int MatchOrder { get { return matchOrder; } }
        public int AirOrder { get { return airOrder; } }
        public int KickerOrder { get { return kickerOrder; } }
        public StraightRank StraightRank { get { return straightRank; } }
        public int StraightDrawOrder { get { return straightDrawOrder; } }
        public StraightType StraightType { get { return straightType; } }
        public List<int> StraightRanks { get { return straightRanks; } }

        // rank match variables
        protected int[] rankMatches;
        protected int matchRank;
        protected int kickerRank;
        protected int boardMatches;
        protected int matchOrder;
        protected int airOrder;
        protected int kickerOrder;
        protected StraightRank straightRank;
        protected int straightDrawOrder;

        // straight variables
        protected StraightType straightType;
        protected List<int> straightRanks;

        // flush variables
        protected int suitCount;
        protected FlushType flushType;
        protected char suit;
        protected int flushOrder;

        protected bool usedCache;

        public HandStrengthCalculator(Hand hand) {
            this.hand = hand;
            handStrength.Category = HandCategory.Unknown;
            handStrength.Type = HandType.Unknown;
            handStrength.StrengthLabel = "";
            handStrength.StrengthOrder = 0;
            strengthCalculated = false;
            usedCache = false;
        }

        private void determineHandStrength() {
            /*
            HandStrengthCalculator cachedObject;

            if (ranksCache.TryGetValue(hand.GetRanks(), out cachedObject))
            {
                rankMatches = cachedObject.RankMatches;
                matchRank = cachedObject.MatchRank;
                kickerRank = cachedObject.KickerRank;
                boardMatches = cachedObject.BoardMatches;
                matchOrder = cachedObject.MatchOrder;
                airOrder = cachedObject.AirOrder;
                kickerOrder = cachedObject.KickerOrder;
                straightRank = cachedObject.StraightRank;
                straightDrawOrder = cachedObject.StraightDrawOrder;

                straightType = cachedObject.StraightType;
                straightRanks = cachedObject.StraightRanks;

                usedCache = true;
            } else {
            */
            determineMatchesAndRanks();
            determineStraightCount();
            
            determineSuitCount();
            determineHandCategory();
            determineHandType();
            /*
            if(!usedCache)
            {
                ranksCache.Add(hand.GetRanks(), this);
            }
            */
            strengthCalculated = true;
        }

        private void determineMatchesAndRanks() {
            if(usedCache)
            {
                return;
            }
            // what we need:
            // #matches card 1 & 2, rank of the matches, kickerrank
            rankMatches = new int[] {0,0};
            foreach(Card flopCard in hand.FlopCards) {
                int index = 0;
                int kickerIndex = 1;
                foreach(Card holeCard in hand.HoleCards) {
                    if(flopCard.RankNumber == holeCard.RankNumber) {
                        rankMatches[index]++;
                        matchRank = holeCard.RankNumber;
                        kickerRank = hand.HoleCards[kickerIndex].RankNumber;
                    }
                    index++;
                    kickerIndex--;
                }
            }
        }

        private void determineStraightCount() {
            if(usedCache)
            {
                return;
            }
            
            List<Card> cards = new List<Card>(hand.FlopCards);
            straightType = StraightType.OpenEnded;

            cards.AddRange(hand.HoleCards);
            cards.Sort(hand.CardComp);
            if(cards[cards.Count - 1].RankNumber == 14) {
                cards.Insert(0,new Card('1','h'));
            }

            int straightCount = 1;
            int consecutive = 1;
            bool gap = false;

            int cardRank = cards[0].RankNumber;
            int lastRank;
            straightRanks = new List<int>();
            straightRanks.Add(cardRank);

            for(int i=1; i < cards.Count; i++) {
                lastRank = cardRank;
                cardRank = cards[i].RankNumber;
                if(cardRank == lastRank + 1) {
                    straightRanks.Add(cardRank);
                    straightCount++;
                    consecutive++;
                } else if(cardRank == lastRank + 2 && gap == false) {
                    if(straightCount < 4) {
                        straightRanks.Add(cardRank);
                        straightType = StraightType.Gutshot;
                        straightCount++;
                        consecutive = 1;
                        gap = true;
                    } else if(straightCount == 4)  {
                        break;
                    }
                } else {
                    if (straightCount > 3) {
                        break;
                    } else {
                        straightCount = 1;
                        consecutive = 1;
                        gap = false;
                        straightRanks.Clear();
                        straightRanks.Add(cardRank);
                    }
                }
            }
            if(consecutive == 5) {
                straightType = StraightType.Straight;
            } else if (consecutive == 4) {
                // exception for A 2 3 4 and J Q K A
                if(straightRanks[0] == 1 || straightRanks[straightRanks.Count-1] == 14) {
                    straightType = StraightType.Gutshot;
                    // exception for A 3 4 5 6
                    if(straightRanks.Count == 5 && straightRanks[1] == 3) {
                        straightType = StraightType.OpenEnded;
                    }
                }
            } else if(straightCount < 4) {
                straightType = StraightType.None;
            }

            // check for double gutters
            if(straightType == StraightType.Gutshot) {
                int firstRank = cards[0].RankNumber;
                if(firstRank + 2 == cards[1].RankNumber
                        && firstRank + 3 == cards[2].RankNumber
                        && firstRank + 4 == cards[3].RankNumber
                        && firstRank + 6 == cards[4].RankNumber) {
                    straightRanks.Clear();
                    straightRanks.AddRange(new int[] {cards[0].RankNumber,cards[1].RankNumber,cards[2].RankNumber,cards[3].RankNumber,cards[4].RankNumber});
                    straightType = StraightType.DoubleGutter;
                } else if(firstRank == 1) {
                    cards.RemoveAt(0);
                    if(firstRank + 2 == cards[1].RankNumber
                            && firstRank + 3 == cards[2].RankNumber
                            && firstRank + 4 == cards[3].RankNumber
                            && firstRank + 6 == cards[4].RankNumber) {
                        straightRanks.Clear();
                        straightRanks.AddRange(new int[] {cards[0].RankNumber,cards[1].RankNumber,cards[2].RankNumber,cards[3].RankNumber,cards[4].RankNumber});
                        straightType = StraightType.DoubleGutter;
                    }
                }
            }
        }
        private void determineStraigthDrawStrength() {
            if(usedCache)
            {
                return;
            }

            if(straightRanks[straightRanks.Count-1] == 14) {
                straightRank = StraightRank.Nut;
                straightDrawOrder = 1;
                return;
            }

            List<int> drawHoleCardPositions = new List<int>();
            int holeCard  = hand.HoleCards[0].RankNumber;
            int holeCardIndex = 0;
            foreach(int rank in straightRanks) {
                if(rank > holeCard) {
                    if(holeCardIndex == 1) {
                        break;
                    } else {
                        holeCard = hand.HoleCards[1].RankNumber;
                        holeCardIndex = 1;
                    }
                } else if (rank == holeCard) {
                    drawHoleCardPositions.Add(straightRanks.Count - straightRanks.IndexOf(rank));
                    holeCard = hand.HoleCards[1].RankNumber;
                    holeCardIndex = 1;
                }
            }

            switch (straightType)
            {
                case StraightType.OpenEnded:
                    if (drawHoleCardPositions.Count == 2)
                    {
                        straightRank = StraightRank.Nut;
                        straightDrawOrder = 1;
                    }
                    else
                    {
                        straightRank = StraightRank.Middle;
                        straightDrawOrder = 2;
                    }

                    break;
                case StraightType.Gutshot:
                    if (drawHoleCardPositions.Count == 2)
                    {
                        if(drawHoleCardPositions[0] < 4) {
                            straightRank = StraightRank.Nut;
                            straightDrawOrder = 1;
                        } else if(drawHoleCardPositions[1] < 3) {
                            straightRank = StraightRank.Middle;
                            straightDrawOrder = 2;
                        } else {
                            straightRank = StraightRank.Bottom;
                            straightDrawOrder = 3;
                        }
                    } else if(drawHoleCardPositions.Count == 1) {
                        if(drawHoleCardPositions[0] < 3) {
                            straightRank = StraightRank.Middle;
                            straightDrawOrder = 2;
                        } else {
                            straightRank = StraightRank.Bottom;
                            straightDrawOrder = 3;
                        }
                    }
                    break;
                case StraightType.DoubleGutter:
                    if(drawHoleCardPositions[0] < 5) {
                        straightRank = StraightRank.Nut;
                        straightDrawOrder = 1;
                    } else if(drawHoleCardPositions[1] < 4) {
                        straightRank = StraightRank.Middle;
                        straightDrawOrder = 2;
                    } else {
                        straightRank = StraightRank.Bottom;
                        straightDrawOrder = 3;
                    }
                    break;
            
            }
        }
        private void determineStraightStrength() {
            if(straightRanks[straightRanks.Count-1] == 14) {
                handStrength.StrengthLabel = StraightRank.Nut.ToString();
                handStrength.StrengthOrder = 1;
                return;
            }

            if(hand.HoleCards[0].RankNumber > straightRanks[0]) {
                handStrength.StrengthLabel = StraightRank.Nut.ToString();
                handStrength.StrengthOrder = 1;
            } else if(hand.HoleCards[1].RankNumber > straightRanks[1]) {
                handStrength.StrengthLabel = StraightRank.Middle.ToString();
                handStrength.StrengthOrder = 2;
            } else {
                handStrength.StrengthLabel = StraightRank.Bottom.ToString();
                handStrength.StrengthOrder = 3;
            }
        }
        private void determineSuitCount() {
            suitCount = 0;

            int flushRank = 0;
            int flushBoost = 0;
            
            if(!hand.isSuited()) {
                // if the hand is offsuit, we'll count how many times each suit appears, keep the highest count of the two and save the suit
                foreach(Card holeCard in hand.HoleCards) {
                    int cardsuitcount = 1; // the suit appears once in our hand
                    var cardflushboost = 0;
                    foreach (Card flopCard in hand.FlopCards) {
                        if (holeCard.Suit == flopCard.Suit) {
                            cardsuitcount++;
                            if(holeCard.RankNumber < flopCard.RankNumber) {
                                cardflushboost++;
                            }
                        }
                    }
                    if(cardsuitcount > suitCount) {
                        suitCount = cardsuitcount;
                        suit = holeCard.Suit;
                        flushRank = holeCard.RankNumber;
                        flushBoost = cardflushboost;
                    }
                }

            } else {
                // if the hand is suited, we'll count how many times the suit appears and save the suit
                suitCount = 2; // the suit appears twice in our hand
                flushRank = hand.HoleCards[1].RankNumber; // biggest rank in our hand
                suit = hand.HoleCards[0].Suit;
                foreach (Card flopCard in hand.FlopCards) {
                    if (flopCard.Suit == suit) {
                        suitCount++;
                        if(flushRank < flopCard.RankNumber) {
                            flushBoost++;
                        }
                    }
                }
            }
            switch(suitCount) {
                case 3:
                    flushType = hand.isSuited() ? FlushType.SuitedBDFD : FlushType.OffsuitBDFD;
                    break;
                case 4:
                    flushType = hand.isSuited() ? FlushType.SuitedFD : FlushType.OffsuitFD;
                    break;
                case 5:
                    flushType = FlushType.Flush;
                    break;
                default:
                    flushType = FlushType.None;
                    break;
            }
            flushOrder = 15 - flushRank - flushBoost;

        }

        // translates rank matches, straighttypes and suitcounts into a hand category
        private void determineHandCategory() {
            if(flushType == FlushType.Flush) {
                if(straightType == StraightType.Straight) {
                    handStrength.Category = HandCategory.StraightFlush;
                } else {
                    handStrength.Category = HandCategory.Flush;
                }
            } else if (straightType == StraightType.Straight) {
                handStrength.Category = HandCategory.Straight;
            } else {
                Array.Sort(rankMatches);
                if(!hand.isPocketPair()) {
                    if(rankMatches[0] == 0 && rankMatches[1] == 0) {
                        determineHighCardOrder();
                        switch(airOrder) {
                            case 1:
                                handStrength.Category = HandCategory.NutAir;
                                break;
                            case 2:
                                handStrength.Category = HandCategory.SecondNutAir;
                                break;
                            default:
                                handStrength.Category = HandCategory.RestAir;
                                break;
                        }
                    } else if(rankMatches[0] == 0 && rankMatches[1] == 1) {
                        determineMatchOrder();
                        switch(matchOrder) {
                            case 1:
                                handStrength.Category = HandCategory.TopPair;
                                break;
                            case 2:
                                handStrength.Category = HandCategory.SecondPair;
                                break;
                            case 3:
                                handStrength.Category = HandCategory.ThirdPair;
                                break;
                        }
                    } else if(rankMatches[0] == 1 && rankMatches[1] == 1) {
                        handStrength.Category = HandCategory.TwoPair;
                    } else if(rankMatches[0] == 0 && rankMatches[1] == 2) {
                        handStrength.Category = HandCategory.ThreeOfAKind;
                    } else if(rankMatches[0] == 1 && rankMatches[1] == 2) {
                        handStrength.Category = HandCategory.FullHouse;
                    } else if(rankMatches[0] == 0 && rankMatches[1] == 3) {
                        handStrength.Category = HandCategory.Quads;
                    }
                } else {
                    if(rankMatches[1] == 1) {
                        determineBoardMatches();
                        if(boardMatches == 0) {
                            handStrength.Category = HandCategory.ThreeOfAKind;
                        } else if(boardMatches == 1) {
                            handStrength.Category = HandCategory.FullHouse;
                        }
                    } else if(rankMatches[1] == 2) {
                        handStrength.Category = HandCategory.Quads;
                    } else if(rankMatches[1] == 0) {
                        determinePocketPairCategory();
                    }
                }
            }
        }
        // determines the hand type
        private void determineHandType() {
            string methodName = $"determine{handStrength.Category.ToString()}Type";
            Type type = typeof(HandStrengthCalculator);
            MethodInfo method = type.GetMethod(methodName,BindingFlags.NonPublic | BindingFlags.Instance);
            try {
                method.Invoke(this,null);
            } catch (NullReferenceException) {
                // if(e.Equals("useless")) {}
                Console.WriteLine($"Method missing for determining the handtype for {handStrength.Category} hands");
                // "unkown type"
            }
        } 
        // counts the amount of rank matches on the board
        private void determineBoardMatches() {
            boardMatches = 0;
            for (int i=0; i < hand.FlopCards.Length - 1; i++) {
                if(hand.FlopCards[i].RankNumber == hand.FlopCards[i+1].RankNumber) {
                    boardMatches++;
                }
            }
        }

        private void determinePocketPairCategory() {
            int handRank = hand.HoleCards[0].RankNumber;
            if(handRank < hand.FlopCards[0].RankNumber) {
                handStrength.Category = HandCategory.LowPair;
            } else if(handRank < hand.FlopCards[1].RankNumber) {
                if(hand.FlopCards[1].RankNumber == hand.FlopCards[2].RankNumber) {
                    handStrength.Category = HandCategory.Underpair;
                } else {
                    handStrength.Category = HandCategory.NogroupPP;
                }
            } else if(handRank < hand.FlopCards[2].RankNumber) {
                handStrength.Category = HandCategory.Underpair;
            } else {
                handStrength.Category = HandCategory.Overpair;
            }
        }

        // calculates the highcard order (1: nuthigh, 2: second-nut-high, ...) 
        private void determineHighCardOrder() {
            

            int highRank = hand.HoleCards[1].RankNumber;
            int lowRank = hand.HoleCards[0].RankNumber;
            
            int highCardBoost = 0;
            int lowCardBoost = 1;
            int previousRank = 0;
            foreach(Card flopCard in hand.FlopCards) {
                if(flopCard.RankNumber > highRank && flopCard.RankNumber != previousRank) {
                    highCardBoost++;
                }
                if(flopCard.RankNumber > lowRank && flopCard.RankNumber != previousRank) {
                    lowCardBoost++;
                }
                previousRank = flopCard.RankNumber;
            }
            airOrder = 15 - highRank - highCardBoost;
            kickerOrder = 15 - lowRank - lowCardBoost;
        }

        // translates the pair rank and kickerrank into an order (1: nut, 2: 2nd nuts, 3: 3rd nuts, ...)
        private void determineMatchOrder() {      
            if(usedCache)
            {
                return;
            }
            
            matchOrder = 1;

            int kickerBoost = 0;
            int previousRank = 0;
            
            foreach (Card flopCard in hand.FlopCards) {
                if(matchRank < flopCard.RankNumber && flopCard.RankNumber != previousRank) {
                    matchOrder++;
                }
                if(kickerRank < flopCard.RankNumber && flopCard.RankNumber != previousRank) {
                    kickerBoost++;
                }
                previousRank = flopCard.RankNumber;
            }
            kickerOrder = 15-kickerRank-kickerBoost;
        }
        

        // determines the nut air sub-category (NA-S-FD, NA-S-BDFD, NA-O-NFD, NA-O-NBDFD, NA-OESD, NA-OTHER)
        private void determineNutAirType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"NA_");
            handStrength.StrengthLabel = kickerOrder.ToString();
            handStrength.StrengthOrder = kickerOrder;
        }

        // determines the nut air sub-category (SA-S-FD, SA-S-BDFD, SA-O-NFD, SA-O-NBDFD, SA-OESD, SA-OTHER)
        private void determineSecondNutAirType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"SA_");
            handStrength.StrengthLabel = kickerOrder.ToString();
            handStrength.StrengthOrder = kickerOrder;
        }
        
        // determines the rest air sub-category (RA-S-FD, RA-S-BDFD, RA-O-NFD, RA-O-NBDFD, RA-OESD, RA-GS, RA-OTHER)
        private void determineRestAirType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"RA_");
            switch(handStrength.Subtype) {
                case HandSubtype.S_FD:
                case HandSubtype.S_BDFD:
                    handStrength.StrengthLabel = flushOrder.ToString();
                    handStrength.StrengthOrder = flushOrder;
                    break;
                case HandSubtype.S_FD_OESD:
                case HandSubtype.S_FD_GS:
                case HandSubtype.OESD:
                case HandSubtype.GS:
                    determineStraigthDrawStrength();
                    handStrength.StrengthLabel = straightRank.ToString();
                    handStrength.StrengthOrder = straightDrawOrder;
                    break;
                case HandSubtype.OTHER:
                    handStrength.StrengthLabel = airOrder.ToString();
                    handStrength.StrengthOrder = airOrder;
                    break;
                default:
                    Console.WriteLine($"Impossible handtype: rest air {handStrength.Subtype}");
                    break;
            }
        }

        // determines the top pair sub-category (TP-S-FD, TP-S-BDFD, TP-O-NFD, TP-O-NBDFD, TP-OESD, TP-OTHER)
        private void determineTopPairType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"TP_");
            determinePairOrder();
        }

        // determines the second pair sub-category (SP-S-FD, SP-S-BDFD, SP-O-NFD, SP-O-NBDFD, SP-OESD, SP-OTHER)
        private void determineSecondPairType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"SP_");
            determinePairOrder();
        }

        // determines the third pair sub-category (3P-S-FD, 3P-S-BDFD, 3P-O-NFD, 3P-O-NBDFD, 3P-OESD, 3P-OTHER)
        private void determineThirdPairType() {
            determineSubtype();
            getHandType(handStrength.Subtype,"TrP_");
            determinePairOrder();
        }

        private void determineOverpairType() {
            determinePPSubtype();
            getHandType(handStrength.Subtype,"OP_");
            determinePPOrder();
        }

        private void determineUnderpairType() {
            determinePPSubtype();
            getHandType(handStrength.Subtype,"UP_");
            determinePPOrder();
        }

        private void determineNogroupPPType() {
            determinePPSubtype();
            getHandType(handStrength.Subtype,"PP_");
            determinePPOrder();
        }

        private void determineLowPairType() {
            determinePPSubtype();
            getHandType(handStrength.Subtype,"LP_");
            determinePPOrder();
        }

        private void determineTwoPairType() {
            switch(flushType) {
                case FlushType.OffsuitBDFD:
                case FlushType.SuitedFD:
                    handStrength.Type = HandType.TwP_BDNFD;
                    break;
                default:
                    handStrength.Type = HandType.TwP;
                    break;
            }
            determineTwoPairOrder();
        }

        private void determineThreeOfAKindType() {
            determineMatchOrder();
            if(hand.isPocketPair()) {
                handStrength.Type = HandType.SET; 
                switch(matchOrder) {
                    case 1:
                        handStrength.StrengthLabel = SetType.Top.ToString();
                        break;
                    case 2:
                        handStrength.StrengthLabel = SetType.Middle.ToString();
                        break;
                    case 3:
                        handStrength.StrengthLabel = SetType.Bottom.ToString();
                        break;
                }
                handStrength.StrengthOrder = matchOrder;
            } else {
                handStrength.Type = HandType.TRIPS;
                handStrength.StrengthLabel = kickerOrder.ToString();
                handStrength.StrengthOrder = kickerOrder;
            }
        }

        private void determineFullHouseType() {
            handStrength.Type = HandType.FULLHOUSE; 
            if(hand.isPocketPair()) {
                if(matchRank == hand.FlopCards[2].RankNumber) {
                    handStrength.StrengthLabel = "Nut";
                    handStrength.StrengthOrder = 1;
                } else {
                    handStrength.StrengthLabel = "2ndNut";
                    handStrength.StrengthOrder = 2;
                }
            } else {
                if(hand.FlopCards[1].RankNumber == hand.FlopCards[2].RankNumber) {
                    handStrength.StrengthLabel = "Nut";
                    handStrength.StrengthOrder = 1;
                } else {
                    handStrength.StrengthLabel = "2ndNut";
                    handStrength.StrengthOrder = 2;
                }
            }
        }

        private void determineFlushType() {
            int order = 15 - hand.HoleCards[0].RankNumber;
            switch (flushOrder) {
                case 1:
                    handStrength.Type = HandType.FL_NUT; 
                    break;
                case 2:
                    handStrength.Type = HandType.FL_2ND; 
                    break;
                default:
                    handStrength.Type = HandType.FL_OTHER; 
                    order = flushOrder;
                    break;
            }
            handStrength.StrengthLabel = order.ToString();
            handStrength.StrengthOrder = order;
        }
        private void determineStraightType() {
            determinePPSubtype();
            getHandType(handStrength.Subtype,"ST_");
            determineStraightStrength();
        }
        private void determineQuadsType() {
            handStrength.Type = HandType.QUADS; 
            handStrength.StrengthLabel = "Quads";
            handStrength.StrengthOrder = 1;
        }
        private void determineStraightFlushType() {
            handStrength.Type = HandType.SF; 
            determineStraightStrength();
        }

        // combines prefix and subtype into a hand type
        private HandType getHandType(HandSubtype subtype, string prefix) {
            return handStrength.Type = (HandType)Enum.Parse(typeof(HandType),prefix + subtype);
        }

        // determines the subtype for air and one pair hands (see enum Subtype)
        private void determineSubtype() {
            handStrength.Subtype = HandSubtype.OTHER;
            switch (flushType) {
                case FlushType.SuitedFD:
                    if(handStrength.Category == HandCategory.RestAir) {
                        switch (straightType) {
                            case StraightType.OpenEnded:
                                handStrength.Subtype = HandSubtype.S_FD_OESD;
                                break;
                            case StraightType.Gutshot:
                                handStrength.Subtype = HandSubtype.S_FD_GS;
                                break;
                            default:
                                handStrength.Subtype = HandSubtype.S_FD;
                                break;
                        }
                    } else {
                        handStrength.Subtype = HandSubtype.S_FD;
                    }
                    return;
                case FlushType.SuitedBDFD:
                    handStrength.Subtype = HandSubtype.S_BDFD;
                    break;
                case FlushType.OffsuitFD:
                    if(flushOrder == 1) handStrength.Subtype = HandSubtype.O_NFD;
                    return;
                case FlushType.OffsuitBDFD:
                    if(flushOrder == 1) handStrength.Subtype = HandSubtype.O_BDNFD;
                    break;
            }
            switch (straightType)
            {
                case StraightType.OpenEnded:
                case StraightType.DoubleGutter:
                    handStrength.Subtype = HandSubtype.OESD;
                    break;
                case StraightType.Gutshot:
                    if(handStrength.Category == HandCategory.RestAir && handStrength.Subtype == HandSubtype.OTHER) handStrength.Subtype = HandSubtype.GS;
                    break;
            }
        }
        // determines the subtype for pocket pair hands and straights
        private void determinePPSubtype() {
            switch(flushType) {
                case FlushType.SuitedFD:
                case FlushType.OffsuitFD:
                    handStrength.Subtype = HandSubtype.FD;
                    break;
                case FlushType.SuitedBDFD:
                case FlushType.OffsuitBDFD:
                    handStrength.Subtype = HandSubtype.BDFD;
                    break;
                default:
                    handStrength.Subtype = HandSubtype.OTHER;
                    break;
            }
        }
        private void determinePairOrder() {
            handStrength.StrengthLabel = kickerOrder.ToString();
            handStrength.StrengthOrder = kickerOrder;
        }
        private void determinePPOrder() {
            handStrength.StrengthLabel = hand.GetRanksLabel();
            handStrength.StrengthOrder = 15 - hand.HoleCards[0].RankNumber;
        }

        private void determineTwoPairOrder() {
            bool topMatch = (hand.HoleCards[1].RankNumber == hand.FlopCards[2].RankNumber);
            bool midMatch = (hand.HoleCards[0].RankNumber == hand.FlopCards[1].RankNumber);
            if(topMatch && midMatch) {
                handStrength.StrengthLabel = TwoPairType.TopTwo.ToString();
                handStrength.StrengthOrder = 1;
            } else if(topMatch) {
                handStrength.StrengthLabel = TwoPairType.TopBottom.ToString();
                handStrength.StrengthOrder = 2;
            } else {
                handStrength.StrengthLabel = TwoPairType.BottomTwo.ToString();
                handStrength.StrengthOrder = 3;
            }
        }

    }

}