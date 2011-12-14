namespace TestWebApp
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Nancy;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class HelloModule : NancyModule
    {
        public HelloModule()
        {
            Get["/"] = parameters => "Hello World";

            Get["/hello"] = parameters => "Hello World";

            Get["/hi"] = parameters => "Hi World";

            Get["/env"] = ENV;

            Get["/db"] = DB;
        }

        public Response DB(dynamic parameters)
        {
            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings["Default"];

            var sb = new StringBuilder("<html><body><pre>");
            try
            {
                if (null != css && false == String.IsNullOrWhiteSpace(css.ConnectionString))
                {
                    using (var conn = new SqlConnection(css.ConnectionString))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT GETDATE()";
                            sb.AppendLine(cmd.ExecuteScalar().ToString());
                        }
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT @@VERSION";
                            sb.AppendLine(cmd.ExecuteScalar().ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Append(ex.Message);
            }
            sb.Append("</pre></body></html>");
            return sb.ToString();
        }

        public Response ENV(dynamic parameters)
        {
            var sb = new StringBuilder("<html><body><pre>");

            RequestHeaders hdrs = Request.Headers;

            sb.AppendLine("Accept:");
            sb.AppendLines(hdrs.Accept);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("AcceptCharset:");
            sb.AppendLines(hdrs.AcceptCharset);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("AcceptEncoding:");
            sb.AppendLines(hdrs.AcceptEncoding);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("AcceptLanguage:");
            sb.AppendLines(hdrs.AcceptLanguage);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Authorization: ");
            sb.AppendLine(hdrs.Authorization);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("CacheControl:");
            sb.AppendLines(hdrs.CacheControl);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("ContentType: ");
            sb.AppendLine(hdrs.ContentType);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Date: ");
            sb.AppendLine(hdrs.Date.ToString());
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Host: ");
            sb.AppendLine(hdrs.Host);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("MaxForwards: ");
            sb.AppendLine(hdrs.MaxForwards.ToString());
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Referrer: ");
            sb.AppendLine(hdrs.Referrer);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("UserAgent:");
            sb.AppendLine(hdrs.UserAgent);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Server Host Name: ");
            sb.AppendLine(HostName);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("Server IP Addresses:");
            var ips = GetLocalIPAddresses().Select(i => i.ToString());
            sb.AppendLines(ips);
            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("Server AppSettings:");
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                string value = ConfigurationManager.AppSettings[key];

                sb.AppendFormat("Key: {0} ", key);

                if (key.StartsWith("VCAP_"))
                {
                    try
                    {
                        JObject parsed = JObject.Parse(value);
                        sb.AppendFormat("JSON: {0}", parsed.ToString(Formatting.Indented));
                    }
                    catch
                    {
                        sb.AppendFormat("Value: {0}", value);
                    }
                }
                else
                {
                    sb.AppendFormat("Value: {0}", value);
                }
                sb.AppendLine();
            }

            sb.Append("</pre></body></html>");
            return sb.ToString();
        }

        private static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            IPHostEntry host = Dns.GetHostEntry(HostName);
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
        }

        private static string HostName
        {
            get { return Dns.GetHostName(); }
        }
    }

    public static class StringBuilderExtensionMethods
    {
        public static void AppendLines(this StringBuilder argThis, IEnumerable<string> argLines)
        {
            if (null != argLines)
            {
                foreach (string line in argLines)
                {
                    argThis.AppendLine(line);
                }
            }
        }
    }
}