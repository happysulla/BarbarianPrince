using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class ReturnToActiveEventDialog : Window
   {
      public ReturnToActiveEventDialog()
      {
         InitializeComponent();
         Image imageTutorial = new Image() { Name = "Tutorial0", Width = 370, Height = 70, HorizontalAlignment=HorizontalAlignment.Center, Source = MapItem.theMapImages.GetBitmapImage("Tutorial0") };
         myGrid.Children.Add(imageTutorial);
         Grid.SetRow(imageTutorial, 1);
         Grid.SetColumn(imageTutorial, 0);
      }

      private void ButtonGoto_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
         Close();
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
