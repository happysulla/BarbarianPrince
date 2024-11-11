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
using static BarbarianPrince.InventoryDisplayDialog;

namespace BarbarianPrince
{
   public partial class FeatDisplayDialog : Window
   {
      public bool CtorError = false;
      private RuleDialogViewer myRulesManager = null;
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private bool myIsAllFeatsShown = false;
      private bool myIsFeat0Shown = false;
      //-----------------------------------------------
      public FeatDisplayDialog(RuleDialogViewer rm)
      {
         InitializeComponent();
         if (null == rm)
         {
            Logger.Log(LogEnum.LE_ERROR, "InventoryDisplayDialog(): rv=null");
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         UpdateGridRows();
      }
      //-----------------------------------------------
      private void UpdateGridRows()
      {
         if( (false == GameEngine.theFeatsInGame.myIs500GoldWin) && (false == myIsAllFeatsShown) && (false == myIsFeat0Shown))
         {
            System.Windows.Controls.Button b = new Button { Name="myFeat0", FontFamily = myFontFam1, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show" };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, 2);
         }
         else
         {
            TextBlock tb = new TextBlock();
            tb.Inlines.Add(new Run("Win the Game by accumulating 500 or more gold."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, 2 );
         }
      }
      //-----------------------------------------------
      private void ButtonShowRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowRule(b.Name))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + b.Name);
      }
      private void ButtonShowFeat_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         switch (b.Name)
         {
            case "myFeat0":
               myIsFeat0Shown = true;
               break;
            default:
               break;
         }
         UpdateGridRows();
      }
      private void ButtonShowAll_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowRule(b.Name))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + b.Name);
      }
   }
}
