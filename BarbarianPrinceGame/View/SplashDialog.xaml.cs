using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BarbarianPrince
{
   public partial class SplashDialog : Window
   {
      public SplashDialog()
      {
         InitializeComponent();
         Image image = new Image() { Stretch=Stretch.Uniform, IsHitTestVisible=false, IsEnabled = false, Margin = new Thickness(0, 5, 0, 0), Source = MapItem.theMapImages.GetBitmapImage("GameBox") };
         myViewBoxSplash.Child = image;
         this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - this.MinHeight) / 2.0;
         this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth - this.MinWidth) / 2.0;
      }
   }
}
