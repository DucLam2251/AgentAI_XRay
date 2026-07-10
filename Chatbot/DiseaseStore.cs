using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq;
using Chatbot.Models;

namespace Chatbot
{
    public class DiseaseStore
    {
        private readonly string _path;
        private readonly object _lock = new object();
        private List<JsonElement> _items = new List<JsonElement>();
        // parallel list of embeddings aligned with _items
        private List<double[]> _embeddings = new List<double[]>();
        private const int EmbeddingDim = 256;

        // Build text used to compute embedding for a record: disease_name + all confidence keywords
        private static string BuildTextForEmbedding(JsonElement element)
        {
            var parts = new List<string>();
            if (element.TryGetProperty("disease_name", out var dn) && dn.ValueKind == JsonValueKind.String)
                parts.Add(dn.GetString() ?? string.Empty);

            if (element.TryGetProperty("confidence_keywords", out var ck) && ck.ValueKind == JsonValueKind.Object)
            {
                string[] keys = new[] { "primary", "secondary", "symptoms", "negative" };
                foreach (var k in keys)
                {
                    if (ck.TryGetProperty(k, out var arr) && arr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var v in arr.EnumerateArray())
                        {
                            if (v.ValueKind == JsonValueKind.String)
                                parts.Add(v.GetString() ?? string.Empty);
                        }
                    }
                }
            }

            return string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        public DiseaseStore(string path)
        {
            _path = path;
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                    File.WriteAllText(_path, "[]");
                }

                var text = File.ReadAllText(_path);
                var doc = JsonDocument.Parse(text);
                var arr = doc.RootElement.EnumerateArray();
                _items = new List<JsonElement>();
                _embeddings = new List<double[]>();
                foreach (var el in arr)
                {
                    _items.Add(el);
                    // if embedding present in file, load it; otherwise compute from disease_name
                    bool embeddingLoaded = false;
                    if (el.TryGetProperty("embedding", out var emb) && emb.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<double>();
                        foreach (var v in emb.EnumerateArray())
                        {
                            if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d))
                                list.Add(d);
                        }
                        // Check if embedding is valid: not empty AND has non-zero values
                        if (list.Count > 0 && list.Any(x => x != 0))
                        {
                            _embeddings.Add(list.ToArray());
                            embeddingLoaded = true;
                        }
                    }

