using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using GEngine.Game;
using GEngine.Engine;

namespace GEngine
{
    public static class Loader
    {
        public static bool TryParseIni(string fileLocation, out Dictionary<string, string> result)
        {
            result = null;
            if (!File.Exists(fileLocation))
            {
                return false;
            }
            string[] ini = File.ReadAllLines(fileLocation);
            result = new Dictionary<string, string>();
            Regex dataPairPattern = new Regex(@"(.+)\s?=\s?(.+)", RegexOptions.None);
            foreach (string line in ini)
            {
                Match match = dataPairPattern.Match(line);
                if (match.Success)
                {
                    result.Add(match.Groups[1].Value.Trim(' ', '"'), match.Groups[2].Value.Trim(' ', '"'));
                } else
                {
                    //not a valid line
                    Debug.Log($"'{line}' is not a valid line.", "ParseIni");
                }
            }
            return true;
        }
        public static void LoadAllScenes()
        {
            Debug.Log($"[AssetLoader] Loading scene objects...");
            var scenes = AppDomain.CurrentDomain.GetAssemblies()
                                                .SelectMany(assemby => assemby.GetTypes())
                                                .Where(type => type.IsSubclassOf(typeof(Scene)));
            int loaded = 0, ignored = 0, failed = 0;
            foreach (var scene in scenes)
            {
                var attributes = scene.GetCustomAttributes().Where(x => { return x.GetType() == typeof(LoaderAttribute); });
                //Debug.Log("LoaderDebug", $"{scene.Name} has {attributes.Count()} loader attributes.");
                LoaderAttribute attrib = (LoaderAttribute)attributes.FirstOrDefault();
                ClassConstructorType constructorType = ClassConstructorType.Automatic;
                LoaderType loader = LoaderType.Automatic;

                if (attrib != null)
                {
                    constructorType = attrib.ConstructorType;
                    loader = attrib.GetLoaderType();
                }

                if (loader == LoaderType.Ignore || GEngine.Scenes.HasScene(scene))
                {
                    ignored++;
                    continue;
                }

                try
                {
                    ConstructorInfo? defConstructor = null;
                    ConstructorInfo? sizeConstructor = null;
                    ConstructorInfo? size2Constructor = null;
                    ConstructorInfo? size3Constructor = null;

                    switch (constructorType)
                    {
                        case ClassConstructorType.Automatic:
                            defConstructor = scene.GetConstructor(new Type[] { });
                            sizeConstructor = scene.GetConstructor(new Type[] { typeof(int), typeof(int) });
                            size2Constructor = scene.GetConstructor(new Type[] { typeof(Size) });
                            size3Constructor = scene.GetConstructor(new Type[] { typeof(Size), typeof(Size) });
                            break;
                        case ClassConstructorType.Default:
                            defConstructor = scene.GetConstructor(new Type[] { });
                            break;
                        case ClassConstructorType.Type1:
                            sizeConstructor = scene.GetConstructor(new Type[] { typeof(int), typeof(int) });
                            break;
                        case ClassConstructorType.Type2:
                            size2Constructor = scene.GetConstructor(new Type[] { typeof(Size) });
                            break;
                        case ClassConstructorType.Type3:
                            size3Constructor = scene.GetConstructor(new Type[] { typeof(Size), typeof(Size) });
                            break;
                    }

                    Scene sceneObject;

                    if (defConstructor != null) // ctr()
                    {
                        sceneObject = (Scene)defConstructor.Invoke(Array.Empty<object>());
                    }
                    else if (sizeConstructor != null) // ctr(int, int)
                    {
                        sceneObject = (Scene)sizeConstructor.Invoke(new object[] { GEngine.Game.Properties.InternalResolution.W, GEngine.Game.Properties.InternalResolution.H });
                    }
                    else if (size2Constructor != null) // ctr(Size)
                    {
                        sceneObject = (Scene)size2Constructor.Invoke(new object[] { GEngine.Game.Properties.InternalResolution });
                    }
                    else if (size3Constructor != null) // ctr(Size, Size)
                    {
                        sceneObject = (Scene)size3Constructor.Invoke(new object[] { GEngine.Game.Properties.InternalResolution, GEngine.Game.Properties.InternalResolution });
                    }
                    else
                    {
                        throw new Exception("No suitable constructor found.");
                    }
                    //sceneObject.ReferenceType = scene;
                    GEngine.Scenes.AddScene(sceneObject);
                    loaded++;
                } catch (Exception ex)
                {
                    Debug.Log($"[AssetLoader] Could not load scene {scene.FullName}.\n" +
                              $"          [*] Reason: {ex.Message}");
                    failed++;
                }
            }
            if (loaded != 0 || ignored != 0)
            {
                Debug.Log($"[AssetLoader] Loaded {loaded} scene(s). Ignored {ignored} scene(s).");
            }
            if (failed != 0)
            {
                Debug.Log($"[AssetLoader] Failed loading {failed} scene(s).");
            }
        }
    }
}
