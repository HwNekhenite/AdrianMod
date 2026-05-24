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
    public class GunNPC : ModNPC
    {

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2; // Example: 4 animation frames
            NPC.ai[1] = 0;
            
        }
        public override void SetDefaults()
        {
            NPC.width = 367;
            NPC.height = 137;
            NPC.scale = 0.6f;
            NPC.damage = 100;
            NPC.defense = 20;
            NPC.lifeMax = 10000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 60f;
            NPC.knockBackResist = 0f; // Bosses should not be knocked back
            NPC.aiStyle = -1; // -1 means custom AI
            NPC.boss = false; // Marks as a boss
            NPC.lavaImmune = true;
            NPC.noGravity = true; // Usually true for flying bosses
            NPC.noTileCollide = true;
        }

        public override void FindFrame(int frameHeight)
        {
            // Match the AI: Firing starts at 30 and happens every 5 ticks.
            // We show the flash frame (Frame 1) for 2 ticks (ticks 0 and 1 of the cycle).
            if (Timer >= 30 && Timer % 5 < 2)
            {
                NPC.frame.Y = frameHeight; // Frame 1 (Muzzle Flash)
            }
            else
            {
                NPC.frame.Y = 0; // Frame 0 (Idle Gun)
            }
        }

        public float Timer { get => NPC.ai[0]; set => NPC.ai[0] = value; }

        public bool fireSprite = false;

        void MoveTowardsPlayer(Player player, float speed = 12)
        {

            float x = player.Center.X;
            float y = player.Center.Y;

            Vector2 Movedir = new Vector2(x, y) - new Vector2(NPC.Center.X, NPC.Center.Y);

            NPC.velocity = Vector2.Normalize(Movedir) * 8;
        }

        public override void AI()
        {

            NPC.TargetClosest();
            Player player = Main.player[NPC.target];
            Vector2 look = player.Center - NPC.Center;

            NPC.rotation = look.ToRotation();

            if (player.Center.X > NPC.Center.X)
            {
                NPC.direction = -1;
                NPC.spriteDirection = -1;
            }
            else
            {
                NPC.direction = 1;
                NPC.spriteDirection = 1;
                NPC.rotation += MathHelper.Pi;
            }

            Timer++;

            int parentID = (int)NPC.ai[1];
            bool isDetatched = NPC.ai[2] == 1;

            if (!isDetatched)
            {
                NPC.dontTakeDamage = true;

                NPC parent = Main.npc[parentID];

                if (parent.active && parent.type == ModContent.NPCType<AdrianBossNPC>())
                {
                    Vector2 baseOffset = new Vector2(200, -20);
                    NPC.Center = parent.Center + baseOffset;

                }

                if (Timer == 180)
                {
                    NPC.ai[2] = 1;
                }
            }

            if (isDetatched)
            {
                NPC.dontTakeDamage = false;

                // --- NEW DISTANCE-KEEPING MOVEMENT ---
                float distanceToPlayer = Vector2.Distance(NPC.Center, player.Center);
                Vector2 directionAway = NPC.Center - player.Center; // Vector pointing AWAY from player
                Vector2 directionTo = player.Center - NPC.Center;     // Vector pointing TO player

                NPC.ai[3]++;
                if (NPC.ai[3] >= 120)
                {
                    // Pick a random angle and dash that way
                    Vector2 dashAngle = Main.rand.NextVector2Circular(20f, 20f);
                    NPC.velocity = dashAngle;
                    NPC.ai[3] = 0; // Reset dash cooldown
                }

                // 2. POSITIONING PHYSICS
                if (distanceToPlayer < 350f)
                {
                    // TOO CLOSE: Fly away aggressively
                    NPC.velocity = Vector2.Normalize(directionAway) * 12;
                }
                else if (distanceToPlayer > 600f)
                {
                    // TOO FAR: Catch up to the player
                    NPC.velocity = Vector2.Normalize(directionTo) * 12;
                }
                else
                {
                    // SWEET SPOT: Apply high friction so it "hovers" in place
                    NPC.velocity *= 0.94f;

                    // Subtle circling movement (makes it harder to hit)
                    Vector2 orbit = directionTo.RotatedBy(MathHelper.PiOver2); // Perpendicular to player
                    NPC.velocity += Vector2.Normalize(orbit) * 0.5f;
                }
            }

            



            if (Timer >= 30)
            {
                if (Timer % 5 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item11, NPC.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 muzzleOffset = new Vector2(168.5f, -5.5f * NPC.direction) * NPC.scale;
                        Vector2 rotatedOffset = muzzleOffset.RotatedBy(look.ToRotation());

                        Vector2 firePos = NPC.Center + rotatedOffset;

                        Vector2 shootVel = Vector2.Normalize(look) * 8f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), firePos, new Vector2(shootVel.X, Main.rand.NextFloat(shootVel.Y -2f, shootVel.Y + 2f)), ProjectileID.DeathLaser, 30, 1f);
                    }
                }
                
            }

            if (Timer == 180)
            {
                Timer = 0;
            }
        }
    }


}
