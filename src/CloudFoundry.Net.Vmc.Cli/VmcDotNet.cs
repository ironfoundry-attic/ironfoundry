using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using CloudFoundry.Net.Vmc;

namespace CloudFoundry.Net.Vmc.Cli
{
    public class VmcDotNet
    {
        public static string url = "";
        public static string accesstoken = "";
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Currently only login, info and push commands are supported");
            }
             else if (args[0] == "Target")
            {
                
                WriteTargetFile(args[1]);
                Console.WriteLine("Target set to: {0}", args[1]);
            }

            else if (args[0] == "Info")
            {
                url = ReadTargetFile();
                accesstoken = ReadTokenFile();
                if (url == "")
                {
                    Console.WriteLine("Please set a target");
                }
                else if (accesstoken.Length > 0)
                {
                    JObject obj = JObject.Parse(accesstoken);
                    VmcManager cfm = new VmcManager();
                    cfm.AccessToken = (string)obj.Value<string>(url);
                    cfm.URL = url;
                    Console.WriteLine(cfm.Info());
                }
                else
                {
                    VmcManager cfm = new VmcManager();
                    cfm.URL = url;
                    Console.WriteLine(cfm.Info());
                }
            }
            else if (args[0] == "Login")
            {
                url = ReadTargetFile();
                if (url == "")
                {
                    Console.WriteLine("Please set a target");
                }
                else
                {
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    Console.Write("Password: ");
                    string password = Console.ReadLine(); //should figure out how to turn this to *
                    VmcManager cfm = new VmcManager();
                    cfm.URL = url;
                    string returnvalue = cfm.LogIn(email, password);
                    if (returnvalue.Contains("token"))
                    {
                        WriteTokenFile(returnvalue.Replace("token",url));
                    }
                    else
                    {
                        Console.WriteLine("Login Failed");
                    }
                }
            }
            else if (args[0] == "Push")
            {
                url = ReadTargetFile();
                accesstoken = ReadTokenFile();
                if (url == "")
                {
                    Console.WriteLine("Please set a target");
                }
                else if (accesstoken.Length > 0)
                {
                    Console.Write("App Name: ");
                    string appname = "johndoe" ; //Console.ReadLine();
                    Console.Write("Directory Location (ex. c:\\appdir ): ");
                    string dirlocation = "c:\\testapp"; //Console.ReadLine();
                    Console.Write("Deployed URL (ex. xyz.cloudfoundry.com) ");
                    string deployedURL = "johndoe.cloudfoundry.com"; //Console.ReadLine();
                    Console.Write("Type of Application: (ex. sinatra, java) ");
                    string apptype = "sinatra"; //Console.ReadLine();
                    Console.Write("Memory Reservation: (ex. 128) ");
                    string memalloc = "128"; //Console.ReadLine();

                    JObject obj = JObject.Parse(accesstoken);
                    VmcManager cfm = new VmcManager();
                    cfm.AccessToken = (string)obj.Value<string>(url);
                    cfm.URL = url;
                    Console.Write("Return data: ");
                    Console.WriteLine(cfm.Push(appname, deployedURL, dirlocation, apptype, memalloc));
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Please login first.");
                }

            }
        }

        static string ReadTargetFile() {
            StreamReader infile = new StreamReader(Environment.GetEnvironmentVariable("USERPROFILE") + "\\.vmc_target");
            return infile.ReadLine();
        }

        static void WriteTargetFile(string target){
            StreamWriter outfile = new StreamWriter(Environment.GetEnvironmentVariable("USERPROFILE") + "\\.vmc_target");
            outfile.WriteLine(target);
            outfile.Close();
        
        }
        static string ReadTokenFile()
        {
            StreamReader infile = new StreamReader(Environment.GetEnvironmentVariable("USERPROFILE") + "\\.vmc_token");
            return infile.ReadLine();
        }

        static void WriteTokenFile(string token)
        {
            StreamWriter outfile = new StreamWriter(Environment.GetEnvironmentVariable("USERPROFILE") + "\\.vmc_token");
            outfile.WriteLine(token);
            outfile.Close();

        }
    }
}
