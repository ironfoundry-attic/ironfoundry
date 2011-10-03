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
    public class VcapCredentialManager : IDisposable
    {
        private const string TOKEN_FILE = ".vmc_token";
        private const string TARGET_FILE = ".vmc_target";

        private readonly string tokenFile;
        private readonly string targetFile;

        private bool disposed = false;
        private bool shouldWrite = true;

        private readonly IDictionary<string, AccessToken> tokenDict = new Dictionary<string, AccessToken>();

        private Uri currentTarget;

        private VcapCredentialManager(string argJson)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            tokenFile = Path.Combine(userProfilePath, TOKEN_FILE);
            targetFile = Path.Combine(userProfilePath, TARGET_FILE);

            if (argJson.IsNullOrWhiteSpace())
            {
                parseJson(readTokenFile());
            }
            else
            {
                parseJson(argJson);
            }

            currentTarget = readTargetFile();
        }

        public VcapCredentialManager() : this(null) { }

        public VcapCredentialManager(string argTokenJson, bool argShouldWrite) : this(argTokenJson)
        {
            shouldWrite = argShouldWrite;
        }

        public Uri CurrentTarget
        {
            get { return currentTarget; }
        }

        public string CurrentToken
        {
            get { return GetFor(CurrentTarget.AbsoluteUri).Token; }
        }

        public void SetTarget(string argUri)
        {
            currentTarget = new Uri(argUri);
            writeTargetFile();
        }

        public void RegisterFor(string argUri, string argJson)
        {
            parseJson(argJson, true);
        }

        public AccessToken GetFirst()
        {
            return tokenDict.FirstOrDefault().Value;
        }

        public AccessToken GetFor(string argUri)
        {
            return tokenDict[argUri];
        }

        public bool HasToken
        {
            get { return false == CurrentToken.IsNullOrWhiteSpace(); }
        }

        private void parseJson(string argTokenJson, bool argShouldWrite = false)
        {
            Dictionary<string, string> allTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(argTokenJson);
            foreach (KeyValuePair<string, string> kvp in allTokens)
            {
                string uri = kvp.Key;
                string token = kvp.Value;
                tokenDict[uri] = new AccessToken(uri, token);
            }
            if (argShouldWrite)
            {
                writeTokenFile();
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

        private Uri readTargetFile()
        {
            string contents = File.ReadAllText(targetFile);
            return new Uri(contents);
        }

        private void writeTargetFile()
        {
            File.WriteAllText(targetFile, currentTarget.AbsoluteUri);
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~VcapCredentialManager()
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