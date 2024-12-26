using INStructed.Interfaces;
using INStructed.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace INStructed.Services
{
    public class BuildingEditorService
    {
        private string jsonPath;

        public BuildingEditorService(string configDirectory)
        {
            jsonPath = Path.Combine(configDirectory, "buildings.json");
        }

        public Building LoadBuilding()
        {
            if (!File.Exists(jsonPath))
                return null;

            string jsonData = File.ReadAllText(jsonPath);
            return JsonConvert.DeserializeObject<Building>(jsonData);
        }

        public void SaveBuilding(Building building)
        {
            string jsonData = JsonConvert.SerializeObject(building, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonData);
        }
    }
}