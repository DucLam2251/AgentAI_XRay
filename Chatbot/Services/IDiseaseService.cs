using Chatbot.Models;

namespace Chatbot.Services
{
    public interface IDiseaseService
    {
        List<Disease> GetAll();
        Disease? GetById(string id);
        void Add(Disease el);
        bool Update(string id, Disease el);
        bool Delete(string id);
        // RAG symptom search: return top-k DiseaseMatch
        List<Chatbot.Models.DiseaseMatch> FindTopKBySymptoms(string query, int k = 3);
    }
}
