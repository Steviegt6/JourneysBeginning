﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace JourneysBeginning.Content.Globals.GlobalNPCs
{
    public class AnglerShopGlobalNPC : GlobalNPC
    {
        public static bool HasSeenWarnMessage = false;

        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Angler)
                SetupAnglerShop(shop, ref nextSlot);
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton)
        {
            if (npc.type == NPCID.Angler && !firstButton)
            {
                if (!HasSeenWarnMessage)
                {
                    Main.NewText("Please note that the Angler's shop extends past the normal NPC shop limit. Consider getting a mod that extends shops if you have not already!", Color.Red);
                    HasSeenWarnMessage = true;
                }

                Main.playerInventory = true;
                Main.npcChatText = "";
                Main.npcShop = Main.MaxShopIDs - 1;
                Main.instance.shop[Main.npcShop].SetupShop(NPCID.Angler);
                Main.PlaySound(SoundID.MenuTick);
            }
        }

        private void SetupAnglerShop(Chest shop, ref int nextSlot)
        {
            AnglerShopData.AddQuestItem(new AnglerShopData(0, false,
                ItemID.FuzzyCarrot,
                ItemID.AnglerHat,
                ItemID.AnglerVest,
                ItemID.AnglerPants,
                ItemID.GoldenBugNet,
                ItemID.FishHook,
                /*minecarp,*/
                ItemID.HighTestFishingLine,
                ItemID.AnglerEarring,
                ItemID.TackleBox,
                ItemID.FishermansGuide,
                ItemID.WeatherRadio,
                ItemID.Sextant,
                ItemID.SeashellHairpin,
                ItemID.MermaidAdornment,
                ItemID.MermaidTail,
                ItemID.FishCostumeMask,
                ItemID.FishCostumeShirt,
                ItemID.FishCostumeFinskirt,
                ItemID.LifePreserver,
                ItemID.ShipsWheel,
                ItemID.CompassRose,
                ItemID.WallAnchor,
                ItemID.PillaginMePixels,
                ItemID.TreasureMap,
                ItemID.GoldfishTrophy,
                ItemID.BunnyfishTrophy,
                ItemID.SwordfishTrophy,
                ItemID.SharkteethTrophy,
                ItemID.ShipInABottle,
                ItemID.SeaweedPlanter,
                ItemID.CoralstoneBlock,
                ItemID.FishingPotion,
                ItemID.SonarPotion,
                ItemID.CratePotion,
                ItemID.MasterBait, // lol! :joy: :rogl;LF :rofl;
                ItemID.JourneymanBait,
                ItemID.ApprenticeBait), shop, ref nextSlot);

            AnglerShopData.AddQuestItem(new AnglerShopData(10, true,
                ItemID.FinWings,
                ItemID.BottomlessBucket,
                ItemID.SuperAbsorbantSponge), shop, ref nextSlot);

            AnglerShopData.AddQuestItem(new AnglerShopData(ItemID.HotlineFishingHook, reqQuests: 25, hardmode: true), shop, ref nextSlot);
            AnglerShopData.AddQuestItem(new AnglerShopData(ItemID.GoldenFishingRod, reqQuests: 30), shop, ref nextSlot);
        }
    }
}