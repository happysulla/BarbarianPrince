using System.Windows;

namespace BarbarianPrince
{
    /// <summary>
    /// Interaction logic for SpashScreen.xaml
    /// </summary>
    public partial class SplashDialog : Window
    {
        public SplashDialog()
        {
            InitializeComponent();
            this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - this.MinHeight) / 2.0;
            this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth - this.MinWidth) / 2.0;
        }
    }
}
