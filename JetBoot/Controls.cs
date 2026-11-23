using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

namespace JetBoot
{
    // Shows and remaps the Rewired bindings of the first player (keyboard + gamepad).
    public class Controls
    {
        int playerIndex;
        int deviceTab; // 0 = clavier, 1 = manette
        bool rebinding;
        string status = "";
        float statusUntil;
        bool hooked;

        public bool Rebinding => rebinding;

        Player? GetPlayer()
        {
            if (!ReInput.isReady) return null;
            var players = ReInput.players.GetPlayers(false);
            if (players == null || players.Count == 0) return null;
            playerIndex = Mathf.Clamp(playerIndex, 0, players.Count - 1);
            return players[playerIndex];
        }

        void Hook()
        {
            if (hooked || !ReInput.isReady) return;
            hooked = true;
            var m = InputMapper.Default;
            m.options.timeout = 6f;
            m.options.ignoreMouseXAxis = true;
            m.options.ignoreMouseYAxis = true;
            m.options.allowKeyboardKeysWithModifiers = false;
            m.options.checkForConflicts = true;
            m.options.defaultActionWhenConflictFound = InputMapper.ConflictResponse.Replace;
            m.InputMappedEvent += d => { rebinding = false; Save(); Flash(L.T("Assigné : ") + d.actionElementMap.elementIdentifierName); };
            m.StoppedEvent += d => { if (rebinding) Flash(L.T("Annulé")); rebinding = false; };
        }

        void Flash(string s) { status = s; statusUntil = Time.unscaledTime + 3f; }

        static void Save()
        {
            try { ReInput.userDataStore?.Save(); }
            catch (Exception e) { Debug.LogWarning("[JetBoot] rewired save failed: " + e.Message); }
        }

        public void CancelRebind()
        {
            if (rebinding && ReInput.isReady) InputMapper.Default.Stop();
            rebinding = false;
        }

        public void Update() { }

        // "Back / Select" on any gamepad, read through Rewired's gamepad template.
        public static bool GamepadBackPressed()
        {
            if (!ReInput.isReady) return false;
            foreach (var j in ReInput.controllers.Joysticks)
            {
                var g = j.GetTemplate<IGamepadTemplate>();
                if (g != null && (g.actionTopRow2.justPressed || g.center1.justPressed)) return true; // Y (Xbox) / Triangle, or Back
            }
            return false;
        }

        // What each Takeover 2 action does in Jet Fury (read from the jet's "input" FSM and the menus).
        static readonly Dictionary<string, string> JetNames = new Dictionary<string, string>
        {
            { "steer+", "Virer à droite" }, { "steer-", "Virer à gauche" }, { "steer", "Virer (gauche / droite)" },
            { "steer_y+", "Manche vers le haut" }, { "steer_y-", "Manche vers le bas" }, { "steer_y", "Manche (haut / bas)" },
            { "handgun", "Accélérer (postcombustion)" }, { "run", "Ralentir" },
            { "punch", "Mitrailleuse (Vulcan)" }, { "jump", "Missiles" }, { "kick", "Leurres (Flares)" },
            { "special", "Esquive (Evade)" },
            { "start", "Start / Pause" }, { "menu next", "Menu : valider" }, { "menu back", "Menu : retour" },
            { "up", "Menu : haut" }, { "down", "Menu : bas" }, { "left", "Menu : gauche" }, { "right", "Menu : droite" },
        };

        static string ActionLabel(InputAction a, ActionElementMap aem)
        {
            string key = a.name;
            if (a.type == InputActionType.Axis && (aem.elementType == ControllerElementType.Button || aem.axisRange != AxisRange.Full))
                key += aem.axisContribution == Pole.Positive ? "+" : "-";
            if (JetNames.TryGetValue(key, out var jet)) return L.T(jet);
            return L.T("Takeover 2 (inutilisé) : ") + a.name;
        }

        // Jet Fury actions first, in the order of the table above; the unused Takeover 2 ones last.
        static int Order(InputAction a, ActionElementMap aem)
        {
            int i = 0;
            foreach (var k in JetNames.Keys)
            {
                if (k == a.name || k == a.name + "+" || k == a.name + "-") return i;
                i++;
            }
            return 1000 + a.id;
        }

