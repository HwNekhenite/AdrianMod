using AdrianMod.Content.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AdrianMod.Content.Items
{
    public class IpptGold : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red; // post moonlord rarity
            Item.maxStack = 999;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<AdrianBossNPC>());
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<AdrianBossNPC>());
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.GoldBar);
            recipe.AddIngredient(ItemID.CelestialSigil);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.PlatinumBar);
            recipe2.AddIngredient(ItemID.CelestialSigil);
            recipe2.AddTile(TileID.LunarCraftingStation);
            recipe2.Register();
        }
    }


}
