using System;
using System.Collections.Generic;
using System.Linq;
using GameplayKit.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameplayKit.Tests
{
    /// <summary>
    /// Garantías del catálogo: todo componente del kit se puede agregar desde "Add Component" y guardarse
    /// en escenas y prefabs. Atrapa errores que las pruebas de juego no ven (ej. una clase en un archivo
    /// con otro nombre funciona con AddComponent por código, pero aparece como "Missing Script" al guardarla).
    /// </summary>
    public class ComponentCatalogTests
    {
        private static IEnumerable<Type> KitBehaviours() =>
            typeof(CharacterCore).Assembly.GetTypes()
                .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition)
                .OrderBy(t => t.FullName);

        [Test]
        public void EveryComponent_HasItsOwnScriptFile()
        {
            var withScript = new HashSet<Type>(
                AssetDatabase.FindAssets("t:MonoScript", new[] { "Packages/com.alejocastellanos.gameplaykit/Runtime" })
                    .Select(guid => AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid)))
                    .Where(script => script != null && script.GetClass() != null)
                    .Select(script => script.GetClass()));

            var orphans = KitBehaviours().Where(t => !withScript.Contains(t)).Select(t => t.Name).ToList();
            Assert.IsEmpty(orphans, "Estas clases no están en un archivo con su mismo nombre y no se pueden guardar en escenas/prefabs: " + string.Join(", ", orphans));
        }

        [Test]
        public void EveryComponent_CanBeAddedToAnEmptyGameObject()
        {
            var failures = new List<string>();
            foreach (var type in KitBehaviours())
            {
                var go = new GameObject("CatalogTest");
                try
                {
                    if (go.AddComponent(type) == null) failures.Add(type.Name);
                }
                catch (Exception e) { failures.Add($"{type.Name} ({e.GetType().Name})"); }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            Assert.IsEmpty(failures, "No se pudieron agregar: " + string.Join(", ", failures));
        }

        [Test]
        public void NoComponent_SharesItsNameWithABuiltInUnityComponent()
        {
            var builtIn = new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("UnityEngine"))
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsPublic && typeof(Component).IsAssignableFrom(t))
                .Select(t => t.Name));
            var clashes = KitBehaviours().Where(t => builtIn.Contains(t.Name)).Select(t => t.Name).ToList();
            Assert.IsEmpty(clashes, "Comparten nombre con componentes de Unity (Add Component falla): " + string.Join(", ", clashes));
        }

        [Test]
        public void MenuBuilders_CreatePlayerAndEnemyWithoutMissingScripts()
        {
            var player = GameplayKit.Editor.GameplayKitMenu.BuildPlayer(Vector2.zero);
            var enemy = GameplayKit.Editor.GameplayKitMenu.BuildEnemy(Vector2.zero);
            try
            {
                Assert.AreEqual("Player", player.tag);
                Assert.IsNotNull(player.GetComponent<CharacterCore>());
                Assert.IsNotNull(player.GetComponent<KeyboardInputReader>());
                Assert.IsFalse(player.GetComponents<Component>().Any(c => c == null));
                Assert.IsFalse(enemy.GetComponents<Component>().Any(c => c == null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(enemy);
            }
        }
    }
}
