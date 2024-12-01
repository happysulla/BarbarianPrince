﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;

namespace BarbarianPrince
{
   [Serializable]
   public struct BloodSpot
   {
      public int mySize;      // diameter  of blood spot
      public double myLeft;   // left of where blood spot exists on canvas
      public double myTop;    // top of where blood spot exists on canvas
      public BloodSpot(int range, Random r)
      {
         mySize = r.Next(8) + 5;
         myLeft = r.Next(range);
         myTop = r.Next(range);
      }
      public BloodSpot(int size, double left, double top)
      {
         mySize = size;
         myLeft = left;
         myTop = top;
      }
   }
   [Serializable]
   public class MapItem : IMapItem
   {
      private const double PERCENT_MAPITEM_COVERED = 40.0;
      private const int NOT_FLYING = -1000;
      [NonSerialized] public static IMapImages theMapImages = new MapImages();
      [NonSerialized] private static BitmapImage theAlly = theMapImages.GetBitmapImage("Ally");
      [NonSerialized] private static BitmapImage theMuscle = theMapImages.GetBitmapImage("Muscle");
      [NonSerialized] private static BitmapImage theCoinPileSingle = theMapImages.GetBitmapImage("CoinPileSingle");
      [NonSerialized] private static BitmapImage theCoinBar = theMapImages.GetBitmapImage("CoinBar");
      [NonSerialized] private static BitmapImage theFugitive = theMapImages.GetBitmapImage("Fugitive");
      [NonSerialized] private static BitmapImage theStructureDeny = theMapImages.GetBitmapImage("StructureDeny");
      [NonSerialized] private static BitmapImage theGroup = theMapImages.GetBitmapImage("Group");
      [NonSerialized] private static BitmapImage thePlagueDust = theMapImages.GetBitmapImage("OPlagueDust");
      [NonSerialized] private static BitmapImage theBloodSpot = theMapImages.GetBitmapImage("OBlood1");
      [NonSerialized] private static BitmapImage thePoisonSpot = theMapImages.GetBitmapImage("OPoison");
      [NonSerialized] private static BitmapImage thResurrected = theMapImages.GetBitmapImage("Resurrected");
      [NonSerialized] private static BitmapImage theMia = theMapImages.GetBitmapImage("OUNC");
      [NonSerialized] private static BitmapImage theRunAway = theMapImages.GetBitmapImage("ORUNS");
      [NonSerialized] private static BitmapImage theKia = theMapImages.GetBitmapImage("OKIA");
      [NonSerialized] private static Random theRandom = new Random();
      //--------------------------------------------------
      public string Name { get; set; } = "";
      public string TopImageName { get; set; } = "";
      public string BottomImageName { get; set; } = "";
      public string OverlayImageName { get; set; } = "";
      public List<BloodSpot> myWoundSpots = new List<BloodSpot>();
      public List<BloodSpot> WoundSpots { get => myWoundSpots; }
      public List<BloodSpot> myPoisonSpots = new List<BloodSpot>();
      public List<BloodSpot> PoisonSpots { get => myPoisonSpots; }
      public double Zoom { get; set; } = 1.0;
      public bool IsHidden { get; set; } = false;
      public bool IsExposedToUser { get; set; } = false;
      //--------------------------------------------------
      public int Endurance { get; set; } = 0;
      public int Movement { get; set; } = 4;
      public int Combat { get; set; } = 0;
      public int Wound { get; set; } = 0;
      public int Poison { get; set; } = 0;
      public int Coin { get; set; } = 0;
      public int WealthCode { get; set; } = 0;
      public int Food { get; set; } = 0;
      public int StarveDayNum { get; set; } = 0;
      public int StarveDayNumOld { get; set; } = 0;
      public int MovementUsed { get; set; } = 0;
      //--------------------------------------------------
      public IMapItem Rider { get; set; } = null;
      public IMapItems Mounts { get; set; } = new MapItems();
      public IMapItems LeftOnGroundMounts { get; set; } = new MapItems();
      public Dictionary<IMapItem, int> CarriedMembers { get; set; } = new Dictionary<IMapItem, int>();
      //--------------------------------------------------
      public ITerritory Territory { get; set; } = null;
      public ITerritory TerritoryStarting { get; set; } = null;
      public ITerritories GuideTerritories { get; set; } = new Territories();
      public IMapPoint Location { get; set; } = new MapPoint(0.0, 0.0);
      //--------------------------------------------------
      public bool IsGuide { get; set; } = false;
      public bool IsKilled { get; set; } = false;
      public bool IsUnconscious { get; set; } = false;
      public bool IsRunAway { get; set; } = false;
      public bool IsExhausted { get; set; } = false;
      public bool IsSunStroke { get; set; } = false;
      public bool IsPlagued { get; set; } = false;
      public int PlagueDustWound { get; set; } = 0;
      public bool IsPlayedMusic { get; set; } = false;
      public bool IsCatchCold { get; set; } = false;
      public bool IsMountSick { get; set; } = false;
      public bool IsShowFireball { get; set; } = false;
      public bool IsDisappear { get; set; } = false;
      public bool IsRiding { get; set; } = false;
      public bool IsFlying { get; set; } = false;
      public bool IsSecretGatewayToDarknessKnown { set; get; } = false;
      public bool IsFugitive { set; get; } = false;
      public bool IsPoisonApplied { get; set; } = false;
      public bool IsResurrected { get; set; } = false;
      public bool IsShieldApplied { get; set; } = false;
      public bool IsTrueLove { set; get; } = false;
      public bool IsFickle { set; get; } = false;
      public int GroupNum { set; get; } = 0;
      public int PayDay { set; get; } = 0;
      public int Wages { set; get; } = 0;
      public bool IsAlly { set; get; } = false;
      public bool IsLooter { set; get; } = false;
      public bool IsTownCastleTempleLeave { set; get; } = false;
      //--------------------------------------------------
      public bool IsMoved { get; set; } = false;
      private bool myIsFlipped = false;
      public bool IsAnimated
      {
         set
         {
            IMapImage mii = theMapImages.Find(this.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.set() could not find map image for " + this.TopImageName);
               return;
            }
            mii.IsAnimated = value;
         }
         get
         {
            IMapImage mii = theMapImages.Find(this.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.get() could not find map image for " + this.TopImageName);
               return false;
            }
            return mii.IsAnimated;
         }
      }
      //----------------------------------------------------------------------------
      public List<SpecialEnum> SpecialKeeps { get => mySpecialKeeps; } // Special possessions that cannot be shared
      public List<SpecialEnum> SpecialShares { get => mySpecialShares; } // Special possessions that can be given away
      private List<SpecialEnum> mySpecialKeeps = new List<SpecialEnum>();
      private List<SpecialEnum> mySpecialShares = new List<SpecialEnum>();
      //----------------------------------------------------------------------------
      public MapItem() { }
      public MapItem(string aName, double zoom, bool isHidden, bool isAnimated, string topImageName)
      {
         try
         {
            this.Name = aName;
            this.Zoom = zoom;
            this.IsHidden = isHidden;
            this.Location = null;
            this.TopImageName = topImageName;
            this.BottomImageName = null;
            this.Territory = null;
            IMapImage mii = theMapImages.Find(topImageName);
            if (null == mii)
            {
               mii = (IMapImage)new MapImage(topImageName);
               theMapImages.Add(mii);
            }
            this.IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      public MapItem(IMapItem mi)
      {
         foreach (SpecialEnum item in mi.SpecialKeeps)
            this.AddSpecialItemToKeep(item);
         foreach (SpecialEnum item in mi.SpecialShares)
            this.AddSpecialItemToShare(item);
         if ("Prince" == mi.Name) // there is only once prince
         {
            this.Name = mi.Name;
         }
         else
         {
            this.Name = mi.Name + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
         }
         this.Endurance = mi.Endurance;
         this.Movement = mi.Movement;
         this.Combat = mi.Combat;
         this.TopImageName = mi.TopImageName;
         this.BottomImageName = mi.BottomImageName;
         this.OverlayImageName = mi.OverlayImageName;
         this.Zoom = mi.Zoom;
         this.IsHidden = mi.IsHidden;
         this.IsAnimated = mi.IsAnimated;
         this.Location = mi.Location;
         //--------------------------------------------------
         this.StarveDayNum = mi.StarveDayNum;
         this.StarveDayNumOld = mi.StarveDayNumOld;
         this.IsUnconscious = mi.IsUnconscious;
         this.IsKilled = mi.IsKilled;
         this.IsRunAway = mi.IsRunAway;
         this.IsPlagued = mi.IsPlagued;
         this.PlagueDustWound = mi.PlagueDustWound;
         this.Wound = mi.Wound;
         this.Poison = mi.Poison;
         this.Coin = mi.Coin;
         this.WealthCode = mi.WealthCode;
         this.Food = mi.Food;
         this.MovementUsed = MovementUsed;
         //--------------------------------------------------
         foreach (IMapItem mount in mi.Mounts)
         {
            IMapItem mountClone = new MapItem(mount);
            this.Mounts.Add(mountClone);
         }
         //--------------------------------------------------
         this.Territory = mi.Territory;
         this.TerritoryStarting = mi.TerritoryStarting;
         this.GuideTerritories = mi.GuideTerritories;
         //--------------------------------------------------
         this.IsGuide = mi.IsGuide;
         this.IsRiding = mi.IsRiding;
         this.IsFlying = mi.IsFlying;
         this.IsSecretGatewayToDarknessKnown = mi.IsSecretGatewayToDarknessKnown;
         this.IsPoisonApplied = mi.IsPoisonApplied;
         this.IsFickle = mi.IsFickle;
         this.Wages = mi.Wages;
         this.IsAlly = mi.IsAlly;
         this.IsLooter = mi.IsLooter;
         this.IsTownCastleTempleLeave = mi.IsTownCastleTempleLeave;
      }
      protected MapItem(string name, bool isDynamic)
      {
         this.Name = name;
      }
      public MapItem(string aName, double zoom, bool isHidden, bool isAnimated, bool isGuide, string topImageName, string bottomImageName, IMapPoint aStartingPoint)
      {

         this.Name = aName;
         this.Zoom = zoom;
         this.IsHidden = isHidden;
         this.Location = aStartingPoint;
         this.TopImageName = topImageName;
         this.BottomImageName = bottomImageName;
         this.Territory = null;
         try
         {
            IMapImage mii = theMapImages.Find(topImageName);
            if (null == mii)
            {
               mii = (IMapImage)new MapImage(topImageName);
               theMapImages.Add(mii);
            }
            mii = theMapImages.Find(bottomImageName);
            if (null == mii)
            {
               mii = (IMapImage)new MapImage(bottomImageName);
               theMapImages.Add(mii);
            }
            this.IsAnimated = isAnimated; // This must come after the creating of the image
            this.IsGuide = isGuide; // This must come after the creating of the image
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      public MapItem(string aName, double zoom, bool isHidden, bool isAnimated, bool isGuide, string topImageName, string bottomImageName, MapPoint aStartingPoint, ITerritory territory) :
        this(aName, zoom, isHidden, isAnimated, isGuide, topImageName, bottomImageName, aStartingPoint)
      {
         Territory = territory;
         TerritoryStarting = territory;
      }
      public MapItem(string aName, double zoom, bool isHidden, bool isAnimated, bool isGuide, string topImageName, string bottomImageName, ITerritory territory, int endurance, int combat, int wealthCode) :
         this(aName, zoom, isHidden, isAnimated, isGuide, topImageName, bottomImageName, territory.CenterPoint)
      {
         Territory = territory;
         TerritoryStarting = territory;
         Endurance = endurance;
         Combat = combat;
         WealthCode = wealthCode;
      }
      //----------------------------------------------------------------------------
      public bool IsSpecialist()
      {
         if (true == this.Name.Contains("Priest"))
            return true;
         else if (true == this.Name.Contains("Monk"))
            return true;
         else if (true == this.Name.Contains("Magician"))
            return true;
         else if (true == this.Name.Contains("Wizard"))
            return true;
         else if (true == this.Name.Contains("Witch"))
            return true;
         return false;
      }
      public bool IsMagicUser()
      {
         if (true == this.Name.Contains("Magician"))
            return true;
         else if (true == this.Name.Contains("Wizard"))
            return true;
         else if (true == this.Name.Contains("Witch"))
            return true;
         return false;
      }
      public void SetLocation(int counterCount)
      {
         this.Location = new MapPoint(this.Territory.CenterPoint.X - Utilities.theMapItemOffset + (counterCount * Utilities.STACK), this.Territory.CenterPoint.Y - Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
      }
      public bool AddNewMount(MountEnum mt = MountEnum.Horse)
      {
         if (true == this.IsFlyer())
         {
            Logger.Log(LogEnum.LE_ERROR, "AddNewMount(): name=" + Name + " cannot have mounts");
            return false;
         }
         if (MountEnum.Pegasus == mt)
         {
            string mountName = "Pegasus" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem pegasus = new MapItem(mountName, 1.0, false, false, false, "MPegasus", "MPegasus", this.Territory, 0, 0, 0);
            Logger.Log(LogEnum.LE_MOUNT_CHANGE, "AddNewMount(): add=" + mountName + " for mi=" + Name);
            this.Mounts.Add(pegasus);
            if (false == this.Name.Contains("Giant"))  // mounts cannot carry giants
            {
               this.IsRiding = true;                   // assume to be riding until the first night regardless of load
               this.IsFlying = true;
            }
         }
         else if (MountEnum.Horse == mt)
         {
            string mountName = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(mountName, 1.0, false, false, false, "MHorse", "MHorse", this.Territory, 0, 0, 0);
            Logger.Log(LogEnum.LE_MOUNT_CHANGE, "AddNewMount(): add=" + mountName + " for mi=" + Name);
            this.Mounts.Add(horse);
            if (false == this.Name.Contains("Giant"))  // mounts cannot carry giants
               this.IsRiding = true;                   // assume to be riding until the first night regardless of load
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "AddNewMount(): reached default w/ type=" + mt.ToString());
            return false;
         }
         //--------------------------------------------- Rotate the griffon or pegasus to the top
         bool isGriffonOwned = false;
         bool isHarpyOwned = false;
         bool isPegasusOwned = false;
         foreach (IMapItem mount in this.Mounts)
         {
            if ((true == mount.Name.Contains("Griffon")) && (false == mount.Name.Contains("Giant")))
            {
               isGriffonOwned = true;
               IsFlying = true;
               IsRiding = true;
               mount.Rider = this;
            }
            else if ((true == mount.Name.Contains("Harpy")) && (false == mount.Name.Contains("Giant")))
            {
               isHarpyOwned = true;
               IsFlying = true;
               IsRiding = true;
               mount.Rider = this;
            }
            else if ((true == mount.Name.Contains("Pegasus")) && (false == mount.Name.Contains("Giant")))
            {
               isPegasusOwned = true;
               IsFlying = true;
               IsRiding = true;
            }
         }
         if (true == isGriffonOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Griffon")) // get to top
               this.Mounts.Rotate(1);
         }
         else if (true == isHarpyOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Harpy")) // get to top
               this.Mounts.Rotate(1);
         }
         else if (true == isPegasusOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Pegasus")) // get to top
               this.Mounts.Rotate(1);
         }
         return true;
      }
      public bool AddMount(IMapItem mount)
      {
         if (true == this.IsFlyer())
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMount(): name=" + this.Name + " cannot have mounts");
            return false;
         }
         Logger.Log(LogEnum.LE_MOUNT_CHANGE, "AddMount(): adding mount=" + mount.Name + " to " + this.Name);
         this.Mounts.Add(mount);                    // add the mount to the list
         if (false == this.Name.Contains("Giant"))  // mounts cannot carry giants
         {
            if (true == mount.IsFlyingMountCarrier())
            {
               this.IsRiding = true;                   // assume to be riding until the first night regardless of load
               this.IsFlying = true;
               mount.Rider = this;
            }
            else if (true == mount.Name.Contains("Pegasus"))
            {
               this.IsRiding = true;                   // assume to be riding until the first night regardless of load
            }
            else if (true == mount.Name.Contains("Horse"))
            {
               this.IsRiding = true;                   // assume to be riding until the first night regardless of load
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "AddMount(): reached default w/ name=" + mount.Name);
               return false;
            }
         }
         //--------------------------------------------- Rotate the griffon or pegasus to the top
         bool isGriffonOwned = false;
         bool isHarpyOwned = false;
         bool isPegasusOwned = false;
         foreach (IMapItem m in this.Mounts)
         {
            if (true == m.Name.Contains("Griffon"))
               isGriffonOwned = true;
            else if (true == m.Name.Contains("Harpy"))
               isHarpyOwned = true;
            else if (true == m.Name.Contains("Pegasus"))
               isPegasusOwned = true;
         }
         if (true == isGriffonOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Griffon")) // get to top
               this.Mounts.Rotate(1);
         }
         else if (true == isHarpyOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Harpy")) // get to top
               this.Mounts.Rotate(1);
         }
         else if (true == isPegasusOwned)
         {
            while (false == this.Mounts[0].Name.Contains("Pegasus")) // get to top
               this.Mounts.Rotate(1);
         }
         return true;
      }
      public void SetMountState(IMapItem mount)
      {
         this.IsRiding = false;
         this.IsFlying = false;
         if (Utilities.PersonBurden <= this.GetFreeLoad())
         {
            if (false == mount.IsExhausted)
            {
               this.IsRiding = true;
               if ((0 == mount.StarveDayNum) && ((true == mount.IsFlyingMount()))) // flying mount and no starve days
                  this.IsFlying = true;
            }
         }
      }
      public void SetWounds(int wound, int poison)
      {
         if ((0 == wound) && (0 == poison))
            return;
         //------------------------------------------------
         Logger.Log(LogEnum.LE_MAPITEM_WOUND, "SetWounds(): Name=" + this.Name + " wound=" + wound.ToString() + " poison=" + poison.ToString());
         int woundBefore = this.Wound;
         int poisonBefore = this.Poison;
         int totalBefore = woundBefore + poisonBefore;
         if (totalBefore + poison < this.Endurance) // ensure only damage up to Endurance
         {
            this.Poison += poison;
            if (totalBefore + wound + poison < this.Endurance)
               this.Wound += wound;
            else
               this.Wound += (this.Endurance - totalBefore - poison);
         }
         else
         {
            this.Poison += (this.Endurance - totalBefore);
         }
         //------------------------------------------------
         int spotDelta = (int)Math.Round(PERCENT_MAPITEM_COVERED * (this.Wound - woundBefore) / this.Endurance);
         Logger.Log(LogEnum.LE_MAPITEM_WOUND, "SetWounds(): Name=" + this.Name + " woundBefore=" + woundBefore.ToString() + " this.Wound=" + this.Wound.ToString() + " ++++++spotDelta=" + spotDelta.ToString());
         for (int spots = 0; spots < spotDelta; ++spots) // splatter the MapItem with random blood spots
         {
            int range = (int)(Utilities.theMapItemSize);
            BloodSpot spot = new BloodSpot(range, theRandom);
            myWoundSpots.Add(spot);
         }
         //------------------------------------------------
         spotDelta = (int)Math.Round(PERCENT_MAPITEM_COVERED * (this.Poison - poisonBefore) / this.Endurance);
         Logger.Log(LogEnum.LE_MAPITEM_POISION, "SetWounds(): Name=" + this.Name + " poisonBefore=" + poisonBefore.ToString() + " this.Poison=" + this.Poison.ToString() + " ++++++spotDelta=" + spotDelta.ToString());
         for (int spots = 0; spots < spotDelta; ++spots) // splatter the MapItem with random blood spots
         {
            int range = (int)(Utilities.theMapItemSize);
            BloodSpot spot = new BloodSpot(range, theRandom);
            myPoisonSpots.Add(spot);
         }
         //------------------------------------------------
         int healthRemaining = this.Endurance;
         int diff = healthRemaining - this.Wound;
         if (diff < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWounds(): hr=" + healthRemaining.ToString() + " w=" + this.Wound.ToString() + " p=" + this.Poison.ToString());
            this.Wound = healthRemaining;
            this.Poison = 0;
            healthRemaining = 0;
         }
         else
         {
            healthRemaining -= this.Wound;
            diff = healthRemaining - this.Poison;
            if (diff < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetWounds(): hr=" + healthRemaining.ToString() + " w=" + this.Wound.ToString() + " p=" + this.Poison.ToString());
               this.Poison = healthRemaining;
               healthRemaining = 0;
            }
            else
            {
               healthRemaining -= this.Poison;
            }
         }
         if ((1 == healthRemaining) && (1 < this.Endurance))
            this.IsUnconscious = true;
         else if (0 == healthRemaining)
            this.IsKilled = true;
      }
      public void HealWounds(int wound, int poison)
      {
         if ((0 == wound) && (0 == poison))
            return;
         //------------------------------------------------
         int woundBefore = this.Wound;
         if (woundBefore < wound)
            this.Wound = 0;
         else
            this.Wound = woundBefore - wound;
         int poisonBefore = this.Poison;
         if (poisonBefore < poison)
            this.Poison = 0;
         else
            this.Poison = poisonBefore - poison;
         //------------------------------------------------
         int spotDelta = (int)Math.Round(PERCENT_MAPITEM_COVERED * (woundBefore - this.Wound) / this.Endurance);
         Logger.Log(LogEnum.LE_MAPITEM_WOUND, "SetWounds(): Name=" + this.Name + " woundBefore=" + woundBefore.ToString() + " this.Wound=" + this.Wound.ToString() + "---------spotDelta=" + spotDelta.ToString());
         for (int spots = 0; spots < spotDelta; ++spots) // remove a random wound splatter
         {
            if (0 < myWoundSpots.Count)
            {
               int i = theRandom.Next(myWoundSpots.Count);
               myWoundSpots.RemoveAt(i);
            }
         }
         //------------------------------------------------
         spotDelta = (int)Math.Round(PERCENT_MAPITEM_COVERED * (poisonBefore - this.Poison) / this.Endurance);
         Logger.Log(LogEnum.LE_MAPITEM_POISION, "HealWounds(): Name=" + this.Name + " poisonBefore=" + poisonBefore.ToString() + " poisonCurrent=" + this.Poison.ToString() + "---------spotDelta=" + spotDelta.ToString());
         for (int spots = 0; spots < spotDelta; ++spots) // remove a random poison splatter
         {
            if (0 < myPoisonSpots.Count)
            {
               int i = theRandom.Next(myPoisonSpots.Count);
               myPoisonSpots.RemoveAt(i);
            }
         }
         //------------------------------------------------
         int healthRemaining = this.Endurance - this.Wound - this.Poison;
         if ((1 < healthRemaining) && (1 < this.Endurance))
            this.IsUnconscious = false;
      }
      //----------------------------------------------------------------------------
      public bool RemoveVictimMountAndLoad()
      {
         // This function assume that the victim is carrying its maximum amount
         // which is 10 if riding a horse or not riding
         int loadCanCarry = 0;
         if ((true == Name.Contains("Eagle")) || (true == Name.Contains("Falcon")))
         {
            return true;
         }
         else if (true == IsFlyingMountCarrier())
         {
            int maxLoad1 = Utilities.MaxMountLoad;
            if (true == this.IsExhausted)
               maxLoad1 = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            loadCanCarry = (maxLoad1 >> StarveDayNum);
            if (null != this.Rider)
            {
               loadCanCarry -= Utilities.PersonBurden;
               this.Rider.IsKilled = true;
               this.Rider.Mounts.Remove(this);
               this.Rider = null;
            }
         }
         else if (true == IsRiding)
         {
            if (0 == Mounts.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "RemoveVictimMountAndLoad(): Invalid state isRiding=true but no mounts for mi=" + this.Name + " #m=" + Mounts.Count.ToString());
               IsRiding = false;
            }
            else
            {
               int maxMountLoad = Utilities.MaxMountLoad;
               IMapItem mount = Mounts[0];
               if (true == mount.IsExhausted)
                  maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted
               loadCanCarry = (maxMountLoad >> Mounts[0].StarveDayNum);
               loadCanCarry -= Utilities.PersonBurden; // 20 for man riding and what he can carry
               if (loadCanCarry < 0)
               {
                  int maxLoad1 = Utilities.MaxLoad;
                  if (true == this.IsExhausted)
                     maxLoad1 = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
                  loadCanCarry = (maxLoad1 >> StarveDayNum);
               }
               if (true == mount.IsFlyingMountCarrier())
               {
                  mount.IsKilled = true;
                  mount.Rider = null;
               }
               Mounts.Remove(mount);
            }
         }
         else
         {
            int maxLoad1 = Utilities.MaxLoad;
            if (true == this.IsExhausted)
               maxLoad1 = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            loadCanCarry = (maxLoad1 >> StarveDayNum);
         }
         //----------------------------------------------------
         if (loadCanCarry < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveVictimMountAndLoad(): Invalid loadCanCarry=" + loadCanCarry.ToString());
            return false;
         }
         //----------------------------------------------------
         int foodLoadToRemove = Math.Min(this.Food, loadCanCarry);
         loadCanCarry -= foodLoadToRemove;
         this.Food -= foodLoadToRemove;
         //----------------------------------------------------
         int remainder = this.Coin % 100;
         int hundreds = this.Coin - remainder;
         int coinLoads = hundreds / 100;
         if ((0 < remainder) && (0 < loadCanCarry))
         {
            this.Coin -= remainder;
            --loadCanCarry;
         }
         int coinLoadToRemove = Math.Min(coinLoads, loadCanCarry);
         loadCanCarry -= coinLoadToRemove;
         this.Coin -= coinLoadToRemove * 100;
         //----------------------------------------------------
         SpecialKeeps.Clear();
         SpecialShares.Clear();
         return true;
      }  // Remove the victim and his/her carried loads. If riding, remove the mount also
      public bool RemoveMountWithLoad(IMapItem deadMount)
      {
         if (0 == Mounts.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveMountWithLoad(): mi=" + Name + " Mounts.Count=0 for deadMount=" + deadMount.Name);
            return false;
         }
         //----------------------------------------
         // Determine how much mounts can carrying
         int loadCanCarryBefore = 0;
         for (int i = 0; i < this.Mounts.Count; ++i)
         {
            IMapItem mount = this.Mounts[i];
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == mount.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
            int load = (maxMountLoad >> mount.StarveDayNum);
            if ((0 == i) && (true == IsRiding))
               load -= Utilities.PersonBurden;
            loadCanCarryBefore += load;
         }
         int maxLoad = Utilities.MaxLoad;
         if (true == this.IsExhausted)
            maxLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
         if ((false == IsRiding) && (false == this.IsKilled) && (false == this.IsUnconscious))
            loadCanCarryBefore += (maxLoad >> StarveDayNum);
         if (loadCanCarryBefore < 0)
         {
            this.Food = 0;
            this.Coin = 0;
            Logger.Log(LogEnum.LE_ERROR, "RemoveMountWithLoad(): mi=" + Name + " mount=" + deadMount.Name + " loadCanCarryBefore=" + loadCanCarryBefore.ToString());
            return false;
         }
         //----------------------------------------
         if (true == deadMount.IsFlyingMountCarrier()) // The only time a mount can be a griffon is if it is ridden
         {
            if (null == deadMount.Rider) // must hav a rider... need to remove the rider
            {
               Logger.Log(LogEnum.LE_ERROR, "RemoveMountWithLoad(): Inavlid state mi=" + Name + " deadMount.Rider=null for deadMount=" + deadMount.Name);
               return false;
            }
            deadMount.Rider.Mounts.Remove(deadMount);
            deadMount.Rider = null;
            this.IsRiding = false;
            this.IsFlying = false;
         }
         else
         {
            IMapItem firstMount = this.Mounts[0];
            foreach (IMapItem mount in this.Mounts) // remove the mount
            {
               if (mount.Name == deadMount.Name)
               {
                  if (deadMount.Name == firstMount.Name) // if this is teh first mount, make sure rider is no longer riding
                  {
                     this.IsRiding = false;
                     this.IsFlying = false;
                  }
                  Mounts.Remove(deadMount.Name);
                  break;
               }
            }
         }
         //----------------------------------------
         // Determine how much mounts can carrying
         int loadCanCarryAfter = 0;
         for (int i = 0; i < this.Mounts.Count; ++i)
         {
            IMapItem mount = this.Mounts[i];
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == mount.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
            int load = (maxMountLoad >> mount.StarveDayNum);
            if ((0 == i) && (true == IsRiding))
               load -= Utilities.PersonBurden;
            loadCanCarryAfter += load;
         }
         int maxLoad1 = Utilities.MaxLoad;
         if (true == this.IsExhausted)
            maxLoad1 = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
         if ((false == IsRiding) && (false == this.IsKilled) && (false == this.IsUnconscious))
            loadCanCarryAfter += (maxLoad1 >> StarveDayNum);
         //----------------------------------------
         float percentCarry = (float)loadCanCarryAfter / (float)loadCanCarryBefore;
         float percentRemoved = 1.0f - percentCarry;
         //----------------------------------------
         // Remove percentage of carrying load
         int coinToRemove = (int)((float)this.Coin * percentRemoved);
         this.Coin -= coinToRemove;
         int foodToRemove = (int)((float)this.Food * percentRemoved);
         this.Food -= foodToRemove;
         //----------------------------------------
         int freeLoads = GetFreeLoad();
         if (freeLoads < 0) // GetFreeLoad() resets the loads to match what can be carried
         {
            Logger.Log(LogEnum.LE_ERROR, "RemoveMountWithLoad(): mi=" + Name + " mount=" + deadMount.Name + " freeLoads=" + freeLoads.ToString());
            return false;
         }
         return true;
      }
      public void RemoveMountedMount()
      {
         if (false == IsRiding)
            return;
         if (0 == Mounts.Count)
            return;
         IMapItem mount = Mounts[0];
         if (null != mount.Rider)
            mount.Rider = null;
         if (false == mount.IsFlyingMountCarrier()) // do not remove Griffon/Harpy from party
            Mounts.Remove(mount);
         //-------------------------
         // Switch rider to next mount
         if (0 < Mounts.Count)
         {
            IMapItem newMount = Mounts[0];
            if (true == newMount.IsFlyingMountCarrier())
               newMount.Rider = this;
            IsRiding = true;
         }
         else
         {
            IsRiding = false;
         }
         IsFlying = false;
      }
      public void RemoveUnmountedMounts()
      {
         IMapItems adbandonedMounts = new MapItems();
         if (false == this.IsRiding) // if not riding, remove every mount
         {
            foreach (IMapItem mount in this.Mounts)
               adbandonedMounts.Add(mount);
         }
         else
         {
            string mountName = "";
            foreach (IMapItem mount in this.Mounts)  // logic to always remove flying mount, and finally, choose the first horse
            {
               if (true == mount.IsFlyingMount())
               {
                  mountName = mount.Name;
                  break;
               }
            }
            if ("" == mountName)
            {
               if (0 < this.Mounts.Count)
                  mountName = this.Mounts[0].Name;
            }
            foreach (IMapItem mount in this.Mounts)
            {
               if (mount.Name != mountName)
                  adbandonedMounts.Add(mount);
            }
         }
         foreach (IMapItem mount in adbandonedMounts)
            this.Mounts.Remove(mount.Name);
      }
      public bool RemoveNonFlyingMounts()
      {
         this.LeftOnGroundMounts.Clear();
         IMapItems adbandonedMounts = new MapItems();
         foreach (IMapItem mount in this.Mounts)
         {
            if ((true == mount.Name.Contains("Horse")) || (true == mount.IsExhausted) || (0 < mount.StarveDayNum))
               adbandonedMounts.Add(mount);
         }
         foreach (IMapItem mount in adbandonedMounts)
         {
            this.LeftOnGroundMounts.Add(mount);
            this.Mounts.Remove(mount.Name);
         }
         this.IsFlying = false;
         this.IsRiding = false;
         if (0 < this.Mounts.Count)
         {
            IMapItem mount = this.Mounts[0];
            if (true == mount.Name.Contains("Pegasus"))
            {
               this.IsFlying = true;
               this.IsRiding = true;
               return true;
            }
            else if (true == mount.IsFlyingMountCarrier())
            {
               this.IsFlying = true;
               this.IsRiding = true;
               mount.Rider = this;
               return true;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "RemoveNonFlyingMounts(): unknown mount=" + mount.Name);
               return false;
            }
         }
         return true;
      }
      //----------------------------------------------------------------------------
      public int GetNumSpecialItem(SpecialEnum item)
      {
         int numItem = 0;
         foreach (SpecialEnum item1 in mySpecialKeeps)
         {
            if (item == item1)
               ++numItem;
         }
         foreach (SpecialEnum item2 in mySpecialShares)
         {
            if (item == item2)
               ++numItem;
         }
         return numItem;
      }
      public bool IsSpecialItemHeld(SpecialEnum item)
      {
         foreach (SpecialEnum item1 in SpecialKeeps)
         {
            if (item == item1)
            {
               Logger.Log(LogEnum.LE_GET_ITEM, "IsSpecialItemHeld(): Keep mi=" + this.Name + " has item=" + item.ToString());
               return true;
            }

         }
         foreach (SpecialEnum item2 in mySpecialShares)
         {
            if (item == item2)
            {
               Logger.Log(LogEnum.LE_GET_ITEM, "IsSpecialItemHeld(): Share mi=" + this.Name + " has item=" + item.ToString());
               return true;
            }
         }
         return false;
      }
      public bool AddSpecialItemToKeep(SpecialEnum item)
      {
         bool isAlreadyHave = IsSpecialItemHeld(item);
         switch (item)
         {
            case SpecialEnum.HealingPoition: break;
            case SpecialEnum.CurePoisonVial: break;
            case SpecialEnum.GiftOfCharm: break;
            case SpecialEnum.EnduranceSash:
               if (false == isAlreadyHave)
                  ++this.Endurance;
               break;
            case SpecialEnum.ResistanceTalisman: break;
            case SpecialEnum.PoisonDrug: break;
            case SpecialEnum.MagicSword:
               if (false == isAlreadyHave)
                  ++this.Combat;
               break;
            case SpecialEnum.AntiPoisonAmulet: break;
            case SpecialEnum.PegasusMount: break;
            case SpecialEnum.PegasusMountTalisman: break;
            case SpecialEnum.CharismaTalisman: break;
            case SpecialEnum.NerveGasBomb: break;
            case SpecialEnum.ResistanceRing: break;
            case SpecialEnum.ResurrectionNecklace: break;
            case SpecialEnum.ShieldOfLight: break;
            case SpecialEnum.RoyalHelmOfNorthlands: break;
            case SpecialEnum.TrollSkin: break;
            case SpecialEnum.DragonEye: break;
            case SpecialEnum.RocBeak: break;
            case SpecialEnum.GriffonClaws: break;
            case SpecialEnum.Foulbane: break;
            case SpecialEnum.MagicBox: break;
            case SpecialEnum.HydraTeeth: break;
            case SpecialEnum.StaffOfCommand: break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "AddSpecialItemToKeep(): reached default possession=" + item.ToString());
               return false;
         }
         Logger.Log(LogEnum.LE_ADD_ITEM, "AddSpecialItemToKeep(): Keep mi=" + this.Name + " adding item=" + item.ToString());
         this.SpecialKeeps.Add(item);
         return true;
      }
      public bool AddSpecialItemToShare(SpecialEnum item)
      {
         bool isAlreadyHave = IsSpecialItemHeld(item);
         switch (item)
         {
            case SpecialEnum.HealingPoition: break;
            case SpecialEnum.CurePoisonVial: break;
            case SpecialEnum.GiftOfCharm: break;
            case SpecialEnum.EnduranceSash:
               if (false == isAlreadyHave)
                  ++this.Endurance;
               break;
            case SpecialEnum.ResistanceTalisman: break;
            case SpecialEnum.PoisonDrug: break;
            case SpecialEnum.MagicSword:
               if (false == isAlreadyHave)
                  ++this.Combat;
               break;
            case SpecialEnum.AntiPoisonAmulet: break;
            case SpecialEnum.PegasusMount: break;
            case SpecialEnum.PegasusMountTalisman: break;
            case SpecialEnum.CharismaTalisman: break;
            case SpecialEnum.NerveGasBomb: break;
            case SpecialEnum.ResistanceRing: break;
            case SpecialEnum.ResurrectionNecklace: break;
            case SpecialEnum.ShieldOfLight: break;
            case SpecialEnum.RoyalHelmOfNorthlands: break;
            case SpecialEnum.TrollSkin: break;
            case SpecialEnum.DragonEye: break;
            case SpecialEnum.RocBeak: break;
            case SpecialEnum.GriffonClaws: break;
            case SpecialEnum.Foulbane: break;
            case SpecialEnum.MagicBox: break;
            case SpecialEnum.HydraTeeth: break;
            case SpecialEnum.StaffOfCommand: break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "AddSpecialItemToShare(): reached default possession=" + item.ToString());
               return false;
         }
         Logger.Log(LogEnum.LE_ADD_ITEM, "AddSpecialItemToKeep(): Share mi=" + this.Name + " adding item=" + item.ToString());
         this.mySpecialShares.Add(item);
         return true;
      }
      public bool RemoveSpecialItem(SpecialEnum item)
      {
         bool isItemRemoved = false;
         foreach (SpecialEnum possession in this.SpecialKeeps) // first check if in the keeps list.
         {
            if (possession.ToString() == item.ToString())
            {
               SpecialKeeps.Remove(possession);
               isItemRemoved = true;
               break;
            }
         }
         if (false == isItemRemoved) // if item is held, determine if it is still held. If not, remove the benefit
         {
            Logger.Log(LogEnum.LE_REMOVE_ITEM, "RemoveSpecialItem(): 1-NOT FOUND count=" + SpecialKeeps.Count.ToString() + " in SpecialKeeps item=" + item.ToString() + " for mi=" + this.Name);
         }
         else
         {
            Logger.Log(LogEnum.LE_REMOVE_ITEM, "RemoveSpecialItem(): 1-REMOVED count=" + SpecialKeeps.Count.ToString() + " from SpecialKeeps item=" + item.ToString() + " for mi=" + this.Name);
            bool isItemStillHeld = IsSpecialItemHeld(item);
            switch (item)
            {
               case SpecialEnum.HealingPoition: break;
               case SpecialEnum.CurePoisonVial: break;
               case SpecialEnum.GiftOfCharm: break;
               case SpecialEnum.EnduranceSash:
                  if (false == isItemStillHeld)
                     --this.Endurance;
                  break;
               case SpecialEnum.ResistanceTalisman: break;
               case SpecialEnum.PoisonDrug: break;
               case SpecialEnum.MagicSword:
                  if (false == isItemStillHeld)
                     --this.Combat;
                  break;
               case SpecialEnum.AntiPoisonAmulet: break;
               case SpecialEnum.PegasusMount: break;
               case SpecialEnum.PegasusMountTalisman: break;
               case SpecialEnum.CharismaTalisman: break;
               case SpecialEnum.NerveGasBomb: break;
               case SpecialEnum.ResistanceRing: break;
               case SpecialEnum.ResurrectionNecklace: break;
               case SpecialEnum.ShieldOfLight: break;
               case SpecialEnum.RoyalHelmOfNorthlands: break;
               case SpecialEnum.TrollSkin: break;
               case SpecialEnum.DragonEye: break;
               case SpecialEnum.RocBeak: break;
               case SpecialEnum.GriffonClaws: break;
               case SpecialEnum.Foulbane: break;
               case SpecialEnum.MagicBox: break;
               case SpecialEnum.HydraTeeth: break;
               case SpecialEnum.StaffOfCommand: break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "RemoveSpecialItem(): 1 - reached default possession=" + item.ToString());
                  return false;
            }
            return true;
         }
         //---------------------------------------------------------
         foreach (SpecialEnum possession in this.SpecialShares) // first check if in the keeps list.
         {
            if (possession.ToString() == item.ToString())
            {
               SpecialShares.Remove(possession);
               isItemRemoved = true;
               break;
            }
         }
         if (false == isItemRemoved) // if item is held, determine if it is still held. If not, remove the benefit
         {
            Logger.Log(LogEnum.LE_REMOVE_ITEM, "RemoveSpecialItem(): 2-NOT FOUND count=" + SpecialShares.Count.ToString() + " in SpecialShares item=" + item.ToString() + " for mi=" + this.Name);
         }
         else
         {
            Logger.Log(LogEnum.LE_REMOVE_ITEM, "RemoveSpecialItem(): 2-REMOVED count=" + SpecialShares.Count.ToString() + " from SpecialShares item=" + item.ToString() + " for mi=" + this.Name);
            bool isItemStillHeld = IsSpecialItemHeld(item);
            switch (item)
            {
               case SpecialEnum.HealingPoition: break;
               case SpecialEnum.CurePoisonVial: break;
               case SpecialEnum.GiftOfCharm: break;
               case SpecialEnum.EnduranceSash:
                  if (false == isItemStillHeld)
                     --this.Endurance;
                  break;
               case SpecialEnum.ResistanceTalisman: break;
               case SpecialEnum.PoisonDrug: break;
               case SpecialEnum.MagicSword:
                  if (false == isItemStillHeld)
                     --this.Combat;
                  break;
               case SpecialEnum.AntiPoisonAmulet: break;
               case SpecialEnum.PegasusMount: break;
               case SpecialEnum.PegasusMountTalisman: break;
               case SpecialEnum.CharismaTalisman: break;
               case SpecialEnum.NerveGasBomb: break;
               case SpecialEnum.ResistanceRing: break;
               case SpecialEnum.ResurrectionNecklace: break;
               case SpecialEnum.ShieldOfLight: break;
               case SpecialEnum.RoyalHelmOfNorthlands: break;
               case SpecialEnum.TrollSkin: break;
               case SpecialEnum.DragonEye: break;
               case SpecialEnum.RocBeak: break;
               case SpecialEnum.GriffonClaws: break;
               case SpecialEnum.Foulbane: break;
               case SpecialEnum.MagicBox: break;
               case SpecialEnum.HydraTeeth: break;
               case SpecialEnum.StaffOfCommand: break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "RemoveSpecialItem(): 2 - reached default possession=" + item.ToString());
                  return false;
            }
            return true;
         }
         return false;
      }
      //----------------------------------------------------------------------------
      public int GetMaxFreeLoad()
      {
         if ((true == this.Name.Contains("Eagle")) || (true == this.Name.Contains("Falcon")) || (true == this.IsSunStroke) || (true == this.IsUnconscious))
            return 0;
         int freeLoad = 0;
         if (true == this.IsFlyingMountCarrier())
         {
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == this.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the load if exhausted 
            freeLoad = (maxMountLoad >> this.StarveDayNum);
         }
         else
         {

            int maxPersonLoad = Utilities.MaxLoad;
            if (true == this.IsExhausted)
               maxPersonLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            int personLoad = maxPersonLoad >> this.StarveDayNum; // divide by half for each starve day
            int mountLoad = 0;
            foreach (IMapItem mount in this.Mounts) // now add the mounts
            {
               int maxMountLoad = Utilities.MaxMountLoad;
               if (true == mount.IsExhausted)
                  maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
               mountLoad += (maxMountLoad >> mount.StarveDayNum);
            }
            freeLoad = personLoad + mountLoad;
            Logger.Log(LogEnum.LE_VIEW_SHOW_LOADS, "GetMaxFreeLoad(): 2=> fl=" + freeLoad.ToString() + " ml=" + mountLoad.ToString() + " pl=" + personLoad.ToString());
         }
         return freeLoad;
      }
      public int GetFreeLoad()
      {
         if ((true == IsKilled) || (true == IsUnconscious)) 
            return 0;
         if(true == this.IsFlyer())
         {
            if ((true == this.IsFlyingMountCarrier()) && (null != this.Rider)) // Griffon & Harpy free load counted with rider
               return 0;
         }
         //------------------------------------------
         bool isPreviouslyRiding = this.IsRiding;
         int mountCarry = 0;
         foreach (IMapItem mount in this.Mounts)
         {
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == mount.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
            int load = (maxMountLoad >> mount.StarveDayNum);
            mountCarry += load;
         }
         if (0 < this.Mounts.Count) // This routine can only dismount. SetMountState() can cause mounting
         {
            IMapItem mount = this.Mounts[0];
            if ((0 < mount.StarveDayNum) || (true == mount.IsExhausted))
            {
               this.IsRiding = false;
               this.IsFlying = false;
            }
         }
         int loadCanCarry = mountCarry;
         //------------------------------------------
         int personCarry = 0;
         if (true == this.IsFlyingMountCarrier()) // if this is true, must not have a rider
         {
            int maxLoad = Utilities.MaxMountLoad;
            if (true == this.IsExhausted)
               maxLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            personCarry = maxLoad >> this.StarveDayNum;
         }
         else
         {
            int maxLoad = Utilities.MaxLoad;
            if (true == this.IsExhausted)
               maxLoad = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            personCarry = maxLoad >> this.StarveDayNum;
            if (true == this.IsRiding)
            {
               if (personCarry < Utilities.PersonBurden)
                  this.IsRiding = false;
               else
                  personCarry -= Utilities.PersonBurden; // 20 for man riding and what he can carry
            }
         }
         loadCanCarry += personCarry;
         //------------------------------------------
         int coinLoads = 0;
         if (0 < this.Coin)
         {
            int remainder = this.Coin % 100;
            int hundreds = this.Coin - remainder;
            coinLoads = hundreds / 100;
            if (0 < remainder)
               ++coinLoads;
            if ((true == this.IsRiding) && (loadCanCarry < coinLoads))
            {
               if (false == this.IsFlyingMountCarrier()) // if this is true, must not have a rider
               {
                  this.IsRiding = false;
                  loadCanCarry += Utilities.PersonBurden;
               }
            }
            if (loadCanCarry < coinLoads)
            {
               int totalLoad = this.Food + coinLoads;
               Logger.Log(LogEnum.LE_ERROR, "GetFreeLoad(): 1-Invalid state mi=" + this.Name + " r?=" + isPreviouslyRiding.ToString() + "-->" + this.IsRiding.ToString() + " #m=" + Mounts.Count.ToString() + " ==> (mc=" + mountCarry.ToString() + ")+(pc=" + personCarry.ToString() + ")=(lcc=" + loadCanCarry.ToString() + ") ==> (cl=" + coinLoads.ToString() + ")+(f=" + this.Food.ToString() + ") = " + totalLoad.ToString());
               coinLoads = loadCanCarry;
               this.Coin = remainder + (loadCanCarry - 1) * 100;
               this.Food = 0;
               loadCanCarry = 0;
            }
            else
            {
               loadCanCarry -= coinLoads;
            }
         }
         if ((true == this.IsRiding) && (loadCanCarry < this.Food))
         {
            if (false == this.IsFlyingMountCarrier()) // if this is true, must not have a rider
            {
               this.IsRiding = false;
               loadCanCarry += Utilities.PersonBurden;
            }
         }
         if (loadCanCarry < this.Food)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetFreeLoad(): 2-Invalid state mi=" + this.Name + " r?=" + isPreviouslyRiding.ToString() + "-->" + this.IsRiding.ToString() + " #m=" + Mounts.Count.ToString() + " --> (mc=" + mountCarry.ToString() + ")+(pc=" + personCarry.ToString() + ")-(cl=" + coinLoads.ToString() + ") = (lcc=" + loadCanCarry.ToString() + ") and f=" + this.Food.ToString());
            this.Food = loadCanCarry;
            loadCanCarry = 0;
         }
         else
         {
            loadCanCarry -= this.Food;
         }
         return loadCanCarry;
      }   // get free load - dismount if load does not support - but do not mount 
      public int GetFreeLoadWithoutModify()
      {
         int mountCarry = 0;
         foreach (IMapItem mount in this.Mounts)
         {
            int maxMountLoad = Utilities.MaxMountLoad;
            if (true == mount.IsExhausted)
               maxMountLoad = Utilities.MaxMountLoad >> 1; // e120 - half the mount load if exhausted 
            int load = (maxMountLoad >> mount.StarveDayNum);
            mountCarry += load;
         }
         int loadCanCarry = mountCarry;
         //------------------------------------------
         if ((true == IsKilled) || (true == IsUnconscious) || (true == Name.Contains("Eagle")) || (true == Name.Contains("Falcon"))) // Griffon & Harpy free load counted with rider
         {
            // do nothing - only the mount loads apply
         }
         else if (true == IsFlyingMountCarrier()) // Griffon & Harpy free load counted with rider
         {
            int personCarry = Utilities.MaxMountLoad;
            if (true == this.IsExhausted)
               personCarry = Utilities.MaxMountLoad >> 1; // e120 - half the load if exhausted 
            if (0 < personCarry)
            {
               if (null == this.Rider)
                  personCarry = personCarry >> this.StarveDayNum;
               else
                  personCarry = 0; // Griffon & Harpy free load counted with rider
            }
            loadCanCarry += personCarry;
         }
         else
         {
            int personCarry = Utilities.MaxLoad;
            if (true == this.IsExhausted)
               personCarry = Utilities.MaxLoad >> 1; // e120 - half the load if exhausted 
            if (0 < personCarry)
            {
               personCarry = personCarry >> this.StarveDayNum;
               if (true == this.IsRiding)
               {
                  if (personCarry < Utilities.PersonBurden)
                     this.IsRiding = false;
                  else
                     personCarry -= Utilities.PersonBurden;
               }
               loadCanCarry += personCarry;
            }
         }
         //------------------------------------------
         int coinLoads = 0;
         if (0 < this.Coin)
         {
            int remainder = this.Coin % 100;
            int hundreds = this.Coin - remainder;
            coinLoads = hundreds / 100;
            if (0 < remainder)
               ++coinLoads;
            loadCanCarry -= coinLoads;
         }
         loadCanCarry -= this.Food;
         if (loadCanCarry < 0)
            Logger.Log(LogEnum.LE_FREE_LOAD, "GetFreeLoadWithoutModify(): name=" + this.Name + " lc=" + loadCanCarry.ToString() + " fl=" + Food.ToString() + " cl=" + coinLoads.ToString() + "(coins=" + this.Coin.ToString() + ") ml=" + mountCarry.ToString() + " kia?=" + this.IsKilled + " mia?=" + this.IsUnconscious);
         return loadCanCarry;
      }   // get free load - dismount if load does not support - but do not mount 
      public int GetFlyLoad()
      {
         int loadCanCarry = 0;
         //------------------------------------------
         if ((true == this.Name.Contains("Eagle")) || (true == this.Name.Contains("Falcon"))) // Falcons/Eagles can always fly unless exhausted
         {
            if (false == this.IsExhausted)
               return 0;
            else
               return NOT_FLYING;
         }
         //------------------------------------------
         if (false == this.IsFlyer())
         {
            if (0 == this.Mounts.Count) // if no mounts, the only way to fly if being carried
            {
               if (true == this.IsFlying)
                  return 0;
               else
                  return NOT_FLYING;
            }
         }
         //------------------------------------------
         int i = 0;
         IMapItem mountedAnimal = null;
         foreach (IMapItem mount in this.Mounts)
         {
            if ((0 != mount.StarveDayNum) || (true == mount.IsExhausted) || (false == mount.IsFlyingMount())) // any non-flying mounts or horses means cannot fly
               return NOT_FLYING;
            loadCanCarry += Utilities.MaxMountLoad;
            if (0 == i)
            {
               mountedAnimal = mount;
               loadCanCarry -= -Utilities.PersonBurden;
            }
         }
         //------------------------------------------
         int maxLoad = 0;
         if (true == this.IsFlyingMountCarrier())
            maxLoad = Utilities.MaxMountLoad;
         else
            maxLoad = Utilities.MaxLoad;
         if (true == this.IsExhausted)
            maxLoad = maxLoad >> 1; // e120 - half the load if exhausted 
         loadCanCarry += (maxLoad >> this.StarveDayNum);
         //------------------------------------------
         int coinLoads = 0;
         if (0 < this.Coin)
         {
            int remainder = this.Coin % 100;
            int hundreds = this.Coin - remainder;
            coinLoads = hundreds / 100;
            if (0 < remainder)
               ++coinLoads;
            loadCanCarry -= coinLoads;
         }
         loadCanCarry -= this.Food;
         //------------------------------------------
         if (-1 < loadCanCarry)
         {
            this.IsFlying = true;
            if (null != mountedAnimal)
               this.IsRiding = true;
         }
         return loadCanCarry;
      }   // get what load can be carried if flying...this function can return negative which indication something needs to be dropped
      public bool IsFlyer()
      {
         if ((true == Name.Contains("Eagle")) || (true == Name.Contains("Falcon")) || (true == IsFlyingMountCarrier()))
            return true;
         return false;
      }
      public bool IsFlyingMount()
      {
         if ((true == Name.Contains("Pegasus")) || (true == this.IsFlyingMountCarrier()))
            return true;
         return false;
      }
      public bool IsFlyingMountCarrier()
      {
         if ((true == Name.Contains("Griffon")) || (true == Name.Contains("Harpy")))
            return true;
         return false;
      }
      public void Reset()
      {
         IsKilled = false;
         IsUnconscious = false;
         IsRunAway = false;
         IsMoved = false;
         if (false == this.IsFlyer())
         {
            IsFlying = false;
            IsRiding = false;
         }
         else
         {
            IsFlying = true;
            IsRiding = true;
         }
         IsPoisonApplied = false;
         IsSunStroke = false;
         IsExhausted = false;
         IsPlagued = false;
         PlagueDustWound = 0;
         IsCatchCold = false;
         IsShowFireball = false;
         MovementUsed = 0;
         Wound = 0;
         Poison = 0;
         Coin = 0;
         Food = 0;
         StarveDayNum = 0;
         Mounts.Clear();
         CarriedMembers.Clear();
         SpecialKeeps.Clear();
         SpecialShares.Clear();
         OverlayImageName = "";
         WoundSpots.Clear();
         PoisonSpots.Clear();
      }
      public void ResetPartial()
      {
         IsRunAway = false;
         IsMoved = false;
         if (false == this.IsFlyer())
         {
            IsFlying = false;
            IsRiding = false;
         }
         else
         {
            IsFlying = true;
            IsRiding = true;
         }
         MovementUsed = 0;
         Coin = 0;
         Food = 0;
         Mounts.Clear();
         CarriedMembers.Clear();
         List<SpecialEnum> specialItems = new List<SpecialEnum>();
         foreach (SpecialEnum item in mySpecialKeeps)
            specialItems.Add(item);
         foreach (SpecialEnum item in mySpecialShares)
            specialItems.Add(item);
         foreach (SpecialEnum item in specialItems)
         {
            if (false == this.RemoveSpecialItem(item))
               Logger.Log(LogEnum.LE_ERROR, "ResetPartial(): 1-cannot find item=" + item.ToString());
         }
         specialItems.Clear();
      }
      public void Flip()
      {
         if (false == myIsFlipped)
         {
            myIsFlipped = true;
            string temp = TopImageName;
            TopImageName = BottomImageName;
            BottomImageName = temp;
         }
      }
      public void Unflip()
      {
         if (true == myIsFlipped)
         {
            myIsFlipped = false;
            string temp = TopImageName;
            TopImageName = BottomImageName;
            BottomImageName = temp;
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("<");
         sb.Append(this.Name);
         sb.Append(">T=<");
         sb.Append(this.Territory.Name);
         sb.Append(">E=<");
         sb.Append(this.Endurance.ToString());
         sb.Append(">C=<");
         sb.Append(this.Combat.ToString());
         sb.Append(">#m<");
         sb.Append(this.Mounts.Count.ToString());
         sb.Append(">WC=<");
         sb.Append(this.WealthCode.ToString());
         sb.Append(">");
         return sb.ToString();
      }
      //---------------------------------------------------------------------------- static functions
      public static void Shuffle(ref List<IMapItem> mapItems)
      {
         for (int j = 0; j < 10; ++j)
         {
            List<IMapItem> newOrder = new List<IMapItem>();
            // Random select card in myCards list and remove it.  Then add it to new list. 
            int count = mapItems.Count;
            for (int i = 0; i < count; i++)
            {
               int index = Utilities.RandomGenerator.Next(mapItems.Count);
               if (index < mapItems.Count)
               {
                  IMapItem randomIndex = (IMapItem)mapItems[index];
                  mapItems.RemoveAt(index);
                  newOrder.Add(randomIndex);
               }
            }
            mapItems = newOrder;
         }
      }
      public static void SetButtonContent(Button b, IMapItem mi, bool isStatsShown, bool isAdornmentsShown, bool isSwordOrShieldShown = false, bool isBloodSpotsShown = true)
      {
         Grid g = new Grid() { };
         if (false == mi.IsAnimated)
         {
            Image img = new Image() { Source = theMapImages.GetBitmapImage(mi.TopImageName), Stretch = Stretch.Fill };
            img.Source = theMapImages.GetBitmapImage(mi.TopImageName);
            g.Children.Add(img);
            if ((0 < mi.PlagueDustWound) && (true == isAdornmentsShown))
            {
               Image plagueDust = new Image() { Stretch = Stretch.Fill, Source = thePlagueDust };
               g.Children.Add(plagueDust);
            }
            //----------------------------------------------------
            Canvas c = new Canvas() { };
            if (true == mi.IsAlly)
            {
               double s2 = 0.8 * Utilities.theMapItemOffset;
               Image stackedCoin = new Image() { Height = s2, Width = s2, Source = theAlly };
               c.Children.Add(stackedCoin);
               Canvas.SetLeft(stackedCoin, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(stackedCoin, 1.1 * Utilities.ZOOM * Utilities.theMapItemOffset);
            }
            else if (0 < mi.Wages)
            {
               double s1 = 0.8 * Utilities.theMapItemOffset;
               Image muscle = new Image() { Height = s1, Width = s1, Source = theMuscle };
               c.Children.Add(muscle);
               Canvas.SetLeft(muscle, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s1);
               Canvas.SetTop(muscle, 1.1 * Utilities.ZOOM * Utilities.theMapItemOffset);
            }
            else if (true == mi.IsLooter)
            {
               double s2 = 0.8 * Utilities.theMapItemOffset;
               Image stackedCoin = new Image() { Height = s2, Width = s2, Source = theCoinPileSingle };
               c.Children.Add(stackedCoin);
               Canvas.SetLeft(stackedCoin, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(stackedCoin, 1.1 * Utilities.ZOOM * Utilities.theMapItemOffset);
            }
            else if (true == mi.IsFickle)
            {
               double s2 = 1.2 * Utilities.theMapItemOffset;
               Image coinBar = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("CoinBar") };
               c.Children.Add(coinBar);
               Canvas.SetLeft(coinBar, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(coinBar, 1.1 * Utilities.ZOOM * Utilities.theMapItemOffset);
            }
            //----------------------------------------------------
            if (true == mi.IsFugitive)
            {
               double s2 = Utilities.theMapItemOffset;
               Image fugitiveDeny = new Image() { Height = 0.55 * s2, Width = s2, Source = theFugitive };
               c.Children.Add(fugitiveDeny);
               Canvas.SetLeft(fugitiveDeny, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(fugitiveDeny, 0);
            }
            else if (true == mi.IsTownCastleTempleLeave)
            {
               double s2 = Utilities.theMapItemOffset;
               Image structDeny = new Image() { Height = s2, Width = s2, Source = theStructureDeny };
               c.Children.Add(structDeny);
               Canvas.SetLeft(structDeny, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(structDeny, 0);
            }
            else if (0 < mi.GroupNum)
            {
               double s2 = Utilities.theMapItemOffset;
               Image group = new Image() { Height = s2, Width = s2, Source = theGroup };
               c.Children.Add(group);
               Canvas.SetLeft(group, Utilities.ZOOM * Utilities.theMapItemOffset - 0.5 * s2);
               Canvas.SetTop(group, -1);
            }
            //----------------------------------------------------
            if (true == isStatsShown)
            {
               FontFamily fontFam = new FontFamily("Franklin Gothic Heavy");
               Thickness padding = new Thickness(0);
               if (false == mi.TopImageName.StartsWith("M")) // Not a Mount
               {
                  int totalWounds = mi.Wound + mi.Poison;
                  Label labelForEndurance = new Label() { Content = mi.Endurance.ToString(), FontWeight = FontWeights.Bold, FontFamily = fontFam, Padding = padding };
                  Label labelForCombat = new Label() { Content = mi.Combat.ToString(), FontWeight = FontWeights.Bold, FontFamily = fontFam, Padding = padding };
                  Label labelForWounds = new Label() { Content = totalWounds.ToString(), FontWeight = FontWeights.Bold, FontFamily = fontFam, Padding = padding };
                  c.Children.Add(labelForEndurance);
                  c.Children.Add(labelForCombat);
                  c.Children.Add(labelForWounds);
                  Canvas.SetLeft(labelForEndurance, 0);
                  Canvas.SetTop(labelForEndurance, 0);
                  if (9 < mi.Combat)
                     Canvas.SetLeft(labelForCombat, 36);
                  else
                     Canvas.SetLeft(labelForCombat, 42);
                  Canvas.SetTop(labelForCombat, 0);
                  Canvas.SetLeft(labelForWounds, 0);
                  Canvas.SetTop(labelForWounds, 38);
                  if (true == mi.Name.Contains("Mirror"))
                  {
                     labelForEndurance.Foreground = Brushes.White;
                     labelForCombat.Foreground = Brushes.White;
                     labelForWounds.Foreground = Brushes.White;
                  }
               }
               Label labelForStarveDayNum = new Label() { Content = mi.StarveDayNum.ToString(), FontWeight = FontWeights.Bold, FontFamily = fontFam, Padding = padding };
               c.Children.Add(labelForStarveDayNum);
               if (9 < mi.StarveDayNum)
                  Canvas.SetLeft(labelForStarveDayNum, 36);
               else
                  Canvas.SetLeft(labelForStarveDayNum, 42);
               Canvas.SetTop(labelForStarveDayNum, 38);
               if (true == mi.Name.Contains("Mirror"))
                  labelForStarveDayNum.Foreground = Brushes.White;
            }
            //----------------------------------------------------
            if (true == isBloodSpotsShown)
            {
               foreach (BloodSpot bs in mi.WoundSpots) // create wound spot on canvas
               {
                  Image spotImg = new Image() { Stretch = Stretch.Fill, Height = bs.mySize, Width = bs.mySize, Source = theBloodSpot };
                  c.Children.Add(spotImg);
                  Canvas.SetLeft(spotImg, bs.myLeft);
                  Canvas.SetTop(spotImg, bs.myTop);
               }
               foreach (BloodSpot bs in mi.PoisonSpots) // create poison spot on canvas
               {
                  Image spotImg = new Image() { Stretch = Stretch.Fill, Height = bs.mySize, Width = bs.mySize, Source = thePoisonSpot };
                  c.Children.Add(spotImg);
                  Canvas.SetLeft(spotImg, bs.myLeft);
                  Canvas.SetTop(spotImg, bs.myTop);
               }
            }
            if (true == isAdornmentsShown)
            {
               if ((true == isSwordOrShieldShown) && (true == mi.IsShieldApplied)) // Show shield if in combat and activated
                  SetButtonContentAdornments(mi, c, SpecialEnum.ShieldOfLight);
               foreach (SpecialEnum possession in mi.SpecialKeeps)
               {
                  if ((true == isSwordOrShieldShown) && (SpecialEnum.ShieldOfLight == possession)) // do not show shield if in combat 
                     continue;
                  SetButtonContentAdornments(mi, c, possession);
               }
               foreach (SpecialEnum possession in mi.SpecialShares)
               {
                  if ((true == isSwordOrShieldShown) && (SpecialEnum.ShieldOfLight == possession)) // do not show shield if in combat 
                     continue;
                  SetButtonContentAdornments(mi, c, possession);
               }
            }
            g.Children.Add(c);
            //----------------------------------------------------
            if ("" != mi.OverlayImageName)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage(mi.OverlayImageName) };
               g.Children.Add(overlay);
            }
            //----------------------------------------------------
            if (true == mi.IsResurrected)
            {
               Image resurrected = new Image() { Stretch = Stretch.Fill, Source = thResurrected };
               g.Children.Add(resurrected);
            }
            //----------------------------------------------------
            if (true == mi.IsDisappear)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "Smoke.gif", UriKind.Absolute);
               bmi.EndInit();
               Image img1 = new Image { Source = bmi, Stretch = Stretch.UniformToFill, StretchDirection = StretchDirection.Both };
               ImageBehavior.SetAnimatedSource(img1, bmi);
               g.Children.Add(img1);
            }
            else if (true == mi.IsKilled) // add Overlays
            {
               Image kia = new Image() { Stretch = Stretch.Fill, Source = theKia };
               g.Children.Add(kia);
            }
            else if ((true == mi.IsUnconscious) && ("ORest" != mi.OverlayImageName)) // if unconscous person is resting, do not show UNC image on counter
            {
               Image mia = new Image() { Stretch = Stretch.Fill, Source = theMia };
               g.Children.Add(mia);
            }
            else if (true == mi.IsRunAway)
            {
               Image runs = new Image() { Stretch = Stretch.Fill, Source = theRunAway };
               g.Children.Add(runs);
            }
            else if (true == mi.IsPlagued)
            {
               Image foam = new Image() { Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("Foam") };
               g.Children.Add(foam);
            }
            if (true == mi.IsShowFireball)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "Fireball.gif", UriKind.Absolute);
               bmi.EndInit();
               Image img1 = new Image { Source = bmi, Stretch = Stretch.UniformToFill, StretchDirection = StretchDirection.Both };
               ImageBehavior.SetAnimatedSource(img1, bmi);
               g.Children.Add(img1);
            }
         }
         else
         {
            IMapImage mii = theMapImages.Find(mi.TopImageName);
            g.Children.Add(mii.ImageControl);
         }
         b.Content = g;
      }
      private static void SetButtonContentAdornments(IMapItem mi, Canvas c, SpecialEnum possession)
      {
         double s0 = Utilities.ZOOM * Utilities.theMapItemSize;  // MapItem Width
         double s0a = 0.5 * s0; // middle of MapItem
         double s0b = 0.75 * s0; // middle of MapItem
         double s1 = Utilities.theMapItemOffset; // middle of MapItem
         double s1a = 0.5 * s1; // middle of MapItem
         double s1b = 0.25 * s1; // middle of MapItem
         double s2 = Utilities.theMapItemOffset * 0.8;  // 80% of MapItem Offset
         double s2a = 0.5 * s2;
         double s2b = 0.25 * s2;
         double s2c = 0.125 * s2;
         double s3 = Utilities.ZOOM * Utilities.theMapItemOffset; // MapItem Height
         double s4 = 0.8 * s0;
         double s4a = 0.4 * s0;
         double s4b = 0.3 * s0;
         double s5 = Utilities.theMapItemOffset + 3;
         double s5a = 0.25 * s5;
         switch (possession)
         {
            case SpecialEnum.HealingPoition:
               Image woundHeal = new Image() { Height = s1, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("PotionHeal") };
               c.Children.Add(woundHeal);
               Canvas.SetLeft(woundHeal, -s2);
               Canvas.SetTop(woundHeal, 0);
               break;
            case SpecialEnum.CurePoisonVial:
               Image poisonHeal = new Image() { Height = s1, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("PotionCure") };
               c.Children.Add(poisonHeal);
               Canvas.SetLeft(poisonHeal, -s2);
               Canvas.SetTop(poisonHeal, s3 - s1 / 2);
               break;
            case SpecialEnum.GiftOfCharm:
               Image giftCharm = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("CharmGift") };
               c.Children.Add(giftCharm);
               Canvas.SetLeft(giftCharm, s0);
               Canvas.SetTop(giftCharm, 0);
               break;
            case SpecialEnum.EnduranceSash:
               Image sash = new Image() { Height = 8, Width = s0, Source = MapItem.theMapImages.GetBitmapImage("Sash") };
               c.Children.Add(sash);
               Canvas.SetLeft(sash, 0);
               Canvas.SetTop(sash, s3 - 8);
               break;
            case SpecialEnum.ResistanceTalisman:
               Image rTalisman = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance") };
               c.Children.Add(rTalisman);
               Canvas.SetLeft(rTalisman, -s2);
               Canvas.SetTop(rTalisman, s0 - s2);
               break;
            case SpecialEnum.PoisonDrug:
               Image poisonDrug = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug") };
               c.Children.Add(poisonDrug);
               Canvas.SetLeft(poisonDrug, s0);
               Canvas.SetTop(poisonDrug, s3 - s1a);
               break;
            case SpecialEnum.MagicSword:
               Image sword = new Image() { Height = s2 + s2c, Width = s2 + s2c, Source = MapItem.theMapImages.GetBitmapImage("Sword1") };
               c.Children.Add(sword);
               Canvas.SetLeft(sword, 0);
               Canvas.SetTop(sword, s0a - s2 - s2c); // bottom middle
               break;
            case SpecialEnum.AntiPoisonAmulet:
               Image antiPoisonAmulet = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison") };
               c.Children.Add(antiPoisonAmulet);
               Canvas.SetLeft(antiPoisonAmulet, -s2);
               Canvas.SetTop(antiPoisonAmulet, s3 - s1a); // overlay cure poison vial
               break;
            case SpecialEnum.PegasusMountTalisman:
               Image pegasus = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("TalismanPegasus") };
               c.Children.Add(pegasus);
               Canvas.SetLeft(pegasus, s0a - s2a);
               Canvas.SetTop(pegasus, s0 - s2); // bottom enter
               break;
            case SpecialEnum.CharismaTalisman:
               Image charismaTalisman = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma") };
               c.Children.Add(charismaTalisman);
               Canvas.SetLeft(charismaTalisman, s0);
               Canvas.SetTop(charismaTalisman, s0 - s2 - 1); // bottom right
               break;
            case SpecialEnum.NerveGasBomb:
               Image nerveGasBomb = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb") };
               c.Children.Add(nerveGasBomb);
               Canvas.SetLeft(nerveGasBomb, s0 - s2a);
               Canvas.SetTop(nerveGasBomb, s3 - s1a);
               break;
            case SpecialEnum.ResistanceRing:
               Image ring = new Image() { Height = s5, Width = s5, Source = MapItem.theMapImages.GetBitmapImage("RingResistence") };
               c.Children.Add(ring);
               Canvas.SetLeft(ring, -s5a);
               Canvas.SetTop(ring, -s5a);
               break;
            case SpecialEnum.ResurrectionNecklace:
               Image necklace = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("Necklace") };
               c.Children.Add(necklace);
               Canvas.SetLeft(necklace, s0a - s2a);
               Canvas.SetTop(necklace, +s2a);
               break;
            case SpecialEnum.ShieldOfLight:
               Image sheild = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("Shield") };
               c.Children.Add(sheild);
               Canvas.SetLeft(sheild, s0a + s2b);
               Canvas.SetTop(sheild, s0a - s2a - s2c);
               break;
            case SpecialEnum.RoyalHelmOfNorthlands:
               Image helm = new Image() { Height = s2, Width = s2, Source = MapItem.theMapImages.GetBitmapImage("Helmet") };
               c.Children.Add(helm);
               Canvas.SetLeft(helm, s0a - s2a);
               Canvas.SetTop(helm, -s2b);
               break;
            default:
               break;
         }
      }
   }
   //--------------------------------------------------------------------------
   [Serializable]
   public class MapItems : IEnumerable, IMapItems
   {
      private readonly ArrayList myList;
      public MapItems() { myList = new ArrayList(); }
      public MapItems(IMapItems mapItems)
      {
         myList = new ArrayList();
         foreach (IMapItem item in mapItems) { this.Add(item); }
      }
      public void Add(IMapItem mi) { myList.Add(mi); }
      public IMapItem RemoveAt(int index)
      {
         IMapItem mi = (IMapItem)myList[index];
         myList.RemoveAt(index);
         return mi;
      }
      public void Insert(int index, IMapItem mi) { myList.Insert(index, mi); }
      public int Count { get { return myList.Count; } }
      public void Reverse() { myList.Reverse(); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItem mi) { return myList.Contains(mi); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItem mi) { return myList.IndexOf(mi); }
      public void Remove(IMapItem mi) { myList.Remove(mi); }
      public IMapItem Find(string miName)
      {
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            if (miName == Utilities.RemoveSpaces(mi.Name))
               return mi;
         }
         return null;
      }
      public IMapItem Remove(string miName)
      {
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            if (miName == mi.Name)
            {
               myList.Remove(mi);
               return mi;
            }
         }
         return null;
      }
      public IMapItem this[int index]
      {
         get { return (IMapItem)(myList[index]); }
         set { myList[index] = value; }
      }
      public IMapItems Shuffle()
      {
         IMapItems newOrder = new MapItems();
         // Random select card in myCards list and
         // remove it.  Then add it to new list. 
         int count = myList.Count;
         for (int i = 0; i < count; i++)
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               IMapItem randomIndex = (IMapItem)myList[index];
               myList.RemoveAt(index);
               newOrder.Add(randomIndex);
            }
         }

         return newOrder;
      }
      public IMapItems SortOnFreeLoad()
      {
         MapItems sortedMapItems = new MapItems(); // return this with highest free load at beginning of list
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((false == mi1.IsUnconscious) && (false == mi1.IsKilled) && (false == mi1.Name.Contains("Eagle")) && (false == mi1.Name.Contains("Falcon")) )
            {
               int freeLoad1 = mi1.GetFreeLoadWithoutModify();
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  int freeLoad2 = mi2.GetFreeLoadWithoutModify();
                  if (freeLoad2 < freeLoad1)
                  {
                     sortedMapItems.Insert(index, mi1); // insert mi1 in front of mi2 if free load greater
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public IMapItems SortOnCombat()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((false == mi1.IsUnconscious) && (false == mi1.IsKilled))
            {
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  if (mi2.Combat < mi1.Combat)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public IMapItems SortOnFood()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((false == mi1.IsUnconscious) && (false == mi1.IsKilled))
            {
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  if (mi2.Food <= mi1.Food)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public IMapItems SortOnCoin()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((false == mi1.IsUnconscious) && (false == mi1.IsKilled))
            {
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  if (mi2.Coin <= mi1.Coin)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public IMapItems SortOnMount()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((false == mi1.IsUnconscious) && (false == mi1.IsKilled))
            {
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  if (mi2.Mounts.Count <= mi1.Mounts.Count)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public IMapItems SortOnGroupNum()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            int index = 0;
            foreach (IMapItem mi2 in sortedMapItems)
            {
               if (mi1.GroupNum <= mi2.GroupNum)
               {
                  if (mi1.GroupNum < mi2.GroupNum)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  else if (mi2.Combat < mi1.Combat)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
               }
               ++index;
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }
         return sortedMapItems;
      }
      public void Rotate(int numOfRotates)
      {
         for (int j = 0; j < numOfRotates; j++)
         {
            Object temp = myList[0];
            for (int i = 0; i < myList.Count - 1; i++)
               myList[i] = myList[i + 1];
            myList[myList.Count - 1] = temp;
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            sb.Append(mi.Name);
            sb.Append(" ");
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
   //--------------------------------------------------------------------------
   public static class MyMapItemExtensions
   {
      public static IMapItem Find(this IList<IMapItem> list, string miName)
      {
         IEnumerable<IMapItem> results = from mi in list where mi.Name == miName select mi;
         if (0 < results.Count())
            return results.First();
         else
            return null;
      }
   }
}
