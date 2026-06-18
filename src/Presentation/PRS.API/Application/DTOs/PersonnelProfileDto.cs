namespace PRS.Application.DTOs
{
    // Used for Step 1: Initial creation in Personnel_Guid
    public class InitializePersonnelDto
    {
        public string Name { get; set; }
        public string Employee_ID { get; set; }
        public string Email_ID { get; set; }
        public string Guid_Country { get; set; } // e.g., "IND" or "USA"
    }

    // Used for Step 2: The Search Grid results
    public class PersonnelSearchResultDto
    {
        public int Unique_ID { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Employee_ID { get; set; }
        public string Email_ID { get; set; }
        public string Record_Status { get; set; }
        // If they have a completed profile, show the Pseudo ID, otherwise null
        public string Pseudo_Party_ID { get; set; } 
    }

    // Used for Step 3 & 4: Completing the profile in Personnel_Global
    public class CompleteProfileDto
    {
        public int Personnel_Guid_ID { get; set; } // The ID of the user we are updating
        public int Work_Office_Location_ID { get; set; }
        public int Grade_ID { get; set; }
        public int Line_Of_Service_ID { get; set; }
        public int Employment_Status_ID { get; set; }
        public string Portfolio_Required { get; set; }
    }
}