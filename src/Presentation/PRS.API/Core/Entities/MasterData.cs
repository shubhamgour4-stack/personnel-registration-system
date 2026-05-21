namespace PRS.Core.Entities
{
    public class Grade
    {
        public int Grade_ID { get; set; }
        public string Rank { get; set; }
        public string Rank_Code { get; set; }
        public bool Status { get; set; }
        public DateTime Last_Updated_Date { get; set; }
        public string Last_Updated_By { get; set; }
    }

    public class LineOfService
    {
        public int LOS_ID { get; set; }
        public string Line_Of_Service { get; set; }
        public string LOS_CODE { get; set; }
        public DateTime Last_Updated_Date { get; set; }
        public string Last_Updated_By { get; set; }
    }

    public class EmploymentStatus
    {
        public int Employment_Status_ID { get; set; }
        public string Employment_Status { get; set; }
        public string Employment_Status_Code { get; set; }
    }

    public class GlobalCountry
    {
        public string Country_Code { get; set; } 
        public string Country_Description { get; set; }
        public bool Country_Status { get; set; }
    }

    public class WorkOfficeLocation
    {
        public int Work_Office_ID { get; set; }
        public string Country_Code { get; set; } 
        public string Work_Office_Code { get; set; } 
        public string Work_Office_Description { get; set; }
        public bool WOL_Status { get; set; }
    }
}