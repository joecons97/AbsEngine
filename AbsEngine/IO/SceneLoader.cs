using AbsEngine.ECS;
using AbsEngine.Rendering;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AbsEngine.IO;

internal static class SceneLoader
{
    public static Scene LoadSceneFromFile(string fileName)
    {
        if (File.Exists(fileName) == false)
            throw new FileNotFoundException(fileName);

        var jsonNodes = JsonNode.Parse(File.ReadAllText(fileName)) ?? throw new InvalidDataException();
        var jsonObject = jsonNodes.AsObject();

        if (IsRawScene(jsonObject))
        {
            return LoadRawScene(jsonObject);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static bool IsRawScene(JsonObject jsonObject)
    {
        if (jsonObject.TryGetPropertyValue("type", out var node) && node != null)
        {
            return node.AsValue().GetValue<string>() == "Raw";
        }

        return false;
    }

    private static Type? GetType(string name)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .FirstOrDefault(t => t.Name == name);
    }

    private static Scene LoadRawScene(JsonObject jsonObject)
    {
        var scene = Scene.Load();
        scene.Name = jsonObject["name"]?.GetValue<string>()!;

        var dependencies = jsonObject["dependencies"]?.AsArray();
        LoadDependencies(scene, dependencies);

        var components = jsonObject["components"]?.AsArray();
        var refComps = LoadComponents(scene, components);

        var entities = jsonObject["entities"]?.AsArray();
        LoadEntities(scene, entities, refComps);

        var systems = jsonObject["systems"]?.AsArray();
        LoadSystems(scene, systems);

        return scene;
    }

    private static Dictionary<Guid, Component> LoadComponents(Scene scene, JsonArray? json)
    {
        Dictionary<Guid, Component> _refComponents = new();

        if (json != null)
        {
            foreach (var component in json)
            {
                if (component == null)
                    continue;

                var guid = component["id"]?.AsValue().GetValue<Guid>();
                if (guid == null) continue;

                var typeName = component["type"]?.AsValue().GetValue<string>();
                if (string.IsNullOrEmpty(typeName)) continue;

                Type type = GetType(typeName) ?? throw new ArgumentOutOfRangeException(nameof(typeName), typeName, "");
                Component instance = (Component?)Activator.CreateInstance(type) ?? throw new ArgumentOutOfRangeException(nameof(type), type, "");

                _refComponents.Add(guid.Value, instance);
                scene.AddAsset(guid.GetValueOrDefault(), instance);
                scene.EntityManager.AddComponent(instance);

                foreach (var prop in type.GetProperties())
                {
                    var node = component[prop.Name];
                    if (node == null) continue;

                    if (prop.PropertyType.IsValueType)
                    {
                        if (node is JsonValue jsonValue)
                        {
                            var val = jsonValue.Deserialize(prop.PropertyType);
                            prop.SetValue(instance, val);
                        }
                        else
                        {
                            var inst = Activator.CreateInstance(prop.PropertyType);
                            foreach (var prop2 in prop.PropertyType.GetFields())
                            {
                                var node2 = node[prop2.Name];
                                if (node2 == null) continue;

                                prop2.SetValue(inst, node2.Deserialize(prop2.FieldType));
                            }
                            prop.SetValue(instance, inst);
                        }
                    }
                    else if (Guid.TryParse(node.AsValue().GetValue<string>(), out Guid id))
                    {
                        var asset = scene.GetAsset<object>(id);
                        prop.SetValue(instance, asset);
                    }
                }
            }
        }

        return _refComponents;
    }

    private static List<(Guid, object)> LoadDependencies(Scene scene, JsonArray? json)
    {
        var result = new List<(Guid, object)>();

        if (json != null)
        {
            foreach (var dependency in json)
            {
                if (dependency == null)
                    continue;

                var guid = dependency["id"]?.AsValue().GetValue<Guid>();
                if (guid == null) continue;

                var type = dependency["type"]?.AsValue().GetValue<string>();
                if (string.IsNullOrEmpty(type)) continue;

                var file = dependency["file"]?.AsValue().GetValue<string>();
                if (string.IsNullOrEmpty(file)) continue;

                object? asset = null;


                asset = type switch
                {
                    "Mesh" => MeshLoader.LoadMesh(file),
                    "Shader" => ShaderLoader.LoadShader(file),
                    _ => throw new Exception($"Not expected value: {type}"),
                };
                if (asset != null)
                {
                    scene.AddAsset(guid.Value, asset);
                    result.Add(new(guid.Value, asset));
                }
            }
        }

        return result;
    }

    private static List<Entity> LoadEntities(Scene scene, JsonArray? json, Dictionary<Guid, Component> loadedComponents)
    {
        var result = new List<Entity>();

        if (json != null)
        {
            foreach (var entity in json)
            {
                if (entity == null)
                    continue;

                var ent = scene.EntityManager.CreateEmptyEntity();
                var entityComponents = entity["components"]?.AsArray();
                if (entityComponents != null)
                {
                    foreach (var entComp in entityComponents)
                    {
                        if (entComp == null)
                            continue;

                        var guid = entComp["id"]?.AsValue().GetValue<Guid>();
                        if (guid == null) continue;

                        var comp = loadedComponents[guid.Value];
                        comp.Entity = ent;
                        ent.AddComponent(comp);
                    }
                }
                result.Add(ent);
            }
        }

        return result;
    }

    private static List<ECS.System> LoadSystems(Scene scene, JsonArray? json)
    {
        var result = new List<ECS.System>();

        if (json != null)
        {
            foreach (var item in json)
            {
                if(item == null) continue;  

                var typeName = item.GetValue<string>();

                var type = GetType(typeName);
                if(type == null) continue;

                var instance = scene.RegisterSystem(type);
                result.Add(instance);
            }
        }

        return result;
    }

}
