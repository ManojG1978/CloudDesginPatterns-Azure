using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace DurableFunctions_Patterns
{
    public interface IEmployeeCounter
    {
        void IncrementEmployee();
        Task<int> GetNewEmployeeCount();
        Task<int> GetWaitingForApprovalCount();
        void IncrementWaitingForApproval();
        void Delete();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class EmployeeCounter : IEmployeeCounter
    {
        [JsonProperty("employeesAdded")]
        public int EmployeesAdded { get; set; }

        [JsonProperty("waitingForApproval")]
        public int WaitingForApproval { get; set; }

        public void IncrementEmployee() 
        {
            EmployeesAdded++;
            WaitingForApproval--;
        }

        public void IncrementWaitingForApproval() 
        {
            WaitingForApproval++;
        }

        public Task<int> GetNewEmployeeCount() 
        {
            return Task.FromResult(EmployeesAdded);
        }

        public Task<int> GetWaitingForApprovalCount() 
        {
            return Task.FromResult(WaitingForApproval);
        }

        public void Delete() 
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(EmployeeCounter))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<EmployeeCounter>();
    }
}
