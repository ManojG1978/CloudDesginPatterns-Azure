namespace DurableFunctions_Patterns
{
    public class NewEmployeeRequest
    {
        public string EmployeeFullName { get; set; }

        public int EmployeeId { get; set; }

        public bool BadgePrinted { get; set; }

        public bool RegisterGroupInsurance { get; set; }

        public string ManagerFullName { get; set; }

        public bool ManagerApproved { get; set; }

        public string Remarks { get; set; }
    }
}