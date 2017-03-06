using System.ComponentModel.DataAnnotations;
using PublishProfileContracts;

namespace PublishProfileManager.Models
{
    public class FileSystemPublishProfile : PublishProfile, IFileSystemPublishProfile
    {

        public const string PublishMethod = "FileSystem";
        public FileSystemPublishProfile()
            : base(PublishMethod)
        {
            DeleteExistingFiles = false;
        }

        [Display(Order = 1)]
        public string PublishUrl { get; set; }

        [Display(Order = 2)]
        public bool DeleteExistingFiles { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
