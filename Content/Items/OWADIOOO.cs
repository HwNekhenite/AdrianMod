using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace AdrianMod.Content.Items
{
    public class OWADIOOO : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;          // Hitbox width
            Item.height = 32;         // Hitbox height
            Item.useTime = 20;        // how fast you can click it
            Item.useAnimation = 20;   
            Item.useStyle = ItemUseStyleID.HoldUp; // The player holds it over their head
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;    // Set to false if you want one boom per click
        }

        public override bool? UseItem(Player player)
        {
            // This ensures the sound only plays on the player's computer (prevents doubling in MP)
            if (player.whoAmI == Main.myPlayer)
            {
                // Play your custom BigBoom sound
                // I added a random pitch variance so it sounds slightly different every time
                SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianSpawn") with
                {
                    PitchVariance = 0.4f,
                    
                    Volume = 1.2f
                });

                // OPTIONAL: Add a screen shake to make the sound feel "heavy"
                //Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(player.Center, Main.rand.NextVector2Circular(5, 5), 5f, 3f, 10, 1000f, "MemeItem"));
            }

            CombatText.NewText(player.getRect(), Color.Red, "Happy ORD Adrian!", true);

            return true;
        }

    }
}