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

namespace BarbarianPrince
{
   public partial class PartyDisplayDialog : Window
   {
      public PartyDisplayDialog(IGameInstance gi, Canvas c, Button b)
      {
         InitializeComponent();
         double size = Utilities.ZOOM * Utilities.theMapItemSize;
         double princeSize = gi.Prince.Zoom * Utilities.theMapItemSize;
         System.Windows.Point location = b.PointToScreen(new Point(princeSize, princeSize));
         this.Left = location.X;
         this.Top = location.Y;
         //-----------------------------
         this.Height = size + 2;
         this.Width = size * (gi.PartyMembers.Count - 1) + 2; // not showing prince
          //-----------------------------
         foreach (IMapItem mi in gi.PartyMembers)
         {
            if (true == mi.Name.Contains("Prince"))
               continue;
            System.Windows.Controls.Button newButton = new Button { Name = Utilities.RemoveSpaces(mi.Name), Width = size, Height = size, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
            MapItem.SetButtonContent(newButton, mi, true, false, true, false); // This sets the image as the button's content
            this.myWrapPanel.Children.Add(newButton);   
         }
      }
   }
}
