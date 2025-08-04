using AspNetCorePlugin.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace AspNetCorePlugin.Services
{
    public class DynamicViewFileProvider : IFileProvider
    {
        private readonly string _viewsRootPath;
        private readonly string _projectRoot;
        private readonly bool _rootExists;

        public DynamicViewFileProvider(string viewsRootPath)
        {
            _viewsRootPath = Path.GetFullPath(viewsRootPath);
            _projectRoot = Environment.CurrentDirectory;
            _rootExists = Directory.Exists(_viewsRootPath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!_rootExists)
            {
                return new NotFoundFileInfo(subpath);
            }

            var filePath = Path.Combine(_viewsRootPath, subpath.TrimStart('/'));
            var fileInfo = new PhysicalFileInfo(new FileInfo(filePath));

            if (!fileInfo.Exists)
            {
                filePath = Path.Combine(_projectRoot, subpath.TrimStart('/'));
                fileInfo = new PhysicalFileInfo(new FileInfo(filePath));
            }

            if (!fileInfo.Exists)
            {
                filePath = Path.Combine(_projectRoot, "Views", subpath.TrimStart('/'));
                fileInfo = new PhysicalFileInfo(new FileInfo(filePath));
            }

            if (fileInfo.Exists)
            {
                return fileInfo;
            }

            // Can't be found as a file, so might exist as a resource (ie, published).
            var normalizedSubpath = subpath.TrimStart('/').Replace("/", ".");
            normalizedSubpath = normalizedSubpath.Replace("Pages.", "Views.");
            var resourceName = $"AspNetCorePlugin.Views.{normalizedSubpath}";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                return new EmbeddedResourceFileInfo(resourceName, stream, Assembly.GetExecutingAssembly());
            }

            return new NotFoundFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var directoryPath = Path.Combine(_viewsRootPath, subpath.TrimStart('/'));
            if (Directory.Exists(directoryPath))
            {
                return new PhysicalDirectoryContents(directoryPath);
            }
            return NotFoundDirectoryContents.Singleton;
        }

        public IChangeToken Watch(string filter)
        {
            var fullPath = Path.Combine(_viewsRootPath, filter.TrimStart('/'));
            return new PhysicalFileProvider(_viewsRootPath).Watch(fullPath);
        }
    }
}