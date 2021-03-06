﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    /// <summary>
    /// Specialist class for filtering mobs.
    /// </summary>
    public class MobFilter
    {
        public bool AllMobs { get; set; }

        public bool GroupMobs { get; set; }
        
        public int FightNumber { get; set; }

        public bool Exclude0XPMobs { get; set; }

        public string MobName { get; set; }
        public int MobXP { get; set; }

        public bool CustomSelection { get; set; }
        public HashSet<int> CustomBattleIDs { get; set; }

        public int Count
        {
            get
            {
                return SelectedBattles.Count;
            }
        }

        public HashSet<int> SelectedBattles
        {
            get
            {
                HashSet<int> selectedBattles = new HashSet<int>();

                if (AllMobs)
                {
                    foreach (var mobEntry in MobXPHandler.Instance.CompleteMobList)
                    {
                        if (!Exclude0XPMobs || mobEntry.XP > 0)
                            selectedBattles.Add(mobEntry.BattleID);
                    }
                }
                else if (GroupMobs)
                {
                    if (MobXP < 0)
                    {
                        foreach (var mobEntry in MobXPHandler.Instance.CompleteMobList.Where(m => m.Name == MobName))
                        {
                            selectedBattles.Add(mobEntry.BattleID);
                        }
                    }
                    else
                    {
                        foreach (var mobEntry in MobXPHandler.Instance.CompleteMobList.Where(m => m.Name == MobName && m.BaseXP == MobXP))
                        {
                            selectedBattles.Add(mobEntry.BattleID);
                        }
                    }
                }
                else if (CustomSelection)
                {
                    selectedBattles = CustomBattleIDs;
                }
                else
                {
                    if (FightNumber > 0)
                        selectedBattles.Add(FightNumber);
                }

                return selectedBattles;
            }
        }
    }

    /// <summary>
    /// Class to define thread-safe extension methods for plugin UI elements.
    /// </summary>
    public static class PluginExtensions
    {
        #region Combo Box manipulation
        /// <summary>
        /// UI-thread-safe means of clearing a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        public static void CBReset(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox> thisFunc = CBReset;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo });
                return;
            }

            combo.Items.Clear();
        }

        /// <summary>
        /// UI-thread-safe means of adding an array of strings to a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="strings"></param>
        public static void CBAddStrings(this ToolStripComboBox combo, string[] strings)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, string[]> thisFunc = CBAddStrings;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo, strings });
                return;
            }

            if (strings == null)
                throw new ArgumentNullException("strings");

            if (strings.Length == 0)
                return;

            combo.Items.AddRange(strings);
        }

        /// <summary>
        /// UI-thread-safe means of getting an array of strings from a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static string[] CBGetStrings(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, string[]> thisFunc = CBGetStrings;
                return (string[])combo.ComboBox.Invoke(thisFunc, new object[] { combo });
            }

            string[] stringList = new string[combo.Items.Count];
            for (int i = 0; i < combo.Items.Count; i++)
            {
                stringList[i] = combo.Items[i].ToString();
            }

            return stringList;
        }

        /// <summary>
        /// UI-thread-safe means of selecting an index in a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="index">The new index value to select.  A value of -1 selects
        /// the last entry in the list, rather than unselecting any entry.</param>
        public static void CBSelectIndex(this ToolStripComboBox combo, int index)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, int> thisFunc = CBSelectIndex;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo, index });
                return;
            }

            if (combo.Items.Count == 0)
                return;

            if (index < 0)
                index = combo.Items.Count - 1;

            if (index >= combo.Items.Count)
                index = combo.Items.Count - 1;

            combo.SelectedIndex = index;
        }

        /// <summary>
        /// UI-thread-safe means of selecting an item in a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="name"></param>
        public static void CBSelectItem(this ToolStripComboBox combo, string name)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Action<ToolStripComboBox, string> thisFunc = CBSelectItem;
                combo.ComboBox.Invoke(thisFunc, new object[] { combo, name });
                return;
            }

            if (name == null)
                throw new ArgumentNullException("name");

            if (combo.Items.Count > 0)
                combo.SelectedItem = name;
        }

        /// <summary>
        /// UI-thread-safe means of getting the selected index from a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static int CBSelectedIndex(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, int> thisFunc = CBSelectedIndex;
                return (int)combo.Owner.Invoke(thisFunc, new object[] { combo });
            }

            return combo.SelectedIndex;
        }

        /// <summary>
        /// UI-thread-safe means of getting the selected item from a ToolStripComboBox.
        /// </summary>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static string CBSelectedItem(this ToolStripComboBox combo)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, string> thisFunc = CBSelectedItem;
                return (string)combo.ComboBox.Invoke(thisFunc, new object[] { combo });
            }

            if (combo.SelectedIndex >= 0)
                return combo.SelectedItem.ToString();
            else
                return string.Empty;
        }
        #endregion

        #region Specialist functions for dealing with Mob lists
        public static MobFilter CBGetMobFilter(this ToolStripComboBox combo)
        {
            return combo.CBGetMobFilter(false);
        }

        /// <summary>
        /// Extension function to extract out formatted mob list info from a
        /// drop-down combo box.
        /// </summary>
        /// <param name="combo">The combo box with formatted mob names in it.</param>
        /// <param name="include0XPMobs">Flag to tell whether 0 XP mobs should be included (only relevent to All).</param>
        /// <returns>Returns a MobFilter object containing the relevant filter information.</returns>
        public static MobFilter CBGetMobFilter(this ToolStripComboBox combo, bool exclude0XPMobs)
        {
            if (combo.ComboBox.InvokeRequired)
            {
                Func<ToolStripComboBox, MobFilter> thisFunc = CBGetMobFilter;
                return (MobFilter)combo.ComboBox.Invoke(thisFunc, new object[] { combo });
            }

            MobFilter filter = new MobFilter { AllMobs = true, GroupMobs = false, FightNumber = 0,
                                               MobName = "",
                                               MobXP = -1,
                                               Exclude0XPMobs = exclude0XPMobs,
                                               CustomSelection = false
            };


            if (combo.SelectedIndex >= 0)
            {
                string filterText = combo.SelectedItem.ToString();

                if (filterText != "All")
                {
                    Regex mobBattle = new Regex(@"\s*(?<battleID>\d+):\s+(?<mobName>.*)");
                    Match mobBattleMatch = mobBattle.Match(filterText);

                    if (mobBattleMatch.Success == true)
                    {
                        filter.AllMobs = false;
                        filter.GroupMobs = false;
                        filter.MobName = mobBattleMatch.Groups["mobName"].Value;
                        filter.FightNumber = int.Parse(mobBattleMatch.Groups["battleID"].Value);

                        return filter;
                    }

                    Regex mobAndXP = new Regex(@"((?<mobName>.*(?<! \())) \(((?<xp>\d+)\))|(?<mobName>.*)");
                    Match mobAndXPMatch = mobAndXP.Match(filterText);

                    if (mobAndXPMatch.Success == true)
                    {
                        filter.AllMobs = false;
                        filter.GroupMobs = true;

                        filter.MobName = mobAndXPMatch.Groups["mobName"].Value;

                        if ((mobAndXPMatch.Groups["xp"] != null) && (mobAndXPMatch.Groups["xp"].Value != string.Empty))
                        {
                            filter.MobXP = int.Parse(mobAndXPMatch.Groups["xp"].Value);
                        }
                        else
                        {
                            filter.MobXP = -1;
                        }
                    }
                }
            }

            return filter;
        }

        /// <summary>
        /// An extension method to determine whether a given InteractionsRow passes the filter
        /// check set by the specified MobFilter object.  Checks the filter against the instigator
        /// of the action.
        /// </summary>
        /// <param name="mobFilter">The filter to check against.</param>
        /// <param name="rowToCheck">The InteractionsRow to check.</param>
        /// <returns>Returns true if the row passes the filter test, otherwise false.</returns>
        public static bool CheckFilterMobActor(this MobFilter mobFilter, KPDatabaseDataSet.InteractionsRow rowToCheck)
        {
            if (mobFilter.AllMobs == true)
            {
                // If no battle attached, include it as "between battles" actions; valid
                if (rowToCheck.IsBattleIDNull() == true)
                    return true;

                if (mobFilter.Exclude0XPMobs)
                    return (rowToCheck.BattlesRow.ExperiencePoints > 0);
                else
                    return true;
            }

            if (mobFilter.CustomSelection == true)
            {
                if (rowToCheck.IsBattleIDNull() == true)
                    return false;

                if (mobFilter.CustomBattleIDs.Contains(rowToCheck.BattleID))
                {
                    if (rowToCheck.IsActorIDNull() == true)
                        return false;

                    return (rowToCheck.ActorID == rowToCheck.BattlesRow.EnemyID);
                }
            }

            if (mobFilter.GroupMobs == false)
            {
                if ((rowToCheck.IsBattleIDNull() == false) &&
                    (rowToCheck.BattleID == mobFilter.FightNumber))
                    return true;
                else
                    return false;
            }

            if (mobFilter.MobName == string.Empty)
                return false;

            if (rowToCheck.IsActorIDNull() == true)
                return false;

            if (rowToCheck.IsBattleIDNull() == true)
                return false;

            if (rowToCheck.CombatantsRowByActorCombatantRelation.CombatantName == mobFilter.MobName)
            {
                if (mobFilter.MobXP == -1)
                    return true;

                if (mobFilter.MobXP == MobXPHandler.Instance.GetBaseXP(rowToCheck.BattleID))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An extension method to determine whether a given InteractionsRow passes the filter
        /// check set by the specified MobFilter object.  Checks the filter against the target
        /// of the action.
        /// </summary>
        /// <param name="mobFilter">The filter to check against.</param>
        /// <param name="rowToCheck">The InteractionsRow to check.</param>
        /// <returns>Returns true if the row passes the filter test, otherwise false.</returns>
        public static bool CheckFilterMobTarget(this MobFilter mobFilter, KPDatabaseDataSet.InteractionsRow rowToCheck)
        {
            if (mobFilter.AllMobs == true)
            {
                // If no battle attached, include it as "between battles" actions; valid
                if (rowToCheck.IsBattleIDNull() == true)
                    return true;

                if (mobFilter.Exclude0XPMobs)
                    return (rowToCheck.BattlesRow.ExperiencePoints > 0);
                else
                    return true;
            }

            if (mobFilter.CustomSelection == true)
            {
                if (rowToCheck.IsBattleIDNull() == true)
                    return false;

                if (mobFilter.CustomBattleIDs.Contains(rowToCheck.BattleID))
                {
                    if (rowToCheck.IsTargetIDNull() == true)
                        return false;

                    return (rowToCheck.TargetID == rowToCheck.BattlesRow.EnemyID);
                }
            }

            if (mobFilter.GroupMobs == false)
            {
                if ((rowToCheck.IsBattleIDNull() == false) &&
                    (rowToCheck.BattleID == mobFilter.FightNumber))
                    return true;
                else
                    return false;
            }

            if (mobFilter.MobName == string.Empty)
                return false;

            if (rowToCheck.IsTargetIDNull() == true)
                return false;

            if (rowToCheck.IsBattleIDNull() == true)
                return false;

            if (rowToCheck.CombatantsRowByTargetCombatantRelation.CombatantName == mobFilter.MobName)
            {
                if (mobFilter.MobXP == -1)
                    return true;

                if (mobFilter.MobXP == MobXPHandler.Instance.GetBaseXP(rowToCheck.BattleID))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An extension method to determine whether a given InteractionsRow passes the filter
        /// check set by the specified MobFilter object.  Checks the filter against the battle
        /// during which the action occurs
        /// </summary>
        /// <param name="mobFilter">The filter to check against.</param>
        /// <param name="rowToCheck">The InteractionsRow to check.</param>
        /// <returns>Returns true if the row passes the filter test, otherwise false.</returns>
        public static bool CheckFilterMobBattle(this MobFilter mobFilter, KPDatabaseDataSet.InteractionsRow rowToCheck)
        {
            if (mobFilter.AllMobs == true)
            {
                // If no battle attached, include it as "between battles" actions; valid
                if (rowToCheck.IsBattleIDNull() == true)
                    return true;

                if (mobFilter.Exclude0XPMobs)
                    return (rowToCheck.BattlesRow.ExperiencePoints > 0);
                else
                    return true;
            }

            if (mobFilter.CustomSelection == true)
            {
                if (rowToCheck.IsBattleIDNull() == true)
                    return false;

                return mobFilter.CustomBattleIDs.Contains(rowToCheck.BattleID);
            }

            if (mobFilter.GroupMobs == false)
            {
                if ((rowToCheck.IsBattleIDNull() == false) &&
                    (rowToCheck.BattleID == mobFilter.FightNumber))
                    return true;
                else
                    return false;
            }

            if (mobFilter.MobName == string.Empty)
                return false;

            if (rowToCheck.IsBattleIDNull() == true)
                return false;

            if (rowToCheck.BattlesRow.IsEnemyIDNull() == true)
                return false;

            if (rowToCheck.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == mobFilter.MobName)
            {
                if (mobFilter.MobXP == -1)
                    return true;

                if (mobFilter.MobXP == MobXPHandler.Instance.GetBaseXP(rowToCheck.BattleID))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// An extension method to determine whether a given InteractionsRow passes the filter
        /// check set by the specified MobFilter object.  Checks the filter against the target
        /// of the action.
        /// </summary>
        /// <param name="mobFilter">The filter to check against.</param>
        /// <param name="rowToCheck">The InteractionsRow to check.</param>
        /// <returns>Returns true if the row passes the filter test, otherwise false.</returns>
        public static bool CheckFilterBattle(this MobFilter mobFilter, KPDatabaseDataSet.BattlesRow rowToCheck)
        {
            if (mobFilter.AllMobs == true)
            {
                if (mobFilter.Exclude0XPMobs)
                    return (rowToCheck.ExperiencePoints > 0);
                else if (rowToCheck.IsEnemyIDNull() == true)
                    return false;
                else if ((EntityType)rowToCheck.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob)
                    return true;
            }

            if (rowToCheck.DefaultBattle == true)
                return false;

            if (mobFilter.CustomSelection == true)
            {
                return mobFilter.CustomBattleIDs.Contains(rowToCheck.BattleID);
            }

            if (mobFilter.GroupMobs == false)
            {
                if (rowToCheck.BattleID == mobFilter.FightNumber)
                    return true;
                else
                    return false;
            }

            if (mobFilter.MobName == string.Empty)
                return false;

            if (rowToCheck.IsEnemyIDNull() == true)
                return false;

            if ((EntityType)rowToCheck.CombatantsRowByEnemyCombatantRelation.CombatantType != EntityType.Mob)
                return false;

            if (rowToCheck.CombatantsRowByEnemyCombatantRelation.CombatantName == mobFilter.MobName)
            {
                if (mobFilter.MobXP == -1)
                    return true;

                if (mobFilter.MobXP == MobXPHandler.Instance.GetBaseXP(rowToCheck.BattleID))
                    return true;
            }

            return false;
        }
        #endregion
    }
}

