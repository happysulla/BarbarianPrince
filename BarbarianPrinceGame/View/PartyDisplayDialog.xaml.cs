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
      private ScrollViewer myScrollViewer = null;
      public PartyDisplayDialog(IGameInstance gi, Canvas c, Button b)
      {
         InitializeComponent();
         System.Windows.Media.Matrix currentMatrix = ScreenExtensions.GetMatrixFromVisual(this);
         double princeSize = (gi.Prince.Zoom * Utilities.theMapItemSize);
         double partyMemberSize = (Utilities.ZOOM * Utilities.theMapItemSize);
         //-----------------------------
         foreach (IMapItem mi in gi.PartyMembers) // set contents of WrapPanel
         {
            if (true == mi.Name.Contains("Prince"))
               continue;
            System.Windows.Controls.Button newButton = new Button { Name = Utilities.RemoveSpaces(mi.Name), Width = partyMemberSize, Height = partyMemberSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
            MapItem.SetButtonContent(newButton, mi, true, false, true, false); // This sets the image as the button's content
            this.myWrapPanel.Children.Add(newButton);
         }
         //-----------------------------
         this.Width = (partyMemberSize * (gi.PartyMembers.Count - 1) + 2); // not showing prince
         this.Height = (partyMemberSize + 2);
         //-----------------------------
         myScrollViewer = (ScrollViewer)c.Parent;
         double aw = myScrollViewer.ActualWidth;
         double ho = myScrollViewer.HorizontalOffset;
         double cw = c.ActualWidth;
         double delta = 0;
         if (cw < aw)
            delta = (aw - cw - System.Windows.SystemParameters.VerticalScrollBarWidth) / (2 * Utilities.ZoomCanvas);
         Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): aw=" + aw.ToString() + " ho=" + ho.ToString() + " cw=" + cw.ToString() + " delta=" + delta.ToString());

         //-----------------------------
         System.Windows.Point bottomRight = b.PointToScreen(new Point(princeSize, princeSize)); // bottom right of button
         double bottomRightRight = bottomRight.X * currentMatrix.M11;
         double bottomRightBottom = bottomRight.Y * currentMatrix.M22;   
         double rw = (Canvas.GetLeft(b) + princeSize) * Utilities.ZoomCanvas + this.Width;
         double awho = (aw + ho);
         Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): br=" + bottomRight.X.ToString() + " rw=" + rw.ToString() + " awho=" + awho.ToString() );
         if ( rw < awho-delta )
         {
            this.Left = bottomRightRight;
            Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): Left=" + this.Left.ToString());
         }
         else
         {
            double d1 = rw - (awho-delta);
            this.Left = bottomRightRight - d1;
            Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): d1=" + d1.ToString()  + " Left=" + this.Left.ToString());
         }
         //-----------------------------
         double bw = ((Canvas.GetTop(b) + princeSize) * Utilities.ZoomCanvas) + this.Height;
         Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): bb=" + bottomRight.Y.ToString());
         if (bw < c.ActualHeight)
         {
            this.Top = bottomRightBottom;
            Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): Top=" + this.Top.ToString());
         }
         else
         {
            System.Windows.Point topLeft = b.PointToScreen(new Point(0, 0)); // top left of button
            this.Top = topLeft.Y * currentMatrix.M22 - this.Height;
            Logger.Log(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG, "PartyDisplayDialog(): topLeft=" + topLeft.ToString() + " Top=" + this.Top.ToString());
         }
      }
   }
}
