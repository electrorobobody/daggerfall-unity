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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallConnect;

namespace DaggerfallWorkshop.Game.UserInterface
{
    /// <summary>
    /// Output quest information on HUD to view state in real-time and optionally step-through execution.
    /// Uses some non-bindable keys (not all implemented):
    ///  * Ctrl+F10     Toggle debugger HUD open/close, closing will also resume normal quest execution
    ///  * Ctrl+F11     Toggle step-through at any time (will open debugger HUD if not open)
    ///  * Ctrl+Enter   Step execution to next task/action (only when debugger HUD open and step-through enabled)
    ///  * ]            Show next quest tasks/vars/timers (only when debugger HUD open)
    ///  * [            Show previous quest tasks/vars/timers (only when debugger HUD open)
    /// </summary>
    public class HUDQuestDebugger : Panel
    {
        const int maxTaskRows = 20;
        const int maxTimerRows = 3;
        const int rowHeight = 10;
        const int taskColWidth = 60;
        const int timerColWidth = 100;
        const string noQuestsRunning = "NO QUESTS RUNNING";
        const string questRunning = "Running";
        const string questFinished = "Finished";
        const int taskLabelPoolCount = 84;
        const int timerLabelPoolCount = 20;

        ulong[] allQuests;
        int currentQuestIndex;
        Quest currentQuest;

        TextLabel questNameLabel = new TextLabel();
        TextLabel processLabel = new TextLabel();
        TextLabel tasksHeaderLabel = new TextLabel();
        TextLabel timersHeaderLabel = new TextLabel();
        TextLabel[] taskLabelPool = new TextLabel[taskLabelPoolCount];
        TextLabel[] timerLabelPool = new TextLabel[timerLabelPoolCount];

