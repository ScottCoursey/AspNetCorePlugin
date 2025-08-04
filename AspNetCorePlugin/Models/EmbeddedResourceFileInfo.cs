using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace AspNetCorePlugin.Models
{
    internal class EmbeddedResourceFileInfo : IFileInfo
    {
        private readonly string _name;
        private readonly Stream _stream;
        private readonly Assembly _assembly;

        public EmbeddedResourceFileInfo(string name, Stream stream, Assembly assembly)
        {
            _name = name;
            _stream = stream;
            _assembly = assembly;
        }

        public bool Exists => true;
        public long Length => _stream.Length;
        public string PhysicalPath => null;
        public string Name => Path.GetFileName(_name);
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return _assembly.GetManifestResourceStream(_name);
        }
    }
}