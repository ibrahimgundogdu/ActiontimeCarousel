using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class InspectionModel
    {
        public int Id { get; set; }
        public DateTime InspectionDate { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string Inspector { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Language { get; set; } = "en";
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Completed { get; set; } = false;
        public bool Closed { get; set; } = false;
        public List<InspectionPartSummaryModel>? PartSummaryList { get; set; }
        public List<InspectionAnswer>? AnswerList { get; set; }

    }
    public class InspectionPartModel
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string Inspector { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Language { get; set; } = "en";
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Closed { get; set; } = false;
        public InspectionPartSummaryModel PartSummary { get; set; }
        public InspectionQuestion Question { get; set; }
        public InspectionAnswer? Answer { get; set; }
        public List<InspectionImage>? Images { get; set; }
        public int currentPageId { get; set; }
        public int nextPageId { get; set; }
        public int prevPageId { get; set; }


    }
    public class InspectionPartSummaryModel
    {
        public int InspeectionId { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int TotalItem { get; set; }
        public int VotedItem { get; set; }
        public double TotalRate { get; set; } = 0;

    }

    public class InspectionAnswer
    {
        public int Id { get; set; }
        public int InspeectionId { get; set; }
        public int PartId { get; set; }
        public int Number { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string EstimateAnswer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Employee { get; set; } = string.Empty;
        public DateTime? InpectionDate { get; set; }

    }

    public class InspectionQuestion
    {
        public int Id { get; set; }
        public int inspectionTypeId { get; set; }
        public int inspectionCatId { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public string number { get; set; } = string.Empty;
        public string itemName { get; set; } = string.Empty;
        public string itemNameTr { get; set; } = string.Empty;
        public int answerType { get; set; }
        public int estimatedTime { get; set; }
        public string? estimatedAnswer { get; set; }
        public string sortBy { get; set; } = string.Empty;
        public bool isPart { get; set; } = false;

    }


    public class InspectionImage
    {
        public int Id { get; set; }
        public int inspectionItemId { get; set; }
        public string imageName { get; set; } = string.Empty;
        public string sortBy { get; set; } = string.Empty;

    }

}
