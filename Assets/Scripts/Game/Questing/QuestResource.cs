﻿using System;
using DaggerfallWorkshop.Utility;
using System.Text.RegularExpressions;

namespace DaggerfallWorkshop.Game.Questing
{
    /// <summary>
    /// All quest resources hosted by a quest must inherit from this base class.
    /// Symbol will be set by inheriting class after parsing source text.
    /// </summary>
    public abstract class QuestResource : IDisposable
    {
        Quest parentQuest = null;
        Symbol symbol;
        int infoMessageID = -1;
        int usedMessageID = -1;
        int rumorsMessageID = -1;
        bool hasPlayerClicked = false;
        bool isHidden = false;

        [NonSerialized]
        QuestResourceBehaviour questResourceBehaviour = null;

        /// <summary>
        /// Symbol of this quest resource (if any).
        /// </summary>
        public Symbol Symbol
        {
            get { return symbol; }
            protected set { symbol = value; }
        }

        /// <summary>
        /// Parent quest of this quest resource (if any).
        /// </summary>
        public Quest ParentQuest
        {
            get { return parentQuest; }
        }

        /// <summary>
        /// Gets or sets message ID for "tell me about" this resource in dialog.
        /// -1 is default and means no message of this type is associated with this resource.
        /// </summary>
        public int InfoMessageID
        {
            get { return infoMessageID; }
            set { infoMessageID = value; }
        }

        /// <summary>
        /// Gets or sets message ID to display when resource (item) is used by player.
        /// -1 is default and means no message of this type is associated with this resource.
        /// </summary>
        public int UsedMessageID
        {
            get { return usedMessageID; }
            set { usedMessageID = value; }
        }

        /// <summary>
        /// Gets or sets message ID for "any news?" about this resource in dialog.
        /// -1 is default and means no message of this type is associated with this resource.
        /// </summary>
        public int RumorsMessageID
        {
            get { return rumorsMessageID; }
            set { rumorsMessageID = value; }
        }

        /// <summary>
        /// Gets flag stating if player has clicked on this Person resource in world.
        /// If consuming click rearm using RearmPlayerClick().
        /// </summary>
        public bool HasPlayerClicked
        {
            get { return hasPlayerClicked; }
        }

        /// <summary>
        /// Gets or sets flag to hide this quest resource in world.
        /// Has no effect on Foes or quest resources not active inside scene.
        /// </summary>
        public bool IsHidden
        {
            get { return isHidden; }
            set { SetHidden(value); }
        }

        /// <summary>
        /// Gets or sets reference to QuestResourceBehaviour in scene.
        /// This property is not serialized - it should be set at time of injection or deserialization.
        /// </summary>
        public QuestResourceBehaviour QuestResourceBehaviour
        {
            get { return questResourceBehaviour; }
            set
            {
                questResourceBehaviour = value;
                questResourceBehaviour.OnGameObjectDestroy += QuestResourceBehaviour_OnGameObjectDestroy;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentQuest">Parent quest owning this resource. Can be null.</param>
        public QuestResource(Quest parentQuest)
        {
            this.parentQuest = parentQuest;
        }

        /// <summary>
        /// Set the resource source line.
        /// Always call this base method to ensure optional message tags are parsed.
        /// </summary>
        /// <param name="line">Source line for this resource.</param>
        public virtual void SetResource(string line)
        {
            ParseMessageTags(line);
        }

        /// <summary>
        /// Expand a macro from this resource.
        /// </summary>
        /// <param name="macro">Type of macro to expand.</param>
        /// <param name="textOut">Expanded text for this macro type. Empty if macro cannot be expanded.</param>
        /// <returns>True if macro expanded, otherwise false.</returns>
        public virtual bool ExpandMacro(MacroTypes macro, out string textOut)
        {
            textOut = string.Empty;

            return false;
        }

        /// <summary>
        /// Called every Quest tick.
        /// Allows quest resources to perform some action.
        /// Quest will not call Tick() on ActionTemplate items, they should override Update() instead.
        /// </summary>
        public virtual void Tick(Quest caller)
        {
            // Show or hide GameObject for related QuestResourceBehaviour
            if (questResourceBehaviour)
            {
                // Ignore for Foes
                if (this is Foe)
                    return;

                // Show hide GameObject mapped to this QuestResource based on hidden flag
                // This can conflict with other code that has disabled GameObject for other reasons
                questResourceBehaviour.gameObject.SetActive(!IsHidden);
            }
        }

        /// <summary>
        /// Parse optional message tags from this resource.
        /// </summary>
        /// <param name="line"></param>
        protected void ParseMessageTags(string line)
        {
            string matchStr = @"anyInfo (?<info>\d+)|used (?<used>\d+)|rumors (?<rumors>\d+)";

            // Get message tag matches
            MatchCollection matches = Regex.Matches(line, matchStr);
            if (matches.Count == 0)
                return;

            // Grab tag values
            foreach (Match match in matches)
            {
                // Match info message id
                Group info = match.Groups["info"];
                if (info.Success)
                    infoMessageID = Parser.ParseInt(info.Value);

                // Match used message id
                Group used = match.Groups["used"];
                if (used.Success)
                    usedMessageID = Parser.ParseInt(used.Value);

                // Match rumors message id
                Group rumors = match.Groups["rumors"];
                if (rumors.Success)
                    rumorsMessageID = Parser.ParseInt(rumors.Value);
            }
        }

        /// <summary>
        /// Called when quest ends so resource can clean up.
        /// </summary>
        public virtual void Dispose()
        {
            RaiseOnDisposeEvent();
        }

        /// <summary>
        /// Check if player click has been triggered.
        /// </summary>
        public void SetPlayerClicked()
        {
            hasPlayerClicked = true;
        }

        /// <summary>
        /// Rearm click so player can click again if quest allows it.
        /// </summary>
        public void RearmPlayerClick()
        {
            hasPlayerClicked = false;
        }

        #region Private Methods

        void SetHidden(bool value)
        {
            // Do not allow this for Foes
            // They are a one-to-many virtual resource unlike Items and NPCs which are one-to-one once instantiated in world
            if (this is Foe)
                return;

            // Set hidden flag for other resources
            isHidden = true;
        }

        #endregion

        #region Events

        // OnDispose
        public delegate void OnDisposeEventHandler();
        public event OnDisposeEventHandler OnDispose;
        protected virtual void RaiseOnDisposeEvent()
        {
            if (OnDispose != null)
                OnDispose();
        }

        #endregion

        #region Event Handlers

        private void QuestResourceBehaviour_OnGameObjectDestroy(QuestResourceBehaviour questResourceBehaviour)
        {
            // Clean up when target GameObject being destroyed
            questResourceBehaviour.OnGameObjectDestroy -= QuestResourceBehaviour_OnGameObjectDestroy;
            questResourceBehaviour = null;
        }

        #endregion
    }
}