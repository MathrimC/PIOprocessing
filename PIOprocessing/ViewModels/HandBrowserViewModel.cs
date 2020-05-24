using Caliburn.Micro;
using PIOprocessing.Models;
using System;
using System.Collections.Generic;
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
        public BindableCollection<HandModel> Hands { get; set; }
        public System.Windows.Controls.DataGrid HandsGrid { get; set; }

        public HandBrowserViewModel(HandGroup handGroup)
        {

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
            HandsGrid = new System.Windows.Controls.DataGrid();
            HandsGrid.ItemsSource = Hands;

            for (int i=0; i < handGroup.GetFrequencyLabels().Length; i++)
            {
                Binding b = new Binding($"Frequencies[{i}]");
                DataGridTextColumn column = new DataGridTextColumn();
                column.Binding = b;
                column.Header = handGroup.GetFrequencyLabels()[i];
                HandsGrid.Columns.Add(column);
            }

        }
    }
}
