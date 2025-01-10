using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class ShowAboutDialog : Window
   {
      public ShowAboutDialog()
      {
         InitializeComponent();
         //--------------------------------------
         StringBuilder sb = new StringBuilder();
         sb.Append("Verson: ");
         Version version = Assembly.GetExecutingAssembly().GetName().Version;
         sb.Append(version.ToString());
         sb.Append("_");
         DateTime linkTimeLocal = Utilities.GetLinkerTime(Assembly.GetExecutingAssembly());
         sb.Append(linkTimeLocal.ToString());
         myTextBox.Text = sb.ToString();  
      }
      //-------------------------------------------------------------------------------
      private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
         }
         e.Handled = true;
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
