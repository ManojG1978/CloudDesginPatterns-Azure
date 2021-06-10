using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DurableFunctions_Patterns
{
    public static class Chaining
    {
        [FunctionName("NewEmployeeProcess")]
        public static async Task<NewEmployeeRequest> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var employeeRuest = new NewEmployeeRequest
            {
                EmployeeFullName = new Faker().Name.FullName()
            };

            employeeRuest = await context.CallActivityAsync<NewEmployeeRequest>("GenerateEmployeeId", employeeRuest);
            employeeRuest = await context.CallActivityAsync<NewEmployeeRequest>("AssignManager", employeeRuest);
            string remarks;
            using (var timeoutCts = new CancellationTokenSource())
            {
                var dueTime = context.CurrentUtcDateTime.AddSeconds(300);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                remarks = $"Waiting for Approval for Employee:{employeeRuest.EmployeeFullName} from the assigned manager: [{employeeRuest.ManagerFullName}]";
                employeeRuest.Remarks = remarks;
                log.LogInformation(remarks);

                var entityId = new EntityId(nameof(EmployeeCounter), nameof(EmployeeCounter));

                var proxy = context.CreateEntityProxy<IEmployeeCounter>(entityId);
                proxy.IncrementWaitingForApproval();

                context.SetCustomStatus(remarks);

                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("ApprovalEvent");
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    remarks = $"Approval for employee[{employeeRuest.EmployeeFullName}] received from the assigned manager: [{employeeRuest.ManagerFullName}]";
                    proxy.IncrementEmployee();
                    context.SetCustomStatus(remarks);
                    log.LogInformation(remarks);
                    employeeRuest.ManagerApproved = true;
                }
                else
                {
                    remarks = $"Approval was not received from the assigned manager: [{employeeRuest.ManagerFullName}]. Please escalate";
                    context.SetCustomStatus(remarks);
                    log.LogInformation(remarks);
                    employeeRuest.ManagerApproved = false;
                    employeeRuest.Remarks = remarks;
                    return employeeRuest;
                }
            }

            employeeRuest = await context.CallActivityAsync<NewEmployeeRequest>("RegisterForGroupInsurance", employeeRuest);
            employeeRuest = await context.CallActivityAsync<NewEmployeeRequest>("PrintBadge", employeeRuest);

            remarks = $"All formalities completed for Employee {employeeRuest.EmployeeFullName}, ID: {employeeRuest.EmployeeId}";
            context.SetCustomStatus(remarks);
            employeeRuest.Remarks = remarks;
            return employeeRuest;
        }


        [FunctionName("GenerateEmployeeId")]
        public static NewEmployeeRequest GenerateEmployeeId([ActivityTrigger] NewEmployeeRequest request, ILogger log)
        {
            request.EmployeeId = new Faker().Random.Number(1000000, 100000000);
            log.LogInformation($"Generated ID [{request.EmployeeId}] for {request.EmployeeFullName}.");
            return request;
        }

        [FunctionName("AssignManager")]
        public static NewEmployeeRequest AssignManager([ActivityTrigger] NewEmployeeRequest request, ILogger log)
        {
            request.ManagerFullName = new Faker().Name.FullName();
            log.LogInformation($"Assigning manager {request.ManagerFullName} for Employee: {request.EmployeeFullName}.");
            return request;
        }

        [FunctionName("RegisterForGroupInsurance")]
        public static NewEmployeeRequest RegisterForGroupInsurance([ActivityTrigger] NewEmployeeRequest request, ILogger log)
        {
            request.RegisterGroupInsurance = true;
            log.LogInformation($"Registered group insurance for EmployeeID: {request.EmployeeId}.");
            return request;
        }

        [FunctionName("PrintBadge")]
        public static NewEmployeeRequest PrintBadge([ActivityTrigger] NewEmployeeRequest request, ILogger log)
        {
            log.LogInformation($"Printed Badge for EmployeeID: {request.EmployeeId}.");
            request.BadgePrinted = true;
            return request;
        }

        [FunctionName("GetCounts")]
        public static async Task<JObject> GetCounts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "counts")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = new EntityId(nameof(EmployeeCounter), nameof(EmployeeCounter));
            var response = await client.ReadEntityStateAsync<JObject>(entityId);
            return response.EntityState;
        }

        [FunctionName("NewEmployeeProcess_Start")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await starter.StartNewAsync("NewEmployeeProcess", null);

            log.LogInformation($"Started new Employee Process with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}