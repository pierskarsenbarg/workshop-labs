using Pulumi;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.ContainerRegistry.Inputs;
using Pulumi.AzureNative.Web.Inputs;
using System.Linq;
using Pulumi.AzureNative.Web;
using Pulumi.Docker;

class MyStack : Stack
{
    public MyStack()
    {
        var baseStack = new StackReference("pierskarsenbarg/workshop-lab1/dev");

        var resourceGroupName = baseStack.RequireOutput("ResourceGroupName").Apply(x => x.ToString());
        var containerRegistry = new Registry("registry", new RegistryArgs
        {
            ResourceGroupName = resourceGroupName,
            AdminUserEnabled = true,
            Sku = new SkuArgs { Name = "Basic" }
        });

        var registryLogin = GetRegistryLogin(resourceGroupName, containerRegistry.Name);

        var appServicePlan = new AppServicePlan("appServicePlan", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroupName,
            Kind = "Linux",
            Reserved = true,
            Sku = new SkuDescriptionArgs 
            {
                Name = "B1",
                Tier = "Basic"
            }
        });

        var image = new Image("dockerImage", new ImageArgs
        {
            ImageName = Output.Format($"{containerRegistry.LoginServer}/myapp:v1.0.0"),
            Build = new DockerBuild
            {
                Context = "../app"
            },
            Registry = new ImageRegistry
            {
                Server = containerRegistry.LoginServer,
                Username = registryLogin.registryUsername,
                Password = registryLogin.registryPassword
            }
        });

        var app = new WebApp("app", new WebAppArgs
        {
            ResourceGroupName = resourceGroupName,
            ServerFarmId = appServicePlan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                        {
                            Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                            Value = "false"
                        },
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_URL",
                            Value = Output.Format($"https://{containerRegistry.LoginServer}")
                        },
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_USERNAME",
                            Value = registryLogin.registryUsername
                        },
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_PASSWORD",
                            Value = registryLogin.registryPassword
                        },
                        new NameValuePairArgs
                        {
                            Name = "WEBSITES_PORT",
                            Value = "80" // This should be the same as the port exposed by the container
                        }
                },
                AlwaysOn = true,
                LinuxFxVersion = Output.Format($"DOCKER|{image.ImageName}")
            },
            HttpsOnly = true
        });

        this.Endpoint = Output.Format($"https://{app.DefaultHostName}");
    }

    [Output]
    public Output<string> Endpoint {get;set;}

    private (Output<string> registryUsername, Output<string> registryPassword) GetRegistryLogin(Output<string> resourceGroupName, Output<string> registryName) 
        {
            var credentials = Output.Tuple(resourceGroupName, registryName).Apply(values =>
                ListRegistryCredentials.InvokeAsync(new ListRegistryCredentialsArgs
                {
                    ResourceGroupName = values.Item1,
                    RegistryName = values.Item2
                }));
            var registryUsername = credentials.Apply(c => c.Username ?? "");
            var registryPassword = credentials.Apply(c => Output.CreateSecret(c.Passwords.First().Value ?? ""));
            return (registryUsername, registryPassword);
        }
}
