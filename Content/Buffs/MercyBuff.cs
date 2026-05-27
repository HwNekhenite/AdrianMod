using Terraria;
using Terraria.ModLoader;

namespace AdrianMod.Content.Buffs
{
    public class MercyBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true; // Stays as long as Adrian is alive
            Main.debuff[Type] = false; // It's a "gift" from Adrian
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // The logic is handled by the Boss NPC checking for this buff
        
    
        }
    }
}