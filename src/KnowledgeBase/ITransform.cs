using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel
{
    public interface ITransform
    {
        /// <summary>
        /// Runs the transform operation over the given resource.
        /// </summary>
        /// <param name="input">The resource to transform</param>
        /// <returns>One or more transformed resources that are the result of this transformation.</returns>
        Task<IEnumerable<ContentResource>> Run(ContentResource input);
    }
}
