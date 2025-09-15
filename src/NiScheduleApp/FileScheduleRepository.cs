using NiScheduleApp.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NiSchedule.App;

namespace NiScheduleApp
{
    public class FileScheduleRepository : IScheduleRepository
    {
        private readonly string _filePath = Path.Combine("data", "schedules.json");
    private List<ScheduleItem> _items = new List<ScheduleItem>();

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public FileScheduleRepository()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _items = JsonConvert.DeserializeObject<List<ScheduleItem>>(json, _jsonSettings) ?? new List<ScheduleItem>();
            }
        }

        public void Add(ScheduleItem item)
        {
            _items.Add(item);
            Save();
        }

        public void Delete(string id)
        {
            var found = _items.FirstOrDefault(x => x.Id == id);
            if (found != null)
            {
                _items.Remove(found);
                Save();
            }
        }

        public List<ScheduleItem> GetAll()
        {
            return _items.ToList();
        }

        private void Save()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var json = JsonConvert.SerializeObject(_items, Formatting.Indented, _jsonSettings);
            File.WriteAllText(_filePath, json);
        }
    }
}
