
namespace PublishProfileContracts
{
    public interface IFTPPublishProfile : IAuthenticationPublishProfile, IPublishProfile
    {
        string PublishUrl { get; }
        bool DeleteExistingFiles { get; }
        bool FtpPassiveMode { get; }
        string FtpSitePath { get; }
        bool LaunchSiteAfterPublish { get; }
        string SiteUrlToLaunchAfterPublish { get; }
    }
}
