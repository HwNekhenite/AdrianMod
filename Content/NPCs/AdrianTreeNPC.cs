
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace AdrianMod.Content.NPCs
{

    [AutoloadBossHead]
    public class AdrianTreeNPC : ModNPC
    {

        public override void SetStaticDefaults()
        {

        }

        public float GroundY => NPC.ai[0];
        public float Timer { get => NPC.ai[1]; set => NPC.ai[1] = value; }
        public bool Sprouted = false;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (!Sprouted)
            {
                return false;
            }

            return base.CanHitPlayer(target, ref cooldownSlot);
        }

        public override void SetDefaults()
        {
            NPC.width = 117;
            NPC.height = 435;
            NPC.scale = 1f;
            NPC.damage = 100;
            NPC.defense = 20;
            NPC.lifeMax = 125000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 60f;
            NPC.knockBackResist = 0f; // Bosses should not be knocked back
            NPC.aiStyle = -1; // -1 means custom AI
            NPC.boss = true; // Marks as a boss
            NPC.lavaImmune = true;
            NPC.noGravity = true; // Usually true for flying bosses
            NPC.noTileCollide = true;
        }

        public override void OnKill()
        {

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC adrian = Main.npc[i];
                if (adrian.active && adrian.type == ModContent.NPCType<AdrianBossNPC>())
                {
                    adrian.lifeMax = 300000;
                    adrian.life = adrian.lifeMax;
                    adrian.ai[2] = 2;               // Set Phase to 2 (Phase 3 in logic)
                    adrian.alpha = 0;               // Make visible
                    adrian.dontTakeDamage = false;  // Make killable
                    adrian.immortal = false;        // Remove 1HP protection
                    adrian.chaseable = true;        // Let Zenith target him
                    adrian.boss = true;
                    adrian.noGravity = true;
                    adrian.noTileCollide = true; // Bring back health bar

                    // Re-center him where the tree was
                    adrian.Center = NPC.Center;

                    // Play massive sound
                    SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/Phase3") with { Volume = 2f });

                    // Tell his AI to start Phase 3
                    var adrianMod = adrian.ModNPC as AdrianBossNPC;
                    adrianMod.SwitchState(AdrianBossNPC.AIState.TeleportWindup);

                    adrian.netUpdate = true;

                    break;
                }
            }
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                // no valid target found, despawn
                if (Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                {
                    NPC.velocity.Y += 0.5f; // float away
                    NPC.timeLeft = 3;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<AdrianBossNPC>())
                        {
                            Main.npc[i].active = false; 
                        }
                    }


                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<GunNPC>())
                        {
                            Main.npc[i].active = false; // Removes the gun instantly
                                                        // Optional: Add a little puff of smoke where the gun was
                            for (int d = 0; d < 10; d++)
                                Dust.NewDust(Main.npc[i].position, Main.npc[i].width, Main.npc[i].height, DustID.Smoke);
                        }
                    }
                }

                Main.fastForwardTimeToDawn = true;
                Main.raining = false;
                return;
            }

            // 1. INITIAL POSITIONING
            if (NPC.localAI[0] == 0)
            {
                // Start with the TIP at groundY (700 is sprite height)
                NPC.Bottom = new Vector2(NPC.Center.X, GroundY + 700);
                NPC.localAI[0] = 1;
            }

            // 2. THE SPROUT
            if (!Sprouted)
            {
                NPC.velocity.Y = -5f; // Rise up
                if (NPC.alpha > 0) NPC.alpha -= 1;

                // Stop 88 pixels deep
                if (NPC.Bottom.Y <= GroundY + 88)
                {
                    NPC.Bottom = new Vector2(NPC.Bottom.X, GroundY + 88);
                    NPC.velocity = Vector2.Zero;
                    Sprouted = true;
                    NPC.immortal = false;
                    CombatText.NewText(NPC.getRect(), Color.Red, "NIBBLER REALM", true);
                    SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/BigBoom") with { Volume = 2f });
                }
                return;
            }
            else
            {
                Timer++;

                if (Timer % 270 == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/BigBoom") with { Pitch = 0.8f, Volume = 1.5f });

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Define the spawn distance (400 pixels to the left and right)
                        float offsetDistance = 200f;

                        // Spawn Left Gun
                        int gunL = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - (int)offsetDistance, (int)NPC.Center.Y, ModContent.NPCType<GunNPC>(), 0, NPC.whoAmI);

                        // Spawn Right Gun
                        int gunR = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + (int)offsetDistance, (int)NPC.Center.Y, ModContent.NPCType<GunNPC>(), 0, NPC.whoAmI);

                        // Sync both
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, gunL);
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, gunR);
                        }
                    }

                }

                int barrageCycle = (int)Timer % 480; // 8 second total cycle

                // Start the barrage at frame 120 of the cycle
                // We fire when (barrageCycle - 120) is 0, 30, 60, 90, 120 (5 shots)
                if (barrageCycle >= 120 && barrageCycle <= 240)
                {
                    int timeInBarrage = barrageCycle - 120;

                    if (timeInBarrage % 30 == 0)
                    {
                        // 1. SOUND: Play the AdrianSlash sound from the tree
                        SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.2f, Volume = 1f }, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {

                            // Aiming logic (from Tree Center to Player)
                            Vector2 shootVel = Vector2.Normalize(player.Center - NPC.Center) * 18f;

                            // Spawn the Sonic Boom
                            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ProjectileID.DD2SquireSonicBoom, 50, 1f);

                            if (p < 1000)
                            {
                                Main.projectile[p].scale = 2f; // Even bigger because it's a tree firing it
                                Main.projectile[p].extraUpdates = 2; // High speed
                                Main.projectile[p].hostile = true;
                                Main.projectile[p].friendly = false;
                                Main.projectile[p].tileCollide = false;

                                // Adjust Hitbox for the 4f scale
                                Main.projectile[p].width = 142;
                                Main.projectile[p].height = 56;
                                Main.projectile[p].timeLeft = 180; // Lasts longer to clear the screen

                                Main.projectile[p].netUpdate = true;

                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, p);
                                }
                            }

       
                        }
                    }
                }
            }
        }
    }
}
