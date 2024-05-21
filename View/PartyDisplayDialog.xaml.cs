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
using static BarbarianPrince.EventViewerHuntMgr;
using static System.Windows.Forms.AxHost;

namespace BarbarianPrince
{
   public partial class PartyDisplayDialog : Window
   {
      public PartyDisplayDialog(IGameInstance gi, Canvas c, Button mapButton)
      {
         InitializeComponent();
         foreach(IMapItem mi in gi.PartyMembers)
         {
            this.Height = Utilities.ZOOM * Utilities.theMapItemSize;
            this.Width = Utilities.ZOOM * Utilities.theMapItemSize;
            Button b = CreateButton(mi);
            this.myDockPanel.Children.Add(b);   
            c.Children.Add(this);
            double x = Canvas.GetRight(mapButton);
            double y = Canvas.GetBottom(mapButton);
            Canvas.SetRight(this, x);
            Canvas.SetBottom(this, y);
         }
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         b.IsEnabled = true;
         MapItem.SetButtonContent(b, mi, false, false); // This sets the image as the button's content
         return b;
      }
   }
}
