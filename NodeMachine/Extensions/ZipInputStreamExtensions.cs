using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace NodeMachine.Extensions
{
    public static class ZipInputStreamExtensions
    {
        public static void UnpackToDirectory(this ZipInputStream zip, string path, IFileSystem fs)
        {
            ZipEntry theEntry;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                string directoryName = fs.Path.GetDirectoryName(theEntry.Name);
                string fileName = fs.Path.GetFileName(theEntry.Name);

                //Create directory for this entry
                if (!string.IsNullOrEmpty(directoryName))
                    fs.Directory.CreateDirectory(fs.Path.Combine(path, directoryName));

                //If there is no filename this was a directory entry, skip to the next entry
                if (fileName == String.Empty)
                    continue;

                //Copy the file from zip into filesystem
                using (Stream streamWriter = fs.File.Create(fs.Path.Combine(path, theEntry.Name)))
                {
                    zip.CopyTo(streamWriter);
                    streamWriter.Flush();
                }
            }

            Thread.Sleep(100);
        }
    }
}
