namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Types;

    /*
     * Access token file is a dictionary of 
     {
        "http://api.vcap.me": "04085b0849221a6c756b652e62616b6b656e4074696572332e636f6d063a0645546c2b078b728b4e2219000104ec0d65746833caddd87eac48b0b2989604"},
        "http://api.vcap.me": "04085b0849221a6c756b652e62616b6b656e4074696572332e636f6d063a0645546c2b078b728b4e2219000104ec0d65746833caddd87eac48b0b2989604"}
     }
     */
    public class AccessTokenManager : IDisposable
    {
        private const string TOKEN_FILE = ".vmc_token";
        private readonly string tokenFile = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), TOKEN_FILE);

        private bool disposed = false;
        private bool shouldWrite = true;

        private readonly IDictionary<string, AccessToken> tokenDict = new Dictionary<string, AccessToken>();

        public AccessTokenManager()
        {
            parseJson(readTokenFile());
        }

        public AccessTokenManager(string argTokenJson, bool argShouldWrite)
        {
            parseJson(argTokenJson);
            shouldWrite = argShouldWrite;
        }

        public AccessToken CreateFor(string argUri, string argJson)
        {
            parseJson(argJson);
            return GetFor(argUri);
        }

        public AccessToken GetFirst()
        {
            return tokenDict.FirstOrDefault().Value;
        }

        public AccessToken GetFor(string argUri)
        {
            return tokenDict[argUri];
        }

        private void parseJson(string argTokenJson)
        {
            Dictionary<string, string> allTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(argTokenJson);
            foreach (KeyValuePair<string, string> kvp in allTokens)
            {
                string uri = kvp.Key;
                string token = kvp.Value;
                tokenDict[uri] = new AccessToken(uri, token);
            }
        }

        private string readTokenFile()
        {
            return File.ReadAllText(tokenFile);
        }

        private void writeTokenFile()
        {
            File.WriteAllText(tokenFile, JsonConvert.SerializeObject(tokenDict));
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AccessTokenManager()
        {
            dispose(false);
        }

        private void dispose(bool argDisposing)
        {
            if (argDisposing && false == disposed)
            {
                disposed = true;
                if (shouldWrite)
                {
                    writeTokenFile();
                }
            }
        }
    }
}