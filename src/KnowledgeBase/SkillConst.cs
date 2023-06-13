using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SS.SemanticKernel.Extensions.KnowledgeBase
{
    public class SkillConst
    {
        public static Dictionary<string, string> Constants = new Dictionary<string, string>();

        public static string Load(string key)
        {
            if (Constants.ContainsKey(key))
            {
                return Constants[key];
            }
            var result = Assembly.GetExecutingAssembly()
                     .LoadEmbeddedResource($"SS.SemanticKernel.Extensions.Skills.{key}");
            Constants.Add(key, result);
            return result;
        }


    }
}
