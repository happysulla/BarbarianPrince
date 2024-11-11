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
   public partial class ShowReportErrorDialog : Window
   {
      public ShowReportErrorDialog()
      {
         InitializeComponent();
         StringBuilder sb = new StringBuilder();
         sb.Append("Verson: ");
         Version version = Assembly.GetExecutingAssembly().GetName().Version;
         sb.Append(version.ToString());
         sb.Append("_");
         DateTime linkTimeLocal = GetLinkerTime(Assembly.GetExecutingAssembly());
         sb.Append(linkTimeLocal.ToString());
         myTextBox.Text = sb.ToString();
      }
      //-------------------------------------------------------------------------------
      private DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
      {
         var filePath = assembly.Location;
         const int c_PeHeaderOffset = 60;
         const int c_LinkerTimestampOffset = 8;
         var buffer = new byte[2048];
         using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            stream.Read(buffer, 0, 2048);
         var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
         var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
         var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
         var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
         var tz = target ?? TimeZoneInfo.Local;
         var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);
         return localTime;
      }
      private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
      {
         Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
         e.Handled = true;
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
