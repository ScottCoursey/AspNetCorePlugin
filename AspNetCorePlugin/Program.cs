using AspNetCorePlugin.Services;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Runtime.Loader;
using File = System.IO.File;

namespace AspNetCorePlugin
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var serviceList = new List<ServiceSet>
            {
                new ServiceSet
                {
                    Service = typeof(IPluggableService),
                    Implementation = typeof(PluggableService)
                }
            };

            var execFolder = GetExecutableFolder();
            var pluginFolder = Path.Combine(execFolder, "Plugins");
            var customViewsFolder = Path.Combine(execFolder, "Plugins", "CustomViews");
            var customStaticFolder = Path.Combine(execFolder, "Plugins", "CustomStatic"); // New folder for static files

            CreatePluginFolders(customViewsFolder, customStaticFolder);
            ExtractPluginContent(serviceList, pluginFolder, customViewsFolder, customStaticFolder);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor(); // For IHttpContextAccessor

            // Create dynamic provider for custom views
            var dynamicFileProvider = new DynamicViewFileProvider(customViewsFolder);

            // Create embedded provider for in-memory fallback (compiled/published)
            var embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            // Create composite: custom files first, then embedded resources, then physical root
            var compositeProvider = new CompositeFileProvider(dynamicFileProvider, embeddedFileProvider, builder.Environment.ContentRootFileProvider);

            // Set the app's content root provider to the composite
            builder.Environment.ContentRootFileProvider = compositeProvider;
            Console.WriteLine("Set app ContentRootFileProvider to composite: custom first, then embedded resources, then original root.");

            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("{0}");
                    //options.ViewLocationExpanders.Add(new CustomViewLocationExpander(customViewsFolder));
                })
                .AddRazorRuntimeCompilation(options =>
                {
                    // Clear and set to composite for runtime compilation
                    options.FileProviders.Clear();
                    options.FileProviders.Add(compositeProvider);
                });

            foreach (var service in serviceList)
            {
                builder.Services.AddScoped(service.Service, service.Implementation);
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            var staticDynamicProvider = new PhysicalFileProvider(customStaticFolder);
            var compositeStaticProvider = new CompositeFileProvider(staticDynamicProvider, app.Environment.WebRootFileProvider);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = compositeStaticProvider,
                RequestPath = "" // Serve from root
            });

            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        private static void ExtractPluginContent(List<ServiceSet> serviceList, string pluginFolder, string customViewsFolder, string customStaticFolder)
        {
            if (Directory.Exists(pluginFolder))
            {
                var context = new AssemblyLoadContext("Plugins", true);
                var pluginPaths = Directory.GetFiles(pluginFolder, "*.dll");
                foreach (var pluginPath in pluginPaths)
                {
                    try
                    {
                        var assembly = context.LoadFromAssemblyPath(pluginPath);
                        var classes = assembly.GetTypes()
                            .Where(t => t.IsClass)
                            .ToArray();

                        foreach (var classType in classes)
                        {
                            var serviceSet = serviceList.FirstOrDefault(sl => sl.Implementation.Name == classType.Name);
                            if (serviceSet != null)
                            {
                                serviceSet.Implementation = classType;
                            }
                        }

                        var resources = assembly.GetManifestResourceNames();
                        foreach (var resource in resources)
                        {
                            var relativePath = resource;
                            if (relativePath.StartsWith("ClientPlugin."))
                            {
                                relativePath = relativePath.Substring("ClientPlugin.".Length);
                            }

                            if (relativePath.StartsWith("wwwroot."))
                            {
                                relativePath = relativePath.Substring("wwwroot.".Length);
                            }

                            var lastDot = relativePath.LastIndexOf('.');
                            if (lastDot >= 0)
                            {
                                var pathPart = relativePath.Substring(0, lastDot).Replace(".", "/");
                                var extension = relativePath.Substring(lastDot);
                                relativePath = pathPart + extension;
                            }
                            else
                            {
                                relativePath = relativePath.Replace(".", "/");
                            }

                            var destinationFolder = relativePath.EndsWith(".cshtml") ? customViewsFolder : customStaticFolder;
                            var destinationPath = Path.Combine(destinationFolder, relativePath);
                            var dir = Path.GetDirectoryName(destinationPath);
                            Directory.CreateDirectory(dir);

                            using var stream = assembly.GetManifestResourceStream(resource);
                            if (stream != null)
                            {
                                using (var fileStream = File.Create(destinationPath))
                                {
                                    stream.CopyTo(fileStream);
                                    fileStream.Flush();
                                }
                                if (File.Exists(destinationPath))
                                {
                                    for (int attempt = 0; attempt < 3; attempt++)
                                    {
                                        try
                                        {
                                            Thread.Sleep(100);
                                            var content = File.ReadAllText(destinationPath);
                                            break;
                                        }
                                        catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                                        {
                                            if (attempt == 2) Console.WriteLine($"Failed to read {destinationPath} after 3 attempts");
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error reading {destinationPath}: {ex.Message}");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing plugin {pluginPath}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Plugin folder {pluginFolder} does not exist");
            }
        }

        private static void CreatePluginFolders(string customViewsFolder, string customStaticFolder)
        {
            if (!Directory.Exists(customViewsFolder))
            {
                Directory.CreateDirectory(customViewsFolder);
            }
            if (!Directory.Exists(customStaticFolder))
            {
                Directory.CreateDirectory(customStaticFolder);
            }
        }

        public static string GetExecutableFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}