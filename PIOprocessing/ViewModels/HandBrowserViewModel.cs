using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PIOprocessing.ViewModels
{
    class HandBrowserViewModel : Conductor<Object>
    {
        public DataTable HandsData { get; set; }
        public ICollectionView HandsView { get; set; }

        public string WindowTitle { get; set; }
        //public BindableCollection<HandModel> Hands { get; set; }
        // public System.Windows.Controls.DataGrid DG { get; set; }

        // public HandsModel HM { get; set; }

        // public List<string> Headers { get; set; }
        // public string[][] HandData { get; set; }
        
        public HandBrowserViewModel(HandGroup handGroup, string windowTitle)
        {
            // WindowTitle = $"{handGroup.Spot.Action} {handGroup.Spot.AggPos}vs{handGroup.Spot.CllPos} {handGroup.Spot.BoardSubtype} {handGroup.Spot.BoardSubtype} {handGroup.HandType.ToString()}";
            WindowTitle = windowTitle;
            NotifyOfPropertyChange(() => WindowTitle);
            LoadDataGrid(handGroup);
        }


        public void LoadDataGrid(HandGroup handGroup)
        {
            Console.WriteLine("Loading starts");
            
            HandsData = new DataTable();
            addHeaders(handGroup.GetFrequencyLabels());

            // int rowIndex = 0;
            foreach (int strengthorder in handGroup.GetStrengthOrders())
            {
                foreach (Hand hand in handGroup.GetHands(strengthorder))
                {
                    addRow(hand);
                }
            }
            
            HandsView = CollectionViewSource.GetDefaultView(HandsData);
            HandsView.GroupDescriptions.Clear();
            if(Properties.Settings.Default.GroupHands)
            {
                HandsView.GroupDescriptions.Add(new PropertyGroupDescription("Strength label"));
                
            }
            Console.WriteLine("Loading ends");

        }

        private void addHeaders(string[] freqLabels)
        {
            HandsData.Columns.Add("#").DataType = typeof(int);
            HandsData.Columns.Add("Category");
            HandsData.Columns.Add("Type");
            HandsData.Columns.Add("Strength order").DataType = typeof(int);
            HandsData.Columns.Add("Strength label");
            HandsData.Columns.Add("Holecards");
            HandsData.Columns.Add("Flop");
            foreach (string freqLabel in freqLabels)
            {
                HandsData.Columns.Add(freqLabel).DataType = typeof(float);
            }
            HandsData.Columns.Add("Weight").DataType = typeof(float);
        }

        private void addRow(Hand hand)
        {
            string[] row = new string[HandsData.Columns.Count];

            int index = 0;
            row[index++] = hand.CsvRowNumber.ToString();
            row[index++] = hand.Strength.Category.ToString();
            row[index++] = hand.Strength.Type.ToString();
            row[index++] = hand.Strength.StrengthOrder.ToString();
            row[index++] = hand.Strength.StrengthLabel.ToString();
            row[index++] = hand.HoleCardsText;
            row[index++] = hand.FlopText;
            foreach (float freq in hand.Frequencies.Values)
            {
                row[index++] = freq.ToString();
            }
            row[index] = hand.Weight.ToString();

            HandsData.Rows.Add(row);
        }
    }
}

/*
HM = new HandsModel(handGroup);

Headers = HM.Headers;
HandData = new string[HM.HandData.Count][];
int index2 = 0;
foreach (List<string> datarow in HM.HandData)
{
    HandData[index2++] = datarow.ToArray();
}

DG = new System.Windows.Controls.DataGrid();
// HandsGrid.ItemsSource = Hands;

// DG.Items.Add(HandData[0]);


int index = 0;
foreach(string header in HM.Headers)
{
    DG.Columns.Add(new DataGridTextColumn
    {
        Header = header,
        Binding = new Binding($"HandData[{index}]")
        // Binding = new Binding(string.Format("HandData[{0}]", index++))
    }); 
}
DG.Width = 1000;

*/

/*
Hands = new BindableCollection<HandModel>();
int previouscount = 0;
foreach(int strengthOrder in handGroup.GetStrengthOrders())
{
    foreach(Hand hand in handGroup.GetHands(strengthOrder))
    {
        Hands.Add(new HandModel(hand));
        if (Hands.Count - previouscount == 1000) {
            previouscount = Hands.Count;
            Console.WriteLine("Hands created: " + Hands.Count.ToString());
        }
        if (Hands.Count == 100)
        {
            return;
        }
    }
}
*/

/*
dg.Columns.Add(new DataGridTextColumn
{
    Header = header,
    // Binding = new Binding($"HandData[{index}]")
    Binding = new Binding(string.Format("[{0}]", index++))
});
*/


/*
for (int i=0; i < handGroup.GetFrequencyLabels().Length; i++)
{
    Binding b = new Binding($"Frequencies[{i}]");
    DataGridTextColumn column = new DataGridTextColumn();
    column.Binding = b;
    column.Header = handGroup.GetFrequencyLabels()[i];
    HandsGrid.Columns.Add(column);
}
*/
