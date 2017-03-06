using PublishProfileContracts;

namespace PublishProfileManager.Models
{
    public class MSDeployPackagePublishProfile : PublishProfile, IMSDeployPackagePublishProfile
    {
        public const string PublishMethod = "Package";
        public MSDeployPackagePublishProfile()
            : base(PublishMethod)
        {
        }

        public string PackageLocation { get; private set; }
        public string DeployIisAppPath { get; private set; }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
