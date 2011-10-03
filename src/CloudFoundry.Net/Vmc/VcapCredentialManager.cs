namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        private static readonly Uri DEFAULT_TARGET = new Uri("http://api.vcap.me");

        private readonly string tokenFile;
        private readonly string targetFile;

        private bool disposed = false;
        private bool shouldWrite = true;

        private readonly IDictionary<Uri, AccessToken> tokenDict = new Dictionary<Uri, AccessToken>();

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
            get { return currentTarget ?? DEFAULT_TARGET; }
        }

        public string CurrentToken
        {
            get
            {
                string rv = null;
                AccessToken accessToken = GetFor(CurrentTarget.AbsoluteUri);
                if (null != accessToken)
                {
                    rv = accessToken.Token;
                }
                return rv;
            }
        }

        public void SetTarget(string argUri)
        {
            currentTarget = new Uri(argUri);
        }

        public void RegisterFor(string argUri, string argJson)
        {
            parseJson(argJson, true);
        }

        public AccessToken GetFor(string argUri)
        {
            var uri = new Uri(argUri);
            AccessToken rv;
            tokenDict.TryGetValue(uri, out rv);
            return rv;
        }

        public bool HasToken
        {
            get { return false == CurrentToken.IsNullOrWhiteSpace(); }
        }

        public void StoreTarget()
        {
            if (shouldWrite)
            {
                File.WriteAllText(targetFile, currentTarget.AbsoluteUri);
            }
        }

        private void parseJson(string argTokenJson, bool argShouldWrite = false)
        {
            if (false == argTokenJson.IsNullOrWhiteSpace())
            {
                Dictionary<string, string> allTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(argTokenJson);
                foreach (KeyValuePair<string, string> kvp in allTokens)
                {
                    string uriStr = kvp.Key;
                    string token = kvp.Value;
                    var accessToken = new AccessToken(uriStr, token);
                    tokenDict[accessToken.Uri] = accessToken;
                }
                if (argShouldWrite)
                {
                    writeTokenFile();
                }
            }
        }

        private string readTokenFile()
        {
            string rv = null;

            try
            {
                rv = File.ReadAllText(tokenFile);
            }
            catch (FileNotFoundException) { }

            return rv;
        }

        private void writeTokenFile()
        {
            if (shouldWrite)
            {
                File.WriteAllText(tokenFile, JsonConvert.SerializeObject(tokenDict));
            }
        }

        private Uri readTargetFile()
        {
            Uri rv = null;

            try
            {
                string contents = File.ReadAllText(targetFile);
                rv = new Uri(contents);
            }
            catch (FileNotFoundException) { }

            return rv;
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
                writeTokenFile();
            }
        }
    }
}