using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(test_mod.Core), "test_mod", "1.0.0", "alber", null)]
[assembly: MelonGame("Jan Malitschek", "BetonBrutal")]

namespace test_mod
{
    public class Core : MelonMod
    {
        private List<ModelData> modelsList = new List<ModelData>();
        private Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();
        private string configFilePath = "UserData/models.json";



        public override void OnInitializeMelon()
        {
            if (!File.Exists(configFilePath))
            {
                MelonLogger.Error("JSON file with models not found.");
                return;
            }

            MelonLogger.Msg("Teleport module initialized.");
            string jsonContent = File.ReadAllText(configFilePath);
            ModelsConfig modelsConfig = JsonConvert.DeserializeObject<ModelsConfig>(jsonContent);

            if (modelsConfig != null && modelsConfig.models != null)
            {
                foreach (var modelData in modelsConfig.models)
                {
                    MelonLogger.Msg($"Model: {modelData.modelName}, AssetBundle: {modelData.assetBundlePath}, Spawn key: {modelData.spawnKey}");
                    modelsList.Add(modelData);

                    AssetBundle myBundle = AssetBundle.LoadFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"..\LocalLow\Jan Malitschek\BetonBrutal\ModsData\", modelData.assetBundlePath));
                    if (myBundle == null)
                    {
                        MelonLogger.Error($"Failed to load AssetBundle at path: {modelData.assetBundlePath}");
                        continue;
                    }

                    GameObject modelPrefab = myBundle.LoadAsset<GameObject>(modelData.modelName);
                    if (modelPrefab != null)
                    {
                        MelonLogger.Msg($"Model {modelPrefab.name} loaded.");
                        loadedModels[modelData.modelName] = modelPrefab;

                        if (modelData.modelName == "testtower")
                        {
                            GameObject spawnedModel = GameObject.Instantiate(loadedModels[modelData.modelName]);
                            spawnedModel.transform.position = new Vector3(999, 0, 999);
                        }
                    }
                    else
                    {
                        MelonLogger.Error($"Model {modelData.modelName} not found in AssetBundle.");
                    }
                }
            }
            else
            {
                MelonLogger.Error("Error when reading JSON file or it is empty.");
            }
        }



        public override void OnLateUpdate()
        {
            foreach (var modelData in modelsList)
            {
                Key spawnKey = GetKeyFromString(modelData.spawnKey);

                if (Keyboard.current[spawnKey].wasPressedThisFrame && loadedModels.ContainsKey(modelData.modelName))
                {
                    GameObject player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        Vector3 playerPosition = player.transform.position;
                        GameObject spawnedModel = GameObject.Instantiate(loadedModels[modelData.modelName]);
                        spawnedModel.transform.position = playerPosition;
                        MelonLogger.Msg($"Model {modelData.modelName} spawned at pos: {playerPosition}");
                    }
                    else
                    {
                        MelonLogger.Error("Player not found.");
                    }
                }
            }
        }



        private Key GetKeyFromString(string keyString)
        {
            if (System.Enum.TryParse(keyString, out Key key))
            {
                return key;
            }
            else
            {
                MelonLogger.Error($"Incorrect key: {keyString}");
                return Key.None;
            }
        }



        public class ModelsConfig
        {
            public List<ModelData> models { get; set; }
        }



        public class ModelData
        {
            public string assetBundlePath { get; set; }
            public string modelName { get; set; }
            public string spawnKey { get; set; }
        }
    }
}
