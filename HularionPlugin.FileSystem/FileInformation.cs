#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionPlugin.FileSystem
{
    /// <summary>
    /// Contains information about a file.
    /// </summary>
    public class FileInformation
    {

        /// <summary>
        /// The name of the file without the extension.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The file's extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// The full directory path.
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// The time the file was created.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The time the file was created.
        /// </summary>
        public DateTime CreationDateUtc { get; set; }

        /// <summary>
        /// The time the file was last updated.
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// The time the file was last updated.
        /// </summary>
        public DateTime UpdateDateUtc { get; set; }

        /// <summary>
        /// The size of the file.
        /// </summary>
        public long Size { get; set; }

        public FileInformation()
        {

        }

        public FileInformation(string path)
        {

            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The provided path {0} is not valid. 394lUsnFRE2GatDjRSr0Mg", path);
            }

            var info = new FileInfo(path);
            CreationDate = info.CreationTime;
            CreationDateUtc = info.CreationTimeUtc;
            UpdateDate = info.LastWriteTime;
            UpdateDateUtc = info.LastWriteTimeUtc;
            Size = info.Length;

            Path = path.Split(Constants.DirectoryDelimiter);
            Name = Path[Path.Length - 1];

            if (Name.Contains(Constants.ExtensionDelimiter))
            {
                var index = Name.LastIndexOf(Constants.ExtensionDelimiter);
                Extension = Name.Substring(index + 1, Name.Length - index - 1);
                Name = Name.Substring(0, index);
            }
        }

    }
}
