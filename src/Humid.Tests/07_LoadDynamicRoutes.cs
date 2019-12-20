using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;

namespace Humid.Tests
{
    using Xunit;
    using System;
    using NFluent;
    using System.Collections.Generic;

    using static Humid.Core;
    using static Humid.WebActions;

    public class SampleAreaThatCanBeLoaded : IArea
    {
        
    }
    
    public class LoadDynamicRoutes
    {
        [Fact]
        public void
            routes_can_be_grouped_with_an_interface()
        {
            var testContext = Defaults.Context.With(path:"/c",type:GET);
           
            var emptyRouter = new Router();

            IArea area = new SampleAreaThatCanBeLoaded();
                
            
            var router = emptyRouter
                         + (new Route("/a",OK) | Content("I'm A"))
                         + area;
          
            Assert.Equal(3, router.Routes.Count());

            var route = router.FindRoute(testContext);

            string content = null;
            int status = -1;

            (content,status) = route.ApplyPipeline(testContext); 
            
            Assert.Equal(200,status);
            Assert.Equal("I'm C",content);
        }

        [Fact]
        public void should_load_and_invoke_module()
        {
            var path =
                "/Users/rui/Data/Projects/dotnet_humid/_experimentdata/DynamicRouteModule/publish/DynamicRouteModule.dll";
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                var assloader = new AssemblyLoader();
                
                var asm = Assembly.LoadFile(file.FullName);
                var type = asm.GetType("DynamicRouteModule.Class1");
                Check.That(type).IsNotNull();
                var context = AssemblyLoadContext.GetLoadContext(asm);
                context.Resolving += (loadContext, assemblyName) =>
                {
                    var p = System.IO.Path.Combine(file.Directory.FullName, assemblyName.Name+".dll");
                    //var depAsm = loadContext.LoadFromAssemblyPath(file.Directory.FullName);
                    //return depAsm;
                    return assloader.LoadFromAssemblyPath(p);
                };
                var inst = Activator.CreateInstance(type);
                var method = type.GetMethod("ReadYaml");
                Check.That(method).IsNotNull();
                var sample = @"
- Name : rui";

                dynamic sut = method?.Invoke(inst, new object[]{sample});
                string name = sut[0]["Name"];
                Check.That(name).IsNotNull().And.IsEqualTo("rui");
            }
            else
            {
                throw new Exception("file not exists");
            }
            
        }
    }
    public class AssemblyLoader : AssemblyLoadContext
    {
        // Not exactly sure about this
        protected override Assembly Load(AssemblyName assemblyName)
        {
            try
            {
                var deps = DependencyContext.Default;
                var res = deps.CompileLibraries
                    .Where(d => d?.Name.Contains(assemblyName.Name) == true).ToList();
                var found = res.First();
                if (found != null)
                {
                    var assembly = Assembly.Load(new AssemblyName(found.Name));
                    return assembly;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}