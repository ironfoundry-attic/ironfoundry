namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using IronFoundry.Types;
    using Newtonsoft.Json;

    /*
     * Access token file is a dictionary of 
     {
        "http://api.vcap.me": "04085b0849221a6c756b652e62616b6b656e4074696572332e636f6d063a0645546c2b078b728b4e2219000104ec0d65746833caddd87eac48b0b2989604"},
        "http://api.vcap.me": "04085b0849221a6c756b652e62616b6b656e4074696572332e636f6d063a0645546c2b078b728b4e2219000104ec0d65746833caddd87eac48b0b2989604"}
     }
     */
    public class VcapCredentialManager
    {
        private const string TOKEN_FILE = ".vmc_token";
        private const string TARGET_FILE = ".vmc_target";

        private readonly string tokenFile;
        private readonly string targetFile;

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

        public VcapCredentialManager() : this((string)null) { }

        public VcapCredentialManager(Uri argCurrentTarget) : this((string)null)
        {
            if (null == argCurrentTarget)
            {
                throw new ArgumentNullException("argCurrentTarget");
            }
            SetTarget(argCurrentTarget);
        }

        public VcapCredentialManager(string argTokenJson, bool argShouldWrite) : this(argTokenJson)
        {
            shouldWrite = argShouldWrite;
        }

        public Uri CurrentTarget
        {
            get { return currentTarget ?? Constants.DEFAULT_LOCAL_TARGET; }
        }

        public string CurrentToken
        {
            get
            {
                string rv = null;
                AccessToken accessToken = getFor(CurrentTarget);
                if (null != accessToken)
                {
                    rv = accessToken.Token;
                }
                return rv;
            }
        }

        public void SetTarget(Uri argUri)
        {
            currentTarget = argUri;
        }

        public void SetTarget(string argUri)
        {
            currentTarget = new Uri(argUri);
        }

        public void RegisterToken(string argToken)
        {
            var accessToken = new AccessToken(CurrentTarget, argToken);
            tokenDict[accessToken.Uri] = accessToken;
            writeTokenFile();
        }

        public bool HasToken
        {
            get { return false == CurrentToken.IsNullOrWhiteSpace(); }
        }

        public void StoreTarget()
        {
            if (shouldWrite)
            {
                File.WriteAllText(targetFile, CurrentTarget.AbsoluteUriTrimmed()); // NB: trim end!
            }
        }

        private AccessToken getFor(Uri argUri)
        {
            AccessToken rv;
            tokenDict.TryGetValue(argUri, out rv);
            return rv;
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

        [System.Diagnostics.DebuggerStepThrough]
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
                // NB: ruby vmc writes target uris without trailing slash
                try
                {
                    Dictionary<string, string> tmp = tokenDict.ToDictionary(e => e.Key.AbsoluteUriTrimmed(), e => e.Value.Token);
                    File.WriteAllText(tokenFile, JsonConvert.SerializeObject(tmp));
                }
                catch (IOException)
                {
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private Uri readTargetFile()
        {
            Uri rv = null;

            try
            {
                string contents = File.ReadAllText(targetFile);
                rv = new Uri(contents);
            }
            catch (FileNotFoundException)
            {
            }
            catch (UriFormatException)
            {
                rv = Constants.DEFAULT_TARGET;
            }

            return rv;
        }
    }
}
