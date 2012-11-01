using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sanguosha.UI.Effects
{
    internal static class Global
    {
        public static Uri MakePackUri(string relativeFile)
        {

            StringBuilder uriString = new StringBuilder(); ;
#if !SILVERLIGHT
            uriString.Append("pack://application:,,,");
#endif
            uriString.Append("/" + AssemblyShortName + ";component/" + relativeFile);
            return new Uri(uriString.ToString(), UriKind.RelativeOrAbsolute);
        }

        private static string AssemblyShortName
        {
            get
            {
                if (_assemblyShortName == null)
                {
                    Assembly a = typeof(Global).Assembly;

                    // Pull out the short name.
                    _assemblyShortName = a.ToString().Split(',')[0];
                }

                return _assemblyShortName;
            }
        }

        private static string _assemblyShortName;
    }
}
