Re-signed DLLs:

In order to allow use of these libraries within a Visual Studio Extension,
these libraries were disassembled using ILDASM, signed with the Iron Foundry key
from ../common/IronFoundry.snk and reassembled using ILASM as .NET 4.0 assemblies.

NDesk.Options.dll

In order to faciliate upgrade of these components, resigning will need to occur again. Please reference: 

http://buffered.io/2008/07/09/net-fu-signing-an-unsigned-assembly-without-delay-signing/

Thanks, the Iron Foundry team
