using System;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DynamicRouteModule
{
    public class Class1
    {
        public object ReadYaml(string content)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<dynamic>(content);
        }
    }

    public class Contact
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Mail { get; set; }
    }
}