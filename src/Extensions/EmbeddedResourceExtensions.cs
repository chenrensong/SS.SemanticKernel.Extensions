using Microsoft.SemanticKernel.Security;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SS.SemanticKernel.Extensions
{
    public static class EmbeddedResourceExtensions
    {

   
        public static string LoadEmbeddedResource(this Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
