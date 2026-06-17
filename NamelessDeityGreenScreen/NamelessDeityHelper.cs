using System;
using System.Collections;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    /// <summary>
    /// Helper that locates the Nameless Deity NPC type and drives its
    /// animations via reflection while our mod has its AI frozen.
    /// All reflection look-ups are cached after the first call so they
    /// have negligible per-frame overhead.
    /// </summary>
    public static class NamelessDeityHelper
    {
        // ── identity ─────────────────────────────────────────────────────────
        private const string WotGModName           = "NoxusBoss";
        private const string NamelessDeityTypeName = "NamelessDeityBoss";

        private static int? _cachedType = null;

        // ── state detection ───────────────────────────────────────────────────
        // Intro animation state values from NamelessAIType enum:
        //   Awaken               = 0
        //   OpenScreenTear       = 1
        //   IntroScreamAnimation = 2
        // Any state index <= 2 means the boss is still in the intro cutscene.
        private const int LastIntroStateIndex = 2;

        private static PropertyInfo? _currentStateProp = null;

        // ── animation reflection cache ────────────────────────────────────────
        private static bool          _animCacheBuilt          = false;
        private static MethodInfo?   _performPreUpdateResets  = null;
        private static MethodInfo?   _defaultUniversalHand    = null;
        private static MethodInfo?   _updateWings             = null;
        private static PropertyInfo? _fightTimerProp          = null;
        private static PropertyInfo? _handsProp               = null;
        private static MethodInfo?   _handUpdateMethod        = null;
        private static FieldInfo?    _renderCompositeField    = null;
        private static MethodInfo?   _renderCompositeUpdate   = null;
        private static MethodInfo?   _findRenderStep          = null;
        private static Type?         _danglingVinesStepType   = null;
        private static MethodInfo?   _handleVineRotation      = null;
        private static PropertyInfo? _handsShouldInheritProp  = null;

        // censor position fix
        private static PropertyInfo? _censorPositionProp      = null;
        private static PropertyInfo? _idealCensorPositionProp = null;

        // ─────────────────────────────────────────────────────────────────────

        public static void ResetCache()
        {
            _cachedType              = null;
            _currentStateProp        = null;
            _animCacheBuilt          = false;
            _performPreUpdateResets  = null;
            _defaultUniversalHand    = null;
            _updateWings             = null;
            _fightTimerProp          = null;
            _handsProp               = null;
            _handUpdateMethod        = null;
            _renderCompositeField    = null;
            _renderCompositeUpdate   = null;
            _findRenderStep          = null;
            _danglingVinesStepType   = null;
            _handleVineRotation      = null;
            _handsShouldInheritProp  = null;
            _censorPositionProp      = null;
            _idealCensorPositionProp = null;
        }

        // ── identity helpers ──────────────────────────────────────────────────

        public static int GetNamelessDeityType()
        {
            if (_cachedType.HasValue)
                return _cachedType.Value;

            if (ModLoader.TryGetMod(WotGModName, out Mod wotg) &&
                wotg.TryFind<ModNPC>(NamelessDeityTypeName, out ModNPC deity))
            {
                _cachedType = deity.Type;
                return _cachedType.Value;
            }

            _cachedType = -1;
            return -1;
        }

        public static bool IsNamelessDeity(NPC npc)
        {
            int t = GetNamelessDeityType();
            return t != -1 && npc.type == t;
        }

        public static bool NamelessDeityIsAlive()
        {
            int t = GetNamelessDeityType();
            return t != -1 && NPC.AnyNPCs(t);
        }

        // ── intro cutscene detection ──────────────────────────────────────────

        /// <summary>
        /// Returns true while the boss is still running through its intro
        /// cutscene (Awaken → OpenScreenTear → IntroScreamAnimation).
        /// </summary>
        public static bool IsBossInIntroCutscene(NPC npc)
        {
            if (npc.ModNPC == null)
                return false;

            try
            {
                if (_currentStateProp == null)
                    _currentStateProp = npc.ModNPC.GetType()
                        .GetProperty("CurrentState",
                            BindingFlags.Instance | BindingFlags.Public);

                if (_currentStateProp == null)
                    return false;

                int stateIndex = (int)(object)_currentStateProp.GetValue(npc.ModNPC)!;
                return stateIndex <= LastIntroStateIndex;
            }
            catch
            {
                return true;
            }
        }

        // ── per-frame animation driver ────────────────────────────────────────

        /// <summary>
        /// Drives every animation system that the boss normally updates inside
        /// its AI() call.  Must be called each frame while the boss is frozen.
        /// </summary>
        public static void TickFrozenAnimations(NPC npc)
        {
            if (npc.ModNPC == null)
                return;

            try
            {
                EnsureAnimCache(npc);

                object modNPC = npc.ModNPC;

                // 1. PerformPreUpdateResets – resets HandsShouldInheritOpacity etc.
                _performPreUpdateResets?.Invoke(modNPC, null);

                // 2. Force hands to inherit opacity.
                _handsShouldInheritProp?.SetValue(modNPC, true);

                // 3. Increment FightTimer so sin-wave hand oscillation stays live.
                int ft = 0;
                if (_fightTimerProp != null)
                {
                    ft = (int)_fightTimerProp.GetValue(modNPC)!;
                    ft++;
                    _fightTimerProp.SetValue(modNPC, ft);
                }

                // 4. Wing animation (same formula as IntroScreamAnimation).
                _updateWings?.Invoke(modNPC, new object[] { ft / 54f });

                // 5. Hand hover motion (creates hands if missing, moves them).
                _defaultUniversalHand?.Invoke(modNPC, new object[] { 950f });

                // 6. Update each hand's physics.
                if (_handsProp != null && _handUpdateMethod != null)
                {
                    var hands = _handsProp.GetValue(modNPC) as IList;
                    if (hands != null)
                        foreach (object hand in hands)
                            _handUpdateMethod.Invoke(hand, null);
                }

                // 7. Dangling vine physics.
                if (_renderCompositeField != null && _findRenderStep != null &&
                    _danglingVinesStepType != null && _handleVineRotation != null)
                {
                    object rc = _renderCompositeField.GetValue(modNPC)!;
                    MethodInfo findGeneric = _findRenderStep.MakeGenericMethod(_danglingVinesStepType);
                    object vineStep = findGeneric.Invoke(rc, null)!;
                    _handleVineRotation.Invoke(vineStep, new object[] { npc });
                }

                // 8. Tick render composite – advances texture-swap timer.
                if (_renderCompositeField != null && _renderCompositeUpdate != null)
                {
                    object rc = _renderCompositeField.GetValue(modNPC)!;
                    _renderCompositeUpdate.Invoke(rc, null);
                }

                // 9. Fix censor (black square) position.
                //    CensorPosition is not updated while AI is blocked, so it
                //    drifts from the actual eye.  Force it to IdealCensorPosition
                //    every frame.
                if (_idealCensorPositionProp != null && _censorPositionProp != null)
                {
                    object ideal = _idealCensorPositionProp.GetValue(modNPC)!;
                    _censorPositionProp.SetValue(modNPC, ideal);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<NamelessDeityGreenScreenMod>()
                    ?.Logger.Warn($"[NamelessDeityGreenScreen] TickFrozenAnimations error: {ex.Message}");
            }
        }

        // ── reflection cache builder ──────────────────────────────────────────

        private static void EnsureAnimCache(NPC npc)
        {
            if (_animCacheBuilt)
                return;

            _animCacheBuilt = true;

            Type bossType = npc.ModNPC.GetType();
            var  bFlags   = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            _performPreUpdateResets  = bossType.GetMethod("PerformPreUpdateResets",      bFlags);
            _defaultUniversalHand    = bossType.GetMethod("DefaultUniversalHandMotion",  bFlags);
            _updateWings             = bossType.GetMethod("UpdateWings",                 bFlags);
            _fightTimerProp          = bossType.GetProperty("FightTimer",                bFlags);
            _handsProp               = bossType.GetProperty("Hands",                    bFlags);
            _handsShouldInheritProp  = bossType.GetProperty("HandsShouldInheritOpacity", bFlags);

            // Censor position properties.
            _censorPositionProp      = bossType.GetProperty("CensorPosition",      bFlags);
            _idealCensorPositionProp = bossType.GetProperty("IdealCensorPosition", bFlags);

            // RenderComposite (public field on NamelessDeityBoss).
            _renderCompositeField = bossType.GetField("RenderComposite",
                BindingFlags.Instance | BindingFlags.Public);

            if (_renderCompositeField != null)
            {
                object rc     = _renderCompositeField.GetValue(npc.ModNPC)!;
                Type   rcType = rc.GetType();

                _renderCompositeUpdate = rcType.GetMethod("Update",
                    BindingFlags.Instance | BindingFlags.Public);
                _findRenderStep = rcType.GetMethod("Find",
                    BindingFlags.Instance | BindingFlags.Public);

                _danglingVinesStepType = rcType.Assembly.GetType(
                    "NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps.DanglingVinesStep");

                if (_danglingVinesStepType != null)
                    _handleVineRotation = _danglingVinesStepType.GetMethod(
                        "HandleDanglingVineRotation",
                        BindingFlags.Instance | BindingFlags.Public);
            }

            // NamelessDeityHand.Update() – resolve from the first hand.
            if (_handsProp != null)
            {
                var hands = _handsProp.GetValue(npc.ModNPC) as IList;
                if (hands != null && hands.Count > 0)
                    _handUpdateMethod = hands[0]!.GetType()
                        .GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
            }
        }
    }
}
