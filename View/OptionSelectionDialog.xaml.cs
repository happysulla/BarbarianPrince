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
      private Options myOptions { get; set; } = null;
      public Options NewOptions { get => myOptions; }
      private bool myIsRandomGame = false;
      public OptionSelectionDialog(Options options)
      {
         InitializeComponent();
         myOptions = options.GetDeepCopy(); // make copy b/c do not want to change unless OK button selected by user
         myRadioButtonOriginal.ToolTip = "Play the game as God Intended. Be prepared to lose often.";
         myRadioButtonRandomParty.ToolTip = "Choose random party members.";
         myRadioButtonRandomStart.ToolTip = "Choose random starting hex.";
         myRadioButtonAllRandom.ToolTip = "Everything is random.";
         myRadioButtonMaxFun.ToolTip = "Recommended choices for maximizing fun.";
         myRadioButtonCustom.ToolTip = "Custom choices for all choices.";
         //--------------------
         myCheckBoxAutoSetup.ToolTip = "Skip initial tutorial when starting.";
         myCheckBoxAutoWealth.ToolTip = "All wealth rolls under 5 are automatically rolled.";
         //--------------------
         myCheckBoxPrinceHorse.ToolTip = "Prince starts with a hex.";
         myCheckBoxPrincePegasus.ToolTip = "Prince starts with a Pegasus mount.";
         myCheckBoxPrinceCoin.ToolTip = "Prince starts with 50-99 gold pieces.";
         myCheckBoxPrinceFood.ToolTip = "Prince starts with 10 food.";
         //--------------------
         myRadioButtonPartyOriginal.ToolTip = "Prince starts by himself.";
         myRadioButtonPartyRandom10.ToolTip = "10 random party members are added.";
         myRadioButtonPartyRandom8.ToolTip = "8 random party members are added.";
         myRadioButtonPartyRandom5.ToolTip = "5 random party members are added.";
         myRadioButtonPartyRandom3.ToolTip = "3 random party members are added.";
         myRadioButtonPartyRandom1.ToolTip = "1 random party member is added.";
         myRadioButtonPartyCustom.ToolTip = "Pick and choose what party members begin the game.";
         //--------------------
         myCheckBoxPartyMounted.ToolTip = "All party members get horse unless they are mounts.";
         myCheckBoxPartyAirborne.ToolTip = "All party members get Pegasus unless they are flying characters.";
         //--------------------
         myRadioButtonHexOriginal.ToolTip = "Party starts as originial game intended.";
         myRadioButtonHexRandom.ToolTip = "Party starts on random hex on the map.";
         myRadioButtonHexRandomTown.ToolTip = "Party starts on random town hex.";
         myRadioButtonHexRandomLeft.ToolTip = "Party starts on random left edge.";
         myRadioButtonHexRandomRight.ToolTip = "Party starts on random right edge.";
         myRadioButtonHexRandomBottom.ToolTip = "Party starts on random bottom edge.";
         //--------------------
         myRadioButtonMonsterNormal.ToolTip = "Monsters start with original game attributes.";
         myRadioButtonMonsterLessEasy.ToolTip = "Monsters subtract 2 from endurance and 3 from combat.";
         myRadioButtonMonsterEasy.ToolTip = "Monsters subtract 1 from endurance and combat.";
         myRadioButtonMonsterEasiest.ToolTip = "Monsters have one endurance and combat.";
         //--------------------
         myCheckBoxAutoLostIncrement.ToolTip = "Lost chance decreases on consecutive lost rolls.";
         myCheckBoxExtendTime.ToolTip = "Extend end time from 70 days to 105 days.";
         myCheckBoxReducedLodgingCosts.ToolTip = "Lodging in structures is half price.";
         myCheckBoxAddIncome.ToolTip = "Add 3-6gp at end of each day performing menial tasks during daily activities if not incapacitated.";
         //--------------------
         myCheckBoxNoLostRoll.ToolTip = "Skip Lost Rolls.";
         myCheckBoxNoLostEvent.ToolTip = "Lost encounters never occur.";
         myCheckBoxNoEvent.ToolTip = "Travel encounters never occur. Lost encounters may still occur.";
         myCheckBoxNoRoadEvent.ToolTip = "Road encounters never occur.";
         myCheckBoxNoCrossEvent.ToolTip = "Crossing river encounters never occur.";
         myCheckBoxNoRaftEvent.ToolTip = "Raft encounters never occur when rafting.";
         myCheckBoxNoAirEvent.ToolTip = "Air encounters never occur when flying.";
         myCheckBoxForceLostEvent.ToolTip = "Lost encounters always occur when lost roll is made.";
         myCheckBoxForceLostAfterCross.ToolTip = "Lost encounter always occurs after crossing river.";
         myCheckBoxForceEvent.ToolTip = "Ground encounter always occurs.";
         myCheckBoxForceCrossEvent.ToolTip = "River crossing encounter always occurs.";
         myCheckBoxForceRaftEvent.ToolTip = "Raft encounter always occurs.";
         myCheckBoxForceAirEvent.ToolTip = "Air encounter always occurs when flying.";
         if (false == UpdateDisplay(myOptions))
         {
            Logger.Log(LogEnum.LE_ERROR, "OptionSelectionDialog(): UpdateDisplay() returned false");
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
         bool isFunOption = true;
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-AutoWealthRollForUnderFive");
            return false;
         }
         myCheckBoxAutoWealth.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Prince 
         option = options.Find("PrinceHorse");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PrinceHorse");
            return false;
         }
         myCheckBoxPrinceHorse.IsChecked = option.IsEnabled;
         if( true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
        //-------------------------
         option = options.Find("PrincePegasus");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PrincePegasus");
            return false;
         }
         myCheckBoxPrincePegasus.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("PrinceCoin");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PrinceCoin");
            return false;
         }
         myCheckBoxPrinceCoin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("PrinceFood");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PrinceFood");
            return false;
         }
         myCheckBoxPrinceFood.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Party Members
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
         option = options.Find("RandomParty10");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomParty10");
            return false;
         }
         if( true == option.IsEnabled )
         {
            myRadioButtonPartyRandom10.IsChecked = true;
            isRandomPartyConfig = true;
            isFunOption = false;
         }
         else
         {
            option = options.Find("RandomParty08");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomParty08");
               return false;
            }
            if (true == option.IsEnabled)
            {
               myRadioButtonPartyRandom8.IsChecked = true;
               isRandomPartyConfig = true;
               isFunOption = false;
            }
            else
            {
               option = options.Find("RandomParty05");
               if (null == option)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomParty05");
                  return false;
               }
               if (true == option.IsEnabled)
               {
                  myRadioButtonPartyRandom5.IsChecked = true;
                  isRandomPartyConfig = true;
                  isFunOption = false;
               }
               else
               {
                  option = options.Find("RandomParty03");
                  if (null == option)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomParty03");
                     return false;
                  }
                  if (true == option.IsEnabled)
                  {
                     myRadioButtonPartyRandom3.IsChecked = true;
                     isRandomPartyConfig = true;
                     isFunOption = false;
                  }
                  else
                  {
                     option = options.Find("RandomParty01");
                     if (null == option)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomParty01");
                        return false;
                     }
                     if (true == option.IsEnabled)
                     {
                        myRadioButtonPartyRandom1.IsChecked = true;
                        isRandomPartyConfig = true;
                        isFunOption = false;
                     }
                     else
                     {
                        option = options.Find("PartyCustom");
                        if (null == option)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PartyCustom");
                           return false;
                        }
                        if (true == option.IsEnabled)
                        {
                           myRadioButtonPartyCustom.IsChecked = true;
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
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Dwarf");
                              return false;
                           }
                           myCheckBoxDwarf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Eagle");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Eagle");
                              return false;
                           }
                           myCheckBoxEagle.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Elf");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Elf");
                              return false;
                           }
                           myCheckBoxElf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                           }
                           else
                           {
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Falcon");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Falcon");
                              return false;
                           }
                           myCheckBoxFalcon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Griffon");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Griffon");
                              return false;
                           }
                           myCheckBoxGriffon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Harpy");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Harpy");
                              return false;
                           }
                           myCheckBoxHarpy.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Magician");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Magician");
                              return false;
                           }
                           myCheckBoxMagician.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                           }
                           else
                           {
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Mercenary");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Mercenary");
                              return false;
                           }
                           myCheckBoxMercenary.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                           }
                           else
                           {
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Merchant");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Merchant");
                              return false;
                           }
                           myCheckBoxMerchant.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Minstrel");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Minstrel");
                              return false;
                           }
                           myCheckBoxMinstrel.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Monk");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Monk");
                              return false;
                           }
                           myCheckBoxMonk.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                           }
                           else
                           {
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("PorterSlave");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PorterSlave");
                              return false;
                           }
                           myCheckBoxPorterSlave.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("TrueLove");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-TrueLove");
                              return false;
                           }
                           myCheckBoxTrueLove.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           option = options.Find("Wizard");
                           if (null == option)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-Wizard");
                              return false;
                           }
                           myCheckBoxWizard.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                        }
                     }
                  }
               }
            }
         }
         if( (false == isRandomPartyConfig ) && (false == isCustomPartyConfig) )
            myRadioButtonPartyOriginal.IsChecked = true;
         //------------------------------------------------
         option = options.Find("PartyMounted");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PartyMounted");
            return false;
         }
         myCheckBoxPartyMounted.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         option = options.Find("PartyAirborne");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-PartyAirborne");
            return false;
         }
         myCheckBoxPartyAirborne.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("RandomHex");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomHex");
            return false;
         }
         myRadioButtonHexRandom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("RandomTown");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomTown");
            return false;
         }
         myRadioButtonHexRandomTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("RandomLeft");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomLeft");
            return false;
         }
         myRadioButtonHexRandomLeft.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("RandomRight");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomRight");
            return false;
         }
         myRadioButtonHexRandomRight.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("RandomBottom");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-RandomBottom");
            return false;
         }
         myRadioButtonHexRandomBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0109");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0109");
            return false;
         }
         myRadioButtonHexTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0206");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0206");
            return false;
         }
         myRadioButtonHexRuin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0708");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0708");
            return false;
         }
         myRadioButtonHexRiver.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0711");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0711");
            return false;
         }
         myRadioButtonHexTemple.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("1212");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-1212");
            return false;
         }
         myRadioButtonHexHuldra.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0323");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0323");
            return false;
         }
         myRadioButtonHexDrogat.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("1923");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-1923");
            return false;
         }
         myRadioButtonHexLadyAeravir.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0418");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0418");
            return false;
         }
         myRadioButtonHexFarmland.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0722");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0722");
            return false;
         }
         myRadioButtonHexCountryside.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0409");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0409");
            return false;
         }
         myRadioButtonHexForest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0406");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0406");
            return false;
         }
         myRadioButtonHexHill.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0405");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0405");
            return false;
         }
         myRadioButtonHexMountain.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0411");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0411");
            return false;
         }
         myRadioButtonHexSwamp.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("0407");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-0407");
            return false;
         }
         myRadioButtonHexDesert.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("1905");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-1905");
            return false;
         }
         myRadioButtonHexRoad.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("1723");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-1723");
            return false;
         }
         myRadioButtonHexBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         if ((false == isRandomHexConfig) && (false == isCustomHexConfig))
            myRadioButtonHexOriginal.IsChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Monsters
         option = options.Find("EasiestMonsters");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-EasiestMonsters");
            return false;
         }
         myRadioButtonMonsterEasiest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         else
         {
            option = options.Find("EasyMonsters");
            if (null == option)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-EasiestMonsters");
               return false;
            }
            myRadioButtonMonsterEasy.IsChecked = option.IsEnabled;
            if (true == option.IsEnabled)
            {
               isCustomConfig = true;
            }
            else
            {
               isFunOption = false;
               option = options.Find("LessHardMonsters");
               if (null == option)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-LessHardMonsters");
                  return false;
               }
               myRadioButtonMonsterLessEasy.IsChecked = option.IsEnabled;
               if (true == option.IsEnabled)
               {
                  isCustomConfig = true;
                  isFunOption = false;
               }
               else
               {
                  myRadioButtonMonsterNormal.IsChecked = true;
               }
            }
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Game Options
         option = options.Find("AutoLostDecrease");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-AutoLostDecrease");
            return false;
         }
         myCheckBoxAutoLostIncrement.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         option = options.Find("ExtendEndTime");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ExtendEndTime");
            return false;
         }
         myCheckBoxExtendTime.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         option = options.Find("ReduceLodgingCosts");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ReduceLodgingCosts");
            return false;
         }
         myCheckBoxReducedLodgingCosts.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         option = options.Find("SteadyIncome");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-SteadyIncome");
            return false;
         }
         myCheckBoxAddIncome.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("NoLostRoll");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-NoLostRoll");
            return false;
         }
         myCheckBoxNoLostRoll.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoLostEvent");
            return false;
         }
         myCheckBoxNoLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceLostEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceLostEvent");
            return false;
         }
         myCheckBoxForceLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoEvent");
            return false;
         }
         myCheckBoxNoEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceEvent");
            return false;
         }
         myCheckBoxForceEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoRoadEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoRoadEvent");
            return false;
         }
         myCheckBoxNoRoadEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoAirEvent");
            return false;
         }
         myCheckBoxNoAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceAirEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceAirEvent");
            return false;
         }
         myCheckBoxForceAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoCrossEvent");
            return false;
         }
         myCheckBoxNoCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceCrossEvent");
            return false;
         }
         myCheckBoxForceCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceLostAfterCrossEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceLostAfterCrossEvent");
            return false;
         }
         myCheckBoxForceLostAfterCross.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceNoRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceNoRaftEvent");
            return false;
         }
         myCheckBoxNoRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         option = options.Find("ForceRaftEvent");
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDisplay(): option=null for Find()-ForceRaftEvent");
            return false;
         }
         myCheckBoxForceRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Summary Selection
         if (true == isFunOption)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = true;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == myIsRandomGame)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = true;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if ((true == isCustomConfig) || (true == isCustomPartyConfig) || (true == isCustomHexConfig))
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = true;
         }
         else if (true == isRandomPartyConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = true;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomHexConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = true;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else
         {
            myRadioButtonOriginal.IsChecked = true;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         return true;
      }
      private void ResetPrince()
      {
         Option option = null;
         option = myOptions.Find("PrinceHorse");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found PrinceHorse");
         option = myOptions.Find("PrincePegasus");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found PrincePegasus");
         option = myOptions.Find("PrinceCoin");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found PrinceCoin");
         option = myOptions.Find("PrinceFood");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPrince(): not found PrinceFood");
      }
      private void ResetParty()
      {
         Option option = null;
         option = myOptions.Find("RandomParty10");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty10");
         option = myOptions.Find("RandomParty08");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty08");
         option = myOptions.Find("RandomParty05");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty05");
         option = myOptions.Find("RandomParty03");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty03");
         option = myOptions.Find("RandomParty01");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found RandomParty01");
         option = myOptions.Find("PartyCustom");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetParty(): not found PartyCustom");
      }
      private void ResetPartyMembers()
      {
         Option option = null;
         option = myOptions.Find("Dwarf");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Dwarf");
         option = myOptions.Find("Eagle");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Eagle");
         option = myOptions.Find("Elf");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Elf");
         option = myOptions.Find("Falcon");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Falcon");
         option = myOptions.Find("Griffon");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Griffon");
         option = myOptions.Find("Harpy");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Harpy");
         option = myOptions.Find("Magician");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Magician");
         option = myOptions.Find("Mercenary");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Mercenary");
         option = myOptions.Find("Merchant");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Merchant");
         option = myOptions.Find("Minstrel");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Minstrel");
         option = myOptions.Find("Monk");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Monk");
         option = myOptions.Find("PorterSlave");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found PorterSlave");
         option = myOptions.Find("TrueLove");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found TrueLove");
         option = myOptions.Find("Wizard");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyMembers(): not found Wizard");
      }
      private void ResetPartyOptions()
      {
         Option option = null;
         option = myOptions.Find("PartyMounted");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyOptions(): not found PartyMounted");
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
         option = myOptions.Find("RandomTown");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomTown");
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
         myRadioButtonHexRiver.IsChecked = false;
         option = myOptions.Find("0708");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0708");
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
         myRadioButtonHexCountryside.IsChecked = false;
         option = myOptions.Find("0722");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0722");
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
         option = myOptions.Find("LessHardMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found LessHardMonsters");
         option = myOptions.Find("EasyMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasyMonsters");
         option = myOptions.Find("EasiestMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasiestMonsters");
      }
      private void ResetGameOptions()
      {
         Option option = null;
         option = myOptions.Find("AutoLostDecrease");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found AutoLostDecrease");
         option = myOptions.Find("ExtendEndTime");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found ExtendEndTime");
         option = myOptions.Find("ReduceLodgingCosts");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found ReduceLodgingCosts");
         option = myOptions.Find("SteadyIncome");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found SteadyIncome");
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
         option = myOptions.Find("ForceEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceEvent");
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
      private void SelectRandomPartyChoice()
      {
         int choice = Utilities.RandomGenerator.Next(6);
         Option option = null;
         switch(choice)
         {
            case 0:  break;
            case 1: option = myOptions.Find("RandomParty10"); break;
            case 2: option = myOptions.Find("RandomParty08"); break;
            case 3: option = myOptions.Find("RandomParty05"); break;
            case 4: option = myOptions.Find("RandomParty03"); break;
            case 5: option = myOptions.Find("RandomParty01"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyChoice: reached default choice=" + choice.ToString()); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyChoice(): myOptions.Find() for choice=" + choice.ToString());
         else
            option.IsEnabled = !option.IsEnabled;
      }
      private void SelectRandomHexChoice()
      {
         int choice = Utilities.RandomGenerator.Next(21);
         Option option = null;
         switch (choice)
         {
            case 0: break;
            case 1: option = myOptions.Find("RandomHex"); break;
            case 2: option = myOptions.Find("RandomTown"); break;
            case 3: option = myOptions.Find("RandomLeft"); break;
            case 4: option = myOptions.Find("RandomRight"); break;
            case 5: option = myOptions.Find("RandomBottom"); break;
            case 6: option = myOptions.Find("0109"); break;
            case 7: option = myOptions.Find("0206"); break;
            case 8: option = myOptions.Find("0711"); break;
            case 9: option = myOptions.Find("1212"); break;
            case 10: option = myOptions.Find("0323"); break;
            case 11: option = myOptions.Find("1923"); break;
            case 12: option = myOptions.Find("0418"); break;
            case 13: option = myOptions.Find("0722"); break;
            case 14: option = myOptions.Find("0409"); break;
            case 15: option = myOptions.Find("0406"); break;
            case 16: option = myOptions.Find("0405"); break;
            case 17: option = myOptions.Find("0411"); break;
            case 18: option = myOptions.Find("0407"); break;
            case 19: option = myOptions.Find("1905"); break;
            case 20: option = myOptions.Find("1723"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "SelectRandomHexChoice: reached default choice=" + choice.ToString()); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomHexChoice(): myOptions.Find() for choice=" + choice.ToString());
         else
            option.IsEnabled = !option.IsEnabled;
      }
      private void SelectRandomPrinceChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if( 1 == choice )
         {
            Option option = myOptions.Find("PrinceHorse");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceHorse");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrincePegasus");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrincePegasus");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrinceCoin");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceCoin");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrinceFood");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceFood");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectRandomPartyOptionChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PartyMounted");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PartyMounted");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PartyAirborne");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PartyAirborne");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectRandomGameOptionChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("AutoLostDecrease");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=AutoLostDecrease");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("ExtendEndTime");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ExtendEndTime");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("ReduceLodgingCosts");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ReduceLodgingCosts");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("SteadyIncome");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=SteadyIncome");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectFunGameOptions()
      {
         Option option = myOptions.Find("AutoLostDecrease");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=AutoLostDecrease");
         else
            option.IsEnabled = true;
         option = myOptions.Find("ExtendEndTime");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ExtendEndTime");
         else
            option.IsEnabled = true;
         option = myOptions.Find("ReduceLodgingCosts");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ReduceLodgingCosts");
         else
            option.IsEnabled = true;
         option = myOptions.Find("SteadyIncome");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=SteadyIncome");
         else
            option.IsEnabled = true;
         option = myOptions.Find("EasyMonsters");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=EasyMonsters");
         else
            option.IsEnabled = true;
         option = myOptions.Find("PrinceFood");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=PrinceFood");
         else
            option.IsEnabled = true;
         option = myOptions.Find("PrinceCoin");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=PrinceCoin");
         else
            option.IsEnabled = true;
         option = myOptions.Find("PartyCustom");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=PartyCustom");
         else
            option.IsEnabled = true;
         option = myOptions.Find("Elf");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=Elf");
         else
            option.IsEnabled = true;
         option = myOptions.Find("Magician");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=Magician");
         else
            option.IsEnabled = true;
         option = myOptions.Find("Mercenary");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=Mercenary");
         else
            option.IsEnabled = true;
         option = myOptions.Find("Monk");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=Monk");
         else
            option.IsEnabled = true;
         option = myOptions.Find("RandomHex");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=RandomHex");
         else
            option.IsEnabled = true;
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void StackPanelSummary_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         ResetPrince();
         ResetParty();
         ResetPartyMembers();
         ResetPartyOptions();
         ResetHex();
         ResetMonsters();
         ResetGameOptions();
         ResetEvents();
         myIsRandomGame = false;
         switch (rb.Name)
         {
            case "myRadioButtonOriginal":
               break;
            case "myRadioButtonRandomParty":
               SelectRandomPartyChoice();
               break;
            case "myRadioButtonRandomStart":
               SelectRandomHexChoice();
               break;
            case "myRadioButtonAllRandom":
               myIsRandomGame = true;
               SelectRandomPartyChoice();
               SelectRandomHexChoice();
               SelectRandomPrinceChoice();
               SelectRandomGameOptionChoice();
               SelectRandomPartyOptionChoice();
               break;
            case "myRadioButtonMaxFun":
               SelectFunGameOptions();
               break;
            case "myRadioButtonCustom":
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelOptions_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxAutoSetup": 
               option = myOptions.Find("AutoSetup");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxAutoWealth": 
               option = myOptions.Find("AutoWealthRollForUnderFive");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): reached default name=" + cb.Name); return;
         }
      }
      private void StackPanelPrince_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         myIsRandomGame = false;
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
            case "myRadioButtonPartyRandom10": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty10"); break;
            case "myRadioButtonPartyRandom8": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty08"); break;
            case "myRadioButtonPartyRandom5": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty05"); break;
            case "myRadioButtonPartyRandom3": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty03"); break;
            case "myRadioButtonPartyRandom1": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty01"); break;
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
         myIsRandomGame = false;
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
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelPartyOption_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxPartyMounted": myIsRandomGame = false; option = myOptions.Find("PartyMounted"); break;
            case "myCheckBoxPartyAirborne": myIsRandomGame = false; option = myOptions.Find("PartyAirborne"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelGameOption_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxAutoLostIncrement": option = myOptions.Find("AutoLostDecrease"); break;
            case "myCheckBoxExtendTime": option = myOptions.Find("ExtendEndTime"); break;
            case "myCheckBoxReducedLodgingCosts": option = myOptions.Find("ReduceLodgingCosts"); break;
            case "myCheckBoxAddIncome": option = myOptions.Find("SteadyIncome"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelHex_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetHex();
         switch (rb.Name)
         {
            case "myRadioButtonHexOriginal":
               if (false == UpdateDisplay(myOptions))
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): UpdateDisplay() returned false for name=" + rb.Name);
               return;
            case "myRadioButtonHexRandom": option = myOptions.Find("RandomHex"); break;
            case "myRadioButtonHexRandomTown": option = myOptions.Find("RandomTown"); break;
            case "myRadioButtonHexRandomLeft": option = myOptions.Find("RandomLeft"); break;
            case "myRadioButtonHexRandomRight": option = myOptions.Find("RandomRight"); break;
            case "myRadioButtonHexRandomBottom": option = myOptions.Find("RandomBottom"); break;
            case "myRadioButtonHexTown": option = myOptions.Find("0109"); break;
            case "myRadioButtonHexRuin": option = myOptions.Find("0206"); break;
            case "myRadioButtonHexRiver": option = myOptions.Find("0708"); break;
            case "myRadioButtonHexTemple": option = myOptions.Find("0711"); break;
            case "myRadioButtonHexHuldra": option = myOptions.Find("1212"); break;
            case "myRadioButtonHexDrogat": option = myOptions.Find("0323"); break;
            case "myRadioButtonHexLadyAeravir": option = myOptions.Find("1923"); break;
            case "myRadioButtonHexFarmland": option = myOptions.Find("0418"); break;
            case "myRadioButtonHexCountryside": option = myOptions.Find("0722"); break;
            case "myRadioButtonHexForest": option = myOptions.Find("0409"); break;
            case "myRadioButtonHexHill": option = myOptions.Find("0406"); break;
            case "myRadioButtonHexMountain": option = myOptions.Find("0405"); break;
            case "myRadioButtonHexSwamp": option = myOptions.Find("0411"); break;
            case "myRadioButtonHexDesert": option = myOptions.Find("0407"); break;
            case "myRadioButtonHexRoad": option = myOptions.Find("1905"); break;
            case "myRadioButtonHexBottom": option = myOptions.Find("1723"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): reached default name=" + rb.Name); return;
         }
         myIsRandomGame = false;
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
         myIsRandomGame = false;
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
         myIsRandomGame = false;
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
