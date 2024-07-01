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
   public partial class IconDisplayDialog : Window
   {
      public IconDisplayDialog()
      {
         InitializeComponent();
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
