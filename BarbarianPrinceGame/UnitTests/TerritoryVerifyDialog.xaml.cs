using System;
using System.Windows;
using System.Windows.Controls;
namespace BarbarianPrince
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TerritoryVerifyDialog : Window
    {
        public String RadioOutputText { get; set; } = "Open";
        public String CenterPointX { get; set; } = "";
        public String CenterPointY { get; set; } = "";
        public bool IsTown { get; set; } = false;
        public bool IsCastle { get; set; } = false;
        public bool IsRuin { get; set; } = false;
        public bool IsTemple { get; set; } = false;
        public bool IsOasis { get; set; } = false;
        public TerritoryVerifyDialog(ITerritory t, double anX)
        {
            InitializeComponent();
            myTextBoxName.Text = t.Name;
            //if( 0.0 == anX )
            myTextBoxCenterPointX.Text = t.CenterPoint.X.ToString();
            //else
            //    myTextBoxCenterPointX.Text = anX.ToString();
            myTextBoxCenterPointY.Text = t.CenterPoint.Y.ToString();
            if (true == t.IsTown) { IsTown = true; myCheckBoxTown.IsChecked = true; }
            if (true == t.IsCastle) { IsCastle = true; myCheckBoxCastle.IsChecked = true; }
            if (true == t.IsTemple) { IsTemple = true; myCheckBoxTemple.IsChecked = true; }
            if (true == t.IsOasis) { IsOasis = true; myCheckBoxOasis.IsChecked = true; }
            if (true == t.IsRuin) { IsRuin = true; myCheckBoxRuin.IsChecked = true; }
            switch (t.Type)
            {
                case "Countryside":
                    myRadioButtonCountryside.IsChecked = true;
                    break;
                case "Farmland":
                    myRadioButtonFarmland.IsChecked = true;
                    break;
                case "Forest":
                    myRadioButtonForest.IsChecked = true;
                    break;
                case "Hills":
                    myRadioButtonHill.IsChecked = true;
                    break;
                case "Mountains":
                    myRadioButtonMountain.IsChecked = true;
                    break;
                case "Desert":
                    myRadioButtonDesert.IsChecked = true;
                    break;
                case "Swamp":
                    myRadioButtonSwamp.IsChecked = true;
                    break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): unk type=" + t.Type);
                    break;
            }
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
            IsTown = true;
        }
        private void CheckBox_Checked_IsCastle(object sender, RoutedEventArgs e)
        {
            IsCastle = true;
        }
        private void CheckBox_Checked_IsRuin(object sender, RoutedEventArgs e)
        {
            IsRuin = true;
        }
        private void CheckBox_Checked_IsTemple(object sender, RoutedEventArgs e)
        {
            IsTemple = true;
        }
        private void CheckBox_Checked_IsOasis(object sender, RoutedEventArgs e)
        {
            IsOasis = true;
        }
        private void CheckBox_UnChecked_IsTown(object sender, RoutedEventArgs e)
        {
            IsTown = false;
        }
        private void CheckBox_UnChecked_IsCastle(object sender, RoutedEventArgs e)
        {
            IsCastle = false;
        }
        private void CheckBox_UnChecked_IsRuin(object sender, RoutedEventArgs e)
        {
            IsRuin = false;
        }
        private void CheckBox_UnChecked_IsTemple(object sender, RoutedEventArgs e)
        {
            IsTemple = false;
        }
        private void CheckBox_UnChecked_IsOasis(object sender, RoutedEventArgs e)
        {
            IsOasis = false;
        }
        private void TextBoxCenterPointX_TextChanged(object sender, TextChangedEventArgs e)
        {
            CenterPointX = myTextBoxCenterPointX.Text;
        }
        private void TextBoxCenterPointY_TextChanged(object sender, TextChangedEventArgs e)
        {
            CenterPointY = myTextBoxCenterPointY.Text;
        }
    }
}
