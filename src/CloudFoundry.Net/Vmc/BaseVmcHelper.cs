namespace CloudFoundry.Net.Vmc
{
    using System;
    using RestSharp;
    using Types;

    public abstract class BaseVmcHelper
    {
        private static readonly string[] argFormats = new[]
            {
                null,              // 0
                "{0}",             // 1
                "{0}/{1}",         // 2
                "{0}/{1}/{2}",     // 3
                "{0}/{1}/{2}/{3}", // 4
            };

        protected RestResponse executeRequest(RestClient argClient, RestRequest argRequest)
        {
            RestResponse response = argClient.Execute(argRequest);

            // TODO process error codes!

            return response;
        }

        protected TResponse executeRequest<TResponse>(RestClient argClient, RestRequest argRequest)
            where TResponse: new()
        {
            RestResponse<TResponse> response = argClient.Execute<TResponse>(argRequest);

            // TODO process error codes!

            return response.Data;
        }

        protected RestRequest buildRequest(Method argMethod, params string[] args)
        {
            var rv = new RestRequest
            {
                Method = argMethod,
            };
            return processRequestArgs(rv, args);
        }

        protected RestRequest buildRequest(Method argMethod, DataFormat argFormat, params string[] args)
        {
            var rv = new RestRequest
            {
                Method = argMethod,
                RequestFormat = argFormat,
            };
            return processRequestArgs(rv, args);
        }

        /// <summary>
        /// Uses current target and current token
        /// </summary>
        protected RestClient buildClient()
        {
            return doBuildClient();
        }

        /// <summary>
        /// Uses argument target and associated token
        /// </summary>
        protected RestClient buildClient(Uri argUri)
        {
            return doBuildClient(argUri);
        }

        protected RestClient buildClientNoAuth(Uri argUri)
        {
            return doBuildClient(argUri, false);
        }

        /// <summary>
        /// Uses url and token from cloud
        /// </summary>
        protected RestClient buildClient(Cloud argCloud)
        {
            return doBuildClient(new Uri(argCloud.Url));
        }

        private static RestClient doBuildClient(Uri argUri = null, bool argUseAuth = true)
        {
            /*
             * TODO: newing this up each time entails disk hits
             */
            VcapCredentialManager credMgr;
            if (null == argUri)
            {
                credMgr = new VcapCredentialManager();
            }
            else
            {
                credMgr = new VcapCredentialManager(argUri);
            }

            var rv = new RestClient
            {
                BaseUrl = credMgr.CurrentTarget.AbsoluteUri,
                FollowRedirects = false,
            };

            if (argUseAuth && credMgr.HasToken)
            {
                rv.AddDefaultHeader("AUTHORIZATION", credMgr.CurrentToken);
            }

            return rv;
        }

        private static RestRequest processRequestArgs(RestRequest argRequest, params string[] args)
        {
            if (null == argRequest)
            {
                throw new ArgumentNullException("argRequest");
            }
            if (false == args.IsNullOrEmpty())
            {
                if (args.Length > argFormats.Length)
                {
                    throw new InvalidOperationException();
                }
                argRequest.Resource = String.Format(argFormats[args.Length], args);
            }
            return argRequest;
        }
    }
}