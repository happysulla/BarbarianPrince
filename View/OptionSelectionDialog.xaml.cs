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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
         else
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
      private void ResetSummary()
      {
         myRadioButtonOriginal.IsChecked = false;
         myRadioButtonRandomParty.IsChecked = false;
         myRadioButtonRandomStart.IsChecked = false;
         myRadioButtonAllRandom.IsChecked = false;
         myRadioButtonCustom.IsChecked = false;
      }
      private void ResetPrince()
      {
         Option option = null;
         myCheckBoxPrinceHorse.IsChecked = false;
         option = myOptions.Find("PrinceHorse");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found AutoLostDecrease");
         myCheckBoxPrincePegasus.IsChecked = false;
         option = myOptions.Find("PrincePegasus");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found AutoLostDecrease");
         myCheckBoxPrinceCoin.IsChecked = false;
         option = myOptions.Find("PrinceCoin");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found AutoLostDecrease");
         myCheckBoxPrinceFood.IsChecked = false;
         option = myOptions.Find("PrinceFood");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found AutoLostDecrease");
      }
      private void ResetParty()
      {
         Option option = null;
         myRadioButtonPartyOriginal.IsChecked = false;
         myRadioButtonPartyRandom10.IsChecked = false;
         option = myOptions.Find("RandomParty10");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty10");
         myRadioButtonPartyRandom8.IsChecked = false;
         option = myOptions.Find("RandomParty08");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty08");
         myRadioButtonPartyRandom5.IsChecked = false;
         option = myOptions.Find("RandomParty05");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty05");
         myRadioButtonPartyRandom3.IsChecked = false;
         option = myOptions.Find("RandomParty03");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty03");
         myRadioButtonPartyRandom1.IsChecked = false;
         option = myOptions.Find("RandomParty01");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty01");
         myRadioButtonPartyCustom.IsChecked = false;
         option = myOptions.Find("PartyCustom");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found PartyCustom");
         myRadioButtonPartyCustom.IsChecked = false;
      }
      private void ResetPartyMembers()
      {
         Option option = null;
         myCheckBoxDwarf.IsChecked = false;
         myCheckBoxEagle.IsChecked = false;
         myCheckBoxElf.IsChecked = false;
         myCheckBoxFalcon.IsChecked = false;
         myCheckBoxGriffon.IsChecked = false;
         myCheckBoxHarpy.IsChecked = false;
         myCheckBoxMagician.IsChecked = false;
         myCheckBoxMercenary.IsChecked = false;
         myCheckBoxMerchant.IsChecked = false;
         myCheckBoxMinstrel.IsChecked = false;
         myCheckBoxMonk.IsChecked = false;
         myCheckBoxPorterSlave.IsChecked = false;
         myCheckBoxTrueLove.IsChecked = false;
         myCheckBoxWizard.IsChecked = false;
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
      private void ResetPartyOptions()
      {
         Option option = null;
         myCheckBoxPartyMounted.IsChecked = false;
         option = myOptions.Find("PartyMounted");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyOptions(): not found PartyMounted");
         myCheckBoxPartyAirborne.IsChecked = false;
         option = myOptions.Find("PartyAirborne");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyOptions(): not found PartyAirborne");
      }
      private void ResetHex()
      {
         Option option = null;
         myRadioButtonHexRandom.IsChecked = false;
         option = myOptions.Find("RandomHex");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomHex");
         myRadioButtonHexRandomTown.IsChecked = false;
         option = myOptions.Find("RandomHex");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomHex");
         myRadioButtonHexRandomLeft.IsChecked = false;
         option = myOptions.Find("RandomLeft");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomLeft");
         myRadioButtonHexRandomRight.IsChecked = false;
         option = myOptions.Find("RandomRight");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomRight");
         myRadioButtonHexRandomBottom.IsChecked = false;
         option = myOptions.Find("RandomBottom");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomBottom");
         myRadioButtonHexTown.IsChecked = false;
         option = myOptions.Find("0109");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0109");
         myRadioButtonHexRuin.IsChecked = false;
         option = myOptions.Find("0206");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0206");
         myRadioButtonHexTemple.IsChecked = false;
         option = myOptions.Find("0711");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0711");
         myRadioButtonHexHuldra.IsChecked = false;
         option = myOptions.Find("1212");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1212");
         myRadioButtonHexDrogat.IsChecked = false;
         option = myOptions.Find("0323");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0323");
         myRadioButtonHexLadyAeravir.IsChecked = false;
         option = myOptions.Find("1923");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1923");
         myRadioButtonHexFarmland.IsChecked = false;
         option = myOptions.Find("0418");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0418");
         option = myOptions.Find("0410");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0410");
         myRadioButtonHexForest.IsChecked = false;
         option = myOptions.Find("0409");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0409");
         myRadioButtonHexHill.IsChecked = false;
         option = myOptions.Find("0406");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0406");
         myRadioButtonHexMountain.IsChecked = false;
         option = myOptions.Find("0405");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0405");
         myRadioButtonHexSwamp.IsChecked = false;
         option = myOptions.Find("0411");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0411");
         myRadioButtonHexDesert.IsChecked = false;
         option = myOptions.Find("0407");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0407");
         myRadioButtonHexRoad.IsChecked = false;
         option = myOptions.Find("1905");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1905");
         myRadioButtonHexBottom.IsChecked = false;
         option = myOptions.Find("1723");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1723");
      }
      private void ResetMonsters()
      {
         Option option = null;
         myRadioButtonMonsterNormal.IsChecked = false;
         myRadioButtonMonsterLessEasy.IsChecked = false;
         option = myOptions.Find("LessHardMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found LessHardMonsters");
         myRadioButtonMonsterEasy.IsChecked = false;
         option = myOptions.Find("EasyMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasyMonsters");
         myRadioButtonMonsterEasiest.IsChecked = false;
         option = myOptions.Find("EasiestMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasiestMonsters");
      }
      private void ResetEvents()
      {
         Option option = null;
         myCheckBoxNoLostRoll.IsChecked = false;
         option = myOptions.Find("NoLostRoll");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found NoLostRoll");
         myCheckBoxNoLostEvent.IsChecked = false;
         option = myOptions.Find("ForceNoLostEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoLostEvent");
         myCheckBoxNoEvent.IsChecked = false;
         option = myOptions.Find("ForceNoEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoEvent");
         myCheckBoxNoRoadEvent.IsChecked = false;
         option = myOptions.Find("ForceNoRoadEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoRoadEvent");
         myCheckBoxNoCrossEvent.IsChecked = false;
         option = myOptions.Find("ForceNoCrossEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoCrossEvent");
         myCheckBoxNoRaftEvent.IsChecked = false;
         option = myOptions.Find("ForceNoRaftEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoRaftEvent");
         myCheckBoxNoAirEvent.IsChecked = false;
         option = myOptions.Find("ForceNoAirEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoAirEvent");
         myCheckBoxForceLostEvent.IsChecked = false;
         option = myOptions.Find("ForceLostEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceLostEvent");
         myCheckBoxForceLostAfterCross.IsChecked = false;
         option = myOptions.Find("ForceLostAfterCrossEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceLostAfterCrossEvent");
         myCheckBoxForceEvent.IsChecked = false;
         option = myOptions.Find("ForceLostEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceLostEvent");
         myCheckBoxForceCrossEvent.IsChecked = false;
         option = myOptions.Find("ForceCrossEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceCrossEvent");
         myCheckBoxForceRaftEvent.IsChecked = false;
         option = myOptions.Find("ForceRaftEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceRaftEvent");
         myCheckBoxForceAirEvent.IsChecked = false;
         option = myOptions.Find("ForceNoAirEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoAirEvent");
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void StackPanelSummary_Click(object sender, RoutedEventArgs e)
      {
         ResetPrince();
         ResetParty();
         ResetPartyMembers();
         ResetHex();
         ResetMonsters();
         ResetEvents();
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         switch (rb.Name)
         {
            case "myRadioButtonOriginal":
               myCheckBoxAutoLostIncrement.IsChecked = false;
               option = myOptions.Find("AutoLostDecrease");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
               else
                  option.IsEnabled = false;
               break;
            case "myRadioButtonRandomParty":
               option = myOptions.Find("RandomParty");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
               else
                  option.IsEnabled = true;
               break;
            case "myRadioButtonRandomStart":
               option = myOptions.Find("RandomHex");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
               else
                  option.IsEnabled = true;
               break;
            case "myRadioButtonAllRandom":
               option = myOptions.Find("PartyCustom");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
               else
                  option.IsEnabled = true;
               option = myOptions.Find("RandomHex");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
               else
                  option.IsEnabled = true;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelPrince_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxPrinceHorse": option = myOptions.Find("PrinceHorse"); break;
            case "myCheckBoxPrincePegasus": option = myOptions.Find("PrincePegasus"); break;
            case "myCheckBoxPrinceCoin": option = myOptions.Find("PrinceCoin"); break;
            case "myCheckBoxPrinceFood": option = myOptions.Find("PrinceFood"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): myOptions.Find() for name=" + cb.Name);
         else 
            option.IsEnabled = !option.IsEnabled;
         if ( false == UpdateDisplay(myOptions) )
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelParty_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetParty();
         switch (rb.Name)
         {
            case "myRadioButtonPartyOriginal": ResetPartyMembers(); break;
            case "myRadioButtonPartyRandom10": ResetPartyMembers(); option = myOptions.Find("RandomParty10"); break;
            case "myRadioButtonPartyRandom8": ResetPartyMembers(); option = myOptions.Find("RandomParty08"); break;
            case "myRadioButtonPartyRandom5": ResetPartyMembers(); option = myOptions.Find("RandomParty05"); break;
            case "myRadioButtonPartyRandom3": ResetPartyMembers(); option = myOptions.Find("RandomParty03"); break;
            case "myRadioButtonPartyRandom1": ResetPartyMembers(); option = myOptions.Find("RandomParty01"); break;
            case "myRadioButtonPartyCustom": option = myOptions.Find("PartyCustom"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelPartyMember_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxDwarf": option = myOptions.Find("Dwarf"); break;
            case "myCheckBoxEagle": option = myOptions.Find("Eagle"); break;
            case "myCheckBoxElf": option = myOptions.Find("Elf"); break;
            case "myCheckBoxFalcon": option = myOptions.Find("Falcon"); break;
            case "myCheckBoxGriffon": option = myOptions.Find("Griffon"); break;
            case "myCheckBoxHarpy": option = myOptions.Find("Harpy"); break;
            case "myCheckBoxMagician": option = myOptions.Find("Magician"); break;
            case "myCheckBoxMercenary": option = myOptions.Find("Mercenary"); break;
            case "myCheckBoxMerchant": option = myOptions.Find("Merchant"); break;
            case "myCheckBoxMinstrel": option = myOptions.Find("Minstrel"); break;
            case "myCheckBoxMonk": option = myOptions.Find("Monk"); break;
            case "myCheckBoxPorterSlave": option = myOptions.Find("PorterSlave"); break;
            case "myCheckBoxTrueLove": option = myOptions.Find("TrueLove"); break;
            case "myCheckBoxWizard": option = myOptions.Find("Wizard"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelPartyOption_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         ResetPartyOptions();
         switch (cb.Name)
         {
            case "myCheckBoxPartyMounted": option = myOptions.Find("PartyMounted"); break;
            case "myCheckBoxPartyAirborne": option = myOptions.Find("PartyAirborne"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelHex_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetHex();
         switch (rb.Name)
         {
            case "myRadioButtonHexOriginal": option = myOptions.Find("RandomParty10"); break;
            case "myRadioButtonHexRandom": option = myOptions.Find("RandomHex"); break;
            case "myRadioButtonHexRandomTown": option = myOptions.Find("RandomTown"); break;
            case "myRadioButtonHexRandomLeft": option = myOptions.Find("RandomLeft"); break;
            case "myRadioButtonHexRandomRight": option = myOptions.Find("RandomRight"); break;
            case "myRadioButtonHexRandomBottom": option = myOptions.Find("RandomBottom"); break;
            case "myRadioButtonHexTown": option = myOptions.Find("0109"); break;
            case "myRadioButtonHexRuin": option = myOptions.Find("0206"); break;
            case "myRadioButtonHexTemple": option = myOptions.Find("0711"); break;
            case "myRadioButtonHexHuldra": option = myOptions.Find("1212"); break;
            case "myRadioButtonHexDrogat": option = myOptions.Find("0323"); break;
            case "myRadioButtonHexLadyAeravir": option = myOptions.Find("1923"); break;
            case "myRadioButtonHexFarmland": option = myOptions.Find("0418"); break;
            case "myRadioButtonHexCountryside": option = myOptions.Find("0410"); break;
            case "myRadioButtonHexForest": option = myOptions.Find("0409"); break;
            case "myRadioButtonHexHill": option = myOptions.Find("0406"); break;
            case "myRadioButtonHexMountain": option = myOptions.Find("0405"); break;
            case "myRadioButtonHexSwamp": option = myOptions.Find("0411"); break;
            case "myRadioButtonHexDesert": option = myOptions.Find("0407"); break;
            case "myRadioButtonHexRoad": option = myOptions.Find("1905"); break;
            case "myRadioButtonHexBottom": option = myOptions.Find("1723"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): reached default name=" + rb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelMonster_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetMonsters();
         switch (rb.Name)
         {
            case "myRadioButtonMonsterNormal":
               if (false == UpdateDisplay(myOptions))
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): UpdateDisplay() returned false for name=" + rb.Name); 
               return;
            case "myRadioButtonMonsterLessEasy": option = myOptions.Find("LessHardMonsters"); break;
            case "myRadioButtonMonsterEasy": option = myOptions.Find("EasyMonsters"); break;
            case "myRadioButtonMonsterEasiest": option = myOptions.Find("EasiestMonsters"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): reached default name=" + rb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelEvent_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxNoLostRoll": option = myOptions.Find("NoLostRoll"); break;
            case "myCheckBoxNoLostEvent": option = myOptions.Find("ForceNoLostEvent"); break;
            case "myCheckBoxNoEvent": option = myOptions.Find("ForceNoEvent"); break;
            case "myCheckBoxNoRoadEvent": option = myOptions.Find("ForceNoRoadEvent"); break;
            case "myCheckBoxNoCrossEvent": option = myOptions.Find("ForceNoCrossEvent"); break;
            case "myCheckBoxNoRaftEvent": option = myOptions.Find("ForceNoRaftEvent"); break;
            case "myCheckBoxNoAirEvent": option = myOptions.Find("ForceNoAirEvent"); break;
            case "myCheckBoxForceLostEvent": option = myOptions.Find("ForceLostEvent"); break;
            case "myCheckBoxForceLostAfterCross": option = myOptions.Find("ForceLostAfterCrossEvent"); break;
            case "myCheckBoxForceEvent": option = myOptions.Find("ForceEvent"); break;
            case "myCheckBoxForceCrossEvent": option = myOptions.Find("ForceCrossEvent"); break;
            case "myCheckBoxForceRaftEvent": option = myOptions.Find("ForceRaftEvent"); break;
            case "myCheckBoxForceAirEvent": option = myOptions.Find("ForceAirEvent"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
   }
}
