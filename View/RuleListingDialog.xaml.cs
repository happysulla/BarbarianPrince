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

namespace BarbarianPrince
{
   public partial class RuleListingDialog : Window
   {
      private const int STARTING_RULE_ROW = 49;
      public bool CtorError = false;
      private RuleDialogViewer myRulesManager = null;
      private Thickness myThickness = new Thickness(5, 2, 5, 2);
      private readonly FontFamily myFontFam = new FontFamily("Courier New");
      //----------------------------------------------------------------
      public RuleListingDialog(RuleDialogViewer rm)
      {
         InitializeComponent();
         if (null == rm)
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleListingDialog(): rm=null");
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         int numToDisplay = myRulesManager.Rules.Keys.Count - STARTING_RULE_ROW + 2; // add one for header row and one for separator
         for ( int i=0; i< numToDisplay; ++i )
         {
            RowDefinition rowDef = new RowDefinition();  
            myGrid.RowDefinitions.Add(rowDef);
         }
         UpdateGridRowHeader();
         if (false == UpdateGridRows())
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
      private bool UpdateGridRows()
      {
         int numToDisplay = myRulesManager.Rules.Keys.Count - STARTING_RULE_ROW; // add one for header row and one for separator
         int rowNum = 2;
         for ( int i=0; i< numToDisplay; ++i)
         {
            int ruleNum = i + STARTING_RULE_ROW;
            string key = myRulesManager.Rules.Keys.ElementAt(ruleNum);
            if (4 < key.Length)
               continue;
            string title = myRulesManager.GetTitle(key);
            if( null == title )
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): title=null");
               return false;
            }
            if ("Travelling Event References" == title)
               continue;
            System.Windows.Controls.Button b = new Button { Name=key, FontFamily=myFontFam, FontSize=12, Margin=new Thickness(5), Content=key };
            b.Click += ButtonShowRule_Click;
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            Label label = new Label() { FontFamily=myFontFam, FontSize=12, HorizontalAlignment=System.Windows.HorizontalAlignment.Left, VerticalAlignment=VerticalAlignment.Center, Content=title };
            myGrid.Children.Add(label);
            Grid.SetRow(label, rowNum);
            Grid.SetColumn(label, 1);
            ++rowNum;
         }
         return true;
      }
      private void ButtonShowRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowRule(b.Name))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + b.Name);
      }
   }
}
