﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2017 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using DaggerfallConnect;

namespace DaggerfallWorkshop.Game.Questing.Actions
{
    /// <summary>
    /// Condition which checks if player character at a specific place.
    /// </summary>
    public class PcAt : ActionTemplate
    {
        Symbol placeSymbol;
        Symbol taskSymbol;
        int textId;

        public override string Pattern
        {
            // Notes:
            // Docs use form "pc at aPlace do aTask"
            // But observed quests actually seem to use "pc at aPlace set aTask"
            // Probably a change between writing of docs and Template v1.11.
            // Docs also missing "pc at aPlace set aTask saying nnnn"
            get { return @"pc at (?<aPlace>\w+) set (?<aTask>[a-zA-Z0-9_.]+) saying (?<id>\d+)|pc at (?<aPlace>\w+) set (?<aTask>[a-zA-Z0-9_.]+)"; }
        }

        public PcAt(Quest parentQuest)
            : base(parentQuest)
        {
        }

        public override IQuestAction CreateNew(string source, Quest parentQuest)
        {
            base.CreateNew(source, parentQuest);

            // Source must match pattern
            Match match = Test(source);
            if (!match.Success)
                return null;

            // Factory new pcat
            PcAt pcat = new PcAt(parentQuest);
            pcat.placeSymbol = new Symbol(match.Groups["aPlace"].Value);
            pcat.taskSymbol = new Symbol(match.Groups["aTask"].Value);
            pcat.textId = Parser.ParseInt(match.Groups["id"].Value);

            return pcat;
        }

        /// <summary>
        /// Continuously checks where player is and sets target true/false based on site properties.
        /// </summary>
        public override void Update(Task caller)
        {
            bool result = false;

            // Get place resource
            Place place = ParentQuest.GetPlace(placeSymbol);
            if (place == null)
                return;

            // Check if player at this place
            result = place.IsPlayerHere();

            // Handle positive check
            if (result)
            {
                // "saying" popup
                // TODO: Should this run every time or only once?
                if (textId != 0)
                    ParentQuest.ShowMessagePopup(textId);

                // Enable target task
                ParentQuest.SetTask(taskSymbol);
            }
            else
            {
                // Disable target task
                ParentQuest.UnsetTask(taskSymbol);
            }
        }
    }
}