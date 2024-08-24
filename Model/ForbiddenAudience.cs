using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarbarianPrince
{
   // Certain constraints cause forbidden audience in a town, temple, or castle.
   // If there is a forbidden audience, there is a marker indicating it on the map.
   // When the constraint is removed, the audience can be had.
   // Constraints include number of days, letter of recommendation, and need to make offering.
   [Serializable]
   public class ForbiddenAudience : IForbiddenAudience
   {
      public AudienceConstraintEnum Constraint { get; set; } = AudienceConstraintEnum.ERROR;
      public ITerritory ForbiddenTerritory { get; set; } = null;
      public ITerritory TargetTerritory { get; set; } = null;
      public IMapItem Assistant { get; set; } = null;
      public bool IsOfferingMade { get; set; } = false;
      public int Day { get; set; } = -1; // long number of days indicates forever
      public ForbiddenAudience(ITerritory forbidden)
      {
         ForbiddenTerritory = forbidden;
         TargetTerritory = forbidden;
      }
      public ForbiddenAudience(ITerritory forbidden, ITerritory target)
      {
         ForbiddenTerritory = forbidden;
         TargetTerritory = target;
      }
      public ForbiddenAudience(ITerritory forbidden, int day)
      {
         Constraint = AudienceConstraintEnum.DAY;
         ForbiddenTerritory = forbidden;
         TargetTerritory = forbidden;
         Day = day;
      }
      public ForbiddenAudience(ITerritory forbidden, IMapItem assistant)
      {
         Constraint = AudienceConstraintEnum.ASSISTANT_OR_LETTER;
         ForbiddenTerritory = forbidden;
         TargetTerritory = null; // this must be null in order for UpdateLetterLocation() to work when user sets temple or castle hext
         Assistant = assistant;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("");
         sb.Append("c=");
         sb.Append(Constraint.ToString());
         sb.Append(",ft=");
         sb.Append(ForbiddenTerritory.Name);
         sb.Append(",ft=");
         sb.Append(TargetTerritory.Name);
         sb.Append(",d=");
         sb.Append(Day.ToString());
         return sb.ToString();
      }
   }
   //---------------------------------------------------------
   [Serializable]
   public class ForbiddenAudiences : IEnumerable, IForbiddenAudiences
   {
      private readonly ArrayList myList;
      public ForbiddenAudiences() { myList = new ArrayList(); }
      public int Count { get { return myList.Count; } }
      public void Add(IForbiddenAudience fa) { myList.Add(fa); }
      public IForbiddenAudience RemoveAt(int index)
      {
         IForbiddenAudience fa = (IForbiddenAudience)myList[index];
         myList.RemoveAt(index);
         return fa;
      }
      public void Insert(int index, IForbiddenAudience fa) { myList.Insert(index, fa); }
      public void Reverse() { myList.Reverse(); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IForbiddenAudience fa) { return myList.Contains(fa); }   
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IForbiddenAudience fa) { return myList.IndexOf(fa); }
      public void Remove(IForbiddenAudience fa) { myList.Remove(fa); }
      public IForbiddenAudience this[int index]
      {
         get { return (IForbiddenAudience)(myList[index]); }
         set { myList[index] = value; }
      }
      //-------------------------------------------------------------
      public bool Contains(ITerritory forbidden)
      {
         foreach (IForbiddenAudience fa in myList)
         {
            if (fa.ForbiddenTerritory.Name == forbidden.Name)
               return true;
         }
         return false;
      }
      public void AddClothesConstraint(ITerritory forbidden)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden);
         fa.Constraint = AudienceConstraintEnum.CLOTHES;
         myList.Add(fa);
      }
      public void AddPurifyConstaint(ITerritory forbidden)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden);
         fa.Constraint = AudienceConstraintEnum.PURIFICATION;
         myList.Add(fa);
      }
      public void AddOfferingConstaint(ITerritory forbidden, int day)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden);
         fa.Constraint = AudienceConstraintEnum.OFFERING;
         fa.TargetTerritory = forbidden;
         fa.Day = day;
         myList.Add(fa);
      }
      public void AddLetterConstraint(ITerritory forbidden, ITerritory target=null) // target set to null when user is selecting target with bullseye on map
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden, target);
         fa.Constraint = AudienceConstraintEnum.LETTER;
         myList.Add(fa);
      }
      public void AddLetterGivenConstraint(ITerritory forbidden)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden);
         fa.Constraint = AudienceConstraintEnum.LETTER_GIVEN;
         myList.Add(fa);
      }
      public void AddMonsterKillConstraint(ITerritory forbidden)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden);
         fa.Constraint = AudienceConstraintEnum.MONSTER_KILL;
         myList.Add(fa);
      }
      public void AddTimeConstraint(ITerritory forbidden, int day)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden, day);
         myList.Add(fa);
      }
      public void AddAssistantConstraint(ITerritory forbidden, IMapItem assistant)
      {
         IForbiddenAudience fa = new ForbiddenAudience(forbidden, assistant);
         myList.Add(fa);
      }
      //-------------------------------------------------------------
      public bool UpdateLetterLocation(ITerritory letterTerritory)
      {
         foreach (IForbiddenAudience fa in myList)
         {
            if ((fa.Constraint == AudienceConstraintEnum.ASSISTANT_OR_LETTER) && (null == fa.TargetTerritory) )
            {
               fa.TargetTerritory = letterTerritory;
               return true;
            }
         }
         foreach (IForbiddenAudience fa in myList)
         {
            if ((fa.Constraint == AudienceConstraintEnum.LETTER) && (null == fa.TargetTerritory))
            {
               fa.TargetTerritory = letterTerritory;
               return true;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "UpdateLetterLocation(): Unknown t=" + letterTerritory.Name);
         return false;
      }
      public void UpdateOfferingLocation(ITerritory offeringTerritory) // TODO Offering made causes constraint to be finished.
      {
         foreach (IForbiddenAudience fa in myList)
         {
            if (null == fa.ForbiddenTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateOfferingLocation(): fa.ForbiddenTerritory=null");
               continue;
            }
            if ((fa.Constraint == AudienceConstraintEnum.OFFERING) && (fa.ForbiddenTerritory.Name == offeringTerritory.Name))
               fa.IsOfferingMade = true;
         }
      }
      //-------------------------------------------------------------
      public bool IsClothesConstraint()
      {
         foreach (IForbiddenAudience fa in myList)
         {
            if (AudienceConstraintEnum.CLOTHES == fa.Constraint)
               return true;
         }
         return false;
      }
      public bool IsOfferingsConstraint(ITerritory offeringTerritory, int offeringDay)
      {
         foreach (IForbiddenAudience fa in myList)
         {
            if ((offeringDay <= fa.Day) && (fa.Day < Utilities.FOREVER))
            {
               if (AudienceConstraintEnum.OFFERING == fa.Constraint)
                  return true;
            }
         }
         return false;
      }
      public void RemoveOfferingsConstraints(ITerritory offeringTerritory)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if ((AudienceConstraintEnum.OFFERING == fa.Constraint) && (fa.TargetTerritory.Name == offeringTerritory.Name))
               {
                  Remove(fa);
                  isListModified = true;
                  break;
               }
            }
         }
      }
      public void RemovePurifyConstraints(ITerritory offeringTerritory, ITerritories purifications)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (AudienceConstraintEnum.PURIFICATION == fa.Constraint)
               {
                  purifications.Add(fa.ForbiddenTerritory); // e159  - add +2 when seeking audience in this hex
                  Remove(fa);
                  isListModified = true;
                  break;
               }
            }
         }
      }
      public void RemoveLetterConstraints(ITerritory letterTerritory)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all Forbidden Audiences corresponding to Letters
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (null == fa.TargetTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "RemoveLetterConstraints(): fa.TargetTerritory=null");
                  Remove(fa);
                  isListModified = true;
                  break;
               }
               if (fa.TargetTerritory.Name == letterTerritory.Name)
               {
                  if ((AudienceConstraintEnum.LETTER == fa.Constraint) || (AudienceConstraintEnum.ASSISTANT_OR_LETTER == fa.Constraint))
                  {
                     Remove(fa);
                     isListModified = true;
                     break;
                  }
               }
            }
         }
      }
      public void RemoveLetterGivenConstraints(ITerritory letterTerritory)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all Forbidden Audiences corresponding to Letters
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (null == fa.TargetTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "RemoveLetterGivenConstraints(): fa.TargetTerritory=null");
                  Remove(fa);
                  isListModified = true;
                  break;
               }
               if (fa.TargetTerritory.Name == letterTerritory.Name)
               {
                  if (AudienceConstraintEnum.LETTER_GIVEN == fa.Constraint)
                  {
                     Remove(fa);
                     isListModified = true;
                     break;
                  }
               }
            }
         }
      }
      public void RemoveAssistantConstraints(IMapItem mi)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (AudienceConstraintEnum.ASSISTANT_OR_LETTER == fa.Constraint)
               {
                  if (null == fa.Assistant)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "RemoveAssistantConstraints(): fa.TargetTerritory=null");
                     Remove(fa);
                     isListModified = true;
                     break;
                  }
                  else if (fa.Assistant.Name == mi.Name)
                  {
                     Remove(fa);
                     isListModified = true;
                     break;
                  }
               }
            }
         }
      }
      public void RemoveTimeConstraints(int day)
      {
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if ( ( -1 < fa.Day ) && (fa.Day < day) )
               {
                  if ( (AudienceConstraintEnum.DAY == fa.Constraint) || (AudienceConstraintEnum.OFFERING == fa.Constraint) ) 
                  {
                     Remove(fa);
                     isListModified = true;
                     break;
                  }
               }
            }
         }
      }
      public void RemoveClothesConstraints()
      {
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (AudienceConstraintEnum.CLOTHES == fa.Constraint)
               {
                  Remove(fa);
                  isListModified = true;
                  break;
               }
            }
         }
      }
      public void RemoveMonsterKillConstraints(int numKills)
      {
         if (numKills < 5) // must reach threshold of 5 to remove this constraints
            return;
         bool isListModified = true;
         while (true == isListModified) // remove all ForbiddenAudiences requiring offering
         {
            isListModified = false;
            foreach (IForbiddenAudience fa in myList)
            {
               if (AudienceConstraintEnum.MONSTER_KILL == fa.Constraint)
               {
                  Remove(fa);
                  isListModified = true;
                  break;
               }
            }
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("fa[");
         sb.Append(this.Count.ToString());
         sb.Append("]=");
         foreach (IForbiddenAudience fa in this)
         {
            sb.Append("{");
            sb.Append(fa.ToString());
            sb.Append("}");
         }
         return sb.ToString();
      }
   }
}
