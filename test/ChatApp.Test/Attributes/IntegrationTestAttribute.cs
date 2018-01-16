using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ChatApp.Test.Attrubutes
{
    [TraitDiscoverer("ChatApp.Test.Attrubutes.IntegrationTestDiscoverer", "ChatApp.Test")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class IntegrationTestAttribute : Attribute, ITraitAttribute
    {

    }

    public class IntegrationTestDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "IntegrationTest");
        }
    }
}