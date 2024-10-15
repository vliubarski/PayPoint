namespace Asos.CodeTest
{
    public interface IAppSettings
    {
        bool IsFailoverModeEnabled { get; }

        /// <summary>
        /// Minimum number of failed requests
        /// to get customers from failoverCustomerData
        /// </summary>
        long FailedRequestsThreshold { get; }

        /// <summary>
        /// Indicates age of failed requests in minutes
        /// </summary>
        long FailedRequestsAging { get; }
    }
}