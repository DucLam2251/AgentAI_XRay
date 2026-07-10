using Chatbot.Models;
using System.Collections.Generic;

namespace Chatbot.Data
{
    public class MusculoskeletalDiseasesListVietnamese
    {
        public static List<Disease> GetMusculoskeletalDiseasesVietnamese()
        {
            return new List<Disease>
            {
                // ===== GẤP NỨT XƯƠNG =====
                new Disease
                {
                    DiseaseId = "FRAC_001",
                    DiseaseName = "Gãy Xương Đùi",
                    Severity = 9,
                    EmergencyLevel = 9,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Cố định", "Phẫu thuật", "Vật lý trị liệu" },
                    XrayFindings = new List<string> { "Vết nứt thân xương đùi", "Dislocation", "Góc cong", "Mảnh xương" },
                    RedFlags = new List<string> { "Đau nặng", "Biến dạng chi", "Sưng", "Mất chức năng" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương đùi", "gãy đùi" },
                        Secondary = new List<string> { "thân xương đùi", "xương đùi gần khớp", "xương đùi xa khớp" },
                        Symptoms = new List<string> { "đau đùi nặng", "không thể gánh vác", "sưng đùi", "bầm tím" },
                        Negative = new List<string> { "ảnh X-quang bình thường", "không dislocation" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_002",
                    DiseaseName = "Gãy Xương Chày",
                    Severity = 8,
                    EmergencyLevel = 8,
                    TreatmentDuration = "12-20 tuần",
                    TreatmentMethod = new List<string> { "Bó bột", "Phẫu thuật cố định", "Cột nẹp trong xương" },
                    XrayFindings = new List<string> { "Vết nứt xương chày", "Gãy ngang/xiên/vỡ vụn", "Xương mác liên quan", "Mất căn chỉnh" },
                    RedFlags = new List<string> { "Gãy hở", "Sưng nặng", "Rối loạn mạch máu", "Hội chứng khoang" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương chày", "gãy chày" },
                        Secondary = new List<string> { "thân xương chày", "mặt trên xương chày", "mặt dưới xương chày" },
                        Symptoms = new List<string> { "đau chày", "sưng chân", "không thể đi bộ", "biến dạng" },
                        Negative = new List<string> { "xương mác nguyên vẹn", "không cong" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_003",
                    DiseaseName = "Gãy Xương Mác",
                    Severity = 6,
                    EmergencyLevel = 6,
                    TreatmentDuration = "8-12 tuần",
                    TreatmentMethod = new List<string> { "Quản lý bảo tồn", "Nẹp", "Thay đổi hoạt động" },
                    XrayFindings = new List<string> { "Vết nứt xương mác", "Gãy một xương", "Xương mác xa hay gần khớp" },
                    RedFlags = new List<string> { "Không ổn định khớp cổ chân", "Gãy xương chày liên quan", "Chấn thương liên khớp" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương mác", "gãy mác" },
                        Secondary = new List<string> { "mác xa khớp", "mác gần khớp", "mũi trong" },
                        Symptoms = new List<string> { "đau cạnh ngoài chân", "sưng cổ chân", "khó đi bộ" },
                        Negative = new List<string> { "xương chày nguyên vẹn", "khớp bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_004",
                    DiseaseName = "Gãy Xương Cánh Tay Trên",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "8-12 tuần",
                    TreatmentMethod = new List<string> { "Bó bột, Phẫu thuật cố định", "Cột nẹp trong xương" },
                    XrayFindings = new List<string> { "Vết nứt xương cánh tay trên", "Dislocation", "Vỡ vụn", "Dấu hiệu thần kinh" },
                    RedFlags = new List<string> { "Liệt thần kinh xương bán kính", "Chấn thương mạch máu", "Gãy hở" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương cánh tay trên", "gãy cánh tay" },
                        Secondary = new List<string> { "xương cánh tay gần khớp", "xa khớp", "thân xương" },
                        Symptoms = new List<string> { "đau cánh tay", "sưng vai", "không thể cử động", "tay cứng cót" },
                        Negative = new List<string> { "mạch máu bình thường", "thần kinh bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_005",
                    DiseaseName = "Gãy Xương Bán Kính",
                    Severity = 6,
                    EmergencyLevel = 6,
                    TreatmentDuration = "8-10 tuần",
                    TreatmentMethod = new List<string> { "Bó bột", "Phẫu thuật cố định", "Bản cố định" },
                    XrayFindings = new List<string> { "Vết nứt bán kính", "Gãy xa hay gần khớp", "Mất khả năng xoay" },
                    RedFlags = new List<string> { "Gãy xương quanh liên quan", "Chấn thương thần kinh xung quanh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương bán kính" },
                        Secondary = new List<string> { "bán kính xa khớp", "gần khớp", "thân xương" },
                        Symptoms = new List<string> { "đau cánh tay dưới", "sưng cổ tay", "giảm xoay", "giảm pronation" },
                        Negative = new List<string> { "xương quanh bình thường", "cảm giác tay bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_006",
                    DiseaseName = "Gãy Xương Quanh",
                    Severity = 6,
                    EmergencyLevel = 6,
                    TreatmentDuration = "8-10 tuần",
                    TreatmentMethod = new List<string> { "Bó bột", "Phẫu thuật cố định", "Bản cố định" },
                    XrayFindings = new List<string> { "Vết nứt xương quanh", "Gãy xa hay gần khớp", "Mô hình Monteggia" },
                    RedFlags = new List<string> { "Gãy bán kính dislocation liên quan", "Chấn thương thần kinh liên khớp trước" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương quanh" },
                        Secondary = new List<string> { "quanh xa khớp", "gần khớp", "olecranon" },
                        Symptoms = new List<string> { "đau cánh tay dưới", "sưng khuỷu", "giảm gập khuỷu" },
                        Negative = new List<string> { "bán kính bình thường", "chức năng bàn tay bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_007",
                    DiseaseName = "Gãy Xương Sống Lưng",
                    Severity = 8,
                    EmergencyLevel = 9,
                    TreatmentDuration = "12-24 tuần",
                    TreatmentMethod = new List<string> { "Bó cứng", "Phẫu thuật cố định", "Liên kết", "Phục hồi thần kinh" },
                    XrayFindings = new List<string> { "Vết nứt thân xương", "Vỡ vụn", "Nén", "Đẩy lùi", "Hẹp ống sống" },
                    RedFlags = new List<string> { "Tổn thương tủy sống", "Rối loạn thần kinh", "Gãy không ổn định", "Hội chứng cauda equina" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương sống", "gãy sống lưng" },
                        Secondary = new List<string> { "gãy lưng dưới", "gãy lưng giữa", "gãy cổ" },
                        Symptoms = new List<string> { "đau lưng", "liệt", "tê numb", "mất cảm giác", "mất kiểm soát ruột/bàng quang" },
                        Negative = new List<string> { "cơ sống bình thường", "không hẹp ống sống" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_008",
                    DiseaseName = "Gãy Xương Chậu",
                    Severity = 8,
                    EmergencyLevel = 8,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Dây buộc chậu", "Phẫu thuật cố định", "Cố định nội tạng" },
                    XrayFindings = new List<string> { "Vết nứt vòng chậu", "Gãy nhiều", "Dislocation", "Không ổn định" },
                    RedFlags = new List<string> { "Chấn thương tiết niệu", "Chảy máu", "Chấn thương thần kinh", "Chấn thương liên quan" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy chậu", "gãy xương chậu" },
                        Secondary = new List<string> { "gãy xương mu", "xương ngồi", "xương chậu", "gãy ổ cấp" },
                        Symptoms = new List<string> { "đau chậu", "đau háng", "khó đi bộ", "rối loạn tiểu", "chảy máu trực tràng" },
                        Negative = new List<string> { "cơ chậu bình thường", "chậu ổn định" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_009",
                    DiseaseName = "Gãy Cổ Chân Hai Mũi",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "12-14 tuần",
                    TreatmentMethod = new List<string> { "Phẫu thuật cố định", "Bản và vít cố định", "Phục hồi sau phẫu thuật" },
                    XrayFindings = new List<string> { "Gãy mũi trong", "Gãy mũi ngoài", "Rộng cổ chân", "Chấn thương liên khớp" },
                    RedFlags = new List<string> { "Gãy hở", "Rối loạn mạch máu", "Chấn thương liên khớp", "Liên quan mũi sau" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy cổ chân", "gãy hai mũi", "gãy cổ chân" },
                        Secondary = new List<string> { "mũi trong", "mũi ngoài", "chấn thương liên khớp" },
                        Symptoms = new List<string> { "đau cổ chân", "sưng cổ chân", "không gánh vác", "biến dạng cổ chân" },
                        Negative = new List<string> { "căn chỉnh bình thường", "liên khớp nguyên vẹn" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_010",
                    DiseaseName = "Gãy Cổ Chân Ba Mũi",
                    Severity = 8,
                    EmergencyLevel = 8,
                    TreatmentDuration = "14-16 tuần",
                    TreatmentMethod = new List<string> { "Phẫu thuật cố định ba", "Phục hồi" },
                    XrayFindings = new List<string> { "Gãy mũi trong", "Gãy mũi ngoài", "Gãy mũi sau", "Không ổn định cổ chân" },
                    RedFlags = new List<string> { "Gãy hở", "Chấn thương mạch máu", "Dislocation nặng", "Rối loạn mạch-thần kinh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy ba mũi", "cổ chân ba phần" },
                        Secondary = new List<string> { "mũi sau", "mũi trong", "mũi ngoài" },
                        Symptoms = new List<string> { "đau cổ chân nặng", "sưng nặng", "biến dạng", "rối loạn mạch" },
                        Negative = new List<string> { "không rối loạn mạch" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_011",
                    DiseaseName = "Gãy Xương Gót Chân",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Điều trị bảo tồn", "Phẫu thuật cố định", "Cố định xuyên da", "Bó bột" },
                    XrayFindings = new List<string> { "Vỡ xương gót", "Mất góc Böhler", "Vỡ vụn", "Liên quan khớp dưới cótali" },
                    RedFlags = new List<string> { "Gãy trong khớp", "Vỡ vụn nặng", "Mất chiều cao", "Rối loạn mạch máu" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương gót", "gãy gót chân" },
                        Secondary = new List<string> { "khớp dưới talus", "góc Böhler", "trong khớp" },
                        Symptoms = new List<string> { "đau gót", "sưng gót", "khó đi bộ", "mất chiều cao" },
                        Negative = new List<string> { "góc Böhler bình thường", "ngoài khớp" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_012",
                    DiseaseName = "Gãy Xương Cộc",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Phẫu thuật cố định", "Cố định vít", "Bó bột" },
                    XrayFindings = new List<string> { "Vết nứt cộc", "Dislocation", "Dislocation khớp dưới", "Dấu hiệu rối loạn mạch" },
                    RedFlags = new List<string> { "Gãy cổ cộc", "Rủi ro hoại tử", "Dislocation", "Chấn thương mạch máu" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương cộc" },
                        Secondary = new List<string> { "cổ cộc", "thân cộc", "mặt cộc" },
                        Symptoms = new List<string> { "đau cổ chân", "sưng cổ chân", "giảm cử động cổ chân" },
                        Negative = new List<string> { "mạch máu bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_013",
                    DiseaseName = "Gãy Xương Trung Chân",
                    Severity = 5,
                    EmergencyLevel = 5,
                    TreatmentDuration = "6-8 tuần",
                    TreatmentMethod = new List<string> { "Giày cứng", "Dán", "Quản lý bảo tồn", "Phẫu thuật nếu dislocation" },
                    XrayFindings = new List<string> { "Vết nứt xương trung chân", "Gãy ngang", "Liên quan Lisfranc", "Dislocation" },
                    RedFlags = new List<string> { "Chấn thương Lisfranc", "Gãy nhiều", "Dislocation", "Chấn thương nặn" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương trung chân", "gãy chân" },
                        Secondary = new List<string> { "xương trung chân thứ nhất", "thứ năm", "gãy căng", "gãy Jones" },
                        Symptoms = new List<string> { "đau chân", "sưng chân", "khó đi bộ", "biến dạng chân" },
                        Negative = new List<string> { "không dislocation", "gãy riêng lẻ" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_014",
                    DiseaseName = "Gãy Xương Thuyền",
                    Severity = 6,
                    EmergencyLevel = 5,
                    TreatmentDuration = "8-12 tuần",
                    TreatmentMethod = new List<string> { "Bó bột ngón cái", "Cố định", "Phẫu thuật nếu dislocation", "Cố định vít" },
                    XrayFindings = new List<string> { "Vỡ xương thuyền", "Gãy thắt", "Gãy cực gần", "Hoại tử", "Dấu hiệu hoại tử" },
                    RedFlags = new List<string> { "Dislocation", "Gãy cực gần", "Chẩn đoán muộn", "Hoại tử" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương thuyền", "gãy cổ tay" },
                        Secondary = new List<string> { "xương thuyền thắt", "cực gần", "cực xa" },
                        Symptoms = new List<string> { "đau cổ tay", "sưng hang phía trong cổ tay", "sưng cổ tay", "giảm sức nắm" },
                        Negative = new List<string> { "không dislocation", "mạch máu bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_015",
                    DiseaseName = "Gãy Xương Sườn",
                    Severity = 6,
                    EmergencyLevel = 6,
                    TreatmentDuration = "6-8 tuần",
                    TreatmentMethod = new List<string> { "Kiểm soát đau", "Tập thở", "Dán (hạn chế)", "Hỗ trợ ho" },
                    XrayFindings = new List<string> { "Đường gãy sườn", "Vị trí gãy nhiều", "Mô hình flail chest", "Liên quan màng phổi" },
                    RedFlags = new List<string> { "Flail chest", "Hemothorax", "Pneumothorax", "Bầm phổi", "Chấn thương cơ quan nội tạng" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy sườn", "gãy vành xương" },
                        Secondary = new List<string> { "flail chest", "nhiều sườn", "sườn đầu", "sườn dưới" },
                        Symptoms = new List<string> { "đau thành ngực", "đau khi thở", "đau khi ho", "bầm tím" },
                        Negative = new List<string> { "thành ngực bình thường", "không pneumothorax", "không hemothorax" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_016",
                    DiseaseName = "Gãy Xương Đòn",
                    Severity = 5,
                    EmergencyLevel = 5,
                    TreatmentDuration = "6-12 tuần",
                    TreatmentMethod = new List<string> { "Bó lót", "Bó hình 8", "Phẫu thuật nếu dislocation", "Vật lý trị liệu" },
                    XrayFindings = new List<string> { "Vỡ xương đòn", "Gãy gần/giữa/xa", "Dislocation", "Không liên kết" },
                    RedFlags = new List<string> { "Dislocation >2cm", "Gãy đòn xa", "Rối loạn mạch máu", "Gãy hở" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương đòn", "gãy xương quai vai" },
                        Secondary = new List<string> { "đòn gần", "đòn giữa", "đòn xa", "đòn xa" },
                        Symptoms = new List<string> { "đau vai", "sưng xương đòn", "biến dạng", "giảm cử động cánh tay" },
                        Negative = new List<string> { "dislocation tối thiểu", "không vỡ vụn" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_017",
                    DiseaseName = "Gãy Xương Lưỡi vai",
                    Severity = 6,
                    EmergencyLevel = 6,
                    TreatmentDuration = "8-12 tuần",
                    TreatmentMethod = new List<string> { "Bó lót", "Vật lý trị liệu", "Phẫu thuật nếu dislocation" },
                    XrayFindings = new List<string> { "Vỡ xương lưỡi vai", "Gãy thân", "Gãy hõm", "Dislocation liên quan" },
                    RedFlags = new List<string> { "Liên quan hõm", "Dislocation >10mm", "Chấn thương liên quan", "Chấn thương cao năng lượng" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy xương lưỡi vai", "gãy phần vai" },
                        Secondary = new List<string> { "thân lưỡi vai", "gãy hõm", "cổ lưỡi vai", "acromion" },
                        Symptoms = new List<string> { "đau vai", "sưng vai", "giảm cử động vai", "biến dạng lưỡi vai" },
                        Negative = new List<string> { "dislocation tối thiểu", "không liên quan hõm" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_018",
                    DiseaseName = "Gãy Xương Bánh Chè Đầu Gối",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Cố định", "Phẫu thuật cố định", "Dây cáp", "Cắt bánh chè một phần" },
                    XrayFindings = new List<string> { "Vỡ bánh chè", "Gãy ngang", "Vỡ vụn", "Dislocation", "Gãy cơ duỗi" },
                    RedFlags = new List<string> { "Gãy cơ duỗi", "Dislocation >3mm", "Gãy vỡ vụn", "Chấn thương liên quan" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy bánh chè", "gãy xương bánh chè" },
                        Secondary = new List<string> { "gãy ngang", "vỡ vụn", "gãy nứt dọc", "gãy sao" },
                        Symptoms = new List<string> { "đau đầu gối", "sưng đầu gối", "không thể duỗi đầu gối", "tích máu trong khớp" },
                        Negative = new List<string> { "cơ duỗi nguyên vẹn", "không dislocation" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_019",
                    DiseaseName = "Gãy Xương Ổ Cấp",
                    Severity = 8,
                    EmergencyLevel = 8,
                    TreatmentDuration = "12-20 tuần",
                    TreatmentMethod = new List<string> { "Kéo", "Phẫu thuật cố định", "Bản cố định", "Cố định háng" },
                    XrayFindings = new List<string> { "Vỡ ổ cấp", "Gãy cột", "Gãy hình T", "Gãy cả cột", "Dislocation háng" },
                    RedFlags = new List<string> { "Dislocation háng", "Gãy trong khớp", "Chấn thương mạch máu", "Liệt thần kinh坐骨", "Rối loạn mạch-thần kinh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy ổ cấp", "gãy ổ cấp" },
                        Secondary = new List<string> { "cột trước", "cột sau", "cả cột", "hình T" },
                        Symptoms = new List<string> { "đau háng", "sưng háng", "không thể cử động háng", "không cân bằng chi", "triệu chứng thần kinh坐骨" },
                        Negative = new List<string> { "không dislocation", "háng ổn định" }
                    }
                },
                new Disease
                {
                    DiseaseId = "FRAC_020",
                    DiseaseName = "Gãy Xương Cánh Tay Trên Gần Khớp",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Bó lót", "Vật lý trị liệu", "Phẫu thuật cố định", "Thay khớp vai đảo chiều" },
                    XrayFindings = new List<string> { "Vỡ cánh tay gần", "Gãy nút xương", "Nén", "Dislocation", "Dislocation liên quan" },
                    RedFlags = new List<string> { "Dislocation >1cm", "Chấn thương mạch máu", "Dislocation liên quan", "Liệt thần kinh nách" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "gãy cánh tay gần", "gãy vai" },
                        Secondary = new List<string> { "nút lớn", "nút nhỏ", "cổ phẫu thuật", "nén" },
                        Symptoms = new List<string> { "đau vai", "sưng vai", "giảm cử động vai", "sức yếu cánh tay" },
                        Negative = new List<string> { "dislocation tối thiểu", "thần kinh nách nguyên vẹn" }
                    }
                },

                // ===== TRẬT KHỚP =====
                new Disease
                {
                    DiseaseId = "DISLOC_001",
                    DiseaseName = "Trật Khớp Vai Phía Trước",
                    Severity = 7,
                    EmergencyLevel = 8,
                    TreatmentDuration = "6-12 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Bó lót", "Vật lý trị liệu", "Phẫu thuật nếu tái phát" },
                    XrayFindings = new List<string> { "Đầu xương cánh tay trước dislocation", "Mất liên hệ khớp", "Tổn thương Hill-Sachs", "Tổn thương Bankart" },
                    RedFlags = new List<string> { "Rối loạn mạch máu", "Gãy liên quan", "Trật tái phát", "Chấn thương thần kinh nách" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật vai", "khớp vai trật", "trật phía trước" },
                        Secondary = new List<string> { "Hill-Sachs", "Bankart", "đầu xương cánh tay", "hõm vai" },
                        Symptoms = new List<string> { "đau vai nặng", "mất cử động vai", "cánh tay cố định", "biến dạng vai", "tê" },
                        Negative = new List<string> { "thần kinh bình thường", "không gãy liên quan" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_002",
                    DiseaseName = "Trật Khớp Vai Phía Sau",
                    Severity = 7,
                    EmergencyLevel = 8,
                    TreatmentDuration = "8-12 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Bó lót", "Phục hồi", "Phẫu thuật nếu cần" },
                    XrayFindings = new List<string> { "Đầu xương cánh tay sau dislocation", "Xoay trong cố định", "Tổn thương Hill-Sachs đảo", "Tổn thương Bankart đảo" },
                    RedFlags = new List<string> { "Gãy hõm sau", "Trật mãn tính", "Liên quan cơn động kinh", "Chấn thương thần kinh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật vai phía sau", "trật phía sau" },
                        Secondary = new List<string> { "xoay trong cố định", "dấu hiệu bóng đèn", "Hill-Sachs đảo" },
                        Symptoms = new List<string> { "đau vai", "xoay trong bị khóa", "giảm xoay ngoài", "búi vai phía sau" },
                        Negative = new List<string> { "không dislocation phía trước" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_003",
                    DiseaseName = "Trật Khớp Khuỷu",
                    Severity = 7,
                    EmergencyLevel = 8,
                    TreatmentDuration = "6-10 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Đặt nẹp", "Vận động sớm", "Phẫu thuật nếu gãy liên quan" },
                    XrayFindings = new List<string> { "Mất liên hệ ulno-humeral", "Dislocation sau", "Gãy coronoid liên quan", "Dislocation đầu bán kính" },
                    RedFlags = new List<string> { "Dislocation trước", "Gãy liên quan", "Rối loạn mạch máu", "Chấn thương động mạch cánh tay" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật khuỷu", "dislocation khớp khuỷu" },
                        Secondary = new List<string> { "dislocation sau", "dislocation trước", "gãy coronoid" },
                        Symptoms = new List<string> { "đau khuỷu nặng", "sưng khuỷu", "biến dạng khuỷu", "không thể gập khuỷu", "triệu chứng mạch máu" },
                        Negative = new List<string> { "thần kinh bình thường", "không gãy đáng kể" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_004",
                    DiseaseName = "Trật Khớp Háng",
                    Severity = 8,
                    EmergencyLevel = 9,
                    TreatmentDuration = "6-8 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại khẩn cấp", "Kéo chậu", "Cố định háng", "Phẫu thuật nếu cần" },
                    XrayFindings = new List<string> { "Dislocation đầu xương đùi", "Dislocation sau", "Dislocation trước", "Gãy liên quan", "Mất khoảng cách khớp háng" },
                    RedFlags = new List<string> { "Dislocation sau với gãy xương đùi", "Đặt lại muộn", "Liệt thần kinh坐骨", "Rủi ro hoại tử" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật háng", "dislocation khớp háng" },
                        Secondary = new List<string> { "dislocation sau", "dislocation trước", "thần kinh坐骨" },
                        Symptoms = new List<string> { "đau háng nặng", "biến dạng háng", "rút ngắn hoặc dài ra chi", "không thể cử động háng", "đau thần kinh坐骨" },
                        Negative = new List<string> { "đầu xương đùi bình thường", "gãy liên quan không" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_005",
                    DiseaseName = "Trật Khớp Cổ Chân",
                    Severity = 7,
                    EmergencyLevel = 7,
                    TreatmentDuration = "6-8 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Cố định cổ chân", "Phẫu thuật nếu không ổn định" },
                    XrayFindings = new List<string> { "Mất căn chỉnh talocrural", "Dislocation talus", "Gãy liên quan", "Chấn thương liên khớp" },
                    RedFlags = new List<string> { "Dislocation hở", "Rối loạn mạch máu", "Gãy liên quan", "Chấn thương thần kinh mạch máu", "Gãy xương mác" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật cổ chân", "dislocation cổ chân" },
                        Secondary = new List<string> { "dislocation anterolateral", "dislocation posteromedial", "dislocation talus" },
                        Symptoms = new List<string> { "đau cổ chân nặng", "sưng cổ chân", "biến dạng cổ chân", "không gánh vác", "triệu chứng mạch máu" },
                        Negative = new List<string> { "căn chỉnh cổ chân bình thường", "không rối loạn mạch máu" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_006",
                    DiseaseName = "Trật Bánh Chè Đầu Gối",
                    Severity = 6,
                    EmergencyLevel = 7,
                    TreatmentDuration = "6-12 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Cố định dạng duỗi thẳng", "Bài tập ổn định bánh chè", "Phẫu thuật nếu tái phát" },
                    XrayFindings = new List<string> { "Bánh chè dislocation bên ngoài", "Máng đùi nông", "Gãy liên quan", "Tổn thương sụn" },
                    RedFlags = new List<string> { "Gãy liên quan", "Tổn thương sụn lồi", "Trật tái phát", "Rối loạn mạch máu" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật bánh chè", "dislocation bánh chè" },
                        Secondary = new List<string> { "dislocation bên ngoài", "tổn thương sụn lồi" },
                        Symptoms = new List<string> { "đau đầu gối cấp tính", "đầu gối trượt", "sưng đầu gối", "biến dạng bánh chè", "tích máu" },
                        Negative = new List<string> { "không gãy liên quan", "đặt lại tự phát" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_007",
                    DiseaseName = "Trật Khớp Đầu Gối",
                    Severity = 8,
                    EmergencyLevel = 9,
                    TreatmentDuration = "12-16 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại khẩn cấp", "Sửa chữa mạch máu nếu cần", "Tái cấu trúc dây chằng", "Cố định" },
                    XrayFindings = new List<string> { "Dislocation femoral tibial", "Mất căn chỉnh khớp", "Gãy liên quan", "Dấu hiệu chấn thương mạch máu" },
                    RedFlags = new List<string> { "Chấn thương mạch máu", "Chấn thương động mạch popliteal", "Chấn thương dây chằng nhiều", "Hội chứng khoang", "Dislocation hở" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật đầu gối", "dislocation tibiofemoral" },
                        Secondary = new List<string> { "dislocation trước", "dislocation sau", "chấn thương mạch máu", "chấn thương dây chằng" },
                        Symptoms = new List<string> { "đau đầu gối nặng", "biến dạng đầu gối", "mạch máu giảm", "sưng chi nặng", "da nhợt" },
                        Negative = new List<string> { "mạch máu bình thường", "cảm giác bình thường", "màu da bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_008",
                    DiseaseName = "Trật Khớp Ngón Tay",
                    Severity = 4,
                    EmergencyLevel = 4,
                    TreatmentDuration = "3-4 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Dán bàn tay", "Đặt nẹp", "Vận động sớm" },
                    XrayFindings = new List<string> { "Dislocation khớp PIP hoặc DIP", "Gãy liên quan", "Mất căn chỉnh khớp" },
                    RedFlags = new List<string> { "Gãy liên quan", "Rối loạn mạch máu", "Gân căng", "Dislocation hở" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật ngón tay", "dislocation ngón" },
                        Secondary = new List<string> { "dislocation PIP", "dislocation DIP", "dislocation xương trung tay" },
                        Symptoms = new List<string> { "đau ngón tay", "sưng ngón tay", "biến dạng", "mất cử động" },
                        Negative = new List<string> { "không gãy liên quan", "cảm giác bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "DISLOC_009",
                    DiseaseName = "Trật Khớp Ngón Cái",
                    Severity = 5,
                    EmergencyLevel = 5,
                    TreatmentDuration = "4-6 tuần",
                    TreatmentMethod = new List<string> { "Đặt lại", "Bó ngón cái", "Sửa chữa dây chằng nếu cần", "Vận động sớm" },
                    XrayFindings = new List<string> { "Dislocation khớp CMC hoặc MCP", "Gãy liên quan", "Mất căn chỉnh", "Dislocation xương mân cơ" },
                    RedFlags = new List<string> { "Gãy liên quan", "Chấn thương trò chơi bóng chày", "Rối loạn mạch máu", "Gân kẹt" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "trật ngón cái", "dislocation ngón cái" },
                        Secondary = new List<string> { "dislocation carpometacarpal", "dislocation metacarpophalangeal", "chơi bóng chày" },
                        Symptoms = new List<string> { "đau ngón cái", "sưng ngón cái", "biến dạng", "giảm nắm", "không ổn định" },
                        Negative = new List<string> { "không gãy liên quan", "ổn định sau đặt lại" }
                    }
                },

                // ===== VIÊM KHỚP - CHI TIẾT HƠN =====
                new Disease
                {
                    DiseaseId = "ARTH_001",
                    DiseaseName = "Viêm Khớp Xương - Đầu Gối",
                    Severity = 6,
                    EmergencyLevel = 3,
                    TreatmentDuration = "Mãn tính",
                    TreatmentMethod = new List<string> { "Thuốc chống viêm", "Vật lý trị liệu", "Kiểm soát cân nặng", "Thay khớp đầu gối", "Tiêm corticosteroid" },
                    XrayFindings = new List<string> { "Hẹp khoảng cách khớp", "Loã xương", "Xơ hóa", "Mất sụn", "Biến dạng xương" },
                    RedFlags = new List<string> { "Đau nặng với hoạt động tối thiểu", "Rối loạn chức năng đáng kể", "Vỡ sụn meniscus liên quan" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "viêm khớp xương", "viêm khớp xương đầu gối", "viêm khớp thoái hóa" },
                        Secondary = new List<string> { "hẹp khoảng cách khớp", "loã xương", "gai xương", "xơ hóa" },
                        Symptoms = new List<string> { "đau khớp", "cứng sáng", "sưng", "giảm cơ động", "cơ chân yếu", "đau khi hoạt động" },
                        Negative = new List<string> { "không viêm", "không sưng cấp tính" }
                    }
                },
                new Disease
                {
                    DiseaseId = "ARTH_002",
                    DiseaseName = "Viêm Khớp Xương - Háng",
                    Severity = 6,
                    EmergencyLevel = 3,
                    TreatmentDuration = "Mãn tính",
                    TreatmentMethod = new List<string> { "Thuốc chống viêm", "Vật lý trị liệu", "Thay khớp háng", "Tiêm corticosteroid", "Dụng cụ hỗ trợ" },
                    XrayFindings = new List<string> { "Hẹp khoảng cách khớp", "Loã xương đầu xương đùi", "Thay đổi hõm", "Xơ hóa xương", "Biến dạng" },
                    RedFlags = new List<string> { "Đau nặng ảnh hưởng hoạt động hàng ngày", "Rối loạn chức năng đáng kể", "Rủi ro gãy liên quan" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "viêm khớp xương háng" },
                        Secondary = new List<string> { "đầu xương đùi", "hõm", "hẹp khoảng cách khớp" },
                        Symptoms = new List<string> { "đau háng", "đau bẹn", "giảm cơ động háng", "đi khập khiễng", "cứng sáng" },
                        Negative = new List<string> { "không viêm cấp tính", "không gãy" }
                    }
                },
                new Disease
                {
                    DiseaseId = "ARTH_003",
                    DiseaseName = "Viêm Khớp Thấp - Bàn Tay",
                    Severity = 6,
                    EmergencyLevel = 2,
                    TreatmentDuration = "Mãn tính",
                    TreatmentMethod = new List<string> { "DMARD", "Thuốc sinh học", "Corticosteroid", "Vật lý trị liệu", "Sửa chữa phẫu thuật" },
                    XrayFindings = new List<string> { "Sưng quanh khớp", "Hẹp khoảng cách khớp", "Phản ứng tầng ngoài xương", "Xơ hóa xương", "Xói mòn xương" },
                    RedFlags = new List<string> { "Liên quan khớp nhiều", "Xói mòn xương đáng kể", "Tiến triển nhanh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "viêm khớp thấp", "viêm khớp thấp tay" },
                        Secondary = new List<string> { "xói mòn xương", "hình thành pannus", "subuxation", "cổ thiên nga" },
                        Symptoms = new List<string> { "đau khớp nhiều", "cứng sáng", "sưng đối xứng", "triệu chứng toàn thân", "ấm" },
                        Negative = new List<string> { "không sốt", "chỉ số viêm bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "ARTH_004",
                    DiseaseName = "Viêm Khớp Gút - Ngón Chân Lớn",
                    Severity = 5,
                    EmergencyLevel = 4,
                    TreatmentDuration = "1-2 tuần (cấp tính)",
                    TreatmentMethod = new List<string> { "Thuốc chống viêm", "Colchicine", "Corticosteroid", "Allopurinol", "Thay đổi chế độ ăn" },
                    XrayFindings = new List<string> { "沉积 tophaceous", "Xói mòn dạng thiết kế", "Sưng mô mềm", "Khoảng cách khớp bình thường ban đầu" },
                    RedFlags = new List<string> { "Liên quan khớp nhiều", "Gút tophaceous mãn tính", "Liên quan thận" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "viêm khớp gút", "gút cấp tính" },
                        Secondary = new List<string> { "沉积 tophaceous", "xói mòn dạng thiết kế", "axit uric" },
                        Symptoms = new List<string> { "đau khớp cấp tính", "viêm nặng", "đỏ", "ấm", "sốt" },
                        Negative = new List<string> { "axit uric bình thường", "không沉积 mãn tính" }
                    }
                },
                new Disease
                {
                    DiseaseId = "ARTH_005",
                    DiseaseName = "Viêm Cột Sống Dính",
                    Severity = 7,
                    EmergencyLevel = 3,
                    TreatmentDuration = "Mãn tính",
                    TreatmentMethod = new List<string> { "Thuốc chống viêm", "Thuốc sinh học", "Ức chế TNF", "Vật lý trị liệu", "Liên kết cột sống nếu nặng" },
                    XrayFindings = new List<string> { "Ilitis sacro", "Cột sống tre", "Syndesmophytes", "Hợp nhất xương sống", "Cơ thể xương sống vuông" },
                    RedFlags = new List<string> { "Hợp nhất cột sống nặng", "Rủi ro gãy cột sống", "Viêm mắt trước", "Liên quan tim" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "viêm cột sống dính", "cột sống tre", "AS" },
                        Secondary = new List<string> { "ilitis sacro", "syndesmophytes", "hợp nhất xương sống" },
                        Symptoms = new List<string> { "đau lưng", "cứng sáng", "cứng cột sống lũy tiến", "đau ngực", "viêm mắt" },
                        Negative = new List<string> { "không hợp nhất", "cột sống linh hoạt" }
                    }
                },

                // ===== BỆNH XƯƠ HÓA XƯƠNG =====
                new Disease
                {
                    DiseaseId = "METAB_001",
                    DiseaseName = "Loãng Xương",
                    Severity = 5,
                    EmergencyLevel = 2,
                    TreatmentDuration = "Suốt đời",
                    TreatmentMethod = new List<string> { "Bổ sung canxi", "Vitamin D", "Bisphosphonates", "Liệu pháp hormone", "Denosumab", "Tập thể dục" },
                    XrayFindings = new List<string> { "Mật độ xương giảm", "Nén sống", "Vỏ xương mỏng", "Mất chi tiết hình tầng", "Vỡ sống" },
                    RedFlags = new List<string> { "Vỡ sống nén", "Vỡ háng", "Mất xương đáng kể", "Rủi ro ngã" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "loãng xương", "mất xương", "mật độ xương thấp", "osteoporosis" },
                        Secondary = new List<string> { "T-score", "osteopenia", "nén sống" },
                        Symptoms = new List<string> { "vỡ với chấn thương tối thiểu", "mất chiều cao", "cong cột sống", "đau lưng", "không triệu chứng" },
                        Negative = new List<string> { "canxi bình thường", "vitamin D bình thường", "không nguyên nhân phụ" }
                    }
                },
                new Disease
                {
                    DiseaseId = "METAB_002",
                    DiseaseName = "Bệnh Xương Dễ Vỡ (Osteogenesis Imperfecta)",
                    Severity = 7,
                    EmergencyLevel = 5,
                    TreatmentDuration = "Suốt đời",
                    TreatmentMethod = new List<string> { "Bisphosphonates", "Vật lý trị liệu", "Phẫu thuật chỉnh hình", "Tư vấn di truyền", "Kiểm soát đau" },
                    XrayFindings = new List<string> { "Xương mảnh", "Vỡ nhiều", "Vết sẹo liền", "Loãng xương", "Nén sống", "Xương wormian" },
                    RedFlags = new List<string> { "OI loại III hoặc IV", "Biến dạng cột sống", "Chèn ép cơ sở sọ", "Chèn ép thần kinh" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "bệnh xương dễ vỡ", "xương dễ gãy", "OI" },
                        Secondary = new List<string> { "vỡ nhiều", "xương wormian", "mắt xanh" },
                        Symptoms = new List<string> { "vỡ hay", "đau xương", "yếu cơ", "mất thính lực", "mắt xanh", "thấp bé" },
                        Negative = new List<string> { "rối loạn chuyển hóa bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "METAB_003",
                    DiseaseName = "Còi Xương (Rickets)",
                    Severity = 6,
                    EmergencyLevel = 4,
                    TreatmentDuration = "Thay đổi",
                    TreatmentMethod = new List<string> { "Bổ sung vitamin D", "Bổ sung canxi", "Liệu pháp phosphat", "Sửa chữa nguyên nhân gốc rễ" },
                    XrayFindings = new List<string> { "Rộng khớp phận", "Mất rìa khớp sắc", "Mất vùng canxi sơ cấp", "Hút xương dưới vỏ" },
                    RedFlags = new List<string> { "Còi nặng", "Hypocalcemia", "Cơn co giật", "Tetany", "Mất hô hấp" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "còi xương", "thiếu vitamin D", "còi dinh dưỡng" },
                        Secondary = new List<string> { "rộng khớp phận", "vùng canxi sơ cấp", "hút xương" },
                        Symptoms = new List<string> { "đau xương", "yếu cơ", "thấp bé trì trệ", "vấn đề nha", "co giật cơ", "co giật" },
                        Negative = new List<string> { "canxi bình thường", "phosphat bình thường" }
                    }
                },
                new Disease
                {
                    DiseaseId = "METAB_004",
                    DiseaseName = "Bệnh Paget Xương",
                    Severity = 6,
                    EmergencyLevel = 3,
                    TreatmentDuration = "Mãn tính",
                    TreatmentMethod = new List<string> { "Bisphosphonates", "Thuốc chống viêm", "Calcitonin", "Phẫu thuật chỉnh hình nếu vỡ", "Dụng cụ hỗ trợ" },
                    XrayFindings = new List<string> { "Cấu trúc xương hỗn hợp", "Giai đoạn tiêu", "Giai đoạn xơ hóa", "Dày vỏ xương", "Biến dạng", "Vỡ" },
                    RedFlags = new List<string> { "Vỡ xương bệnh lý", "Chuyển đổi肉 sarcomat", "Chèn ép thần kinh sọ não", "Chèn ép tủy sống", "Suy tim cao cấp" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "bệnh Paget", "bệnh Paget xương", "osteitis deformans" },
                        Secondary = new List<string> { "giai đoạn tiêu", "giai đoạn xơ hóa", "xương hỗn hợp" },
                        Symptoms = new List<string> { "đau xương", "biến dạng", "vỡ", "triệu chứng thần kinh", "mất thính lực", "đau đầu" },
                        Negative = new List<string> { "phosphatase kiềm bình thường", "chỉ một xương" }
                    }
                },

                // ===== BỆNH BẨMSINH VÀ PHÁT TRIỂN =====
                new Disease
                {
                    DiseaseId = "CONG_001",
                    DiseaseName = "Loạn Sản Phát Triển Háng (DDH)",
                    Severity = 7,
                    EmergencyLevel = 5,
                    TreatmentDuration = "Thay đổi",
                    TreatmentMethod = new List<string> { "Phát hiện sớm và nẹp", "Bộ Pavlik", "Kéo", "Giảm phẫu thuật nếu cần", "Theo dõi háng" },
                    XrayFindings = new List<string> { "Tăng tính xoay trước xương đùi", "Hõm nông", "Đầu xương đùi trật", "Canxi hóa trì hoãn", "Dislocation đường Hilgenreiner" },
                    RedFlags = new List<string> { "Chẩn đoán muộn", "Trật không thể giảm", "Hoại tử", "Sai khác chiều dài chi" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "DDH", "loạn sản phát triển", "hông bẩm sinh", "dysplasia háng" },
                        Secondary = new List<string> { "hõm nông", "đầu xương đùi trật", "Hilgenreiner" },
                        Symptoms = new List<string> { "click hông", "nếp da hông không đối xứng", "chi ngắn", "đi bộ trì hoãn", "trendelenburg gait", "giảm tách hông" },
                        Negative = new List<string> { "exam hông bình thường", "test Ortolani âm" }
                    }
                },
                new Disease
                {
                    DiseaseId = "CONG_002",
                    DiseaseName = "Bệnh Legg-Calve-Perthes",
                    Severity = 6,
                    EmergencyLevel = 4,
                    TreatmentDuration = "2-4 năm",
                    TreatmentMethod = new List<string> { "Theo dõi", "Nẹp", "Vật lý trị liệu", "Phẫu thuật varus derotation osteotomy", "Chứa hông" },
                    XrayFindings = new List<string> { "Hoại tử avascular đầu xương đùi", "Vỡ epiphyseal", "Phẳng", "Dấu cột bên", "Angle Hilgenreiner epiphyseal" },
                    RedFlags = new List<string> { "Liên quan cột bên lớn", "Sụp đổ đầu nặng", "Khởi đầu >8 tuổi" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "LCPD", "Legg-Calve-Perthes", "bệnh Perthes" },
                        Secondary = new List<string> { "hoại tử đầu xương đùi AVN", "cột bên", "đầu phẳng" },
                        Symptoms = new List<string> { "đau hông", "đau bẹn", "đi khập khiễng", "giảm cơ động hông", "đau đầu gối", "chi ngắn" },
                        Negative = new List<string> { "không lịch sử chấn thương" }
                    }
                },
                new Disease
                {
                    DiseaseId = "CONG_003",
                    DiseaseName = "Epiphysis Đầu Xương Đùi Bị Trượt (SCFE)",
                    Severity = 7,
                    EmergencyLevel = 8,
                    TreatmentDuration = "6-12 tuần",
                    TreatmentMethod = new List<string> { "Cố định tại chỗ", "Derotation nếu nặng", "Cố định hông", "Dụng cụ nạng", "Bảo vệ phán" },
                    XrayFindings = new List<string> { "Dislocation epiphyseal sau", "Rộng phán", "Dấu cỏ", "Slip sau", "Mất đường Shenton" },
                    RedFlags = new List<string> { "Slip cấp tính trên mãn tính", "Slip nặng >60%", "Liên quan hai bên", "Hoại tử", "Chondrolysis" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "SCFE", "epiphysis bị trượt", "epiphysis xương đùi bị trượt", "SCFE" },
                        Secondary = new List<string> { "dislocation sau", "dấu cỏ", "Shenton" },
                        Symptoms = new List<string> { "đau hông", "đau bẹn", "đau đầu gối", "đi khập khiễng", "xoay ngoài limp", "mất xoay ngoài", "biến dạng xoay ngoài" },
                        Negative = new List<string> { "không slip" }
                    }
                },
                new Disease
                {
                    DiseaseId = "CONG_004",
                    DiseaseName = "Biến Dạng Valgus Đầu Gối",
                    Severity = 4,
                    EmergencyLevel = 2,
                    TreatmentDuration = "Thay đổi",
                    TreatmentMethod = new List<string> { "Theo dõi thường xuyên", "Nẹp nếu đáng kể", "Osteotomy sửa chữa nếu nặng", "Vật lý trị liệu" },
                    XrayFindings = new List<string> { "Tăng góc valgus", "Angulation bên trong", "Mất khoảng cách khớp bên ngoài", "Valgus sinh lý hoặc bệnh lý" },
                    RedFlags = new List<string> { "Valgus nặng >15 độ", "Biến dạng tiến triển", "Đau", "Rối loạn chức năng" },
                    ConfidenceKeywords = new ConfidenceKeywords
                    {
                        Primary = new List<string> { "valgus đầu gối", "đầu gối bàn tay", "genu valgum" },
                        Secondary = new List<string> { "góc valgus", "trục cơ học" },
                        Symptoms = new List<string> { "hình đầu gối bàn tay", "đau khi hoạt động", "mài mòn giày không đều", "rối loạn chức năng" },
                        Negative = new List<string> { "sinh lý", "không đau", "không rối loạn chức năng" }
                    }
                }
            };
        }
    }
}
