using System;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace NoAutoPlay
{
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.RushSecond.NoAutoPlay", "No Auto Play", "1.0.0")]
    public class NoAutoPlay : BaseUnityPlugin
    {
        private static ConfigFile CustomConfigFile { get; set; }

        public static ConfigEntry<float> FireworkProcCoefficient { get; set; }
        public static ConfigEntry<float> DaggerProcCoefficient { get; set; }
        public static ConfigEntry<float> DisposableMissileProcCoefficient { get; set; }
        public static ConfigEntry<float> RoyalCapacitorProcCoefficient { get; set; }

        public void Awake()
        {
            CustomConfigFile = new ConfigFile(Paths.ConfigPath + "\\com.RushSecond.NoAutoPlay.cfg", true);
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            SetAllProcs();



        }

        private static void SetAllProcs()
        {
            FireworkProcCoefficient = CustomConfigFile.Bind<float>(
                    "Proc Coefficients",
                    "Bundle of Fireworks",
                    0.05f,
                    "Proc coefficient of Bundle of Fireworks common item"
            );

            DaggerProcCoefficient = CustomConfigFile.Bind<float>(
                    "Proc Coefficients",
                    "Sacrificial Dagger",
                    0.3f,
                    "Proc coefficient of Sacrificial Dagger legendary item"
            );

            DisposableMissileProcCoefficient = CustomConfigFile.Bind<float>(
                    "Proc Coefficients",
                    "Disposable Missile Launcher",
                    0.2f,
                    "Proc coefficient of Disposable Missile Launcher equipment"
            );

            RoyalCapacitorProcCoefficient = CustomConfigFile.Bind<float>(
                    "Proc Coefficients",
                    "Royal Capacitor",
                    0.4f,
                    "Proc coefficient of Royal Capacitor equipment"
            );

            SetFireworkProc(FireworkProcCoefficient.Value);
            SetDaggerProc(DaggerProcCoefficient.Value);
            SetMissileProc(DisposableMissileProcCoefficient.Value);
            SetCapacitorProc(RoyalCapacitorProcCoefficient.Value);
        }

        [ConCommand(commandName = "reload_NoAutoPlay", flags = ConVarFlags.None, helpText = "Reload the config file of the mod.")]
        private static void reload_NoAutoPlay(ConCommandArgs args)
        {
            CustomConfigFile.Reload();
            SetAllProcs();
        }

        [ConCommand(commandName = "FireworkProc", flags = ConVarFlags.ExecuteOnServer, helpText = "Set's the proc coeff of fireworks")]
        private static void FireworkProc(ConCommandArgs args)
        {
            float? fireworkProcCoeff = args.TryGetArgFloat(0);
            if (fireworkProcCoeff.HasValue)
            {
                SetFireworkProc((float)fireworkProcCoeff);
            }
            else
            {
                Debug.LogError("Couldn't parse the firework proc coefficient");
            }
        }

        private static void SetFireworkProc(float coeff)
        {
            try
            {
                Resources.Load<GameObject>("Prefabs/Projectiles/FireworkProjectile").GetComponent<ProjectileController>().procCoefficient = coeff;
                Debug.Log("Set firework proc coefficient to " + coeff);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static void SetDaggerProc(float coeff)
        {
            try
            {
                Resources.Load<GameObject>("Prefabs/Projectiles/DaggerProjectile").GetComponent<ProjectileController>().procCoefficient = coeff;
                Debug.Log("Set dagger proc coefficient to " + coeff);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static void SetMissileProc(float coeff)
        {
            try
            {
                Resources.Load<GameObject>("Prefabs/Projectiles/MissileProjectile").GetComponent<ProjectileController>().procCoefficient = coeff;
                Debug.Log("Set disposable missile proc coefficient to " + coeff);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static void SetCapacitorProc(float coeff)
        {
            try
            {
                IL.RoR2.Orbs.LightningStrikeOrb.OnArrival += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchInitobj<ProcChainMask>(),
                        x => x.MatchDup()
                        );
                    c.Index += 2;
                    c.Next.Operand = coeff;
                };
                Debug.Log("Set royal capacitor proc coefficient to " + coeff);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}