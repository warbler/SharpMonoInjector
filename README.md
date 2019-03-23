# SharpMonoInjector
SharpMonoInjector is a tool for injecting assemblies into Mono embedded applications, commonly Unity Engine based games. The target process *usually* does not have to be restarted in order to inject an updated version of the assembly. Your unload method must to destroy all of its resources (such as game objects).

SharpMonoInjector works by dynamically generating machine code, writing it to the target process and executing it using CreateRemoteThread. The code calls functions in the mono embedded API. The return value is obtained with ReadProcessMemory.

Both x86 and x64 processes are supported.

In order for the injector to work, the load/unload methods need to match the following method signature:

    static void Method()

In [releases](https://github.com/warbler/SharpMonoInjector/releases), there is a console application and a GUI application available.

![The GUI application](https://i.imgur.com/mPMwlu1.png)
![The console application](https://i.imgur.com/cz8Gyxa.png)
