using System.Diagnostics;
using Xunit;

namespace EventPipeline.Tests;

public sealed class FactDebugOnlyAttribute : FactAttribute
{
    public FactDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached)
            Skip = "Only runs when debugger is attached";
    }
}
