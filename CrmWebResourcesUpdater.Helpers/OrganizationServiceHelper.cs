using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;

namespace CrmWebResourcesUpdater.Helpers
{
    public static class OrganizationServiceHelper
    {
        public static Entity GetSolution(this IOrganizationService service, Guid solutionId)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("friendlyname", "uniquename", "publisherid")
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities.FirstOrDefault();
        }
        
        public static string GetSolutionVersion(this IOrganizationService service, Guid solutionId)
        {
            var solution = service.Retrieve("solution", solutionId, new ColumnSet("friendlyname", "version"));
            return solution.GetAttributeValue<string>("version");
        }
        
        public static void UpdateSolutionVersion(this IOrganizationService service, Guid solutionId, string version)
        {
            if (version == null)
                return;

            var solution = new Entity("solution", solutionId) {["version"] = version};
            service.Update(solution);
        }

        public static OrganizationDetail GetOrganization(this IDiscoveryService service, Guid organizationId)
        {
            var details = GetAllOrganizations(service);
            var result = details.FirstOrDefault(d => d.OrganizationId == organizationId);
            return result;
        }

        public static OrganizationDetail[] GetAllOrganizations(this IDiscoveryService service)
        {
            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse) service.Execute(request);

            var details = response.Details.ToArray();
            return details;
        }
    }
}
