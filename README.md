# SharpMonoInjector
SharpMonoInjector is a tool for injecting assemblies into Mono embedded applications, commonly Unity Engine based games. The target process does not have to be restarted in order to inject an updated version of the assembly. Instead, you need to properly eject the assembly which this tool simplifies, and then inject again.

There are two dependencies: Process.NET for invoking functions in remote processes, and Fasm.NET as the assembler that Process.NET requires. Both are available as Nuget packages and licensed under the MIT license.