                    // fallback: if no valid embedding found, compute from disease_name + confidence_keywords
                    if (!embeddingLoaded)
                    {
                        var textForEmbedding = BuildTextForEmbedding(el);
                        _embeddings.Add(ComputeEmbedding(textForEmbedding, EmbeddingDim));
                    }
                }
            }
            catch
            {
                _items = new List<JsonElement>();
            }
        }

        private void Save()
        {
            lock (_lock)
            {
                // Rebuild a list of objects combining original properties and embedding
                var outList = new List<Dictionary<string, object>>();
                for (int i = 0; i < _items.Count; i++)
                {
                    var el = _items[i];
                    // deserialize original element to dictionary
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(el.GetRawText()) ?? new Dictionary<string, object>();
                    // add embedding
                    if (i < _embeddings.Count)
                        dict["embedding"] = _embeddings[i];
                    outList.Add(dict);
                }

                var json = JsonSerializer.Serialize(outList, new JsonSerializerOptions { WriteIndented = true });

                // Lock file during write operation to prevent access from other processes
                int maxRetries = 3;
                int retryDelay = 100; // milliseconds

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        // Use FileStream with FileShare.None to exclusively lock the file
                        using (var fileStream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (var writer = new StreamWriter(fileStream, System.Text.Encoding.UTF8))
                            {
                                writer.Write(json);
                                writer.Flush();
                                fileStream.Flush();
                            }
                        }
                        // Successfully written, exit retry loop
                        return;
                    }
                    catch (IOException ex) when (attempt < maxRetries - 1)
                    {
                        // File is locked by another process, wait and retry
                        System.Threading.Thread.Sleep(retryDelay);
                        retryDelay *= 2; // Exponential backoff
                    }
                    catch (IOException ex)
                    {
                        // Final attempt failed, throw exception
                        throw new IOException($"Failed to write to file '{_path}' after {maxRetries} attempts: {ex.Message}", ex);
                    }
                }
            }
        }

        // Legacy text search
        public Disease? FindByName(string name)
        {
            var lower = name?.Trim().ToLowerInvariant();
            foreach (var el in _items)
            {
                if (el.TryGetProperty("disease_name", out var dn))
                {
                    if ((dn.GetString() ?? "").ToLowerInvariant().Contains(lower ?? ""))
                    return JsonSerializer.Deserialize<Disease>(el.GetRawText());
                }
            }
            return null;
        }

        // Embedding based search: returns best match or null
        public Disease? FindByEmbedding(string query, double similarityThreshold = 0.6)
        {
            if (string.IsNullOrWhiteSpace(query) || _items.Count == 0)
                return null;

            var qEmb = ComputeEmbedding(query, EmbeddingDim);
            double bestSim = double.NegativeInfinity;
            int bestIdx = -1;
            for (int i = 0; i < _embeddings.Count; i++)
            {
                var emb = _embeddings[i];
                if (emb == null) continue;
                var sim = DotProduct(qEmb, emb);
                if (sim > bestSim)
                {
                    bestSim = sim;
                    bestIdx = i;
                }
            }
            if (bestIdx >= 0 && bestSim >= similarityThreshold)
                return JsonSerializer.Deserialize<Disease>(_items[bestIdx].GetRawText());

            return null;
        }

        public void Add(JsonElement element)
        {
            lock (_lock)
            {
                _items.Add(element);
                // compute embedding for new element from combined fields
                var textForEmbedding = BuildTextForEmbedding(element);
                // no-op patch: preserve existing behavior while ensuring patch applies
                _embeddings.Add(ComputeEmbedding(textForEmbedding, EmbeddingDim));
                Save();
            }
        }

        // Strongly-typed helpers to avoid repeated JsonElement conversion
        public void Add(Disease disease)
        {
            var elem = JsonSerializer.SerializeToElement(disease);
            Add(elem);
        }

        public bool Update(string diseaseId, Disease disease)
        {
            var elem = JsonSerializer.SerializeToElement(disease);
            return Update(diseaseId, elem);
        }

        public List<Disease> GetAllDiseases()
        {
            var list = new List<Disease>();
            foreach (var el in GetAll())
            {
                try
                {
                    var d = JsonSerializer.Deserialize<Disease>(el.GetRawText());
                    if (d != null) list.Add(d);
                }
                catch { }
            }
            return list;
        }

        public Disease? GetByIdDisease(string diseaseId)
        {
            var el = GetById(diseaseId);
            if (el == null) return null;
            try { return JsonSerializer.Deserialize<Disease>(el.Value.GetRawText()); }
            catch { return null; }
        }

        public List<JsonElement> GetAll()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        public JsonElement? GetById(string diseaseId)
        {
            if (string.IsNullOrWhiteSpace(diseaseId)) return null;
            lock (_lock)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var el = _items[i];
                    if (el.TryGetProperty("disease_id", out var id) && id.GetString() == diseaseId)
                        return el;
                }
            }
            return null;
        }

        public bool Update(string diseaseId, JsonElement newElement)
        {
            if (string.IsNullOrWhiteSpace(diseaseId)) return false;
            lock (_lock)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var el = _items[i];
                    if (el.TryGetProperty("disease_id", out var id) && id.GetString() == diseaseId)
                    {
                        _items[i] = newElement;
                        // recompute embedding using combined fields
                        var textForEmbedding = BuildTextForEmbedding(newElement);
                        if (i < _embeddings.Count)
                            _embeddings[i] = ComputeEmbedding(textForEmbedding, EmbeddingDim);
                        else
                            _embeddings.Add(ComputeEmbedding(textForEmbedding, EmbeddingDim));
                        Save();
                        return true;
                    }
                }
            }
            return false;
        }

        // Helper: Get keyword weight based on type (primary/secondary/symptoms/negative)
        private static double GetKeywordWeight(string keywordType)
        {
            return keywordType switch
            {
                "primary" => 2.0,      // Primary keywords (core disease characteristics)
                "secondary" => 1.5,    // Secondary keywords (supporting symptoms)
                "symptoms" => 1.0,     // Symptom keywords (general symptoms)
                "negative" => 0.5,     // Negative keywords (what's NOT present) - lower weight
                _ => 1.0
            };
        }

        // Find top-k records matching query by embedding similarity with weighted scoring
        // NEW LOGIC: Split query into individual symptoms, search each separately, 
        // then calculate weighted score based on keyword type (primary/secondary/symptoms)
        public List<(JsonElement element, double score)> FindTopKBySymptoms(string query, int k = 3)
        {
            var results = new List<(JsonElement, double)>();
            if (string.IsNullOrWhiteSpace(query) || _items.Count == 0) return results;

            // Step 1: Split query into individual symptoms
            // Format: "đau háng, khó chịu khi vận động" → ["đau háng", "khó chịu khi vận động"]
            var symptoms = query.Split(new[] { ',', ';', '&', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToList();

            if (symptoms.Count == 0)
            {
                // Fallback: if no commas, treat entire query as single symptom
                symptoms.Add(query);
            }

            // Step 2: For each disease, store (totalWeightedScore, totalWeight) for weighted average
            var diseaseWeightedScores = new Dictionary<int, (double weightedSum, double weightSum)>();

            for (int i = 0; i < _items.Count; i++)
            {
                diseaseWeightedScores[i] = (0.0, 0.0);
            }

            // Step 3: For each symptom, search across all diseases with keyword-based weighting
            foreach (var symptom in symptoms)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    // Calculate similarity score based on keyword matching
                    // Returns: (similarity score 0-1, keyword weight based on type)
                    var (similarity, keywordWeight) = CalculateSymptomDiseaseScore(_items[i], symptom);

                    var (weightedSum, weightSum) = diseaseWeightedScores[i];
                    diseaseWeightedScores[i] = (
                        weightedSum + similarity * keywordWeight,
                        weightSum + keywordWeight
                    );
                }
            }

            // Step 4: Calculate weighted average score for each disease
            for (int i = 0; i < _items.Count; i++)
            {
                var (weightedSum, weightSum) = diseaseWeightedScores[i];
                if (weightSum > 0)
                {
                    double weightedAvgScore = weightedSum / weightSum;
                    results.Add((_items[i], weightedAvgScore));
                }
            }

            // Step 5: Sort by weighted average score and return top-k
            return results.OrderByDescending(r => r.Item2).Take(k).ToList();
        }

        // Calculate similarity score between symptom query and disease keywords
        // Returns: (similarity 0-1, keywordWeight 0.5-2.0)
        private static (double similarity, double weight) CalculateSymptomDiseaseScore(JsonElement diseaseElement, string symptom)
        {
            if (!diseaseElement.TryGetProperty("confidence_keywords", out var ck) || ck.ValueKind != JsonValueKind.Object)
            {
                // No keywords - use default embedding-based similarity
                return (0.0, 1.0);
            }

            string[] keywordTypes = new[] { "primary", "secondary", "symptoms", "negative" };
            double maxSimilarity = 0.0;
            double resultWeight = 1.0; // Default weight

            // Try to find best matching keyword type for this symptom
            foreach (var keywordType in keywordTypes)
            {
                if (ck.TryGetProperty(keywordType, out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var keyword in arr.EnumerateArray())
                    {
                        if (keyword.ValueKind == JsonValueKind.String)
                        {
                            var keywordStr = keyword.GetString() ?? "";
                            if (string.IsNullOrWhiteSpace(keywordStr)) continue;

                            // Calculate similarity between symptom and keyword
                            double similarity = CalculateTextSimilarity(symptom, keywordStr);

                            // If match found, use this weight (higher priority types stop search)
                            if (similarity > 0.5)  // Threshold for match
                            {
                                if (similarity > maxSimilarity)
                                {
                                    maxSimilarity = similarity;
                                    resultWeight = GetKeywordWeight(keywordType);
                                }
                            }
                        }
                    }
                }
            }

            return (maxSimilarity, resultWeight);
        }

        // Calculate text similarity between two strings (0.0 to 1.0)
        // Uses simple word-level overlap + embedding similarity
        private static double CalculateTextSimilarity(string text1, string text2)
        {
            var t1Lower = text1.ToLowerInvariant();
            var t2Lower = text2.ToLowerInvariant();

            // Exact match
            if (t1Lower == t2Lower) return 1.0;

            // Substring match
            if (t1Lower.Contains(t2Lower) || t2Lower.Contains(t1Lower))
                return 0.85;

            // Word overlap similarity
            var words1 = t1Lower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = t2Lower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            int commonWords = 0;
            foreach (var w1 in words1)
            {
                foreach (var w2 in words2)
                {
                    if (w1 == w2) commonWords++;
                    else if (w1.Contains(w2) || w2.Contains(w1)) commonWords += 1; // Partial word match
                }
            }

            if (commonWords == 0) return 0.0;

            // Similarity = common words / total unique words
            int totalWords = words1.Length + words2.Length - commonWords;
            return (double)commonWords / totalWords;
        }

        public bool Delete(string diseaseId)
        {
            if (string.IsNullOrWhiteSpace(diseaseId)) return false;
            lock (_lock)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var el = _items[i];
                    if (el.TryGetProperty("disease_id", out var id) && id.GetString() == diseaseId)
                    {
                        _items.RemoveAt(i);
                        if (i < _embeddings.Count)
                            _embeddings.RemoveAt(i);
                        Save();
                        return true;
                    }
                }
            }
            return false;
        }

        private static double DotProduct(double[] a, double[] b)
        {
            var n = Math.Min(a.Length, b.Length);
            double s = 0;
            for (int i = 0; i < n; i++) s += a[i] * b[i];
            return s;
        }

        // Simple local embedding: token hashing into fixed-size vector, l2-normalized
        private static double[] ComputeEmbedding(string text, int dim)
        {
            var vec = new double[dim];
            if (string.IsNullOrWhiteSpace(text)) return vec;
            var tokens = text.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', ',', '.', '-', '_', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                // stable-ish hash based on UTF8 bytes
                var b = System.Text.Encoding.UTF8.GetBytes(t);
                int h = 5381;
                foreach (var by in b) h = ((h << 5) + h) + by;
                var idx = Math.Abs(h) % dim;
                vec[idx] += 1.0;
            }
            // L2 normalize
            double sum = 0;
            for (int i = 0; i < dim; i++) sum += vec[i] * vec[i];
            if (sum <= 0) return vec;
            var norm = Math.Sqrt(sum);
            for (int i = 0; i < dim; i++) vec[i] /= norm;
            return vec;
        }
    }
}
