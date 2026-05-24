
using AdrianMod.Content.Items;
using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace AdrianMod.Content.NPCs
{
    [AutoloadBossHead]
    public class AdrianBossNPC : ModNPC
    {

        public override bool CheckActive()
        {
            // Find the current player target
            Player player = Main.player[NPC.target];

            // If the player is DEAD or GONE, return true. 
            // This allows the game to despawn the boss naturally.
            if (!player.active || player.dead)
            {
                return true;
            }

            // ONLY prevent despawn if the player is alive AND we are in the Domain/Transition
            if (CurrentState == AIState.Domain || CurrentState == AIState.Phase2Transition)
            {
                return false;
            }

            return true;
        }

        public override void SetStaticDefaults()
        {

            Main.npcFrameCount[NPC.type] = 1; // Example: 4 animation frames
            NPC.ai[1] = 0;

        }
        public override void SetDefaults()
        {
            NPC.width = 75;
            NPC.height = 150;
            NPC.damage = 100;
            NPC.defense = 20;
            NPC.lifeMax = 150000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 60f;
            NPC.knockBackResist = 0f; // Bosses should not be knocked back
            NPC.aiStyle = -1; // -1 means custom AI
            NPC.boss = true; // Marks as a boss
            NPC.lavaImmune = true;
            NPC.noGravity = true; // Usually true for flying bosses
            NPC.noTileCollide = true;
            Music = MusicID.Boss1;
        }

        public override void OnKill()
        {
            base.OnKill();
            SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianDeath"));

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

            Main.fastForwardTimeToDawn = true;
            Main.raining = false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // Hide the overhead bar during Transition and Domain Blitz
            if (CurrentState == AIState.Phase2Transition || CurrentState == AIState.Domain)
            {
                return false;
            }
            return null; // Return null to use default behavior otherwise
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 1. Guaranteed drop (1 out of 1 chance)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OWADIOOO>(), 1));

           
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (CurrentState == AIState.Phase2Transition || CurrentState == AIState.Domain)
            {
                return false;
            }
            return base.CanHitPlayer(target, ref cooldownSlot);
        }

        public override bool CheckDead()
        {
            if (NPC.ai[2] == 0)
            {
     
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.ai[2] = 1;
                NPC.immortal = true;
                NPC.active = true;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                SwitchState(AIState.Phase2Transition);
                NPC.netUpdate = true;
                return false;
            }
            return true;
        }

        public enum AIState
        {
            Follow = 0,
            SprintWindup = 1,
            TeleportWindup = 2,
            SprintDuration = 3,
            Slash = 4,
            Blitz = 5,
            Gun = 6,
            BigDash = 7,
            Phase2Transition = 8,
            Domain = 9,
            EndAttack = 10
        }

        public float Timer { get => NPC.ai[0]; set => NPC.ai[0] = value; }
        public AIState CurrentState { get => (AIState)NPC.ai[1]; set => NPC.ai[1] = (float)value; }
        public float AttackCounter { get => NPC.ai[3]; set => NPC.ai[3] = value; }

        bool hasSpawed = false;
        public override void AI()
        {

            bool isP3 = NPC.ai[2] >= 2;

            if (!hasSpawed)
            {
                SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianSpawn"));
                hasSpawed = true;
            }

            NPC.TargetClosest(true);

            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                // no valid target found, despawn
                if (Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                {
                    NPC.velocity.Y -= 0.5f; // float away
                    NPC.timeLeft = 3;

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

                if (isP3)
                {
                    Main.fastForwardTimeToDawn = true;
                    Main.raining = false;
                }
                return;
            }

            Timer++;

            switch (CurrentState)
            {
                case AIState.Follow:
                    if (Timer == 15)
                    {
                        int ran = Main.rand.Next(0, 11);

                        if (isP3)
                        {
                            if (ran > 1)
                            {
                                SwitchState(AIState.TeleportWindup);
                            }
                            else
                            {
                                SwitchState(AIState.Follow);
                            }
                        }
                        else
                        {
                            if (ran > 4)
                            {
                                SwitchState(AIState.TeleportWindup);
                            }
                            else
                            {
                                SwitchState(AIState.SprintWindup);
                            }

                        }

                    }
                    else
                    {
                        MoveTowardsPlayer(player);

                        if (Vector2.Distance(player.Center, NPC.Center) <= 100)
                        {
                            SwitchState(AIState.SprintWindup);
                        }
                    }
                    break;

                case AIState.SprintWindup:
                    NPC.velocity *= 0.85f;

                    if (Timer >= 15)
                    {
                        Sprint(player);
                        SwitchState(AIState.SprintDuration);
                    }
                    break;

                case AIState.TeleportWindup:

                    if (Timer < 30)
                    {
                        NPC.alpha = (Timer % 6 < 3) ? 255 : 0;
                    }

                    else if (Timer == 30)
                    {
                        TeleportBehindPlayer(player);
                        SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/BigBoom"));
                    }

                    else if (Timer < 60)
                    {
                        NPC.alpha = (Timer % 6 < 3) ? 255 : 0;
                    }

                    else
                    {
                        NPC.alpha = 0;
                        NPC.dontTakeDamage = false;

                        int ran = Main.rand.Next(10);

                        if (ran < 3)
                        {
                            // GUN: 20% chance (rolls 0 or 1)
                            if (isP3 && Main.rand.Next(2) == 0)
                            {
                                SwitchState(AIState.BigDash);
                            }
                            else
                            {
                                SwitchState(AIState.Gun);
                            }

                        }
                        else if (ran >= 3 && ran < 6)
                        {
                            // SLASH: 40% chance (rolls 2, 3, 4, 5)
                            // ONLY if Adrian is far away
                            if (Vector2.Distance(player.Center, NPC.Center) > 250f)
                            {
                                SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianSlash"));
                                SwitchState(AIState.Slash);
                            }
                            else
                            {
                                // If too close for slash, do a Big Dash instead
                                SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianDash"));
                                SwitchState(AIState.BigDash);
                            }
                        }
                        else
                        {
                            // BIG DASH: 40% base chance (rolls 6, 7, 8, 9)
                            SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/AdrianDash"));
                            SwitchState(AIState.BigDash);
                        }

                    }
                    break;

                case AIState.SprintDuration:
                    if (Timer == 40)
                    {
                        SwitchState(AIState.Follow);
                    }
                    break;

                case AIState.Gun:

                    MoveTowardsPlayer(player, 12);

                    if (Timer == 30)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int gunIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + 100, (int)NPC.Center.Y, ModContent.NPCType<GunNPC>());

                            // Pass Adrian's ID to the gun's ai[1] slot
                            Main.npc[gunIndex].ai[1] = NPC.whoAmI;

                            // Force sync for multiplayer
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, gunIndex);
                        }
                    }

                    if (Timer == 180)
                    {
                        SwitchState(AIState.Follow);
                    }

                    if (Vector2.Distance(player.Center, NPC.Center) <= 100)
                    {
                        SwitchState(AIState.SprintWindup);
                    }


                    break;

                case AIState.BigDash:

                    if (Timer == 10)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                        Dash(player);
                    }
                    if (Timer < 10)
                    {
                        NPC.velocity *= 0.85f;
                    }
                    if (Timer == 50)
                    {
                        SwitchState(AIState.Follow);
                    }
                    break;

                case AIState.Slash:
                    NPC.velocity *= 0.85f; // Windup (Stay still)

                    if (Timer == 30) // The moment of the strike
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 shootVel = Vector2.Normalize(player.Center - NPC.Center) * 15f;

                            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ProjectileID.DD2SquireSonicBoom, NPC.damage, 1f);

                            Main.projectile[p].scale = 2f;
                            Main.projectile[p].extraUpdates = 2;
                            Main.projectile[p].hostile = true;
                            Main.projectile[p].friendly = false;

                            Main.projectile[p].width = 142;
                            Main.projectile[p].height = 56;
                            Main.projectile[p].timeLeft = 120;
                            Main.projectile[p].damage = 100;


                        }

                        // REPOSITION: Adrian lunges slightly in the opposite direction after slashing
                        NPC.velocity = Vector2.Normalize(NPC.Center - player.Center) * 10f;
                    }

                    if (Timer >= 45)
                    {
                        SwitchState(AIState.Follow);
                    }
                    break;

                case AIState.Phase2Transition:

                    NPC.alpha = 0;

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

                    if (Timer == 1)
                    {
                        NPC.velocity = Vector2.Zero;
                        SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/Phase1Kill") with
                        {
                            Volume = 0.5f
                        });



                        Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/Domain1");
                    }

                    if (Timer == 240)
                    {
                        if (Main.dayTime)
                        {
                            Main.fastForwardTimeToDusk = true;
                        }


                        /*if (Main.netMode == NetmodeID.Server && Timer % 5 == 0)
                        {
                            NetMessage.SendData(MessageID.WorldData);
                        }*/
                        // Main.dayRate = 1; // Return time to normal speed
                        //Main.dayTime = false; // Ensure it is night
                        //Main.time = 0; // Lock it at the start of night (7:30 PM)


                        SoundEngine.PlaySound(new SoundStyle("AdrianMod/Content/Sounds/DomainExpansion"));
                        CombatText.NewText(NPC.getRect(), Color.Red, "DOMAIN EXPANSION:", true);
                    }

                    if (Timer == 240 + 180)
                    {
                        Main.fastForwardTimeToDusk = false;
                        Main.dayTime = false;
                        Main.time = 0;



                        SpawnTree(player);
                        //CombatText.NewText(NPC.getRect(), Color.Red, "NIBBLER REALM", true);
                        SwitchState(AIState.Domain);

                    }
                    break;

                case AIState.Domain:

                    if (Timer == 180)
                    {
                        Main.raining = true;
                        Main.maxRaining = 0.5f;
                        Main.cloudAlpha = 0.5f;

                        Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/DOMAINEXPANSION");
                        NPC.dontTakeDamage = true;
                        NPC.immortal = false;
                    }

                    if (NPC.alpha < 255)
                    {
                        NPC.alpha += 5;
                    }
                    break;

                case AIState.EndAttack:
                    {
                        int ran = Main.rand.Next(10);

                        if (isP3)
                        {
                            if (ran < 9)
                            {
                                SwitchState(AIState.TeleportWindup);
                            }
                            else
                            {
                                SwitchState(AIState.Follow);
                            }
                        }
                        else
                        {
                            if (ran < 5)
                            {
                                SwitchState(AIState.TeleportWindup);
                            }
                            else
                            {
                                SwitchState(AIState.Follow);
                            }

                        }

                    }
                    break;
            }

            if (isP3)
            {
                Main.raining = true;
                Main.maxRaining = 1f;
                Main.cloudAlpha = 1f;


                if (Main.rand.NextBool(300)) // Roughly every 5 seconds
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        Main.lightning = 1f; // The flash
                    }
                    SoundEngine.PlaySound(SoundID.Thunder);
                }

                Main.bloodMoon = true;

                if (Main.netMode != NetmodeID.Server)
                {
                    Filters.Scene.Activate("BloodMoon");
                }

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData);
                }
            }

        }

        private void SpawnTree(Player player)
        {
            int tileX = (int)(player.Center.X / 16);
            int tileY = (int)(player.Center.Y / 16);
            float groundY = player.Center.Y + 400; // Default if no ground found

            // Scan Down for Solid or Platforms
            for (int i = 0; i < 50; i++)
            {
                Tile tile = Framing.GetTileSafely(tileX, tileY + i);
                if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                {
                    groundY = (tileY + i) * 16;
                    break;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Pass groundY into ai[0] - the safest slot
                int tree = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X, (int)groundY, ModContent.NPCType<AdrianTreeNPC>(), 0, groundY);
                Main.npc[tree].netUpdate = true;
            }
        }

        public void SwitchState(AIState state)
        {
            CurrentState = state;
            Timer = 0;
            NPC.netUpdate = true;
        }

        void MoveTowardsPlayer(Player player, float speed = 13)
        {

            float x = player.Center.X;
            float y = player.Center.Y;

            Vector2 Movedir = new Vector2(x, y) - new Vector2(NPC.Center.X, NPC.Center.Y);

            NPC.velocity = Vector2.Normalize(Movedir) * speed;
        }

        void TeleportBehindPlayer(Player player)
        {
            float x = player.Center.X;
            float y = player.Center.Y;

            float distance = 350;

            float newX = (x - distance * player.direction);
            float newY = Main.rand.Next((int)(y - distance), (int)(y + distance));

            NPC.Center = new Vector2(newX, newY);

        }

        void Sprint(Player player)
        {

            float x = player.Center.X;
            float y = player.Center.Y;

            Vector2 Dashdir = new Vector2(x, y) - new Vector2(NPC.Center.X, NPC.Center.Y);

            NPC.velocity = Vector2.Normalize(Dashdir) * 17;

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
        }

        void Dash(Player player)
        {

            float x = player.Center.X;
            float y = player.Center.Y;

            Vector2 Dashdir = new Vector2(x, y) - new Vector2(NPC.Center.X, NPC.Center.Y);

            NPC.velocity = Vector2.Normalize(Dashdir) * 30;


        }

        void BlitzDash(Player player)
        {

            float x = player.Center.X;
            float y = player.Center.Y;

            Vector2 Dashdir = new Vector2(x, y) - new Vector2(NPC.Center.X, NPC.Center.Y);

            NPC.velocity = Vector2.Normalize(Dashdir) * 35;


        }
    }

}