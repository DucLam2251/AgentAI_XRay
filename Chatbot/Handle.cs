using Chatbot.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Chatbot
{
    /// <summary>
    /// Enum để xác định ý định của người dùng
    /// </summary>
    public enum UserIntent
    {
        Unknown,              // Không xác định
        GetDiseaseInfo,       // Hỏi thông tin bệnh lý cụ thể
        SearchBySymptoms      // Tìm kiếm bệnh theo biểu hiện/triệu chứng
    }

    /// <summary>
    /// Class lưu các đặc trưng trích xuất từ câu hỏi
    /// </summary>
    public class InputFeatures
    {
        public bool HasSymptoms { get; set; }
        public List<string> ExtractedSymptoms { get; set; } = new List<string>();
        public string? ExtractedDiseaseName { get; set; }
        public bool IsSymptomQuestionPattern { get; set; }  // "... là biểu hiện của bệnh gì?"
        public bool IsDiseaseNamePattern { get; set; }      // "bệnh <tên>"
        public bool IsGeneralQuestion { get; set; }         // "thông tin", "chi tiết", etc.
    }

    public class Handle
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly DiseaseStore _store;

        // 🔥 Tool registry
        public readonly Dictionary<string, Func<string, Task<string>>> ToolHandlers;

        public Handle(string apiKey)
        {
            _apiKey = apiKey;
            _http = new HttpClient();

            // create or load store
            var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "diseases.json");
            _store = new DiseaseStore(dataPath);

            ToolHandlers = new Dictionary<string, Func<string, Task<string>>>
            {
                { "describeXray", DescribeXray },
                { "explainFinding", ExplainFinding }
            };
        }

        public DiseaseStore GetStore() => _store;

        public async Task<AgentResponse> AskAsync(string userInput)
        {
            // If the UI uploaded an image as base64, the input starts with a special prefix
            const string uploadPrefix = "UPLOAD_BASE64:";
            if (!string.IsNullOrEmpty(userInput) && userInput.StartsWith(uploadPrefix))
            {
                try
                {
                    var jsonPart = userInput.Substring(uploadPrefix.Length);
                    var uploadDoc = JsonDocument.Parse(jsonPart);
                    var data = uploadDoc.RootElement.GetProperty("data").GetString() ?? "";
                    var filename = uploadDoc.RootElement.GetProperty("filename").GetString() ?? "upload.jpg";
                    var contentType = uploadDoc.RootElement.TryGetProperty("contentType", out var ct) ? ct.GetString() ?? "" : "";

                    var args = JsonSerializer.Serialize(new { imageBase64 = data, filename, contentType });
                    var result = await DescribeXray(args);

                    // DescribeXray now returns a JSON string { diagnosis, imageBase64 }
                    try
                    {
                        var resultDoc = JsonDocument.Parse(result);
                        var diagnosis = resultDoc.RootElement.GetProperty("diagnosis").GetString() ?? "";
                        var imageBase64 = resultDoc.RootElement.GetProperty("imageBase64").GetString() ?? "";
                        var annotatedImageBase64 = resultDoc.RootElement.TryGetProperty("annotatedImageBase64", out var a) ? a.GetString() ?? imageBase64 : imageBase64;

                        return new AgentResponse
                        {
                            Success = true,
                            Message = diagnosis,
                            ToolName = "describeXray",
                            Data = new { diagnosis, imageBase64, annotatedImageBase64 }
                        };
                    }
                    catch
                    {
                        return new AgentResponse
                        {
                            Success = true,
                            Message = result,
                            ToolName = "describeXray",
                            Data = result
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new AgentResponse { Success = false, Error = ex.Message };
                }
            }

            try
            {
                // Define messages and explicit tool schemas so the model knows parameters
                RequestBodyToAPI requestBody = new RequestBodyToAPI
                {
                    messages = new object[]
                    {
                        new {
                            role = "system",
                            content = @"
Bạn là một trợ lý hỗ trợ chẩn đoán y tế, chuyên về hệ cơ xương. 
Mục tiêu của bạn là hỗ trợ bác sĩ bằng cách:

1. CHẨN ĐOÁN TỪ HÌNH ẢNH: Phân tích phim X-quang cơ xương và đưa ra mô tả chi tiết
   - Gọi 'describeXray' khi người dùng tải ảnh hoặc yêu cầu mô tả
   - Trả về: các phát hiện, dấu hiệu bệnh lý, mức độ nghiêm trọng

2. TÌM KIẾM BỆNH THEO BIỂU HIỆN: Đối sánh triệu chứng/biểu hiện với cơ sở dữ liệu bệnh lý
   - Khi người dùng mô tả triệu chứng (đau, sưng, khó vận động, v.v.), tìm kiếm bệnh phù hợp
   - Trả về: top-3 bệnh có khả năng cao, điểm phù hợp, phương pháp điều trị

3. CẤP THÔNG TIN BỆNH: Cung cấp thông tin chi tiết về một bệnh cụ thể
   - Mức độ, triệu chứng chính, phương pháp điều trị, thời gian điều trị

HƯỚNG DẪN:
- Luôn giữ tinh thần hỗ trợ y bác sĩ, không thay thế chẩn đoán lâm sàng
- Khi gọi tool explainFinding, giải thích chi tiết về phát hiện cụ thể
- Trả lời bằng tiếng Việt, rõ ràng, dễ đọc
- Chỉ trả lời về X-quang/chẩn đoán hình ảnh cơ xương; từ chối các chủ đề khác một cách lịch sự
                            "
                        },
                        new { role = "user", content = userInput }
                    },
                    tools = new object[]
                    {
                        new {
                            type = "function",
                            function = new {
                                name = "describeXray",
                                description = "Phân tích phim X-quang cơ xương: mô tả phát hiện, dấu hiệu bệnh lý, mức độ chi tiết",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        imageUrl = new { type = "string" }
                                    },
                                    required = new[] { "imageUrl" }
                                }
                            }
                        },
                        new {
                            type = "function",
                            function = new {
                                name = "explainFinding",
                                description = "Giải thích chi tiết một phát hiện cụ thể trên X-quang: nguyên nhân, ý nghĩa lâm sàng, liên quan bệnh lý nào",
                                parameters = new {
                                    type = "object",
                                    properties = new {
                                        finding = new { type = "string" }
                                    },
                                    required = new[] { "finding" }
                                }
                            }
                        }
                    }
                };

                // Step 1: Trích xuất các đặc trưng từ câu hỏi
                var features = ExtractFeatures(userInput);

                // Step 2: Phân tích ý định dựa trên các đặc trưng
                var userIntent = AnalyzeUserIntent(features);

                // Nếu người dùng hỏi về bệnh lý cụ thể
                if (userIntent == UserIntent.GetDiseaseInfo)
                {
                    var result = await GetDiseaseInfo(userInput);
                    if (result.Success)
                        return result;
                }

                // Nếu người dùng hỏi về biểu hiện/triệu chứng
                if (userIntent == UserIntent.SearchBySymptoms)
                {
                    try
                    {
                        var result = await SearchDiseaseBySymptoms(userInput);
                        if (result.Success)
                            return result;
                    }
                    catch { /* ignore and continue to normal flow */ }
                }
                var apiDoc = await SendToChatAPI(requestBody);
                var message = apiDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message");

                if (message.TryGetProperty("tool_calls", out var toolCalls) &&
                    toolCalls.GetArrayLength() > 0)
                {
                    var tool = toolCalls[0];

                    var functionName = tool.GetProperty("function")
                        .GetProperty("name").GetString();

                    var argsJson = tool.GetProperty("function")
                        .GetProperty("arguments").GetString();

                    var toolCallId = tool.GetProperty("id").GetString();

                    if (ToolHandlers.TryGetValue(functionName, out var handler))
                    {
                        var result = await handler(argsJson);

                        return await SendToolResult(
                            userInput,
                            result,
                            toolCallId,
                            functionName,
                            argsJson
                        );
                    }

                    return new AgentResponse
                    {
                        Success = false,
                        Error = $"Tool {functionName} không tồn tại"
                    };
                }

                // 🧠 Normal response
                return new AgentResponse
                {
                    Success = true,
                    Message = message.GetProperty("content").GetString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Trích xuất các đặc trưng từ câu hỏi của người dùng
        /// </summary>
        private InputFeatures ExtractFeatures(string userInput)
        {
            var features = new InputFeatures();

            if (string.IsNullOrWhiteSpace(userInput))
                return features;

            var lowerInput = userInput.ToLowerInvariant();

            // Danh sách từ khóa triệu chứng
            var symptomTriggers = new[] { 
                "triệu chứng", "biểu hiện", "dấu hiệu", 
                "đau", "sưng", "khó chịu", "cứng", "tê", "mỏi", "nóng rát",
                "khó vận động", "giảm vận động", "hạn chế vận động", "không thể vận động",
                "yếu", "chảy máu", "cơm máu", "bầm tím", "phù", "nóng", "sốt",
                "gãy", "trật khớp", "thoái hóa", "viêm", "nhiễm", "áp xe"
            };

            // ===== TRÍCH XUẤT 1: Kiểm tra pattern "... là biểu hiện/dấu hiệu của bệnh gì?" =====
            // Pattern linh hoạt: support cả có/không có "của", "gì"/"nào", "nàO"
            var symptomQuestionPatterns = new[] { 
                // Strict patterns (with "của")
                "là biểu hiện của bệnh",
                "là dấu hiệu của bệnh",
                "là triệu chứng của bệnh",

                // Flexible patterns (without "của") - for "là biểu hiện bệnh gì?"
                "là biểu hiện bệnh",
                "là dấu hiệu bệnh",
                "là triệu chứng bệnh",

                // Disease asking patterns
                "bệnh gì có biểu hiện",
                "bệnh nào có biểu hiện",
                "bệnh gì có triệu chứng",
                "bệnh nào có triệu chứng",
                "bệnh gì có dấu hiệu",
                "bệnh nào có dấu hiệu",

                // Reverse patterns
                "biểu hiện của bệnh gì",
                "dấu hiệu của bệnh gì",
                "triệu chứng của bệnh gì",

                // Generic symptom question
                "là bệnh gì"  // Context: "đau ... là bệnh gì" (when symptoms mentioned)
            };
            features.IsSymptomQuestionPattern = symptomQuestionPatterns.Any(p => lowerInput.Contains(p));

            // ===== TRÍCH XUẤT 2: Kiểm tra triệu chứng =====
            var foundSymptoms = new List<string>();
            foreach (var symptom in symptomTriggers)
            {
                if (lowerInput.Contains(symptom))
                {
                    foundSymptoms.Add(symptom);
                }
            }
            features.ExtractedSymptoms = foundSymptoms;
            features.HasSymptoms = foundSymptoms.Count > 0;

            // ===== TRÍCH XUẤT 3: Kiểm tra tên bệnh (pattern "bệnh <tên>") =====
            var diseaseNameMatch = Regex.Match(lowerInput, "\\bbệnh\\s+(.+?)(?:[?!,]|$)", RegexOptions.IgnoreCase);
            if (diseaseNameMatch.Success)
            {
                var diseaseName = diseaseNameMatch.Groups[1].Value.Trim();

                // Loại bỏ nếu là question word (gì, nào, nàO, ai, cái gì, ...)
                // Vì "bệnh gì" không phải tên bệnh cụ thể
                var questionWords = new[] { "gì", "nào", "nàO", "ai", "cái gì", "cái nào", "chi" };
                bool isQuestionWord = questionWords.Any(qw => diseaseName.Equals(qw, StringComparison.OrdinalIgnoreCase) 
                                                               || diseaseName.StartsWith(qw + " ", StringComparison.OrdinalIgnoreCase));

                if (!isQuestionWord)
                {
                    // Valid disease name
                    features.IsDiseaseNamePattern = true;
                    features.ExtractedDiseaseName = diseaseName;
                }
            }

            // ===== TRÍCH XUẤT 4: Kiểm tra câu hỏi tổng quát =====
            // NHƯNG: "là gì" trong context "là biểu hiện bệnh gì" không phải general question
            // Nên check ngoại lệ: nếu IsSymptomQuestionPattern=true → skip IsGeneralQuestion
            var generalQuestionPatterns = new[] { "thông tin", "chi tiết", "là gì", "khác gì", "nó là gì" };
            features.IsGeneralQuestion = !features.IsSymptomQuestionPattern 
                                        && generalQuestionPatterns.Any(p => lowerInput.Contains(p));

            return features;
        }

        /// <summary>
        /// Phân tích ý định người dùng dựa trên các đặc trưng trích xuất
        /// </summary>
        private UserIntent AnalyzeUserIntent(InputFeatures features)
        {
            // ===== LOGIC PHÂN TÍCH =====
            // Priority 1: Pattern "... là biểu hiện của bệnh gì?" → SearchBySymptoms
            if (features.IsSymptomQuestionPattern)
                return UserIntent.SearchBySymptoms;

            // Priority 2: Có tên bệnh cụ thể → GetDiseaseInfo (cao nhất nếu người dùng chỉ định bệnh)
            if (features.IsDiseaseNamePattern && !string.IsNullOrEmpty(features.ExtractedDiseaseName))
                return UserIntent.GetDiseaseInfo;

            // Priority 3: Có triệu chứng nhưng KHÔNG chỉ định bệnh cụ thể → SearchBySymptoms
            if (features.HasSymptoms && !features.IsDiseaseNamePattern)
                return UserIntent.SearchBySymptoms;

            // Priority 4: Câu hỏi tổng quát về bệnh (không có triệu chứng) → GetDiseaseInfo
            if (features.IsGeneralQuestion && !features.HasSymptoms)
                return UserIntent.GetDiseaseInfo;

            return UserIntent.Unknown;
        }

        /// <summary>
        /// Lấy thông tin bệnh lý cụ thể
        /// </summary>
        private async Task<AgentResponse> GetDiseaseInfo(string userInput)
        {
            // Trích xuất tên bệnh từ câu hỏi
            string ExtractDiseaseName(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                var m = Regex.Match(s, "\\bbệnh\\s+(.+?)(?:[\\.\\?,!]|$)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var name = m.Groups[1].Value.Trim();
                    name = Regex.Replace(name, "^(là|về|cho tôi biết về|cho tôi biết|về bệnh)\\s+", "", RegexOptions.IgnoreCase).Trim();
                    return name;
                }
                return "";
            }

            var candidate = ExtractDiseaseName(userInput);
            Disease? maybe = null;
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                maybe = _store.FindByEmbedding(candidate, 0.25);
            }

            // fallback: try embedding on the whole input or legacy text search
            if (maybe == null)
            {
                maybe = _store.FindByEmbedding(userInput, 0.4) ?? _store.FindByName(userInput);
            }

            if (maybe != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("📋 THÔNG TIN BỆNH LÝ\n");
                sb.AppendLine($"Tên bệnh: {maybe.DiseaseName}");
                sb.AppendLine($"Mã bệnh: {maybe.DiseaseId}\n");
                sb.AppendLine("═══════════════════════════════════════\n");

                sb.AppendLine($"🔹 Mức độ: {GetSeverityText(maybe.Severity)}");
                sb.AppendLine($"🔹 Mức độ khẩn cấp: {GetEmergencyText(maybe.EmergencyLevel)}\n");

                if (maybe.TreatmentMethod != null && maybe.TreatmentMethod.Any())
                {
                    sb.AppendLine("💊 Phương pháp điều trị:");
                    foreach (var method in maybe.TreatmentMethod)
                    {
                        sb.AppendLine($"  • {method}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine($"⏱️ Thời gian điều trị: {maybe.TreatmentDuration}\n");

                if (maybe.XrayFindings != null && maybe.XrayFindings.Any())
                {
                    sb.AppendLine("🔬 Dấu hiệu trên X-quang:");
                    foreach (var finding in maybe.XrayFindings)
                    {
                        sb.AppendLine($"  • {finding}");
                    }
                    sb.AppendLine();
                }

                if (maybe.RedFlags != null && maybe.RedFlags.Any())
                {
                    sb.AppendLine("⚠️ Dấu hiệu cảnh báo:");
                    foreach (var flag in maybe.RedFlags)
                    {
                        sb.AppendLine($"  • {flag}");
                    }
                    sb.AppendLine();
                }

                return new AgentResponse
                {
                    Success = true,
                    Message = sb.ToString()
                };
            }

            return new AgentResponse { Success = false };
        }

        /// <summary>
        /// Tìm kiếm bệnh theo biểu hiện/triệu chứng
        /// </summary>
        private async Task<AgentResponse> SearchDiseaseBySymptoms(string userInput)
        {
            // Step 1: Extract keywords from user input using LLM
            var extractedKeywords = await ExtractSymptomKeywords(userInput);

            // Step 2: Search using extracted keywords (now calculates average score per symptom)
            var top = _store.FindTopKBySymptoms(extractedKeywords, 3);
            if (top == null || top.Count == 0)
                return new AgentResponse { Success = false };

            // Step 3: Calculate individual scores for each symptom per disease for detailed display
            var symptomsList = extractedKeywords
                .Split(new[] { ',', ';', '&', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("🔍 KẾT QUẢ TÌM KIẾM BỆNH THEO BIỂU HIỆN\n");
            sb.AppendLine($"Câu hỏi: {userInput}");
            sb.AppendLine($"🔑 Từ khóa trích xuất: {extractedKeywords}");

            sb.AppendLine("\n═══════════════════════════════════════\n");

            for (int i = 0; i < top.Count; i++)
            {
                var el = top[i].Item1;
                var avgScore = top[i].Item2;
                if (avgScore <= 0) continue;
                Disease? d = null;
                try { d = JsonSerializer.Deserialize<Disease>(el.GetRawText()); } catch { }
                if (d == null) continue;

                sb.AppendLine($"【{i + 1}】 {d.DiseaseName}");
                sb.AppendLine($"├─ Độ phù hợp trung bình: {avgScore:F2} ({(avgScore >= 0.7 ? "Cao" : avgScore >= 0.5 ? "Trung bình" : "Thấp")})");

                // Show individual symptom scores
                if (symptomsList.Count > 1)
                {
                    sb.AppendLine($"├─ Trung bình: {avgScore:F2}");
                }
                else
                {
                    sb.AppendLine($"├─ Điểm số: {avgScore:F2}");
                }

                sb.AppendLine($"├─ Mức độ: {GetSeverityText(d.Severity)}");
                sb.AppendLine($"├─ Mức độ khẩn cấp: {GetEmergencyText(d.EmergencyLevel)}");

                var treat = (d.TreatmentMethod != null && d.TreatmentMethod.Count > 0) 
                    ? string.Join("; ", d.TreatmentMethod) 
                    : "Chưa có thông tin";
                sb.AppendLine($"├─ Phương pháp điều trị: {treat}");
                sb.AppendLine($"├─ Thời gian điều trị: {d.TreatmentDuration}");

                // Add X-ray findings if available
                if (d.XrayFindings != null && d.XrayFindings.Count > 0)
                {
                    sb.AppendLine($"└─ Dấu hiệu X-quang: {string.Join(", ", d.XrayFindings)}");
                }

                if (i < top.Count - 1)
                    sb.AppendLine("───────────────────────────────────────\n");
            }

            sb.AppendLine("\n⚠️ LƯU Ý: Đây chỉ là kết quả tham khảo. Cần chẩn đoán lâm sàn và X-quang để xác định chính xác.");

            return new AgentResponse
            {
                Success = true,
                Message = sb.ToString(),
                Data = top.Select(t => new { score = t.Item2, raw = t.Item1.GetRawText() }).ToList()
            };
        }
        // 🔥 Core gửi tool result (dynamic)
        public async Task<AgentResponse> SendToolResult(
            string userInput,
            string result,
            string toolCallId,
            string functionName,
            string argsJson)
        {
            try
            {
                RequestBodyToAPI requestBody = new RequestBodyToAPI
                {
                    messages = new object[]
                    {
                        new { role = "user", content = userInput },

                        new {
                            role = "assistant",
                            tool_calls = new[]
                            {
                                new {
                                    id = toolCallId,
                                    type = "function",
                                    function = new {
                                        name = functionName,
                                        arguments = argsJson
                                    }
                                }
                            }
                        },

                        new {
                            role = "tool",
                            tool_call_id = toolCallId,
                            content = result
                        }
                    }
                };
                var json = await SendToChatAPI(requestBody); 
                var message = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
                return new AgentResponse
                {
                    Success = true,
                    Message = message,
                    ToolName = functionName,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
        public async Task<JsonDocument> SendToChatAPI(RequestBodyToAPI requestBody)
        {

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions"
            );

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(content);

            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
            return doc;
        } 
        private string ParseCity(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("city").GetString() ?? "Hà Nội";
            }
            catch
            {
                return "Hà Nội";
            }
        }

        // 🔧 Helper parse JSON for imageUrl or finding
        private string ParseStringParam(string json, string prop, string defaultValue)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty(prop, out var p) ? p.GetString() ?? defaultValue : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private async Task<string> DescribeXray(string argsJson)
        {
            try
            {
                var imageBase64 = ParseStringParam(argsJson, "imageBase64", "");
                var filename = ParseStringParam(argsJson, "filename", "upload.jpg");

                if (string.IsNullOrEmpty(imageBase64))
                    return "Thiếu tham số imageBase64";

                // Strip data URL prefix if present
                var commaIndex = imageBase64.IndexOf(',');
                if (commaIndex >= 0)
                    imageBase64 = imageBase64.Substring(commaIndex + 1);

                var bytes = Convert.FromBase64String(imageBase64);

                // Save original image
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(filename) is { Length: > 0 } e ? e : ".jpg";
                var safeName = Path.GetFileNameWithoutExtension(filename);
                var unique = safeName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
                await File.WriteAllBytesAsync(Path.Combine(uploadsDir, unique), bytes);

                var dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
                var annotatedImageBase64 = dataUrl;
                var detectionSummary = "Không phát hiện tổn thương rõ ràng.";

                // Call Python detection service
                var xrayServiceUrl = Environment.GetEnvironmentVariable("XRAY_SERVICE_URL") ?? "http://localhost:8000";
                try
                {
                    using var form = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(bytes);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    form.Add(fileContent, "file", filename);

                    var detectResponse = await _http.PostAsync($"{xrayServiceUrl}/detect", form);
                    if (detectResponse.IsSuccessStatusCode)
                    {
                        var detectJson = await detectResponse.Content.ReadAsStringAsync();
                        var detectDoc = JsonDocument.Parse(detectJson);

                        if (detectDoc.RootElement.TryGetProperty("annotated_image_base64", out var annEl))
                            annotatedImageBase64 = "data:image/jpeg;base64," + (annEl.GetString() ?? "");

                        if (detectDoc.RootElement.TryGetProperty("detections", out var dets) && dets.GetArrayLength() > 0)
                        {
                            var labels = new List<string>();
                            foreach (var det in dets.EnumerateArray())
                            {
                                var label = det.TryGetProperty("label", out var l) ? l.GetString() : "unknown";
                                var conf = det.TryGetProperty("confidence", out var c) ? c.GetDouble() : 0;
                                labels.Add($"{label} ({conf:P0})");
                            }
                            detectionSummary = "Phát hiện: " + string.Join(", ", labels);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[XRAY] Python service error: {ex.Message}");
                }

                var diagnosis = await GenerateXrayDescription(detectionSummary);
                var obj = new { diagnosis, imageBase64 = dataUrl, annotatedImageBase64 };
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                return "Lỗi khi xử lý ảnh: " + ex.Message;
            }
        }

        private async Task<string> GenerateXrayDescription(string detectionSummary)
        {
            try
            {
                var prompt = $@"Bạn là bác sĩ chuyên khoa cơ xương. Dựa trên kết quả phân tích ảnh X-quang:

{detectionSummary}

Hãy viết mô tả y khoa ngắn gọn (2-3 câu) bằng tiếng Việt bao gồm:
- Loại tổn thương phát hiện (nếu có)
- Mức độ nghiêm trọng sơ bộ
- Khuyến nghị tiếp theo

Kết thúc bằng: ""Lưu ý: Đây là hỗ trợ AI, cần bác sĩ xác nhận chẩn đoán chính thức.""";

                var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                var body = JsonSerializer.Serialize(new
                {
                    model = "openai/gpt-4o-mini",
                    messages = new[] { new { role = "user", content = prompt } }
                });
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? detectionSummary;
            }
            catch
            {
                return detectionSummary;
            }
        }

        private async Task<string> ExplainFinding(string argsJson)
        {
            var finding = ParseStringParam(argsJson, "finding", "");
            if (string.IsNullOrEmpty(finding))
                return "Thiếu tham số finding";

            return $"Giải thích cho '{finding}': Điều này có thể là dấu hiệu của một đứt đoạn vỏ xương cấp tính phù hợp với gãy; kết hợp lâm sàng và cân nhắc chụp hai mặt hoặc CT nếu cần.";
        }

        /// <summary>
        /// Helper method to convert severity integer to Vietnamese text
        /// </summary>
        private string GetSeverityText(int severity)
        {
            return severity switch
            {
                0 => "Nhẹ",
                1 => "Trung bình",
                2 => "Nặng",
                3 => "Rất nặng",
                _ => "Chưa xác định"
            };
        }

        /// <summary>
        /// Helper method to convert emergency level to Vietnamese text
        /// </summary>
        private string GetEmergencyText(int emergencyLevel)
        {
            return emergencyLevel switch
            {
                0 => "Không khẩn cấp",
                1 => "Cần theo dõi",
                2 => "Cần khám sớm",
                3 => "Khẩn cấp",
                _ => "Chưa xác định"
            };
        }

        /// <summary>
        /// Extract symptom keywords from user input using LLM
        /// Example: "Tôi có các biểu hiện sau: đau háng, khó chịu khi vận động" 
        ///       -> "đau háng, khó chịu khi vận động"
        /// </summary>
        private async Task<string> ExtractSymptomKeywords(string userInput)
        {
            try
            {
                // Build request body similar to SendToChatAPI
                var extractionRequest = new RequestBodyToAPI
                {
                    messages = new object[]
                    {
                        new {
                            role = "system",
                            content = @"Bạn là trợ lý y tế chuyên trích xuất triệu chứng từ câu hỏi.
Nhiệm vụ: Trích xuất CHÍNH XÁC các triệu chứng/biểu hiện y tế từ câu hỏi người dùng.

QUY TẮC:
1. Chỉ trích xuất cụm từ triệu chứng (VD: 'đau háng', 'khó chịu khi vận động', 'sưng khớp gối')
2. Giữ NGUYÊN từ ngữ người dùng dùng, KHÔNG diễn giải
3. Loại bỏ các từ không liên quan (tôi có, bị, các biểu hiện, v.v.)
4. Trả về danh sách triệu chứng cách nhau bởi dấu phẩy

VÍ DỤ:
Input: 'Tôi có các biểu hiện sau: đau háng, khó chịu khi vận động'
Output: 'đau háng, khó chịu khi vận động'

Input: 'Bệnh nhân bị đau khớp gối buổi sáng và cứng khớp'
Output: 'đau khớp gối buổi sáng, cứng khớp'

Chỉ trả về danh sách triệu chứng, không giải thích."
                        },
                        new {
                            role = "user",
                            content = userInput
                        }
                    }
                };

                var json = JsonSerializer.Serialize(extractionRequest);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions"
                );

                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(content);

                var extractedText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? userInput;

                // Clean up the response
                extractedText = extractedText.Trim().Trim('"', '\'', '.', '\n', '\r');

                return string.IsNullOrWhiteSpace(extractedText) ? userInput : extractedText;
            }
            catch (Exception ex)
            {
                // Fallback to original input if extraction fails
                Console.WriteLine($"Keyword extraction failed: {ex.Message}");
                return userInput;
            }
        }
    }
}