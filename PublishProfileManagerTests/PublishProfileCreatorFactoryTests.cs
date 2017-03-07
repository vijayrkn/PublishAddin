using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Evaluation;
using PublishProfileManager;
using PublishProfileManagerTests.Properties;
using Xunit;

namespace PublishProfileManagerTests
{
    public class PublishProfileCreatorFactoryTests
    {
        [Fact]
        public void PublishProfileFactory_ReturnsMsDeployProfileWithCorrectSetttings_FromPublishSettings()
        {
            var msDeployProfile = PublishProfileCreatorFactory.CreateMSDeployPublishProfileFromPublishSettings(TestResources.PublishSettings);
            Assert.Equal(TestResources.MsDeployFromPublishSettings, msDeployProfile.Value);
        }

        [Fact]
        public void PublishProfileFactory_ReturnsEncryptedUserProfile()
        {
            var userProfile = PublishProfileCreatorFactory.CreateUserPublishProfileFromPublishSettings(TestResources.PublishSettings);
            using (XmlTextReader reader = new XmlTextReader(new StringReader(userProfile.Value)))
            {
                Project userProject = new Project(reader);
                Assert.NotNull(userProject.GetProperty("EncryptedPassword").EvaluatedValue);
            }
        }

        [Fact]
        public void PublishProfileFactory_ReturnsCorrectMsDeployPublishProfile()
        {
            string msDeployProfile = PublishProfileCreatorFactory.CreatePublishProfile("MSDeploy").ToString();
            Assert.Equal(TestResources.MSDeployPublishProfile, msDeployProfile);
        }

        [Fact]
        public void PublishProfileFactory_ReturnsCorrectMsDeployPackagePublishProfile()
        {
            string packageProfile = PublishProfileCreatorFactory.CreatePublishProfile("Package").ToString();
            Assert.Equal(TestResources.MSDeployPackagePublishProfile, packageProfile);
        }

        [Fact]
        public void PublishProfileFactory_ReturnsCorrectFileSystemProfile()
        {
            string fileSystemProfile = PublishProfileCreatorFactory.CreatePublishProfile("FileSystem").ToString();
            Assert.Equal(TestResources.FileSystemPublishProfile, fileSystemProfile);
        }

    }
}
