
namespace IronFoundry.Test
{
    using IronFoundry.Types;
    using Newtonsoft.Json;
    using Xunit;

    public class JsonTests
    {
        [Fact]
        public void Test_Deserialize_Detection()
        {
            string frameworkJson = @"
{
  'name': 'rails3',
  'runtimes': [
    {
      'name': 'ruby18',
      'version': '1.8.7',
      'description': 'Ruby 1.8.7'
    },
    {
      'name': 'ruby19',
      'version': '1.9.2p180',
      'description': 'Ruby 1.9.2'
    }
  ],
  'appservers': [
    {
      'name': 'thin',
      'description': 'Thin'
    }
  ],
  'detection': [
    {
      'config/application.rb': true
    },
    {
      'config/environment.rb': true
    }
  ]
}";
            var deserialized = JsonConvert.DeserializeObject<Framework>(frameworkJson);
        }
    }
}