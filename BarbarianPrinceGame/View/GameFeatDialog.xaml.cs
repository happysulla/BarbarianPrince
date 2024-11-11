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
using WpfAnimatedGif;

namespace BarbarianPrince
{
   public partial class GameFeatDialog : Window
   {
      public GameFeatDialog()
      {
         InitializeComponent();
         double sizeOfImage = 500;
         BitmapImage bmi1 = new BitmapImage();
         bmi1.BeginInit();
         bmi1.UriSource = new Uri(MapImage.theImageDirectory + "StarReward.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image imgFeat = new Image { Source = bmi1, Height = sizeOfImage, Width = sizeOfImage, Name = "Feat" };
         ImageBehavior.SetAnimatedSource(imgFeat, bmi1);
         ImageBrush brush = new ImageBrush(bmi1);
         this.Background = brush;
      }
      private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         this.Close();
      }
   }
}

