using DotLiquid;
using System.IO;

namespace Humid.DotLiquid
{
   
    public class LiquidTemplateEngine : ITemplateEngine
    {
        public string RenderTemplate(Context context, string name, object model)
        {
            var templateSource = RenderTemplate(context, name);
            
            Template template = Template.Parse(templateSource); 
            return template.Render(Hash.FromAnonymousObject((dynamic)model)); 
        }

        public string RenderTemplate(Context context, string name)
        {
            var rootPath = context.Server["Site:PhysicalFullPath"];
            var templateRelativePath = Path.Combine("templates",name + ".html");
            var templatePath = Path.Combine(rootPath,templateRelativePath);
            if(File.Exists(templatePath))
                return File.ReadAllText(templatePath);
            
            return null;
        }
    }
}
