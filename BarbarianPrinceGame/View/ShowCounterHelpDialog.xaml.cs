
using System;
using System.Windows;
using System.Windows.Controls;

namespace BarbarianPrince
{
   public partial class ShowCounterHelpDialog : Window
   {
      public bool CtorError { get; } = false;
      private RuleDialogViewer myRulesMgr = null;
      public ShowCounterHelpDialog(RuleDialogViewer rdv)
      {
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCounterHelpDialog(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         InitializeComponent();
         //--------------------------------------------------
         Thickness thickness = new Thickness(5);
         Image imageTutorial = new Image() { Name = "Tutorial1", Width = 200, Height = 200, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Source = MapItem.theMapImages.GetBitmapImage("Tutorial1"), Margin=thickness};
         myGrid.Children.Add(imageTutorial);
         Grid.SetRow(imageTutorial, 1);
         Grid.SetColumn(imageTutorial, 1);
      }
      private void ButtonOK_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule(content))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false for c=" + content);
      }
   }
}
