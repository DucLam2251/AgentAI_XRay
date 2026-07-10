using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Chatbot.Models
{
    public class Disease
    {
        [JsonPropertyName("disease_id")]
        public string DiseaseId { get; set; } = string.Empty;

        [JsonPropertyName("disease_name")]
        public string DiseaseName { get; set; } = string.Empty;

        [JsonPropertyName("severity")]
        public int Severity { get; set; }

        [JsonPropertyName("emergency_level")]
        public int EmergencyLevel { get; set; }

        [JsonPropertyName("treatment_duration")]
        public string TreatmentDuration { get; set; } = string.Empty;

        [JsonPropertyName("treatment_method")]
        public List<string> TreatmentMethod { get; set; } = new List<string>();

        [JsonPropertyName("xray_findings")]
        public List<string> XrayFindings { get; set; } = new List<string>();

        [JsonPropertyName("red_flags")]
        public List<string> RedFlags { get; set; } = new List<string>();

        [JsonPropertyName("confidence_keywords")]
        public ConfidenceKeywords ConfidenceKeywords { get; set; } = new ConfidenceKeywords();
    }

    public class ConfidenceKeywords
    {
        [JsonPropertyName("primary")]
        public List<string> Primary { get; set; } = new List<string>();

        [JsonPropertyName("secondary")]
        public List<string> Secondary { get; set; } = new List<string>();

        [JsonPropertyName("symptoms")]
        public List<string> Symptoms { get; set; } = new List<string>();

        [JsonPropertyName("negative")]
        public List<string> Negative { get; set; } = new List<string>();
    }
}
