using System;
using System.Windows;
using System.Windows.Controls;
namespace BarbarianPrince
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TerritoryCreateDialog : Window
    {
        public String RadioOutputText { get; set; } = "Open";
        public bool IsTown { get; set; } = false;
        public bool IsCastle { get; set; } = false;
        public bool IsRuin { get; set; } = false;
        public bool IsTemple { get; set; } = false;
        public bool IsOasis { get; set; } = false;
        public TerritoryCreateDialog()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            RadioOutputText = radioButton.Content.ToString();
        }
        private void CheckBox_Checked_IsTown(object sender, RoutedEventArgs e)
        {
            IsTown = !IsTown;
        }
        private void CheckBox_Checked_IsCastle(object sender, RoutedEventArgs e)
        {
            IsCastle = !IsCastle;
        }
        private void CheckBox_Checked_IsRuin(object sender, RoutedEventArgs e)
        {
            IsRuin = !IsRuin;
        }
        private void CheckBox_Checked_IsTemple(object sender, RoutedEventArgs e)
        {
            IsTemple = !IsTemple;
        }
        private void CheckBox_Checked_IsOasis(object sender, RoutedEventArgs e)
        {
            IsOasis = !IsOasis;
        }
    }
}
