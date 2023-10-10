 // to destroy our program, we can run "dotnet run destroy"

 using System.Reflection;
 using Pulumi.Automation;

 var destroy = args.Any() && args[0] == "destroy";

var stackName = "dev";

// need to account for the assembly executing from within the bin directory
// when getting path to the local program
var executingDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName;
var workingDir = Path.Combine(executingDir, "..", "..", "..", "..", "Infrastructure");

// create our stack using a local program in the ../../../../fargate directory
var stackArgs = new LocalProgramArgs(stackName, workingDir);
var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

Console.WriteLine("successfully initialized stack");

// set stack configuration specifying the region to deploy
Console.WriteLine("setting up config...");
await stack.SetConfigAsync("azure-native:location", new ConfigValue("UkSouth"));
 Console.WriteLine("config set");

Console.WriteLine("refreshing stack...");
await stack.RefreshAsync(new RefreshOptions { OnStandardOutput = Console.WriteLine, ExpectNoChanges = true });
Console.WriteLine("refresh complete");

if (destroy)
{
    Console.WriteLine("destroying stack...");
    await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
    Console.WriteLine("stack destroy complete");
}
else
{
    Console.WriteLine("updating stack...");
    var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

    if (result.Summary.ResourceChanges != null)
    {
        Console.WriteLine("update summary:");
        foreach (var change in result.Summary.ResourceChanges)
            Console.WriteLine($"    {change.Key}: {change.Value}");
    }

    Console.WriteLine($"Primary storage key: {result.Outputs["primaryStorageKey"].Value}");
}