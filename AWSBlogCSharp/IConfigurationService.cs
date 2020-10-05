using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSBlogCSharp
{
    interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
