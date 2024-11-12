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
      private readonly FontFamily myFontFam1 = new FontFamily("Georgia");
      private bool myIsAllFeatsShown = false;
      private GameFeat myGameFeatToShow = new GameFeat();
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
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int row = Grid.GetRow(ui);
            if (1 < row)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         int rowNum = 2;
         CheckBox cb = new CheckBox() { IsEnabled = false, IsChecked = false, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center , Margin=new Thickness(5)};
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == myGameFeatToShow.myIs500GoldWin) && (false == GameEngine.theFeatsInGame.myIs500GoldWin))
         {
            System.Windows.Controls.Button b = new Button { Name= "myIs500GoldWinShown", FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show" , Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            if(true == GameEngine.theFeatsInGame.myIs500GoldWin)
               cb.IsChecked = true;
            TextBlock tb = new TextBlock(){ FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run("Win the Game by accumulating 500 or more gold."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         cb = new CheckBox() { IsEnabled = false, IsChecked = false, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == myGameFeatToShow.myIsNobleAllyWin) && (false == GameEngine.theFeatsInGame.myIsNobleAllyWin))
         {
            System.Windows.Controls.Button b = new Button { Name = "myIsNobleAllyWin", FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            if (true == GameEngine.theFeatsInGame.myIsNobleAllyWin)
               cb.IsChecked = true;
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run("Win the Game by gaining an Ally during an audience."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
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
            case "myIs500GoldWin": myGameFeatToShow.myIs500GoldWin = true; break;
            case "myIsNobleAllyWin": myGameFeatToShow.myIsNobleAllyWin = true; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ButtonShowFeat_Click(): Reached Default b.Name=" + b.Name);
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
