namespace UserManagementService.Application.GrpcClient
{
    using System;
    using System.Threading.Tasks;

    public class UserClient(UserGrpcService.UserGrpcServiceClient client)
    {
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
