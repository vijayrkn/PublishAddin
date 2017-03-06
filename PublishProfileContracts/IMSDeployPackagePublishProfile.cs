namespace PublishProfileContracts
{
    public interface IMSDeployPackagePublishProfile: IPublishProfile
    {
        string PackageLocation { get; }
        string DeployIisAppPath { get; }
    }
}
