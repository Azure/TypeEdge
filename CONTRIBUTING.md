Please fork, branch and pull-request any changes you'd like to make.

#### Contributor Dev Machine Setup

1. Clone or Fork this Repository **recursively** 

    `git clone https://github.com/paloukari/TypeEdge --recurse-submodules`

1. Install the latest .NET Core SDK ( the minimum version is 2.1.302 version)
1. Replace the "CONNECTION_STRING" in the appsettings.json files with your IoT Hub **owner** connection string
1. Open the **TypeEdge - AD Example** solution and start the emulator of one of the two examples.

#### VS Code/ Visual Studio Debugging
1. You can use the two submodules examples for developing and debugging purposes. 
There are two examples, a simple one that has only a temperature generator, and a more complicated 
one that is running anomaly detection on the edge with continious retraining of the model. 
These examples reference the NuGet packages in "Debug", and the class library projects in "TemplateDevelopment" 
configuration. It is highly recommented to work in "TemplateDevelopment". You might have to restart Visual Studio after you change to another configuration. 
