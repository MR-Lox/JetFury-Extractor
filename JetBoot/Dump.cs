using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace JetBoot
{
    // Temporary diagnostic: dumps the race scene hierarchy and render pipeline setup.
    public class SceneDumper : MonoBehaviour
    {
        public static void Run()
        {
            if (!File.Exists(System.IO.Path.Combine(Application.dataPath, "..", "dump_enabled.txt"))) return;
            var go = new GameObject("JetBootDumper");
            DontDestroyOnLoad(go);
            go.AddComponent<SceneDumper>();
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(8f);
            var sb = new StringBuilder();
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            sb.AppendLine("=== GLOBAL KEYWORDS ===");
            foreach (var k in Shader.enabledGlobalKeywords) sb.AppendLine(" " + k.name);
            sb.AppendLine("=== SHADERS IN USE (renderers) ===");
            var shaderProps = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var r in Resources.FindObjectsOfTypeAll<Renderer>())
            {
                if (!r.gameObject.scene.IsValid()) continue;
                foreach (var m in r.sharedMaterials)
                {
                    if (!m || !m.shader || shaderProps.ContainsKey(m.shader.name)) continue;
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var t in new[] { MaterialPropertyType.Float, MaterialPropertyType.Texture, MaterialPropertyType.Vector, MaterialPropertyType.Int })
                        foreach (var n in m.GetPropertyNames(t)) names.Add(n);
                    shaderProps[m.shader.name] = string.Join(" ", names) + " | kw: " + string.Join(" ", m.shaderKeywords);
                }
            }
            foreach (var kv in shaderProps) sb.AppendLine(" " + kv.Key + " :: " + kv.Value);
            sb.AppendLine("=== VOLUME PROFILES ===");
            foreach (var v in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (v.GetType().Name != "BeautifySettings" && v.GetType().Name != "Volume") continue;
                sb.AppendLine(" " + Path(v.transform) + " " + v.GetType().Name + " enabled=" + v.enabled);
                if (v.GetType().Name == "Volume")
                {
                    var prof = v.GetType().GetField("sharedProfile")?.GetValue(v) ?? v.GetType().GetProperty("sharedProfile")?.GetValue(v);
                    var comps = prof?.GetType().GetField("components")?.GetValue(prof) as IList;
                    if (comps != null)
                        foreach (var c in comps)
                        {
                            sb.AppendLine("   comp " + c.GetType().FullName + " active=" + c.GetType().GetField("active")?.GetValue(c));
                            foreach (var fi in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                            {
                                var val = fi.GetValue(c);
                                var ov = val?.GetType().GetProperty("overrideState")?.GetValue(val);
                                if (ov is bool ob && ob) sb.AppendLine("     " + fi.Name + " = " + val?.GetType().GetProperty("value")?.GetValue(val));
                            }
                        }
                }
            }

            sb.AppendLine("=== RENDER PIPELINE ===");
            var rp = GraphicsSettings.currentRenderPipeline;
            sb.AppendLine("asset: " + (rp ? rp.name + " " + rp.GetType().FullName : "null"));
            if (rp)
            {
                var list = rp.GetType().GetField("m_RendererDataList", F)?.GetValue(rp) as Array;
                if (list != null)
                    foreach (var rd in list)
                    {
                        if (rd == null) continue;
                        sb.AppendLine(" rendererData: " + ((UnityEngine.Object)rd).name + " " + rd.GetType().FullName);
                        var feats = rd.GetType().GetProperty("rendererFeatures", F)?.GetValue(rd) as IList;
                        if (feats != null)
                            foreach (var f in feats)
                            {
                                if (f == null) continue;
                                var o = (UnityEngine.Object)f;
                                var active = f.GetType().GetProperty("isActive", F)?.GetValue(f);
                                sb.AppendLine("   feature: " + o.name + " | " + f.GetType().FullName + " | active=" + active);
                                DumpFields(f, sb, "     ", 0);
                            }
                    }
            }

            sb.AppendLine("=== CAMERAS ===");
            foreach (var c in Camera.allCameras)
                sb.AppendLine($" {Path(c.transform)} depth={c.depth} target={(c.targetTexture ? c.targetTexture.name + " " + c.targetTexture.width + "x" + c.targetTexture.height : "screen")} rect={c.rect}");

            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                sb.AppendLine("=== SCENE " + scene.name + " ===");
                foreach (var root in scene.GetRootGameObjects()) Walk(root.transform, 0, sb);
            }
            sb.AppendLine("=== DONTDESTROYONLOAD ===");
            foreach (var root in gameObject.scene.GetRootGameObjects()) Walk(root.transform, 0, sb);

            sb.AppendLine("=== FSMS (all) ===");
            foreach (var fsmComp in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (!fsmComp || fsmComp.GetType().Name != "PlayMakerFSM" || !fsmComp.gameObject.scene.IsValid()) continue;
                var on = fsmComp.gameObject.name;
                var fsm = fsmComp.GetType().GetProperty("Fsm")?.GetValue(fsmComp);
                if (fsm == null) continue;
                sb.AppendLine("FSM " + on + " :: " + fsmComp.GetType().GetProperty("FsmName")?.GetValue(fsmComp));
                if (!(fsm.GetType().GetProperty("States")?.GetValue(fsm) is Array states)) continue;
                foreach (var st in states)
                {
                    sb.AppendLine("  STATE " + P(st, "Name"));
                    if (!(st.GetType().GetProperty("Actions")?.GetValue(st) is Array actions)) continue;
                    foreach (var act in actions)
                    {
                        if (act == null) continue;
                        var line = new StringBuilder("    " + act.GetType().Name + " {");
                        foreach (var fi in act.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                        {
                            var val = fi.GetValue(act);
                            string txt;
                            if (val == null) txt = "null";
                            else if (val.GetType().Name == "FsmOwnerDefault")
                            {
                                var go = P(val, "GameObject");
                                var gov = go != null ? P(go, "Value") as GameObject : null;
                                txt = P(val, "OwnerOption") + ":" + (gov ? gov!.name : "");
                            }
                            else if (val.GetType().Namespace == "HutongGames.PlayMaker")
                            {
                                var nm = P(val, "Name") as string;
                                var v = P(val, "RawValue") ?? P(val, "Value");
                                txt = (string.IsNullOrEmpty(nm) ? "" : "$" + nm + "=") + v;
                            }
                            else txt = val.ToString();
                            line.Append(" " + fi.Name + "=" + txt + ";");
                        }
                        sb.AppendLine(line.Append(" }").ToString());
                    }
                }
            }

            sb.AppendLine("=== REWIRED ===");
            try { DumpRewired(sb); } catch (Exception e) { sb.AppendLine("err " + e); }

            File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "..", "scene_dump.txt"), sb.ToString());
            Debug.Log("[JetBoot] scene dumped");

            sb.Clear();
            sb.AppendLine("=== UI TEXTS ===");
            foreach (var c in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (!c || !c.gameObject.scene.IsValid()) continue;
                var tp = c.GetType().GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
                if (tp == null || tp.PropertyType != typeof(string)) continue;
                string? txt; try { txt = tp.GetValue(c) as string; } catch { continue; }
                if (!string.IsNullOrWhiteSpace(txt)) sb.AppendLine(" " + Path(c.transform) + " [" + c.GetType().Name + "] " + txt!.Replace('\n', ' '));
            }
            File.AppendAllText(System.IO.Path.Combine(Application.dataPath, "..", "scene_dump.txt"), sb.ToString());
        }

        static void DumpFields(object obj, StringBuilder sb, string indent, int depth)
        {
            if (obj == null || depth > 2) return;
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (var t = obj.GetType(); t != null && t != typeof(UnityEngine.Object) && t != typeof(object); t = t.BaseType)
                foreach (var fi in t.GetFields(F | BindingFlags.DeclaredOnly))
                {
                    object? val;
                    try { val = fi.GetValue(obj); } catch { continue; }
                    if (val is Material m && m)
                    {
                        sb.AppendLine(indent + fi.Name + " = Material " + m.name + " shader=" + (m.shader ? m.shader.name : "") + " kw=" + string.Join(" ", m.shaderKeywords));
                        foreach (var n in m.GetPropertyNames(MaterialPropertyType.Float)) sb.AppendLine(indent + "   " + n + "=" + m.GetFloat(n));
                        foreach (var n in m.GetPropertyNames(MaterialPropertyType.Texture)) sb.AppendLine(indent + "   " + n + "=" + (m.GetTexture(n) ? m.GetTexture(n).name : "null"));
                    }
                    else if (val != null && !(val is UnityEngine.Object) && !fi.FieldType.IsPrimitive && !fi.FieldType.IsEnum && fi.FieldType != typeof(string)
                             && fi.FieldType.IsClass && fi.FieldType.Namespace?.StartsWith("System") != true)
                    {
                        sb.AppendLine(indent + fi.Name + " : " + fi.FieldType.Name);
                        DumpFields(val, sb, indent + "  ", depth + 1);
                    }
                    else sb.AppendLine(indent + fi.Name + " = " + val);
                }
        }

        static object? P(object o, string name)
        {
            foreach (var p in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (p.Name == name && p.GetIndexParameters().Length == 0)
                    try { return p.GetValue(o); } catch { return null; }
            return null;
        }

        static void DumpRewired(StringBuilder sb)
        {
            if (!Rewired.ReInput.isReady) { sb.AppendLine("not ready"); return; }
            foreach (var a in Rewired.ReInput.mapping.Actions)
                sb.AppendLine($" action {a.id} {a.name} type={a.type} cat={a.categoryId}");
            foreach (var j in Rewired.ReInput.controllers.Joysticks)
                sb.AppendLine($" joystick {j.id} {j.name} axes={j.axisCount} buttons={j.buttonCount} guid={j.hardwareTypeGuid}");
            foreach (var p in Rewired.ReInput.players.GetPlayers(true))
            {
                sb.AppendLine($" player {p.id} {p.name}");
                foreach (var m in p.controllers.maps.GetAllMaps())
                {
                    sb.AppendLine($"  map {m.controllerType} ctrl={m.controllerId} cat={m.categoryId} layout={m.layoutId} enabled={m.enabled}");
                    foreach (var aem in m.AllMaps)
                        sb.AppendLine($"    {aem.actionDescriptiveName} <- {aem.elementIdentifierName} ({aem.elementType}, contrib={aem.axisContribution}, range={aem.axisRange}, inv={aem.invert})");
                }
            }
        }

        static string Path(Transform t) => t.parent ? Path(t.parent) + "/" + t.name : t.name;

        static void Walk(Transform t, int depth, StringBuilder sb)
        {
            sb.Append(' ', depth * 2).Append(t.gameObject.activeSelf ? "+ " : "- ").Append(t.name).Append("  [");
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) { sb.Append("null,"); continue; }
                var n = c.GetType().Name;
                if (n == "Transform") continue;
                sb.Append(n);
                if (c is Behaviour b && !b.enabled) sb.Append("(off)");
                if (n == "PlayMakerFSM")
                {
                    var p = c.GetType().GetProperty("FsmName");
                    sb.Append(":" + p?.GetValue(c));
                }
                if (c is Renderer r && r.sharedMaterial) sb.Append(":" + r.sharedMaterial.name + "/" + (r.sharedMaterial.shader ? r.sharedMaterial.shader.name : ""));
                sb.Append(',');
            }
            sb.AppendLine("]");
            foreach (Transform ch in t) Walk(ch, depth + 1, sb);
        }
    }
}
