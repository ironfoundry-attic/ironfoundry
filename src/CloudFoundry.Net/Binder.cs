using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace CloudFoundry.Net
{
    public class Binder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            ResolveEventHandler handler = new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.AssemblyResolve += handler;

            Type returnedType;
            try
            {
                AssemblyName asmName = new AssemblyName(assemblyName);
                var assembly = Assembly.Load(asmName);
                returnedType = assembly.GetType(typeName);
            }
            catch
            {
                returnedType = null;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= handler;
            }

            return returnedType;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string truncatedAssemblyName = args.Name.Split(',')[0];
            Assembly assembly = Assembly.Load(truncatedAssemblyName);
            return assembly;
        }
    }
}
