namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Extensions;
    using Models;
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
        private const string TokenFile = ".vmc_token";
        private const string TargetFile = ".vmc_target";

        private readonly string tokenFile;
        private readonly string targetFile;

        private readonly bool shouldWrite = true;

        private readonly IDictionary<Uri, AccessToken> tokenDict = new Dictionary<Uri, AccessToken>();

        private Uri currentTarget;
        private IPAddress currentTargetIP;
        private int currentTargetPort = 80;

        public static Func<string, string> FileReaderFunc = fileName => File.ReadAllText(fileName);
        public static Action<string, string> FileWriterAction = (fileName, text) => File.WriteAllText(fileName, text);

        public VcapCredentialManager()
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            tokenFile = Path.Combine(userProfilePath, TokenFile);
            targetFile = Path.Combine(userProfilePath, TargetFile);

            ParseJson(ReadTokenFile());

            currentTarget = ReadTargetFile();
        }

        public VcapCredentialManager(Uri currentTarget) : this()
        {
            if (null == currentTarget)
            {
                throw new ArgumentNullException("currentTarget");
            }
            SetTarget(currentTarget);
        }

        public VcapCredentialManager(Uri currentTarget, IPAddress currentTargetIP, int currentTargetPort = 80) : this()
        {
            if (null == currentTarget)
            {
                throw new ArgumentNullException("currentTarget");
            }
            if (null == currentTargetIP)
            {
                throw new ArgumentNullException("currentTargetIP");
            }
            SetTarget(currentTarget, currentTargetIP);
            this.currentTargetPort = currentTargetPort;
        }

        public Uri CurrentTarget
        {
            get { return currentTarget ?? Constants.DEFAULT_LOCAL_TARGET; }
        }

        public IPAddress CurrentTargetIP
        {
            get { return currentTargetIP; }
        }

        public int CurrentTargetPort
        {
            get { return currentTargetPort; }
        }

        public string CurrentToken
        {
            get
            {
                string rv = null;
                AccessToken accessToken = GetFor(CurrentTarget);
                if (null != accessToken)
                {
                    rv = accessToken.Token;
                }
                return rv;
            }
        }

        public void SetTarget(string uri)
        {
            SetTarget(new Uri(uri));
        }

        public void SetTarget(Uri uri)
        {
            currentTarget = uri;
            currentTargetIP = null;
        }

        public void SetTarget(Uri uri, IPAddress ip)
        {
            if (null == uri)
            {
                throw new ArgumentNullException("uri");
            }
            if (null == ip)
            {
                throw new ArgumentNullException("ip");
            }
            currentTarget = uri;
            currentTargetIP = ip;
        }

        public void RegisterToken(string token)
        {
            var accessToken = new AccessToken(CurrentTarget, token);
            tokenDict[accessToken.Uri] = accessToken;
            WriteTokenFile();
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

        private AccessToken GetFor(Uri uri)
        {
            AccessToken rv;
            tokenDict.TryGetValue(uri, out rv);
            return rv;
        }

        private void ParseJson(string tokenJson)
        {
            if (false == tokenJson.IsNullOrWhiteSpace())
            {
                Dictionary<string, string> allTokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
                foreach (KeyValuePair<string, string> kvp in allTokens)
                {
                    string uriStr = kvp.Key;
                    string token = kvp.Value;
                    var accessToken = new AccessToken(uriStr, token);
                    tokenDict[accessToken.Uri] = accessToken;
                }

                WriteTokenFile();
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private string ReadTokenFile()
        {
            string rv = null;

            try
            {
                rv = FileReaderFunc(tokenFile);
            }
            catch (FileNotFoundException) { }

            return rv;
        }

        private void WriteTokenFile()
        {
            if (shouldWrite)
            {
                // NB: ruby vmc writes target uris without trailing slash
                try
                {
                    Dictionary<string, string> tmp = tokenDict.ToDictionary(e => e.Key.AbsoluteUriTrimmed(), e => e.Value.Token);
                    FileWriterAction(tokenFile, JsonConvert.SerializeObject(tmp));
                }
                catch (IOException)
                {
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private Uri ReadTargetFile()
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