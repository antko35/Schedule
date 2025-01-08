namespace UserManagementService.API.GrpcClient.Services
{
    using System;
    using System.Threading.Tasks;

    public class UserClient(UserGrpcService.UserGrpcServiceClient client)
    {
        public async Task<GetClaimResponse> GetUserClaimsAsync(string email)
        {
            var request = new GetClaimsRequest { Email = email };
            var response = await client.GetPersonClaimsAsync(request);

            if (response.Success)
            {
                Console.WriteLine($"Claims : {response.Claims}");
                return response;
            }
            else
            {
                throw new InvalidOperationException($"Failed to get claim: {response.Message}");
            }
        }

        public async Task AddClaimAsync(AddClaimRequest request)
        {
            var response = await client.AddClaimToPersonAsync(request);

            if (response.Success)
            {
                Console.WriteLine($"Claim added successfully: {response.Message}");
            }
            else
            {
                throw new InvalidOperationException($"Failed to add claim: {response.Message}");
            }
        }

        public async Task DeleteClaimAsync(AddClaimRequest request)
        {
            var response = await client.DeletePersonClaimAsync(request);

            if (response.Success)
            {
                Console.WriteLine($"Claim deleted successfully: {response.Message}");
            }
            else
            {
                throw new InvalidOperationException($"Failed to delete claim: {response.Message}");
            }
        }
    }
}
