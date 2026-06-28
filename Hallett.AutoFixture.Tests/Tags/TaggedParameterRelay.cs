using AutoFixture.Kernel;
using System.Reflection;

namespace Hallett.AutoFixture.Tests.Tags
{
    public class TaggedParameterRelay : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var parameterInfo = (ParameterInfo)request;
            var tag = TaggedParameterAttribute.GetTag(parameterInfo);
            return context.Resolve(new TaggedRequest(tag, parameterInfo.ParameterType));
        }
    }
}
