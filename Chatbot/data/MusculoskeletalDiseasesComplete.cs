using Chatbot.Models;
using System.Collections.Generic;
using System.Linq;

namespace Chatbot.Data
{
    public class MusculoskeletalDiseasesComplete
    {
        /// <summary>
        /// Lấy tất cả các bệnh cơ xương khớp (210+ bệnh) từ tất cả các phần
        /// </summary>
        public static List<Disease> GetAllDiseases()
        {
            var diseases = new List<Disease>();

            // Thêm tất cả các bệnh từ các phần khác nhau
            diseases.AddRange(MusculoskeletalDiseasesList.GetMusculoskeletalDiseases());
            //diseases.AddRange(MusculoskeletalDiseasesListPart2.GetAdditionalDiseases());
            //diseases.AddRange(MusculoskeletalDiseasesListPart3.GetPart3Diseases());

            return diseases;
        }
    }
}
