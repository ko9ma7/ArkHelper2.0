using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace ArkHelper.Modules.MaterialCalc
{
    public static class Info
    {
        public static Dictionary<int, List<Material>> Data = new Dictionary<int, List<Material>>();
        static Info()
        {
            
        }

        static bool _inited = false;
        public static void Init()
        {
            if (_inited) return;
            var mainJson = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Address.res + "\\data\\material.json"));
            foreach (var materialJson in mainJson.EnumerateObject())
            {
                var material = JsonSerializer.Deserialize<Material>(JsonSerializer.Serialize(materialJson.Value));
                material.id = materialJson.Name;
                material.json = materialJson.Value;

                if (!Data.ContainsKey(material.level))
                    Data.Add(material.level, new List<Material>());

                Data[material.level].Add(material);
            }
            foreach (var pair in Data)
                foreach (var material in pair.Value)
                {
                    material.equal.Clear();
                    if (material.json.TryGetProperty("equals", out var equalsJson))
                    {
                        foreach (var equalJson in equalsJson.EnumerateObject())
                        {
                            foreach (var _pair in Data.Values)
                            {
                                var add = _pair.Find(t => t.id == equalJson.Name);
                                if (add != null)
                                {
                                    material.equal.Add(add, equalJson.Value.GetInt32());
                                    break;
                                }
                            }
                        }
                    }
                }


            Data = Data.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, o => o.Value); //以字典Key值逆序排序

            _inited = true;
        }
    }
    public class Material
    {
        public JsonElement json { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Dictionary<Material, int> equal = new Dictionary<Material, int>();
        public int level { get; set; }
    }
    public class MaterialStatus
    {
        public int RequireNumber { get; set; }

        public MaterialStatus()
        {
            RequireNumber = 0;
        }
    }
}
