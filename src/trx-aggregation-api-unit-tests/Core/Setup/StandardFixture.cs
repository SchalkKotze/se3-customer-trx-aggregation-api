using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
/*
namespace email_api_unit_tests.Core.Setup;

public class StandardFixture : IClassFixture<StandardContext>
{
    protected StandardFixture(ITestOutputHelper outputHelper, StandardContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));

        Context.Setup(outputHelper, ConfigureServices);
    }

    public StandardContext Context { get; private set; }

    protected virtual void ConfigureServices(ServiceCollection services) { }
}*/