        public HUDQuestDebugger()
            : base()
        {
            QuestMachine.OnQuestStarted += QuestMachine_OnQuestStarted;
            QuestMachine.OnQuestEnded += QuestMachine_OnQuestEnded;

            // Quest name label
            questNameLabel.Text = noQuestsRunning;
            questNameLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            questNameLabel.ShadowPosition = Vector2.zero;
            questNameLabel.Position = new Vector2(0, 0);
            Components.Add(questNameLabel);

            // Process label
            processLabel.TextColor = Color.white;
            processLabel.ShadowPosition = Vector2.zero;
            processLabel.Position = new Vector2(0, 10);
            Components.Add(processLabel);

            // Tasks header label
            tasksHeaderLabel.Text = "Tasks";
            tasksHeaderLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            tasksHeaderLabel.ShadowPosition = Vector2.zero;
            tasksHeaderLabel.Position = new Vector2(0, 25);
            Components.Add(tasksHeaderLabel);

            // Timers header label
            timersHeaderLabel.Text = "Timers";
            timersHeaderLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            timersHeaderLabel.ShadowPosition = Vector2.zero;
            timersHeaderLabel.Position = new Vector2(0, 255);
            Components.Add(timersHeaderLabel);

            // Label pool setup
            SetupTaskLabels(new Vector2(0, 35));
            SetupTimerLabels(new Vector2(0, 265));

            // Set starting state
            ClearCurrentQuest();
            RefreshQuestsList();

            // Tick with QuestMachine
            QuestMachine.OnTick += QuestMachine_OnTick;
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.LeftBracket))
                MovePreviousQuest();
            else if (Input.GetKeyDown(KeyCode.RightBracket))
                MoveNextQuest();
        }

        private void QuestMachine_OnTick()
        {
            // Must at least one running quests
            if (QuestMachine.Instance.QuestCount == 0)
            {
                if (currentQuest != null)
                {
                    ClearCurrentQuest();
                }
                return;
            }

            // If no quest has been set, get the first active quest
            // We know from previous check there is at least one quest available
            if (currentQuest == null)
            {
                RefreshQuestsList();
                SetCurrentQuest(QuestMachine.Instance.GetQuest(allQuests[0]));
            }

            // Update task and timer status
            UpdateQuestStatus();
        }

        #region Private Methods

        void UpdateQuestStatus()
        {
            // Set task status
            Quest.TaskState[] states = currentQuest.GetTaskStates();
            for (int i = 0; i < states.Length; i++)
            {
                if (!states[i].set)
                    taskLabelPool[i].TextColor = Color.gray;
                else
                    taskLabelPool[i].TextColor = Color.green;
            }

            // Set timer status
            QuestResource[] clocks = currentQuest.GetAllResources(typeof(Clock));
            for (int i = 0; i < clocks.Length; i++)
            {
                Clock clock = (Clock)clocks[i];
                timerLabelPool[i].Text = string.Format("{0} [{1}]", clock.Symbol.Original, clock.GetRemainingTimeString());
                if (clock.Enabled && !clock.Finished)
                    timerLabelPool[i].TextColor = Color.green;
                else if (!clock.Enabled && !clock.Finished)
                    timerLabelPool[i].TextColor = Color.gray;
                else
                    timerLabelPool[i].TextColor = Color.red;
            }

            // Set running status
            // TODO: Use this line for step-through debugging
            if (!currentQuest.QuestComplete)
                processLabel.Text = string.Format("[{0}] - {1}", DaggerfallUnity.Instance.WorldTime.Now.MinTimeString(), questRunning);
            else
                processLabel.Text = string.Format("[{0}] - {1}", DaggerfallUnity.Instance.WorldTime.Now.MinTimeString(), questFinished);
        }

        void SetupTaskLabels(Vector2 startPosition)
        {
            // Create a pool of labels for output
            int row = 0, col = 0;
            for (int i = 0; i < taskLabelPoolCount; i++)
            {
                // Get current position
                Vector2 position = startPosition + new Vector2(col * taskColWidth, row * rowHeight);

                // Create label at current position
                TextLabel label = new TextLabel();
                label.Text = string.Format("label{0}", i);
                label.Position = position;
                label.TextColor = Color.gray;
                label.ShadowPosition = Vector2.zero;
                label.Enabled = false;
                taskLabelPool[i] = label;
                Components.Add(label);

                // Step row and column
                if (++row > maxTaskRows)
                {
                    row = 0;
                    col++;
                }
            }
        }

        void SetupTimerLabels(Vector2 startPosition)
        {
            // Create a pool of labels for output
            int row = 0, col = 0;
            for (int i = 0; i < timerLabelPoolCount; i++)
            {
                // Get current position
                Vector2 position = startPosition + new Vector2(col * timerColWidth, row * rowHeight);

                // Create label at current position
                TextLabel label = new TextLabel();
                label.Text = string.Format("label{0}", i);
                label.Position = position;
                label.TextColor = Color.gray;
                label.ShadowPosition = Vector2.zero;
                label.Enabled = false;
                timerLabelPool[i] = label;
                Components.Add(label);

                // Step row and column
                if (++row > maxTimerRows)
                {
                    row = 0;
                    col++;
                }
            }
        }

        void ClearCurrentQuest()
        {
            currentQuest = null;

            // Set headers
            questNameLabel.Text = noQuestsRunning;
            tasksHeaderLabel.Enabled = false;
            processLabel.Enabled = false;
            timersHeaderLabel.Enabled = false;

            // Disable task labels
            for (int i = 0; i < taskLabelPool.Length; i++)
            {
                taskLabelPool[i].Enabled = false;
            }

            // Disable timer labels
            for(int i = 0; i < timerLabelPool.Length; i++)
            {
                timerLabelPool[i].Enabled = false;
            }
        }

        void SetCurrentQuest(Quest quest)
        {
            currentQuest = quest;
            RefreshQuestsList();

            // Set quest index
            questNameLabel.Text = string.Format("[{0} of {1}] ", currentQuestIndex + 1, allQuests.Length);

            // Set quest name
            if (!string.IsNullOrEmpty(quest.DisplayName))
                questNameLabel.Text += string.Format("{0} '{1}' ", quest.QuestName, quest.DisplayName);
            else
                questNameLabel.Text += string.Format("{0} ", quest.QuestName);

            // Set quest UID
            questNameLabel.Text += string.Format("[UID={0}]", quest.UID);

            // Set headers
            tasksHeaderLabel.Enabled = true;
            processLabel.Enabled = true;
            timersHeaderLabel.Enabled = true;

            // Set task labels
            Quest.TaskState[] states = currentQuest.GetTaskStates();
            for (int i = 0; i < states.Length; i++)
            {
                taskLabelPool[i].Enabled = true;
                if (states[i].type == Task.TaskType.Headless)
                    taskLabelPool[i].Text = "startup";
                else if (states[i].type == Task.TaskType.PersistUntil)
                {
                    Task task = quest.GetTask(states[i].symbol);
                    taskLabelPool[i].Text = string.Format("until_{0}", task.TargetSymbol.Name);
                }
                else
                    taskLabelPool[i].Text = states[i].symbol.Name;
            }

            // Set timer status
            QuestResource[] clocks = currentQuest.GetAllResources(typeof(Clock));
            for (int i = 0; i < clocks.Length; i++)
            {
                timerLabelPool[i].Enabled = true;
            }
        }

        void RefreshQuestsList()
        {
            currentQuestIndex = -1;
            allQuests = QuestMachine.Instance.GetAllQuests();
            if (allQuests != null && allQuests.Length > 0 && currentQuest != null)
            {
                // Find index of current quest
                for (int i = 0; i < allQuests.Length; i++)
                {
                    Quest quest = QuestMachine.Instance.GetQuest(allQuests[i]);
                    if (quest != null && quest.UID == currentQuest.UID)
                    {
                        currentQuestIndex = i;
                        break;
                    }
                }
            }
        }

        void MoveNextQuest()
        {
            if (++currentQuestIndex >= allQuests.Length)
                currentQuestIndex = 0;

            SetCurrentQuest(QuestMachine.Instance.GetQuest(allQuests[currentQuestIndex]));
        }

        void MovePreviousQuest()
        {
            if (--currentQuestIndex < 0)
                currentQuestIndex = allQuests.Length - 1;

            SetCurrentQuest(QuestMachine.Instance.GetQuest(allQuests[currentQuestIndex]));
        }

        #endregion

        #region Event Handlers

        private void QuestMachine_OnQuestStarted(Quest quest)
        {
            ClearCurrentQuest();
            SetCurrentQuest(quest);
        }

        private void QuestMachine_OnQuestEnded(Quest quest)
        {
        }

        #endregion
    }
}