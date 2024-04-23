using System;
using System.Windows;
namespace BarbarianPrince
{
    public partial class MapItemCreateDialog : Window
    {
        public String MapItemName { get; set; } = "";
        public double Zoom { get; set; } = 1.0;
        public String TopImageName { get; set; } = "";
        public String BottomImageName { get; set; } = "";
        public String OverlapImageName { get; set; } = "";
        public String Endurance { get; set; } = "";
        public String Combat { get; set; } = "";
        public String Coin { get; set; } = "";
        public bool IsHidden { get; set; } = false;
        public bool IsAnimated { get; set; } = false;
        public bool IsGuide { get; set; } = false;
        public MapItemCreateDialog()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MapItemName = myTextBoxName.Text;
            Zoom = Double.Parse(myTextBoxZoom.Text);
            TopImageName = myTextBoxTopImageName.Text;
            BottomImageName = myTextBoxBottomImageName.Text;
            OverlapImageName = myTextBoxOverlapImageName.Text;
            Endurance = myTextBoxEndurance.Text;
            Combat = myTextBoxCombat.Text;
            Coin = myTextBoxCoin.Text;
            IsHidden = (bool)myCheckBoxHidden.IsChecked;
            IsAnimated = (bool)myCheckBoxAnimated.IsChecked;
            IsGuide = (bool)myCheckBoxGuide.IsChecked;
            this.DialogResult = true;
        }
    }
}
