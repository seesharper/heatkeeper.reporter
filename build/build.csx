#!/usr/bin/env dotnet-script
#load "nuget:Dotnet.Build, 0.12.1"
#load "nuget:dotnet-steps, 0.0.2"


[StepDescription("Runs the tests with test coverage")]
Step testcoverage = () => DotNet.TestWithCodeCoverage();

[StepDescription("Runs all the tests for all target frameworks")]
Step test = () => DotNet.Test();

[StepDescription("Creates the NuGet packages")]
Step pack = () =>
{
    test();
    // testcoverage();
    DotNet.Pack();
};

[StepDescription("Creates the Docker image")]
AsyncStep dockerImage = async () =>
{
    await Command.ExecuteAsync("docker", $@"build --rm -f ""Dockerfile"" -t bernhardrichter/heatkeeper.reporter:{BuildContext.LatestTag} -t bernhardrichter/heatkeeper.reporter:latest .", BuildContext.RepositoryFolder);
    await Docker.BuildAsync("bernhardrichter/heatkeeper.reporter", BuildContext.LatestTag, BuildContext.RepositoryFolder);
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    pack();
    // await dockerImage();
    // if (BuildEnvironment.IsSecure && BuildEnvironment.IsTagCommit)
    // {
    //     await Docker.PushAsync("bernhardrichter/heatkeeper.reporter", BuildContext.LatestTag, BuildContext.BuildFolder);
    //     await Docker.PushAsync("bernhardrichter/heatkeeper.reporter", "latest", BuildContext.BuildFolder); await Docker.PushAsync("bernhardrichter/heatkeeper.reporter", BuildContext.LatestTag, BuildContext.BuildFolder);
    // }
    await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;
