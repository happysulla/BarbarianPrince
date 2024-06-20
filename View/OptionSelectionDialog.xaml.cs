using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class OptionSelectionDialog : Window
   {
      public OptionSelectionDialog(IOptions options)
      {
         InitializeComponent();
         SetOptions(options);
      }
      //----------------------------------------------------------------
      private bool SetOptions(IOptions options)
      {
         bool isCustomConfig = false;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         IOption option = options.Find("AutoSetup");
         if( null == option ) 
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoSetup");
            return false;
         }
         myCheckboxAutoSetup.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("AutoWealthRollForUnderFive");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoWealthRollForUnderFive");
            return false;
         }
         myCheckboxAutoWealth.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("AutoLostDecrease");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoLostDecrease");
            return false;
         }
         myCheckboxAutoLost.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("PrinceHorse");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceHorse");
            return false;
         }
         myCheckboxPrinceHorse.IsChecked = option.IsEnabled;
         if( true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrincePegasus");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrincePegasus");
            return false;
         }
         myCheckboxPrincePegasus.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrinceCoin");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceCoin");
            return false;
         }
         myCheckboxPrinceCoin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrinceFood");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceFood");
            return false;
         }
         myCheckboxPrinceFood.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         IOption optionSummaryRandomParty = options.Find("RandomParty");
         if (null == optionSummaryRandomParty)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomParty");
            return false;
         }
         //-------------------------
         option = options.Find("Dwarf");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Dwarf");
            return false;
         }
         myCheckboxDwarf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Dwarf");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Dwarf");
            return false;
         }
         myCheckboxDwarf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Eagle");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Eagle");
            return false;
         }
         myCheckboxEagle.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Elf");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Elf");
            return false;
         }
         myCheckboxElf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Falcon");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Falcon");
            return false;
         }
         myCheckboxFalcon.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Griffon");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Griffon");
            return false;
         }
         myCheckboxGriffon.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Harpy");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Harpy");
            return false;
         }
         myCheckboxHarpy.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Magician");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Magician");
            return false;
         }
         myCheckboxMagician.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Mercenary");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Mercenary");
            return false;
         }
         myCheckboxMercenary.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Mercenary");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Mercenary");
            return false;
         }
         myCheckboxMerchant.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Merchant");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Merchant");
            return false;
         }
         myCheckboxDwarf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Mercenary");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Mercenary");
            return false;
         }
         myCheckboxDwarf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Minstrel");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Minstrel");
            return false;
         }
         myCheckboxMinstrel.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Monk");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Monk");
            return false;
         }
         myCheckboxMonk.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PorterSlave");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PorterSlave");
            return false;
         }
         myCheckboxPorterSlave.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("TrueLove");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "TrueLove");
            return false;
         }
         myCheckboxTrueLove.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("Wizard");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "Wizard");
            return false;
         }
         myCheckboxWizard.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         IOption optionSummaryRandomHex = options.Find("RandomHex");
         if (null == optionSummaryRandomHex)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomParty");
            return false;
         }
         //-------------------------
         option = options.Find("0109");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0109");
            return false;
         }
         myCheckboxDwarf.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0206");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0206");
            return false;
         }
         myRadiobuttonHexRuin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0711");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0711");
            return false;
         }
         myRadiobuttonHexTemple.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("1212");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1212");
            return false;
         }
         myRadiobuttonHexHuldra.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0323");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0323");
            return false;
         }
         myRadiobuttonHexDrogat.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("1923");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1923");
            return false;
         }
         myRadiobuttonHexLadyAeravir.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0418");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0418");
            return false;
         }
         myRadiobuttonHexFarmland.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0410");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0410");
            return false;
         }
         myRadiobuttonHexFarmland.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0409");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0409");
            return false;
         }
         myRadiobuttonHexForest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0406");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0406");
            return false;
         }
         myRadiobuttonHexHill.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0405");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0405");
            return false;
         }
         myRadiobuttonHexMountain.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0411");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0411");
            return false;
         }
         myRadiobuttonHexSwamp.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("0407");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0407");
            return false;
         }
         myRadiobuttonHexDesert.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("1905");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1905");
            return false;
         }
         myRadiobuttonHexRoad.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("1723");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1723");
            return false;
         }
         myRadiobuttonHexBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("EasiestMonsters");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "EasiestMonsters");
            return false;
         }
         myRadioButtonMonsterEasiest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("EasyMonsters");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "EasyMonsters");
            return false;
         }
         myRadioButtonMonsterEasy.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("LessHardMonsters");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "LessHardMonsters");
            return false;
         }
         myRadioButtonMonsterLessEasy.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("NoLostRoll");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "NoLostRoll");
            return false;
         }
         myCheckboxNoLostRoll.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoLostEvent");
            return false;
         }
         myCheckboxNoLostEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceLostEvent");
            return false;
         }
         myCheckboxForceLostEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoEvent");
            return false;
         }
         myCheckboxNoEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceEvent");
            return false;
         }
         myCheckboxForceEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoRoadEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoRoadEvent");
            return false;
         }
         myCheckboxNoRoadEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoAirEvent");
            return false;
         }
         myCheckboxNoAirEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceAirEvent");
            return false;
         }
         myCheckboxForceAirEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoCrossEvent");
            return false;
         }
         myCheckboxNoCrossEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceCrossEvent");
            return false;
         }
         myCheckboxForceCrossEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceLostAfterCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceLostAfterCrossEvent");
            return false;
         }
         myCheckboxForceLostAfterCross.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceNoRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoRaftEvent");
            return false;
         }
         myCheckboxNoRaftEvent.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("ForceRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceRaftEvent");
            return false;
         }
         myCheckboxForceRaftEvent.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++

         return true;
      }
      //----------------------------------------------------------------
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
