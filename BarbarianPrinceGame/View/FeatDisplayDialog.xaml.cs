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
         Logger.Log(LogEnum.LE_SERIALIZE_FEATS, "FeatDisplayDialog(): \n feats=" + GameEngine.theFeatsInGame.ToString() );
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
         if( true == myIsAllFeatsShown )
            myButtonShowAll.Visibility = Visibility.Hidden;
         //------------------------------------------------------------
         int rowNum = 2;
         bool isFeatDisplayed = myGameFeatToShow.myIsOriginalGameWin;
         bool isFeatChecked = GameEngine.theFeatsInGame.myIsOriginalGameWin;
         string featName = "myIsOriginalGameWin";
         string featDesc = "Win the brutally difficult original game.";
         CheckBox cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center , Margin=new Thickness(5)};
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name=featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show" , Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock(){ FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsLowWitWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsLowWitWin;
         featName = "myIsLowWitWin";
         featDesc = "Win the game with a Wit and Wiles equal to two ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e000c", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIs500GoldWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIs500GoldWin;
         featName = "myIs500GoldWin";
         featDesc = "Win the game by accumulating 500 or more gold ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e001", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsNobleAllyWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsNobleAllyWin;
         featName = "myIsNobleAllyWin";
         featDesc = "Win the game by gaining noble ally ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e152", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsBlessedWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsBlessedWin;
         featName = "myIsBlessedWin";
         featDesc = "Win the game by being blessed by the gods ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e044b", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsStaffOfCommandWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsStaffOfCommandWin;
         featName = "myIsStaffOfCommandWin";
         featDesc = "Win the game by holding the staff of command ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e212m", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsRoyalHelmWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsRoyalHelmWin;
         featName = "myIsRoyalHelmWin";
         featDesc = "Win the game by holding the Royal Helm North of the Tragoth River ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e194", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsHuldraDefeatedInBattleWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsHuldraDefeatedInBattleWin;
         featName = "myIsHuldraDefeatedInBattleWin";
         featDesc = "Win the game by defeating Huldra in battle after securing royal hier ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e144i", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsHuldraDesposedWin;
         isFeatChecked = GameEngine.theFeatsInGame.myIsHuldraDesposedWin;
         featName = "myIsHuldraDesposedWin";
         featDesc = "Win the game by disposing Huldra and replacing with royal heir ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e144f", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsLostOnTime;
         isFeatChecked = GameEngine.theFeatsInGame.myIsLostOnTime;
         featName = "myIsLostOnTime";
         featDesc = "Lost the game by running out of time.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsLostAxeDeath;
         isFeatChecked = GameEngine.theFeatsInGame.myIsLostAxeDeath;
         featName = "myIsLostAxeDeath";
         featDesc = "Lost the game by losing your head ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e203b", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsAirTravel;
         isFeatChecked = GameEngine.theFeatsInGame.myIsAirTravel;
         featName = "myIsAirTravel";
         featDesc = "As a daily action, perform air travel.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsRaftTravel;
         isFeatChecked = GameEngine.theFeatsInGame.myIsRaftTravel;
         featName = "myIsRaftTravel";
         featDesc = "As a daily action, perform raft travel.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsArchTravel;
         isFeatChecked = GameEngine.theFeatsInGame.myIsArchTravel;
         featName = "myIsArchTravel";
         featDesc = "As a daily action, perform arch way travel.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsMinstelAdded;
         isFeatChecked = GameEngine.theFeatsInGame.myIsMinstelAdded;
         featName = "myIsMinstelAdded";
         featDesc = "A minstrel joins your party other than in starting party ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e049", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsEagleAdded;
         isFeatChecked = GameEngine.theFeatsInGame.myIsEagleAdded;
         featName = "myIsEagleAdded";
         featDesc = "An eagle joins your party other than in starting party ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e117", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsFalconAdded;
         isFeatChecked = GameEngine.theFeatsInGame.myIsFalconAdded;
         featName = "myIsFalconAdded";
         featDesc = "A falcon joins your party other than in starting party ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e107", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsMerchantAdded;
         isFeatChecked = GameEngine.theFeatsInGame.myIsMerchantAdded;
         featName = "myIsMerchantAdded";
         featDesc = "A merchant joins your party other than in starting party ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e048e", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsTrueLoveAdded;
         isFeatChecked = GameEngine.theFeatsInGame.myIsTrueLoveAdded;
         featName = "myIsTrueLoveAdded";
         featDesc = "You found your true love ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e228", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(" "));
            Button buttonRule1a = new Button() { Content = "e163c", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule1a.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule1a));
            tb.Inlines.Add(new Run(" "));
            Button buttonRule1 = new Button() { Content = "e212i", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule1.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule1));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsResistenceRingUsed;
         isFeatChecked = GameEngine.theFeatsInGame.myIsResistenceRingUsed;
         featName = "myIsResistenceRingUsed";
         featDesc = "Use Resistence Ring in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule1 = new Button() { Content = "e191", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule1.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule1));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsHydraTeethUsed;
         isFeatChecked = GameEngine.theFeatsInGame.myIsHydraTeethUsed;
         featName = "myIsHydraTeethUsed";
         featDesc = "Use magical hydra teeth in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e140b", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(" "));
            Button buttonRule1 = new Button() { Content = "e141", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule1.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule1));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsRescueHeir;
         isFeatChecked = GameEngine.theFeatsInGame.myIsRescueHeir;
         featName = "myIsRescueHeir";
         featDesc = "Rescue the royal and true hier of Huldra castle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e144a", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsSneakAttack;
         isFeatChecked = GameEngine.theFeatsInGame.myIsSneakAttack;
         featName = "myIsSneakAttack";
         featDesc = "With Heir to throne, perform a sneak attack on Baron Huldra ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e144d", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsStealGems;
         isFeatChecked = GameEngine.theFeatsInGame.myIsStealGems;
         featName = "myIsStealGems";
         featDesc = "Using foulbane, steal Count Dragot's jewels ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e146a", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsLadyAeravirAccused;
         isFeatChecked = GameEngine.theFeatsInGame.myIsLadyAeravirAccused;
         featName = "myIsLadyAeravirAccused";
         featDesc = "Accuse Lady Aeravir of indecency ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e145", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsDragonKiller;
         isFeatChecked = GameEngine.theFeatsInGame.myIsDragonKiller;
         featName = "myIsDragonKiller";
         featDesc = "Kill a dragon in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e098", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsBanditKiller;
         isFeatChecked = GameEngine.theFeatsInGame.myIsBanditKiller;
         featName = "myIsBanditKiller";
         featDesc = "Kill 20 bandits in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e051", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(". # killed = "));
            tb.Inlines.Add(new Run(GameEngine.theFeatsInGame.myNumBanditKill.ToString()));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsOrcKiller;
         isFeatChecked = GameEngine.theFeatsInGame.myIsOrcKiller;
         featName = "myIsOrcKiller";
         featDesc = "Kill 25 orcs in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e055", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(". # killed = "));
            tb.Inlines.Add(new Run(GameEngine.theFeatsInGame.myNumOrcKill.ToString()));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsGoblinKiller;
         isFeatChecked = GameEngine.theFeatsInGame.myIsGoblinKiller;
         featName = "myIsGoblinKiller";
         featDesc = "Kill 30 goblins in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e052", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(". # killed = "));
            tb.Inlines.Add(new Run(GameEngine.theFeatsInGame.myNumGoblinKill.ToString()));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsWolfKiller;
         isFeatChecked = GameEngine.theFeatsInGame.myIsWolfKiller;
         featName = "myIsWolfKiller";
         featDesc = "Kill 35 wolves in battle ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e075", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(". # killed = "));
            tb.Inlines.Add(new Run(GameEngine.theFeatsInGame.myNumWolfKill.ToString()));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsNightsInJail;
         isFeatChecked = GameEngine.theFeatsInGame.myIsNightsInJail;
         featName = "myIsNightsInJail";
         featDesc = "Spend 40 nights in jail ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e060", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run(". # nights = "));
            tb.Inlines.Add(new Run(GameEngine.theFeatsInGame.myNumNightsInJail.ToString()));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsVisitAllTowns;
         isFeatChecked = GameEngine.theFeatsInGame.myIsVisitAllTowns;
         featName = "myIsVisitAllTowns";
         featDesc = "Visit all towns. # visited = " + GameEngine.theFeatsInGame.myVisitedTowns.Count.ToString() + " out of 12.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsVisitAllCastles;
         isFeatChecked = GameEngine.theFeatsInGame.myIsVisitAllCastles;
         featName = "myIsVisitAllCastles";
         featDesc = "Visit all castles. # visited = " + GameEngine.theFeatsInGame.myVisitedCastles.Count.ToString() + " out of 3.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsVisitAllTemples;
         isFeatChecked = GameEngine.theFeatsInGame.myIsVisitAllTemples;
         featName = "myIsVisitAllTemples";
         featDesc = "Visit all temples. # visited = " + GameEngine.theFeatsInGame.myVisitedTemples.Count.ToString() + " out of 5.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsVisitAllRuins;
         isFeatChecked = GameEngine.theFeatsInGame.myIsVisitAllRuins;
         featName = "myIsVisitAllRuins";
         featDesc = "Visit all ruins. # visited = " + GameEngine.theFeatsInGame.myVisitedRuins.Count.ToString() + " out of 3.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsVisitAllOasis;
         isFeatChecked = GameEngine.theFeatsInGame.myIsVisitAllOasis;
         featName = "myIsVisitAllOasis";
         featDesc = "Visit all oasis. # visited = " + GameEngine.theFeatsInGame.myVisitedOasises.Count.ToString() + " out of 4.";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsPurchaseFoulbane;
         isFeatChecked = GameEngine.theFeatsInGame.myIsPurchaseFoulbane;
         featName = "myIsPurchaseFoulbane";
         featDesc = "Purchase foulbane in Temple of Duffyd ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e146", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
         //------------------------------------------------------------
         ++rowNum;
         isFeatDisplayed = myGameFeatToShow.myIsPurchaseChaga;
         isFeatChecked = GameEngine.theFeatsInGame.myIsPurchaseChaga;
         featName = "myIsPurchaseChaga";
         featDesc = "Purchase Chaga drug in town ";
         cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
         myGrid.Children.Add(cb);
         Grid.SetColumn(cb, 0);
         Grid.SetRow(cb, rowNum);
         if ((false == myIsAllFeatsShown) && (false == isFeatDisplayed) && (false == isFeatChecked))
         {
            System.Windows.Controls.Button b = new Button { Name = featName, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
            b.Click += ButtonShowFeat_Click;
            myGrid.Children.Add(b);
            Grid.SetColumn(b, 1);
            Grid.SetRow(b, rowNum);
         }
         else
         {
            TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            tb.Inlines.Add(new Run(featDesc));
            Button buttonRule = new Button() { Content = "e143", FontFamily = myFontFam1, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom };
            buttonRule.Click += ButtonShowEventDialog_Click;
            tb.Inlines.Add(new InlineUIContainer(buttonRule));
            tb.Inlines.Add(new Run("."));
            myGrid.Children.Add(tb);
            Grid.SetColumn(tb, 1);
            Grid.SetRow(tb, rowNum);
         }
      }
      //-----------------------------------------------
      private void ButtonShowEventDialog_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowEventDialog(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + key);
      }
      private void ButtonShowFeat_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         switch (b.Name)
         {
            case "myIsOriginalGameWin": myGameFeatToShow.myIsOriginalGameWin = true; break;
            case "myIsLowWitWin": myGameFeatToShow.myIsLowWitWin = true; break;
            case "myIs500GoldWin": myGameFeatToShow.myIs500GoldWin = true; break;
            case "myIsNobleAllyWin": myGameFeatToShow.myIsNobleAllyWin = true; break;
            case "myIsBlessedWin": myGameFeatToShow.myIsBlessedWin = true; break;
            case "myIsStaffOfCommandWin": myGameFeatToShow.myIsStaffOfCommandWin = true; break;
            case "myIsRoyalHelmWin": myGameFeatToShow.myIsRoyalHelmWin = true; break;
            case "myIsHuldraDefeatedInBattleWin": myGameFeatToShow.myIsHuldraDefeatedInBattleWin = true; break;
            case "myIsHuldraDesposedWin": myGameFeatToShow.myIsHuldraDesposedWin = true; break;
            case "myIsLostOnTime": myGameFeatToShow.myIsLostOnTime = true; break;
            case "myIsLostAxeDeath": myGameFeatToShow.myIsLostAxeDeath = true; break;
            case "myIsAirTravel": myGameFeatToShow.myIsAirTravel = true; break;
            case "myIsRaftTravel": myGameFeatToShow.myIsRaftTravel = true; break;
            case "myIsArchTravel": myGameFeatToShow.myIsArchTravel = true; break;
            case "myIsMinstelAdded": myGameFeatToShow.myIsMinstelAdded = true; break;
            case "myIsEagleAdded": myGameFeatToShow.myIsEagleAdded = true; break;
            case "myIsFalconAdded": myGameFeatToShow.myIsFalconAdded = true; break;
            case "myIsMerchantAdded": myGameFeatToShow.myIsMerchantAdded = true; break;
            case "myIsTrueLoveAdded": myGameFeatToShow.myIsTrueLoveAdded = true; break;
            case "myIsResistenceRingUsed": myGameFeatToShow.myIsResistenceRingUsed = true; break;
            case "myIsHydraTeethUsed": myGameFeatToShow.myIsHydraTeethUsed = true; break;
            case "myIsRescueHeir": myGameFeatToShow.myIsRescueHeir = true; break;
            case "myIsSneakAttack": myGameFeatToShow.myIsSneakAttack = true; break;
            case "myIsStealGems": myGameFeatToShow.myIsStealGems = true; break;
            case "myIsLadyAeravirAccused": myGameFeatToShow.myIsLadyAeravirAccused = true; break;
            case "myIsDragonKiller": myGameFeatToShow.myIsDragonKiller = true; break;
            case "myIsBanditKiller": myGameFeatToShow.myIsBanditKiller = true; break;
            case "myIsOrcKiller": myGameFeatToShow.myIsOrcKiller = true; break;
            case "myIsGoblinKiller": myGameFeatToShow.myIsGoblinKiller = true; break;
            case "myIsWolfKiller": myGameFeatToShow.myIsWolfKiller = true; break;
            case "myIsNightsInJail": myGameFeatToShow.myIsNightsInJail = true; break;
            case "myIsVisitAllTowns": myGameFeatToShow.myIsVisitAllTowns = true; break;
            case "myIsVisitAllCastles": myGameFeatToShow.myIsVisitAllCastles = true; break;
            case "myIsVisitAllTemples": myGameFeatToShow.myIsVisitAllTemples = true; break;
            case "myIsVisitAllRuins": myGameFeatToShow.myIsVisitAllRuins = true; break;
            case "myIsVisitAllOasis": myGameFeatToShow.myIsVisitAllOasis = true; break;
            case "myIsPurchaseFoulbane": myGameFeatToShow.myIsPurchaseFoulbane = true; break;
            case "myIsPurchaseChaga": myGameFeatToShow.myIsPurchaseChaga = true; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ButtonShowFeat_Click(): Reached Default b.Name=" + b.Name);
               break;
         }
         UpdateGridRows();
      }
      private void ButtonShowAll_Click(object sender, RoutedEventArgs e)
      {
         myIsAllFeatsShown = true;
         UpdateGridRows();
      }
   }
}
