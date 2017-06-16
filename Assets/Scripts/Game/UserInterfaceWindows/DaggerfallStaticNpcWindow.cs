// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2016 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility;
using System;
using UnityEngine;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class DaggerfallStaticNpcWindow : DaggerfallPopupWindow
    {
        #region Enums

        enum NpcWindowType
        {
            General,
            GeneralSmall,
            Tavern,
            Guild,
            GuildDisabled,
            Repair,
            OrderOfLamp,
        }

        enum NpcWindowButtonType
        {
            Talk,
            Empty,
            Empty1,
            Empty2,
            Exit,
            Room,
            FoodAndDrinks,
            Goodbye,
            JoinGuid,
            RepairItem,
            Sell,
            OrderOfLampInfo,
            OrderOfLampJoin,
        }

        public enum NpcTypes
        {
            Regular     = 0x0000, // Regular talk
            NpcType0024 = 0x0024, // Regular talk
            NpcType0025 = 0x0025, // Regular talk
            NpcType0029 = 0x0029, // Regular talk
            NpcType003C = 0x003C, // JOIN GUID, TALK, Buy Spells
            NpcType003D = 0x003D, // JOIN GUID, TALK, Training
            NpcType003E = 0x003E, // JOIN GUID, TALK, Teleportation
            NpcType003F = 0x003F, // JOIN GUID, TALK, Get Quest
            NpcType0040 = 0x0040, // JOIN GUID, TALK, Make Spells
            NpcType0041 = 0x0041, // JOIN GUID, TALK, Buy Magic Items
            NpcType0042 = 0x0042, // JOIN GUID, TALK, Daedra Summoning
            NpcType0052 = 0x0052, // Regular talk
            NpcType0055 = 0x0055, // Regular talk
            NpcType005B = 0x005B, // Regular talk
            NpcType0062 = 0x0062, // ??? No response
            NpcType0063 = 0x0063, // Regular talk
            NpcType006B = 0x006B, // Regular talk
            NpcType00CB = 0x00CB,
            NpcType00CF = 0x00CF,
            NpcType00D0 = 0x00D0, // JOIN GUID, TALK, Daedra Summoning
            NpcType00F0 = 0x00F0, // JOIN GUID, TALK, Get Quest
            NpcType00F1 = 0x00F1, // JOIN GUID, TALK, Training
            NpcType00F3 = 0x00F3, // JOIN GUID, TALK, Training
            NpcType00FA = 0x00FA, // JOIN GUID, TALK, Training
            NpcType00FC = 0x00FC, // JOIN GUID, TALK, Training
            NpcType00FE = 0x00FE, // JOIN GUID, TALK, Training
            NpcType0168 = 0x0168,
            NpcType0174 = 0x0174,
            NpcType0196 = 0x0196,
            NpcType01A3 = 0x01A3, // TALK, DAEDRA SUMMONING, QUEST
            NpcType01C5 = 0x01C5, // JOIN GUID, TALK, Buy Potions
            NpcType01C6 = 0x01C6, // JOIN GUID, TALK, Make Potions
            NpcType01C7 = 0x01C7, // JOIN GUID, TALK, Buy Soulgems
            NpcType01C8 = 0x01C8, // JOIN GUID, TALK, Daedra Summoning
            NpcType01CE = 0x01CE, // JOIN GUID, TALK, Buy Potions
            NpcType01CF = 0x01CF, // JOIN GUID, TALK, Make Potions
            NpcType01D0 = 0x01D0, // JOIN GUID, TALK, Daedra Summoning
            NpcType01E5 = 0x01E5, // JOIN GUID, TALK, Buy Potions
            NpcType01E7 = 0x01E7, // JOIN GUID, TALK, Make Potions
            NpcType01E8 = 0x01E8, // JOIN GUID, TALK, Daedra Summoning
            NpcType01EA = 0x01EA, // JOIN GUID, TALK, Buy Potions
            NpcType01EB = 0x01EB, // JOIN GUID, TALK, Make Potions
            NpcType01EC = 0x01EC, // JOIN GUID, TALK, Daedra Summoning
            NpcType01F0 = 0x01F0, // JOIN GUID, TALK, Buy Spells
            NpcType01F1 = 0x01F1, // JOIN GUID, TALK, Make Spells
            NpcType01F2 = 0x01F2, // JOIN GUID, TALK, Daedra Summoning
            NpcType01FE = 0x01FE, // ROOM, TALK, FOOD & DRINKS, GOODBYE
            NpcType0200 = 0x0200, // ??? Quest giver
            NpcType0321 = 0x0321, // JOIN GUID, TALK, Identify
            NpcType0322 = 0x0322, // JOIN GUID, TALK, Make Magic Items
            NpcType0326 = 0x0326, // Regular talk
            NpcType032A = 0x032A, // JOIN GUID, TALK, Make Donation
            NpcType032B = 0x032B, // Regular talk
            NpcType032D = 0x032D, // JOIN GUID, TALK, Cure Disease
            NpcType0351 = 0x0351, // JOIN GUID, TALK, Training
            NpcType0352 = 0x0352, // JOIN GUID, TALK, Repair Item
            NpcType0353 = 0x0353, // JOIN GUID, TALK, Get Quest
            NpcType0354 = 0x0354, // ??? No response. Someone in palace
        }

        #endregion

        #region UI Structs
        class NpcWindowData
        {
            internal string nativeImageName;
            internal NpcWindowButton[] buttons;
        }

        class NpcWindowButton
        {
            internal Rect buttonImageRect;
            internal NpcWindowButtonType buttonType;
        }

        static readonly NpcWindowData generalNpcWindowData = new NpcWindowData
        {
            nativeImageName = "GNRC00I0.IMG", // Window image: TALK, <empty1>, <empty2>, EXIT
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 5, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // <empty 1>
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Empty1,
                },
                new NpcWindowButton // <empty 2>
                {
                    buttonImageRect = new Rect(5, 23, 120, 7),
                    buttonType = NpcWindowButtonType.Empty2,
                },
                new NpcWindowButton // EXIT
                {
                    buttonImageRect = new Rect(47, 36, 37, 9),
                    buttonType = NpcWindowButtonType.Exit,
                },
            }
        };

        static readonly NpcWindowData generalSmallNpcWindowData = new NpcWindowData
        {
            nativeImageName = "GNRC01I0.IMG", // Window image: TALK, <empty>, EXIT
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 5, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // <empty>
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Empty,
                },
                new NpcWindowButton // EXIT
                {
                    buttonImageRect = new Rect(47, 36, 37, 9), // TODO
                    buttonType = NpcWindowButtonType.Exit,
                },
            }
        };

        static readonly NpcWindowData tavernNpcWindowData = new NpcWindowData
        {
            nativeImageName = "TVRN00I0.IMG", // Window image: ROOM, TALK, FOOD & DRINKS, GOODBYE
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // ROOM
                {
                    buttonImageRect = new Rect(5, 5, 120, 7),
                    buttonType = NpcWindowButtonType.Room,
                },
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // FOOD & DRINKS
                {
                    buttonImageRect = new Rect(5, 23, 120, 7),
                    buttonType = NpcWindowButtonType.FoodAndDrinks,
                },
                new NpcWindowButton // GOODBYE
                {
                    buttonImageRect = new Rect(5, 32, 120, 7),
                    buttonType = NpcWindowButtonType.Goodbye,
                },
            }
        };

        static readonly NpcWindowData guildNpcWindowData = new NpcWindowData
        {
            nativeImageName = "GILD00I0.IMG", // Window image: JOIN GUID, TALK, <empty>, EXIT
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // JOIN GUID
                {
                    buttonImageRect = new Rect(5, 5, 120, 7),
                    buttonType = NpcWindowButtonType.JoinGuid,
                },
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // <empty>
                {
                    buttonImageRect = new Rect(5, 23, 120, 7),
                    buttonType = NpcWindowButtonType.Empty,
                },
                new NpcWindowButton // EXIT
                {
                    buttonImageRect = new Rect(47, 36, 37, 9),
                    buttonType = NpcWindowButtonType.Empty,
                },
            }
        };

        static readonly NpcWindowData guildDisabledNpcWindowData = new NpcWindowData
        {
            nativeImageName = "GILD01I0.IMG", // Window image: JOIN GUID (disabled), TALK, <empty>, EXIT
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // <empty>
                {
                    buttonImageRect = new Rect(5, 23, 120, 7),
                    buttonType = NpcWindowButtonType.Empty,
                },
                new NpcWindowButton // EXIT
                {
                    buttonImageRect = new Rect(47, 36, 37, 9),
                    buttonType = NpcWindowButtonType.Exit,
                },
            }
        };

        static readonly NpcWindowData repairNpcWindowData = new NpcWindowData
        {
            nativeImageName = "REPR01I0.IMG", // Window image: REPAIR ITEM, TALK, SELL, EXIT
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // REPAIR ITEM
                {
                    buttonImageRect = new Rect(5, 5, 120, 7),
                    buttonType = NpcWindowButtonType.RepairItem,
                },
                new NpcWindowButton // TALK
                {
                    buttonImageRect = new Rect(5, 14, 120, 7),
                    buttonType = NpcWindowButtonType.Talk,
                },
                new NpcWindowButton // SELL
                {
                    buttonImageRect = new Rect(5, 23, 120, 7),
                    buttonType = NpcWindowButtonType.Sell,
                },
                new NpcWindowButton // EXIT
                {
                    buttonImageRect = new Rect(47, 36, 37, 9),
                    buttonType = NpcWindowButtonType.Exit,
                },
            }
        };

        static readonly NpcWindowData lampOrderNpcWindowData = new NpcWindowData
        {
            nativeImageName = "LAMP00I0.IMG", // Window image: INFORMATION ABOUT ORDER OF THE LAMP, JOIN ORDER OF THE LAMP
            buttons = new NpcWindowButton[]
            {
                new NpcWindowButton // INFORMATION ABOUT ORDER OF THE LAMP
                {
                    buttonImageRect = new Rect(5, 5, 120, 7), // TODO
                    buttonType = NpcWindowButtonType.OrderOfLampInfo,
                },
                new NpcWindowButton // JOIN ORDER OF THE LAMP
                {
                    buttonImageRect = new Rect(5, 14, 120, 7), // TODO
                    buttonType = NpcWindowButtonType.OrderOfLampJoin,
                },
            }
        };

        #endregion

        #region UI Controls

        Panel popupPanel = new Panel();
        TextLabel emptyButtonLabel = new TextLabel();
        TextLabel empty1ButtonLabel = new TextLabel();
        TextLabel empty2ButtonLabel = new TextLabel();

        #endregion

        #region Fields

        public int NpcFactionID;
        public int NpcFlags;

        string emptyButtonText;
        string empty1ButtonText;
        string empty2ButtonText;

        #endregion

        #region Constructors

        public DaggerfallStaticNpcWindow(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
        }

        private void ShowLabel(TextLabel label, Vector2 centerPosition, string text)
        {
            label.Position = centerPosition;
            label.Text = text;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            popupPanel.Components.Add(label);
        }

        #endregion

        #region Overrides

        public override void OnPush()
        {
            base.OnPush();

            // Set window type based on NPC type
            NpcWindowData windowData = null;
            if (Enum.IsDefined(typeof(NpcTypes), NpcFactionID))
            {
                NpcTypes npcType = (NpcTypes)NpcFactionID;
                switch (npcType)
                {
                    case NpcTypes.Regular:
                    case NpcTypes.NpcType0024:
                    case NpcTypes.NpcType0025:
                    case NpcTypes.NpcType0029:
                    case NpcTypes.NpcType0052:
                    case NpcTypes.NpcType0055:
                    case NpcTypes.NpcType005B:
                    case NpcTypes.NpcType0063:
                    case NpcTypes.NpcType006B:
                        // TODO: Switch to regular Talk window
                        break;
                    case NpcTypes.NpcType003C: // JOIN GUID, TALK, Buy Spells
                    case NpcTypes.NpcType01F0:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Buy Spells";
                        break;
                    case NpcTypes.NpcType003D: // JOIN GUID, TALK, Training
                    case NpcTypes.NpcType00F1:
                    case NpcTypes.NpcType00F3:
                    case NpcTypes.NpcType00FA:
                    case NpcTypes.NpcType00FC:
                    case NpcTypes.NpcType00FE:
                    case NpcTypes.NpcType0351:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Training";
                        break;
                    case NpcTypes.NpcType003E: // JOIN GUID, TALK, Teleportation
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Teleportation";
                        break;
                    case NpcTypes.NpcType003F: // JOIN GUID, TALK, Get Quest
                    case NpcTypes.NpcType00F0:
                    case NpcTypes.NpcType0353:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Get Quest";
                        break;
                    case NpcTypes.NpcType0040: // JOIN GUID, TALK, Make Spells
                    case NpcTypes.NpcType01F1:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Make Spells";
                        break;
                    case NpcTypes.NpcType0041: // JOIN GUID, TALK, Buy Magic Items
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Buy Magic Items";
                        break;
                    case NpcTypes.NpcType0042: // JOIN GUID, TALK, Daedra Summoning
                    case NpcTypes.NpcType00D0:
                    case NpcTypes.NpcType01C8:
                    case NpcTypes.NpcType01D0:
                    case NpcTypes.NpcType01E8:
                    case NpcTypes.NpcType01EC:
                    case NpcTypes.NpcType01F2:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Daedra Summoning";
                        break;
                    case NpcTypes.NpcType01A3: // TALK, DAEDRA SUMMONING, QUEST
                        windowData = generalNpcWindowData;
                        empty1ButtonText = "DAEDRA SUMMONING";
                        empty2ButtonText = "QUEST";
                        break;
                    case NpcTypes.NpcType01C5: // JOIN GUID, TALK, Buy Potions
                    case NpcTypes.NpcType01CE:
                    case NpcTypes.NpcType01E5:
                    case NpcTypes.NpcType01EA:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Buy Potions";
                        break;
                    case NpcTypes.NpcType01C6: // JOIN GUID, TALK, Make Potions
                    case NpcTypes.NpcType01CF:
                    case NpcTypes.NpcType01E7:
                    case NpcTypes.NpcType01EB:
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Make Potions";
                        break;
                    case NpcTypes.NpcType01C7: // JOIN GUID, TALK, Buy Soulgems
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Buy Soulgems";
                        break;
                    case NpcTypes.NpcType01FE: // ROOM, TALK, FOOD & DRINKS, GOODBYE
                        windowData = tavernNpcWindowData;
                        break;
                    case NpcTypes.NpcType0321: // JOIN GUID, TALK, Identify
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Identify";
                        break;
                    case NpcTypes.NpcType0322: // JOIN GUID, TALK, Make Magic Items
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Make Magic Items";
                        break;
                    case NpcTypes.NpcType032A: // JOIN GUID, TALK, Make Donation
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Make Donation";
                        break;
                    case NpcTypes.NpcType032D: // JOIN GUID, TALK, Cure Disease
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Cure Disease";
                        break;
                    case NpcTypes.NpcType0352: // JOIN GUID, TALK, Repair Item
                        windowData = guildNpcWindowData;
                        emptyButtonText = "Repair Item";
                        break;
                }
            }

            // Exit if NPC reaction is underfined
            if (windowData == null)
            {
                // CloseWindow(); // TODO: THIS HANGS
                windowData = generalSmallNpcWindowData;
                return;
            }

            // Dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Set window texture from native file
            Texture2D windowTexture = ImageReader.GetTexture(windowData.nativeImageName);

            // Set window position
            const float windowTopPos = 40.0f;
            popupPanel.HorizontalAlignment = HorizontalAlignment.Center;
            popupPanel.Position = new Vector2(0, windowTopPos);
            popupPanel.Size = new Vector2(windowTexture.width, windowTexture.height);
            popupPanel.BackgroundTexture = windowTexture;
            NativePanel.Components.Add(popupPanel);

            // Set buttons
            popupPanel.Components.Remove(emptyButtonLabel);
            popupPanel.Components.Remove(empty1ButtonLabel);
            popupPanel.Components.Remove(empty2ButtonLabel);
            foreach (var buttonData in windowData.buttons)
            {
                Button button = DaggerfallUI.AddButton(buttonData.buttonImageRect, popupPanel);
                switch (buttonData.buttonType)
                {
                    case NpcWindowButtonType.Empty:
                        ShowLabel(emptyButtonLabel, buttonData.buttonImageRect.center, emptyButtonText);
                        button.OnMouseClick += EmptyButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Empty1:
                        ShowLabel(empty1ButtonLabel, buttonData.buttonImageRect.center, empty1ButtonText);
                        button.OnMouseClick += Empty1Button_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Empty2:
                        ShowLabel(empty2ButtonLabel, buttonData.buttonImageRect.center, empty2ButtonText);
                        button.OnMouseClick += Empty2Button_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Exit:
                        button.OnMouseClick += ExitButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.FoodAndDrinks:
                        button.OnMouseClick += TavernFoodAndDrinksButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Goodbye:
                        button.OnMouseClick += TavernGoodbyeButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.JoinGuid:
                        button.OnMouseClick += JoinGuildButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.OrderOfLampInfo:
                        button.OnMouseClick += LampOrderInfoButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.OrderOfLampJoin:
                        button.OnMouseClick += LampOrderJoinButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.RepairItem:
                        button.OnMouseClick += RepairItemButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Room:
                        button.OnMouseClick += TavernRoomButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Sell:
                        button.OnMouseClick += SellButton_OnMouseClick;
                        break;
                    case NpcWindowButtonType.Talk:
                        button.OnMouseClick += TalkButton_OnMouseClick;
                        break;
                }
            }
        }

        #endregion


        #region Event Handlers

        void TalkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void EmptyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void Empty1Button_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void Empty2Button_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void TavernRoomButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void TavernFoodAndDrinksButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void TavernGoodbyeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }

        void JoinGuildButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void RepairItemButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void SellButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void LampOrderInfoButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void LampOrderJoinButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            throw new NotImplementedException();
        }

        void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }

        #endregion

    }
}