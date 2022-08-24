using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingApp1._0.Helpers
{
    public class LocalFilesProcessing
    {
        public static Uri GetAbsoluteUrlForLocalFile(string path)
        {
            var fileUri = new Uri(path, UriKind.RelativeOrAbsolute);

            if (fileUri.IsAbsoluteUri)
            {
                return fileUri;
            }
            else
            {
                var baseUri = new Uri(Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar);

                return new Uri(baseUri, fileUri);
            }
        }
    }
}
