using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishProfileContracts
{
    public interface IPublishProfile
    {
        string WebPublishMethod { get; }

        string LastUsedBuildConfiguration { get; }

        string LastUsedPlatform { get; }

        string PublishFramework { get; }
    }
}
