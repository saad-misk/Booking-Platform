namespace BookingPlatform.Infrastructure.Services.Images.B2CloudStorage
{
    public class CustomHttpMessageHandler : DelegatingHandler
    {
        public CustomHttpMessageHandler() : base(new HttpClientHandler()) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("x-amz-sdk-checksum-algorithm"))
            {
                request.Headers.Remove("x-amz-sdk-checksum-algorithm");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}