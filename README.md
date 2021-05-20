# Workshop labs

There are two folders, `lab1` and `lab2` corresponding to today's labs.

## Deployment instructions

1. `cd lab1`
1. `pulumi stack init dev`
1. `pulumi config set azure-native:location uksouth`
1. `pulumi up` (select yes when it asks you if you want to perform the update)
1. Wait for the deployment to take place
1. `pulumi console` - this will take you to the console for this stack. You'll need to copy the path at the top underneath the Pulumi logo. Depending on how you deployed the resources it'll be something like `{username|orgname/projectname/stackname}`. For me it's `pierskarsenbarg/workshop-lab1/dev`. 
1. `cd ../lab2/appstack`
1. `pulumi stack init dev`
1. `pulumi config set azure-native:location uksouth`
1. Open `MyStack.cs`
1. On this line, change what's in the brackets to be what you took from the Pulumi console (so something like `{username/projectname/stackname}`). Save the file
1. `pulumi up` (select yes when it asks you if you want to perform the update)
1. Wait for deployment to take place and when it has finished you can copy and paste the `Endpoint` output into a browser to view the web app

If you want to deploy a change, you can change the web app in the `app` folder and run `pulumi up` from the `appstack` folder.

## Teardown instructions

Starting in the `lab2/appstack` folder:

1. `pulumi destroy` (select yes when it confirms if you want to do this)
1. Wait for the resources to be deleted
1. `pulumi stack rm dev` (type in the stack name when prompted)
1. `cd ../../lab1`
1. `pulumi destroy` (select yes when it confirms if you want to do this)
1. `pulumi stack rm dev` (type in the stack name when prompted)

## Resource group name issue from the workshop earlier

The issue I had was on this line of the code. I had `var baseStack = Pulumi.StackReference("pierskarsenbarg/lab1/dev");` and it should have been `var baseStack = new Pulumi.StackReference("pierskarsenbarg/lab1/dev");`.
