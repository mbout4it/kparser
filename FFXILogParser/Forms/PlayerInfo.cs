﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Forms
{
    public partial class PlayerInfo : Form
    {
        // Holding class
        internal class CombatantData
        {
            internal string Name { get; set; }
            internal EntityType CombatantType { get; set; }
            internal string Description { get; set; }
        }

        CombatantData[] playerDataList;
        string databaseFilename;

        #region Constructor
        public PlayerInfo()
        {
            InitializeComponent();

            // Load information from the database and work with it
            // in a disconnected state.

            DatabaseManager db = DatabaseManager.Instance;

            if (db == null)
                throw new InvalidOperationException();

            databaseFilename = db.DatabaseFilename;

            var playerData = from com in db.Database.Combatants
                             where ((EntityType)com.CombatantType == EntityType.Player ||
                                (EntityType)com.CombatantType == EntityType.Pet ||
                                (EntityType)com.CombatantType == EntityType.Fellow)
                             orderby com.CombatantName
                             select new CombatantData
                             {
                                 Name = com.CombatantName,
                                 CombatantType = (EntityType)com.CombatantType,
                                 Description = com.PlayerInfo
                             };

            // Put the acquired data in the listbox

            playerDataList = playerData.ToArray();

            combatantListBox.Items.Clear();
            foreach (var player in playerDataList)
            {
                combatantListBox.Items.Add(player.Name);
            }

            combatantListBox.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        private void ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void combatantListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var player = playerDataList[combatantListBox.SelectedIndex];

            combatantType.Text = player.CombatantType.ToString();

            combatantDescription.Text = player.Description;
        }

        private void combatantDescription_TextChanged(object sender, EventArgs e)
        {
            var player = playerDataList[combatantListBox.SelectedIndex];

            player.Description = combatantDescription.Text;
        }

        private void PlayerInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    DatabaseManager db = DatabaseManager.Instance;

                    // Make sure the database is still open
                    if (db == null)
                    {
                        MessageBox.Show("The parse file is no longer open.", "Cannot save",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Make sure it's the same database file as originally loaded
                    if (db.DatabaseFilename != databaseFilename)
                    {
                        MessageBox.Show("The current parse file is not the same one as was used to open this dialog.",
                            "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Verify all the combatant names before proceeding.
                    foreach (var player in playerDataList)
                    {
                        if (db.Database.Combatants.Any(cm => cm.CombatantName == player.Name &&
                            (EntityType)cm.CombatantType == player.CombatantType) == false)
                        {
                            MessageBox.Show("The current parse file does not have the same players as when this dialog opened.",
                                "Cannot save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Ok, we're good to go.
                    foreach (var player in playerDataList)
                    {
                        var dbPlayer = db.Database.Combatants.FirstOrDefault(cm => cm.CombatantName == player.Name &&
                            (EntityType)cm.CombatantType == player.CombatantType);

                        if (dbPlayer != null)
                        {
                            dbPlayer.PlayerInfo = player.Description;
                        }
                    }

                    db.RequestUpdate();
                }
            }
        }
        #endregion

    }
}