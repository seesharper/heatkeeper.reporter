using System.Runtime.CompilerServices;
namespace HeatKeeper.Reporter.Sdk.Tests;

public static class TestAppContextInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        AppEnvironment.IsRunningFromTests = true;
    }
}