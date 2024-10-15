using System;
using System.Configuration;
using Asos.CodeTest.Interfaces;

namespace Asos.CodeTest
{
    public class AppSettings : IAppSettings
    {
        public bool IsFailoverModeEnabled => Convert.ToBoolean(ConfigurationManager.AppSettings["IsFailoverModeEnabled"]);

        public long FailedRequestsThreshold => Convert.ToInt64(ConfigurationManager.AppSettings["FailoverThreshold"]);
               
        public long FailedRequestsAging => Convert.ToInt64(ConfigurationManager.AppSettings["FailedRequestsAging"]);
    }
}