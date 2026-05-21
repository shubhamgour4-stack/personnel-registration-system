namespace PRS.Core.Entities
{
    public class PersonnelGuid
    {
        public int Unique_ID { get; set; }
        public string Name { get; set; }
        public string Employee_ID { get; set; }
        public string Email_ID { get; set; }
        public bool Record_Status { get; set; }
        public string Guid { get; set; } 
        public string Guid_Country { get; set; }
        public DateTime Created_Date { get; set; }
        public DateTime Updated_Date { get; set; }
        
        public PersonnelGlobal PersonnelGlobal { get; set; }
    }

    public class PersonnelGlobal
    {
        public int ID { get; set; } 
        public int Personnel_Guid_ID { get; set; } 
        public int Work_Office_Location_ID { get; set; } 
        public int Grade_ID { get; set; } 
        public int Line_Of_Service_ID { get; set; } 
        public int Employment_Status_ID { get; set; } 
        public bool Portfolio_Required { get; set; }
        public string Pseudo_Party_ID { get; set; }
        public DateTime Created_Date { get; set; }
        public DateTime Updated_Date { get; set; }
        
        public PersonnelGuid PersonnelGuid { get; set; }
    }
}