# OpenRouter Medical Diagnosis Chatbot

## 🎯 Tổng quan

Chatbot AI hỗ trợ chẩn đoán y tế chuyên về hệ cơ xương, sử dụng **RAG (Retrieval-Augmented Generation)** và **Embedding Similarity** để:

1. **Chẩn đoán từ hình ảnh X-quang** - Phân tích và mô tả phát hiện trên phim X-quang
2. **Tìm kiếm bệnh theo triệu chứng** - Đối sánh triệu chứng với cơ sở dữ liệu bệnh lý
3. **Cung cấp thông tin bệnh** - Tra cứu thông tin chi tiết về bệnh cụ thể

## 🚀 Các tính năng chính

### 1. Chẩn đoán từ X-quang
- Upload ảnh X-quang → AI phân tích và mô tả
- Tự động annotate các vùng bất thường
- Gợi ý bệnh lý có thể có

### 2. RAG Symptom Search (Tìm kiếm theo triệu chứng)

**Quy trình:**
```
User Input → Keyword Extraction (LLM) → Embedding Similarity → Top-3 Diseases
```

**Ví dụ:**
```
Input: "Tôi có các biểu hiện: đau háng, khó chịu khi vận động"
	 ↓
Keywords: "đau háng, khó chịu khi vận động"
	 ↓
Results:
  1. Viêm khớp háng (score: 0.87)
  2. Thoái hóa khớp háng (score: 0.72)
  3. Rách labrum háng (score: 0.58)
```

### 3. Tra cứu thông tin bệnh
```
Input: "Cho tôi biết về bệnh Thoái hóa khớp gối"
	 ↓
Output:
  📋 Tên bệnh, mã bệnh
  🔹 Mức độ nghiêm trọng
  💊 Phương pháp điều trị
  ⏱️ Thời gian điều trị
  🔬 Dấu hiệu X-quang
  ⚠️ Dấu hiệu cảnh báo
```

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                        Web UI (index.html)                   │
│  - Chat interface                                            │
│  - Image upload                                              │
│  - Annotated image display                                   │
└────────────────────┬────────────────────────────────────────┘
					 │
					 ↓
┌─────────────────────────────────────────────────────────────┐
│                   API Controller (Program.cs)                │
│  - /api/chat (POST)                                          │
│  - /api/diseases/* (CRUD)                                    │
└────────────────────┬────────────────────────────────────────┘
					 │
					 ↓
┌─────────────────────────────────────────────────────────────┐
│                      Handle.cs (Agent)                       │
│  - AskAsync()           : Main orchestrator                  │
│  - ExtractSymptomKeywords() : LLM keyword extraction         │
│  - DescribeXray()       : X-ray analysis                     │
│  - ExplainFinding()     : Finding explanation                │
└────────────────────┬────────────────────────────────────────┘
					 │
					 ↓
┌─────────────────────────────────────────────────────────────┐
│                  DiseaseStore.cs (RAG Engine)                │
│  - FindTopKBySymptoms() : Embedding similarity search        │
│  - ComputeEmbedding()   : 256-dim embedding                  │
│  - BuildTextForEmbedding() : Concat disease + keywords       │
└────────────────────┬────────────────────────────────────────┘
					 │
					 ↓
┌─────────────────────────────────────────────────────────────┐
│              data/diseases.json (Knowledge Base)             │
│  - disease_id, disease_name                                  │
│  - confidence_keywords (primary, secondary, symptoms)        │
│  - xray_findings, treatment_method                           │
│  - embedding (256 dimensions)                                │
└─────────────────────────────────────────────────────────────┘
```

## 📂 Cấu trúc thư mục

```
OpenRouterClient/
├── Chatbot/
│   ├── Handle.cs                  # Agent chính
│   ├── DiseaseStore.cs            # RAG engine
│   ├── Models/
│   │   ├── Disease.cs             # Disease schema
│   │   └── DiseaseMatch.cs        # Search result DTO
│   ├── Services/
│   │   ├── IDiseaseService.cs     # Service interface
│   │   └── DiseaseService.cs      # Service implementation
│   ├── Controllers/
│   │   └── DiseasesController.cs  # CRUD API
│   ├── data/
│   │   └── diseases.json          # Knowledge base
│   └── wwwroot/
│       ├── index.html             # Chat UI
│       └── admin.html             # Admin CRUD UI
├── docs/
│   ├── RAG-Symptom-Search.md      # RAG documentation
│   └── RAG-Test-Cases.md          # Test cases
└── README.md                      # This file
```

## 🛠️ Cài đặt và chạy

### Yêu cầu
- .NET 10 SDK
- OpenRouter API Key

### Cấu hình

1. Tạo file `appsettings.json`:
```json
{
  "OpenRouterApiKey": "your-api-key-here"
}
```

2. Build và chạy:
```bash
dotnet build
dotnet run --project Chatbot
```

3. Mở trình duyệt:
- Chat UI: `http://localhost:5000`
- Admin UI: `http://localhost:5000/admin.html`

## 📖 Cách sử dụng

### 1. Chat với AI

**Upload X-ray:**
```
1. Click nút "📎 Tải ảnh X-quang"
2. Chọn file ảnh
3. AI sẽ tự động phân tích
```

**Hỏi về triệu chứng:**
```
"Tôi có các biểu hiện: đau háng, khó chịu khi vận động"
```

**Tra cứu bệnh:**
```
"Cho tôi biết về bệnh Thoái hóa khớp gối"
```

### 2. Quản lý cơ sở dữ liệu bệnh

Mở `admin.html` để:
- ✅ Thêm bệnh mới
- ✏️ Sửa thông tin bệnh
- 🗑️ Xóa bệnh
- 📤 Import batch từ JSON

## 🔬 Công nghệ sử dụng

- **Framework**: ASP.NET Core 10 (Minimal API + Controllers)
- **LLM**: OpenRouter (Gemini 2.0 Flash)
- **Embedding**: Custom 256-dim embedding
- **Storage**: File-based JSON (diseases.json)
- **UI**: Vanilla JavaScript + HTML/CSS

## 📊 Hiệu năng

- Keyword extraction: ~500ms (LLM call)
- Embedding search: ~10ms (local vector similarity)
- X-ray analysis: ~2-3s (LLM + image processing)

## ⚠️ Lưu ý quan trọng

⚠️ **Đây là công cụ hỗ trợ, KHÔNG THAY THẾ chẩn đoán lâm sàng của bác sĩ!**

- Kết quả chỉ mang tính tham khảo
- Luôn cần xác nhận bằng khám lâm sàng và các xét nghiệm bổ sung
- Không sử dụng cho trường hợp cấp cứu

## 🎓 Tài liệu tham khảo

- [RAG Symptom Search Documentation](docs/RAG-Symptom-Search.md)
- [Test Cases](docs/RAG-Test-Cases.md)

## 📝 License

MIT License

## 👥 Contributors

- Your Name - Initial work

## 🆘 Support

Nếu gặp vấn đề, vui lòng tạo issue trên GitHub.
