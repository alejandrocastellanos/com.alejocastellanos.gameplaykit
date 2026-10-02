using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameplayKit.DocCapture
{
    /// <summary>
    /// Exporta por reflexión la referencia de todos los componentes del paquete (campos serializados con sus
    /// valores por defecto, tooltips, eventos, propiedades y métodos públicos) a DocCaptures/components.json.
    /// Copia ese archivo a Documentation~/data/components.json después de cambiar el código del paquete.
    /// </summary>
    public static class DocReferenceExporter
    {
        private const BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        [MenuItem("Tools/GameplayKit Docs/Export Component Reference")]
        public static void Export()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "GameplayKit.Runtime");
            var types = asm.GetTypes().Where(t => typeof(MonoBehaviour).IsAssignableFrom(t))
                .OrderBy(t => t.Namespace).ThenBy(t => t.Name).ToList();
            var sb = new StringBuilder("[\n");
            for (int i = 0; i < types.Count; i++)
            {
                if (i > 0) sb.Append(",\n");
                // Un GameObject nuevo por tipo: si se reusara, un RequireComponent de un tipo anterior podría haber
                // agregado ya este componente y AddComponent devolvería null (sin valores por defecto).
                var holder = new GameObject("__docref");
                holder.SetActive(false);
                try { WriteType(sb, types[i], holder); }
                finally { UnityEngine.Object.DestroyImmediate(holder); }
            }
            sb.Append("\n]\n");
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../DocCaptures/components.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[GameplayKit Docs] {types.Count} componentes exportados a {path}");
        }

        private static void WriteType(StringBuilder sb, Type t, GameObject holder)
        {
            Component inst = null;
            if (!t.IsAbstract && !t.IsGenericTypeDefinition) { try { inst = holder.AddComponent(t); } catch { } }
            var req = t.GetCustomAttributes(typeof(RequireComponent), true).Cast<RequireComponent>()
                .SelectMany(r => new[] { r.m_Type0, r.m_Type1, r.m_Type2 }).Where(x => x != null).Select(x => Esc(x.Name)).Distinct();
            var menu = t.GetCustomAttributes(typeof(AddComponentMenu), false).Cast<AddComponentMenu>().Select(m => m.componentMenu).FirstOrDefault();
            var ifaces = t.GetInterfaces().Where(x => x.Namespace != null && x.Namespace.StartsWith("GameplayKit")).Select(x => Esc(x.Name));
            sb.Append("{\"name\":").Append(Esc(t.Name)).Append(",\"namespace\":").Append(Esc(t.Namespace))
              .Append(",\"base\":").Append(Esc(t.BaseType.Name)).Append(",\"abstract\":").Append(t.IsAbstract ? "true" : "false")
              .Append(",\"menu\":").Append(Esc(menu)).Append(",\"requires\":[").Append(string.Join(",", req))
              .Append("],\"interfaces\":[").Append(string.Join(",", ifaces)).Append("],\"fields\":[");

            var chain = new List<Type>();
            for (var x = t; x != null && x != typeof(MonoBehaviour); x = x.BaseType) chain.Insert(0, x);
            bool first = true;
            foreach (var ct in chain)
            foreach (var fi in ct.GetFields(Inst))
            {
                bool ser = (fi.IsPublic && !fi.IsDefined(typeof(NonSerializedAttribute), false)) || fi.IsDefined(typeof(SerializeField), false);
                if (!ser || fi.IsDefined(typeof(HideInInspector), false) || fi.IsInitOnly || fi.IsLiteral) continue;
                var tip = fi.GetCustomAttributes(typeof(TooltipAttribute), false).Cast<TooltipAttribute>().Select(a => a.tooltip).FirstOrDefault();
                var hdr = fi.GetCustomAttributes(typeof(HeaderAttribute), false).Cast<HeaderAttribute>().Select(a => a.header).FirstOrDefault();
                var rng = fi.GetCustomAttributes(typeof(RangeAttribute), false).Cast<RangeAttribute>().Select(a => Val(a.min) + "-" + Val(a.max)).FirstOrDefault();
                string def = "";
                if (inst != null) { try { def = Val(fi.GetValue(inst)); } catch { } }
                bool isEvent = typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(fi.FieldType);
                if (!first) sb.Append(",");
                first = false;
                sb.Append("{\"name\":").Append(Esc(fi.Name)).Append(",\"label\":").Append(Esc(ObjectNames.NicifyVariableName(fi.Name)))
                  .Append(",\"type\":").Append(Esc(TypeName(fi.FieldType))).Append(",\"default\":").Append(Esc(def))
                  .Append(",\"tooltip\":").Append(Esc(tip)).Append(",\"header\":").Append(Esc(hdr)).Append(",\"range\":").Append(Esc(rng))
                  .Append(",\"unityEvent\":").Append(isEvent ? "true" : "false").Append(",\"declaredIn\":").Append(Esc(ct.Name)).Append("}");
            }

            const BindingFlags pub = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            bool Ours(Type d) => d.Namespace != null && d.Namespace.StartsWith("GameplayKit");
            sb.Append("],\"events\":[").Append(string.Join(",", t.GetEvents(pub).Where(e => Ours(e.DeclaringType))
                .Select(e => "{\"name\":" + Esc(e.Name) + ",\"type\":" + Esc(TypeName(e.EventHandlerType)) + "}")));
            sb.Append("],\"properties\":[").Append(string.Join(",", t.GetProperties(pub).Where(p => Ours(p.DeclaringType))
                .Select(p => "{\"name\":" + Esc(p.Name) + ",\"type\":" + Esc(TypeName(p.PropertyType)) + ",\"static\":" +
                             ((p.GetGetMethod() ?? p.GetSetMethod()).IsStatic ? "true" : "false") + ",\"settable\":" + (p.GetSetMethod() != null ? "true" : "false") + "}")));
            sb.Append("],\"methods\":[").Append(string.Join(",", t.GetMethods(pub).Where(m => !m.IsSpecialName && Ours(m.DeclaringType))
                .Select(m => "{\"name\":" + Esc(m.Name) + ",\"signature\":" + Esc(TypeName(m.ReturnType) + " " + m.Name + "(" +
                             string.Join(", ", m.GetParameters().Select(p => TypeName(p.ParameterType) + " " + p.Name + (p.HasDefaultValue ? " = " + Val(p.DefaultValue) : ""))) + ")") +
                             ",\"static\":" + (m.IsStatic ? "true" : "false") + ",\"declaredIn\":" + Esc(m.DeclaringType.Name) + "}")));
            sb.Append("]}");
        }

        private static string Esc(string s) => s == null ? "null"
            : "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "") + "\"";

        private static string TypeName(Type t)
        {
            if (t == typeof(float)) return "float";
            if (t == typeof(int)) return "int";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(string)) return "string";
            if (t == typeof(void)) return "void";
            if (t.IsArray) return TypeName(t.GetElementType()) + "[]";
            if (t.IsGenericType) return t.Name.Split('`')[0] + "<" + string.Join(", ", t.GetGenericArguments().Select(TypeName)) + ">";
            return t.Name;
        }

        private static string Val(object v)
        {
            switch (v)
            {
                case null: return "null";
                case UnityEngine.Object uo: return uo == null ? "null" : uo.name;
                case float f: return f.ToString("0.###", CultureInfo.InvariantCulture);
                case bool b: return b ? "true" : "false";
                case LayerMask lm: return lm.value == -1 ? "Everything" : lm.value == 0 ? "Nothing" : lm.value.ToString();
                case Vector2 v2: return "(" + Val(v2.x) + ", " + Val(v2.y) + ")";
                case Vector3 v3: return "(" + Val(v3.x) + ", " + Val(v3.y) + ", " + Val(v3.z) + ")";
                case Color c: return "#" + ColorUtility.ToHtmlStringRGBA(c);
                case string s: return "\"" + s + "\"";
                case UnityEngine.Events.UnityEventBase _: return "";
                case ICollection col: return col.Count == 0 ? "[]" : "[" + col.Count + " items]";
                case AnimationCurve _: return "curve";
                default: return v.ToString();
            }
        }
    }
}
