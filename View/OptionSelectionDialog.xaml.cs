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
      public bool CtorError { get; }
      public IGameInstance myGameInstance = null;
      private Options myOptions = null;
      public Options NewOptions { get => myOptions; set => myOptions = value; }
      public OptionSelectionDialog(Options options)
      {
         InitializeComponent();
         myOptions = options;
         if ( false == UpdateDisplay(myOptions))
         {
            Logger.Log(LogEnum.LE_ERROR, "OptionSelectionDialog(): SetOptionsInDialog() returned false");
            CtorError = true;
         }
      }
      //----------------------------------------------------------------
      private bool UpdateDisplay(Options options)
      {
         bool isCustomConfig = false;
         bool isRandomPartyConfig = false;
         bool isCustomPartyConfig = false;
         bool isRandomHexConfig = false;
         bool isCustomHexConfig = false;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Auto Rolls
         Option option = options.Find("AutoSetup");
         if( null == option ) 
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoSetup");
            return false;
         }
         myCheckBoxAutoSetup.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("AutoWealthRollForUnderFive");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoWealthRollForUnderFive");
            return false;
         }
         myCheckBoxAutoWealth.IsChecked = option.IsEnabled;
         //-------------------------
         option = options.Find("AutoLostDecrease");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "AutoLostDecrease");
            return false;
         }
         myCheckBoxAutoLostIncrement.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Prince 
         option = options.Find("PrinceHorse");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceHorse");
            return false;
         }
         myCheckBoxPrinceHorse.IsChecked = option.IsEnabled;
         if( true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrincePegasus");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrincePegasus");
            return false;
         }
         myCheckBoxPrincePegasus.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrinceCoin");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceCoin");
            return false;
         }
         myCheckBoxPrinceCoin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("PrinceFood");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "PrinceFood");
            return false;
         }
         myCheckBoxPrinceFood.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Party Members
         option = options.Find("RandomParty10");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomParty10");
            return false;
         }
         myRadioButtonPartyRandom10.IsChecked = option.IsEnabled;
         isRandomPartyConfig = option.IsEnabled;
         if ( false == option.IsEnabled)
         {
            option = options.Find("RandomParty08");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "RandomParty08");
               return false;
            }
            myRadioButtonPartyRandom8.IsChecked = option.IsEnabled;
            isRandomPartyConfig = option.IsEnabled;
            if (false == option.IsEnabled)
            {
               option = options.Find("RandomParty05");
               if (null == option)
               {
                  Logger.Log(LogEnum.LE_ERROR, "RandomParty05");
                  return false;
               }
               myRadioButtonPartyRandom5.IsChecked = option.IsEnabled;
               isRandomPartyConfig = option.IsEnabled;
               if (false == option.IsEnabled)
               {
                  option = options.Find("RandomParty03");
                  if (null == option)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RandomParty03");
                     return false;
                  }
                  myRadioButtonPartyRandom3.IsChecked = option.IsEnabled;
                  isRandomPartyConfig = option.IsEnabled;
                  if (false == option.IsEnabled)
                  {
                     option = options.Find("RandomParty01");
                     if (null == option)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "RandomParty01");
                        return false;
                     }
                     myRadioButtonPartyRandom1.IsChecked = option.IsEnabled;
                     isRandomPartyConfig = option.IsEnabled;
                     if (false == option.IsEnabled)
                     {
                        option = options.Find("PartyCustom");
                        if (null == option)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "PartyCustom");
                           return false;
                        }
                        if (true == option.IsEnabled)
                        {
                           myCheckBoxDwarf.IsEnabled = true;
                           myCheckBoxEagle.IsEnabled = true;
                           myCheckBoxElf.IsEnabled = true;
                           myCheckBoxFalcon.IsEnabled = true;
                           myCheckBoxGriffon.IsEnabled = true;
                           myCheckBoxHarpy.IsEnabled = true;
                           myCheckBoxMagician.IsEnabled = true;
                           myCheckBoxMercenary.IsEnabled = true;
                           myCheckBoxMerchant.IsEnabled = true;
                           myCheckBoxMinstrel.IsEnabled = true;
                           myCheckBoxMonk.IsEnabled = true;
                           myCheckBoxPorterSlave.IsEnabled = true;
                           myCheckBoxTrueLove.IsEnabled = true;
                           myCheckBoxWizard.IsEnabled = true;
                           //-------------------------
                           option = options.Find("Dwarf");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Dwarf");
                              return false;
                           }
                           myCheckBoxDwarf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Eagle");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Eagle");
                              return false;
                           }
                           myCheckBoxEagle.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Elf");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Elf");
                              return false;
                           }
                           myCheckBoxElf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Falcon");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Falcon");
                              return false;
                           }
                           myCheckBoxFalcon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Griffon");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Griffon");
                              return false;
                           }
                           myCheckBoxGriffon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Harpy");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Harpy");
                              return false;
                           }
                           myCheckBoxHarpy.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Magician");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Magician");
                              return false;
                           }
                           myCheckBoxMagician.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Mercenary");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Mercenary");
                              return false;
                           }
                           myCheckBoxMercenary.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Merchant");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Merchant");
                              return false;
                           }
                           myCheckBoxMerchant.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Minstrel");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Minstrel");
                              return false;
                           }
                           myCheckBoxMinstrel.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Monk");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Monk");
                              return false;
                           }
                           myCheckBoxMonk.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("PorterSlave");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "PorterSlave");
                              return false;
                           }
                           myCheckBoxPorterSlave.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("TrueLove");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "TrueLove");
                              return false;
                           }
                           myCheckBoxTrueLove.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                           //-------------------------
                           option = options.Find("Wizard");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Wizard");
                              return false;
                           }
                           myCheckBoxWizard.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                              isCustomPartyConfig = true;
                        }
                        else
                        {
                           myCheckBoxDwarf.IsEnabled = false;
                           myCheckBoxEagle.IsEnabled = false;
                           myCheckBoxElf.IsEnabled = false;
                           myCheckBoxFalcon.IsEnabled = false;
                           myCheckBoxGriffon.IsEnabled = false;
                           myCheckBoxHarpy.IsEnabled = false;
                           myCheckBoxMagician.IsEnabled = false;
                           myCheckBoxMercenary.IsEnabled = false;
                           myCheckBoxMerchant.IsEnabled = false;
                           myCheckBoxMinstrel.IsEnabled = false;
                           myCheckBoxMonk.IsEnabled = false;
                           myCheckBoxPorterSlave.IsEnabled = false;
                           myCheckBoxTrueLove.IsEnabled = false;
                           myCheckBoxWizard.IsEnabled = false;
                        }
                     }
                  }
               }
            }
         }
         if( false == isRandomPartyConfig )
            myRadioButtonPartyOriginal.IsChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("RandomHex");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomHex");
            return false;
         }
         myRadioButtonHexRandom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isRandomHexConfig = true;
         //-------------------------
         option = options.Find("RandomTown");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomTown");
            return false;
         }
         myRadioButtonHexRandomTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isRandomHexConfig = true;
         //-------------------------
         option = options.Find("RandomLeft");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomLeft");
            return false;
         }
         myRadioButtonHexRandomLeft.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isRandomHexConfig = true;
         //-------------------------
         option = options.Find("RandomRight");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomRight");
            return false;
         }
         myRadioButtonHexRandomRight.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isRandomHexConfig = true;
         //-------------------------
         option = options.Find("RandomBottom");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "RandomBottom");
            return false;
         }
         myRadioButtonHexRandomBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isRandomHexConfig = true;
         //-------------------------
         option = options.Find("0109");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0109");
            return false;
         }
         myRadioButtonHexTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0206");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0206");
            return false;
         }
         myRadioButtonHexRuin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0711");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0711");
            return false;
         }
         myRadioButtonHexTemple.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("1212");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1212");
            return false;
         }
         myRadioButtonHexHuldra.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0323");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0323");
            return false;
         }
         myRadioButtonHexDrogat.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("1923");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1923");
            return false;
         }
         myRadioButtonHexLadyAeravir.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0418");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0418");
            return false;
         }
         myRadioButtonHexFarmland.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0410");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0410");
            return false;
         }
         myRadioButtonHexCountryside.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0409");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0409");
            return false;
         }
         myRadioButtonHexForest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0406");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0406");
            return false;
         }
         myRadioButtonHexHill.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0405");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0405");
            return false;
         }
         myRadioButtonHexMountain.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0411");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0411");
            return false;
         }
         myRadioButtonHexSwamp.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("0407");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "0407");
            return false;
         }
         myRadioButtonHexDesert.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("1905");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1905");
            return false;
         }
         myRadioButtonHexRoad.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         //-------------------------
         option = options.Find("1723");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "1723");
            return false;
         }
         myRadioButtonHexBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomHexConfig = true;
         if ((false == isRandomHexConfig) && (false == isCustomHexConfig))
            myRadioButtonHexOriginal.IsChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Monsters
         option = options.Find("EasiestMonsters");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "EasiestMonsters");
            return false;
         }
         myRadioButtonMonsterEasiest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            option = options.Find("EasyMonsters");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "EasyMonsters");
               return false;
            }
            myRadioButtonMonsterEasy.IsChecked = option.IsEnabled;
            if (true == option.IsEnabled)
            {
               isCustomConfig = true;
            }
            else
            {
               option = options.Find("EasyMonsters");
               if (null == option)
               {
                  Logger.Log(LogEnum.LE_ERROR, "EasyMonsters");
                  return false;
               }
               myRadioButtonMonsterEasy.IsChecked = option.IsEnabled;
               if (true == option.IsEnabled)
               {
                  isCustomConfig = true;
               }
               else
               {
                  option = options.Find("LessHardMonsters");
                  if (null == option)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "LessHardMonsters");
                     return false;
                  }
                  myRadioButtonMonsterLessEasy.IsChecked = option.IsEnabled;
                  if (true == option.IsEnabled)
                  {
                     isCustomConfig = true;
                  }
                  else
                  {
                     myRadioButtonMonsterNormal.IsChecked = true;
                  }
               }
            }
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("NoLostRoll");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "NoLostRoll");
            return false;
         }
         myCheckBoxNoLostRoll.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoLostEvent");
            return false;
         }
         myCheckBoxNoLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceLostEvent");
            return false;
         }
         myCheckBoxForceLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoEvent");
            return false;
         }
         myCheckBoxNoEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceEvent");
            return false;
         }
         myCheckBoxForceEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoRoadEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoRoadEvent");
            return false;
         }
         myCheckBoxNoRoadEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoAirEvent");
            return false;
         }
         myCheckBoxNoAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceAirEvent");
            return false;
         }
         myCheckBoxForceAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoCrossEvent");
            return false;
         }
         myCheckBoxNoCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceCrossEvent");
            return false;
         }
         myCheckBoxForceCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceLostAfterCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceLostAfterCrossEvent");
            return false;
         }
         myCheckBoxForceLostAfterCross.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceNoRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceNoRaftEvent");
            return false;
         }
         myCheckBoxNoRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //-------------------------
         option = options.Find("ForceRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "ForceRaftEvent");
            return false;
         }
         myCheckBoxForceRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
            isCustomConfig = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Summary Selection
         if ((true == isCustomConfig) || (true == isCustomPartyConfig) || (true == isCustomHexConfig))
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonCustom.IsChecked = true;
         }
         else if ((true == isRandomPartyConfig) && (true == isRandomHexConfig))
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = true;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomPartyConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = true;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomHexConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = true;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomHexConfig)
         {
            myRadioButtonOriginal.IsChecked = true;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
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

      private void StackPanelSummary_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;  
         switch(rb.Name) 
         {
            case "myRadioButtonOriginal":
               break;
            case "myRadioButtonRandomParty":
               break;
            case "myRadioButtonRandomStart":
               break;
            case "myRadioButtonAllRandom":
               break;
            case "myRadioButtonCustom":
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "StackPanelSummary_Click(): reached default rb.Name=" + rb.Name);
               return;
         }
      }
   }
}
