using IronFoundry.Properties;

namespace IronFoundry.Vcap
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Net;
    using IronFoundry.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    public abstract class VcapRequestBase
    {
        private static readonly ushort[] VMC_HTTP_ERROR_CODES =
        {
            (ushort)HttpStatusCode.BadRequest,              // 400
            (ushort)HttpStatusCode.Forbidden,               // 403
            (ushort)HttpStatusCode.NotFound,                // 404
            (ushort)HttpStatusCode.MethodNotAllowed,        // 405
            (ushort)HttpStatusCode.InternalServerError,     // 500
            (ushort)HttpStatusCode.NotImplemented,          // 501
            (ushort)HttpStatusCode.BadGateway,              // 502
            (ushort)HttpStatusCode.ServiceUnavailable,      // 503
            (ushort)HttpStatusCode.GatewayTimeout,          // 504
            (ushort)HttpStatusCode.HttpVersionNotSupported, // 505
        };

        private readonly VcapCredentialManager credentialManager;
        private readonly RestClient client;
        private readonly string proxyUserEmail;

        private string requestHostHeader;
        protected RestRequest request;

        protected VcapRequestBase(string proxyUserEmail, VcapCredentialManager credentialManager)
        {
            this.proxyUserEmail = proxyUserEmail;
            this.credentialManager = credentialManager;
            client = BuildClient();
        }

        protected VcapRequestBase(string proxyUserEmail, VcapCredentialManager credentialManager, bool useAuthentication, Uri uri = null)
        {
            this.proxyUserEmail = proxyUserEmail;
            this.credentialManager = credentialManager;
            client = BuildClient(useAuthentication, uri);
        }

        public string ErrorMessage { get; protected set; }

        public IRestResponse Execute()
        {
            IRestResponse response = client.Execute(request);
            ProcessResponse(response);
            ErrorMessage = response.ErrorMessage;
            return response;
        }

        public TResponse Execute<TResponse>()
        {
            IRestResponse response = client.Execute(request);
            ProcessResponse(response);
            ErrorMessage = response.ErrorMessage;
            if (response.Content.IsNullOrWhiteSpace())
            {
                return default(TResponse);
            }
            else
            {
                return JsonConvert.DeserializeObject<TResponse>(response.Content);
            }
        }

        protected RestRequest BuildRequest(Method method, params object[] args)
        {
            return BuildRequest(method, DataFormat.Json, args);
        }

        protected RestRequest BuildRequest(Method method, DataFormat format, params object[] args)
        {
            RestRequest rv = BuildRestRequest(method);
            rv.RequestFormat = format;
            return ProcessRequestArgs(rv, args);
        }

        private RestRequest BuildRestRequest(Method method)
        {
            var serializer = new NewtonsoftJsonSerializer();
            var rv = new RestRequest
            {
                Method = method,
                JsonSerializer = serializer,
            };

            if (null != requestHostHeader)
            {
                rv.AddHeader("Host", requestHostHeader);
            }

            return rv;
        }

        private RestClient BuildClient()
        {
            return BuildClient(true);
        }

        private RestClient BuildClient(bool useAuth, Uri uri = null)
        {
            Uri currentTargetUri = uri;
            if (null == currentTargetUri)
            {
                currentTargetUri = credentialManager.CurrentTarget;
            }

            string baseUrl = currentTargetUri.AbsoluteUri;
            if (null != credentialManager.CurrentTargetIP)
            {
                baseUrl = String.Format("{0}://{1}", Uri.UriSchemeHttp, credentialManager.CurrentTargetIP.ToString());
                requestHostHeader = currentTargetUri.Host;
            }

            var deserializer = new NewtonsoftJsonDeserializer();
            var rv = new RestClient
            {
                BaseUrl = baseUrl,
                FollowRedirects = false,
            };
            rv.RemoveHandler(NewtonsoftJsonDeserializer.JsonContentType);
            rv.AddHandler(NewtonsoftJsonDeserializer.JsonContentType, deserializer);

            if (useAuth && credentialManager.HasToken)
            {
                rv.AddDefaultHeader("AUTHORIZATION", credentialManager.CurrentToken);
            }

            if (false == proxyUserEmail.IsNullOrWhiteSpace())
            {
                rv.AddDefaultHeader("PROXY-USER", proxyUserEmail);
            }

            return rv;
        }

        private void ProcessResponse(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw new VcapException(
                    String.Format(Resources.VcapRequest_RestException_Fmt, client.BaseUrl, request.Resource),
                    response.ErrorException);
            }

            if (VMC_HTTP_ERROR_CODES.Contains((ushort)response.StatusCode))
            {
                Exception parseException = null;
                string errorMessage = null;
                if (response.Content.IsNullOrWhiteSpace())
                {
                    errorMessage = String.Format("Error (HTTP {0})", response.StatusCode);
                }
                else
                {
                    try
                    {
                        JObject parsed = JObject.Parse(response.Content);
                        JToken codeToken;
                        JToken descToken;
                        if (parsed.TryGetValue("code", out codeToken) && parsed.TryGetValue("description", out descToken))
                        {
                            errorMessage = String.Format("Error {0}: {1}", codeToken, descToken);
                        }
                        else
                        {
                            errorMessage = String.Format("Error (HTTP {0}): {1}", response.StatusCode, response.Content);
                        }
                    }
                    catch (Exception ex)
                    {
                        parseException = ex;
                    }
                }

                if (null != parseException)
                {
                    errorMessage = String.Format("Error parsing (HTTP {0}):{1}{2}{3}{4}",
                        response.StatusCode, Environment.NewLine, response.Content, Environment.NewLine, parseException.Message);
                    throw new VcapException(errorMessage, parseException);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest ||
                        response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new VcapNotFoundException(errorMessage);
                    }
                    else
                    {
                        throw new VcapException(errorMessage);
                    }
                }
            }
        }

        private static RestRequest ProcessRequestArgs(RestRequest request, params object[] args)
        {
            if (null == request)
            {
                throw new ArgumentNullException("request");
            }
            if (false == args.IsNullOrEmpty())
            {
                request.Resource = String.Join("/", args).Replace("//", "/");
            }
            return request;
        }

        internal RestClient Client
        {
            get { return client; }
        }

        internal RestRequest Request
        {
            get { return request; }
        }

        internal string RequestHostHeader
        {
            get { return requestHostHeader; }
        }
    }

    public class VcapRequest : VcapRequestBase
    {
        public VcapRequest(string proxyUserEmail, VcapCredentialManager credMgr, params object[] resourceParams)
            : this(proxyUserEmail, credMgr, true, null, resourceParams) { }

        public VcapRequest(string proxyUserEmail, VcapCredentialManager credMgr,
            bool useAuth, Uri uri, params object[] resourceParams) : base(proxyUserEmail, credMgr, useAuth, uri)
        {
            request = BuildRequest(Method.GET, resourceParams);
        }
    }

    public class VcapJsonRequest : VcapRequestBase
    {
        public VcapJsonRequest(string proxyUserEmail, VcapCredentialManager credMgr,
            Method method, params string[] resourceParams) : base(proxyUserEmail, credMgr)
        {
            request = BuildRequest(method, DataFormat.Json, resourceParams);
        }

        public void AddBody(object body)
        {
            this.request.AddBody(body);
        }

        public void AddParameter(string param, object value)
        {
            this.request.AddParameter(param, value);
        }

        public void AddFile(string name, string path)
        {
            this.request.AddFile(name, path);
        }
    }
}