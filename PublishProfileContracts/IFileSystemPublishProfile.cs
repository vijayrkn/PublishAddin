namespace PublishProfileContracts
{
    public interface IFileSystemPublishProfile: IPublishProfile
    {
        string PublishUrl { get; }
        bool DeleteExistingFiles { get; }
    }
}
