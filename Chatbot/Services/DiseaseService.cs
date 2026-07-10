using Chatbot.Models;
using System.Text.Json;
using System.Linq;

namespace Chatbot.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly DiseaseStore _store;

        public DiseaseService(DiseaseStore store)
        {
            _store = store;
        }

        public List<Disease> GetAll()
        {
            return _store.GetAllDiseases();
        }

        public Disease? GetById(string id)
        {
            return _store.GetByIdDisease(id);
        }

        public void Add(Disease el)
        {
            _store.Add(el);
        }

        public bool Update(string id, Disease el)
        {
            return _store.Update(id, el);
        }

        public bool Delete(string id) => _store.Delete(id);

        public List<Chatbot.Models.DiseaseMatch> FindTopKBySymptoms(string query, int k = 3)
        {
            var raw = _store.FindTopKBySymptoms(query, k);
            var res = new List<Chatbot.Models.DiseaseMatch>();
            foreach (var item in raw)
            {
                var el = item.Item1;
                var score = item.Item2;
                try
                {
                    var d = JsonSerializer.Deserialize<Disease>(el.GetRawText());
                    if (d != null) res.Add(new Chatbot.Models.DiseaseMatch { Disease = d, Score = score });
                }
                catch { }
            }
            return res;
        }
    }
}