        public void Draw(float s, GUIStyle header, GUIStyle label, GUIStyle small, GUIStyle button, GUIStyle on)
        {
            var player = GetPlayer();
            if (player == null) { GUILayout.Label(L.T("Système de contrôles (Rewired) non prêt."), label); return; }
            Hook();

            var players = ReInput.players.GetPlayers(false);
            if (players.Count > 1)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < players.Count; i++)
                    if (GUILayout.Button(string.IsNullOrEmpty(players[i].descriptiveName) ? players[i].name : players[i].descriptiveName,
                        i == playerIndex ? on : button, GUILayout.Height(38 * s))) { CancelRebind(); playerIndex = i; }
                GUILayout.EndHorizontal();
                GUILayout.Space(6 * s);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L.T("Clavier"), deviceTab == 0 ? on : button, GUILayout.Height(40 * s))) { CancelRebind(); deviceTab = 0; }
            if (GUILayout.Button(L.T("Manette"), deviceTab == 1 ? on : button, GUILayout.Height(40 * s))) { CancelRebind(); deviceTab = 1; }
            GUILayout.EndHorizontal();
            GUILayout.Space(8 * s);

            ControllerType type = deviceTab == 0 ? ControllerType.Keyboard : ControllerType.Joystick;
            int controllerId = 0;
            if (type == ControllerType.Joystick)
            {
                var joy = player.controllers.Joysticks.FirstOrDefault(x => x.GetTemplate<IGamepadTemplate>() != null) ?? player.controllers.Joysticks.FirstOrDefault();
                if (joy == null) { GUILayout.Label(L.T("Aucune manette détectée."), label); DrawFooter(s, small, button, player); return; }
                controllerId = joy.id;
                GUILayout.Label(joy.name, small);
            }

            if (rebinding)
                GUILayout.Label(L.T("Appuyez sur la touche / le bouton à assigner… (6 s)"), header);
            else if (Time.unscaledTime < statusUntil)
                GUILayout.Label(status, header);

            var maps = player.controllers.maps.GetMaps(type, controllerId);
            if (maps == null || maps.Count == 0) { GUILayout.Label(L.T("Aucune configuration pour ce périphérique."), label); DrawFooter(s, small, button, player); return; }

            foreach (var map in maps)
            {
                if (!map.enabled) continue;
                var rows = map.AllMaps.ToList();
                if (rows.Count == 0) continue;
                var cat = ReInput.mapping.GetMapCategory(map.categoryId);
                GUILayout.Label(cat != null ? (string.IsNullOrEmpty(cat.descriptiveName) ? cat.name : cat.descriptiveName) : L.T("Carte ") + map.id, header);
                foreach (var aem in rows.OrderBy(a => Order(ReInput.mapping.GetAction(a.actionId), a)).ThenBy(a => a.axisContribution))
                {
                    var action = ReInput.mapping.GetAction(aem.actionId);
                    if (action == null) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(ActionLabel(action, aem), label, GUILayout.Height(36 * s), GUILayout.Width(380 * s));
                    GUI.enabled = !rebinding;
                    if (GUILayout.Button(aem.elementIdentifierName, button, GUILayout.Height(36 * s), GUILayout.MinWidth(220 * s)))
                        StartRebind(map, aem, action);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10 * s);
            }
            DrawFooter(s, small, button, player);
        }

        void DrawFooter(float s, GUIStyle small, GUIStyle button, Player player)
        {
            GUILayout.Space(8 * s);
            if (GUILayout.Button(L.T("Rétablir les contrôles par défaut"), button, GUILayout.Height(40 * s)))
            {
                CancelRebind();
                player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
                player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
                Save();
                Flash(L.T("Contrôles par défaut rétablis"));
            }
            GUILayout.Label(L.T("Cliquez sur une touche pour la changer. Les contrôles sont partagés avec Takeover 2."), small);
        }

        void StartRebind(ControllerMap map, ActionElementMap aem, InputAction action)
        {
            var range = AxisRange.Full;
            if (action.type == InputActionType.Axis && aem.elementType == ControllerElementType.Button)
                range = aem.axisContribution == Pole.Positive ? AxisRange.Positive : AxisRange.Negative;
            else if (action.type == InputActionType.Axis && aem.axisRange != AxisRange.Full)
                range = aem.axisContribution == Pole.Positive ? AxisRange.Positive : AxisRange.Negative;
            rebinding = InputMapper.Default.Start(new InputMapper.Context
            {
                actionId = aem.actionId,
                controllerMap = map,
                actionRange = range,
                actionElementMapToReplace = aem,
            });
            if (!rebinding) Flash(L.T("Impossible de démarrer l'assignation"));
        }
    }
}
