namespace PayGate.AspNetCore.Tests;

using System.Reflection;

/// <summary>Placeholder until the phase that adds real code: proves discovery, references and coverage collection work.</summary>
public sealed class AssemblySmokeTests
{
    [Theory]
    [InlineData("PayGate.AspNetCore")]
    public void Referenced_assembly_loads(string assemblyName)
    {
        var assembly = Assembly.Load(new AssemblyName(assemblyName));

        assembly.GetName().Name.ShouldBe(assemblyName);
    }
}
