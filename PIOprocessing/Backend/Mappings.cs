using System.Collections.Generic;
using System.IO;

namespace PIOprocessing {
    static class Mappings
    {
        public static char[] Suits = {'h','d','c','s'};
        public static Dictionary<char, int> Ranks = new Dictionary<char,int>
        {
            {'1', 1},{'2',2},{'3',3},{'4',4},{'5',5},{'6',6},{'7',7},{'8',8},{'9',9},{'T',10},{'J',11},{'Q',12},{'K',13},{'A',14}
        };
        public static Dictionary<string,int> ReportColumns = new Dictionary<string,int>
        {
            {"Flop",0},{"Hand",1},{"Weight",2}
        };

    }
}