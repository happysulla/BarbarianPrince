using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class RuleListingDialog : Window
   {
      private const int STARTING_RULE_ROW = 50;
      private const int STARTING_EVENT_ROW = 5;
      public bool CtorError = false;
      private RuleDialogViewer myRulesManager = null;
      private Thickness myThickness = new Thickness(5, 2, 5, 2);
      private readonly FontFamily myFontFam = new FontFamily("Courier New");
      //----------------------------------------------------------------
      public RuleListingDialog(RuleDialogViewer rm, bool isEventDialog = false)
      {
         InitializeComponent();
         if (null == rm)
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleListingDialog(): rm=null");
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         if (true == isEventDialog)
            this.Title = "Event Listing";
         int numToDisplay = myRulesManager.Rules.Keys.Count - STARTING_RULE_ROW + 2; // add one for header row and one for separator
         for ( int i=0; i< numToDisplay; ++i )
         {
            RowDefinition rowDef = new RowDefinition();  
            myGrid.RowDefinitions.Add(rowDef);
         }
         numToDisplay = myRulesManager.Events.Keys.Count - STARTING_EVENT_ROW + 2; // add one for header row and one for separator
         for (int i = 0; i < numToDisplay; ++i)
         {
            RowDefinition rowDef = new RowDefinition();
            myGrid.RowDefinitions.Add(rowDef);
         }
         UpdateGridRowHeader();
         if (false == UpdateGridRows(isEventDialog))
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleListingDialog(): UpdateGridRows() returned false");
            CtorError = true;
            return;
         }
      }
      private void UpdateGridRowHeader()
      {
         Label labelRuleNum = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Rule Number" };
         myGrid.Children.Add(labelRuleNum);
         Grid.SetRow(labelRuleNum, 0);
         Grid.SetColumn(labelRuleNum, 0);
         Label labelRuleTitle = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Rule Title" };
         myGrid.Children.Add(labelRuleTitle);
         Grid.SetRow(labelRuleTitle, 0);
         Grid.SetColumn(labelRuleTitle, 1);
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Visible, Height=1, Fill=Brushes.Black, Margin = myThickness };
         myGrid.Children.Add(r1);
         Grid.SetRow(r1, 1);
         Grid.SetColumn(r1, 0);
         Grid.SetColumnSpan(r1, 2);
      }
      private bool UpdateGridRows(bool isEventDialog)
      {
         if ( true == isEventDialog)
         {
            int numToDisplay = myRulesManager.Events.Keys.Count - STARTING_EVENT_ROW; // add one for header row and one for separator
            int rowNum = 2;
            for (int i = 0; i < numToDisplay; ++i)
            {
               int eventNum = i + STARTING_EVENT_ROW;
               string key = myRulesManager.Events.Keys.ElementAt(eventNum);
               string title = myRulesManager.GetEventTitle(key);
               if (null == title)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): title=null for key=" + key);
                  return false;
               }
               System.Windows.Controls.Button b = new Button { Name = key, FontFamily = myFontFam, FontSize = 12, Margin = new Thickness(5), Content = key };
               b.Click += ButtonShowRule_Click;
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               Label label = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = title };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
               ++rowNum;
               if ("e195" == key)
                  break;
            }
         }
         else
         {
            int numToDisplay = myRulesManager.Rules.Keys.Count - STARTING_RULE_ROW; // add one for header row and one for separator
            int rowNum = 2;
            for (int i = 0; i < numToDisplay; ++i)
            {
               int ruleNum = i + STARTING_RULE_ROW;
               string key = myRulesManager.Rules.Keys.ElementAt(ruleNum);
               if (4 < key.Length)
                  continue;
               string title = myRulesManager.GetRuleTitle(key);
               if (null == title)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): title=null");
                  return false;
               }
               if ("Travelling Event References" == title)
                  continue;
               System.Windows.Controls.Button b = new Button { Name = key, FontFamily = myFontFam, FontSize = 12, Margin = new Thickness(5), Content = key };
               b.Click += ButtonShowRule_Click;
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, 0);
               Label label = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = title };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
               ++rowNum;
            }
         }
         return true;
      }
      private void ButtonShowRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == myRulesManager.ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("t")) // rules based click
         {
            if (false == myRulesManager.ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // rules based click
         {
            if (false == myRulesManager.ShowEventDialog(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
               return;
            }
         }
      }
   }
}
