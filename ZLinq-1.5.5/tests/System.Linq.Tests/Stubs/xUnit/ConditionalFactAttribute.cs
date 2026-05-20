using System.Runtime.CompilerServices;
using ZLinq.Tests;

namespace Xunit;

internal class ConditionalFactAttribute : FactAttribute
{
    public ConditionalFactAttribute(Type type, string key, [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
         : base(sourceFilePath, sourceLineNumber)
    {
        bool isEnabled = ConditionalUtils.IsEnable(type, key);
        if (isEnabled)
            return;

        Skip = $"Skipped by reason: {type.Name}: {key}";
    }
}
