using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace STDTBot.Services
{
    public class ConfigService
    {
        public static IConfigurationRoot GetConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Utilities.GetBasePath())
                    .AddJsonFile(Constants.ConfigFileName, false, true);

                return builder.Build();
            }
            catch
            {
                throw new FileNotFoundException();
            }
        }

    }
}
