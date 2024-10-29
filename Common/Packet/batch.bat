START /WAIT ../../PacketGenerator/bin/PacketGenerator ../../PacketGenerator/PDL.xml

XCOPY /Y NewPacket.cs "../../Client/Packet"
XCOPY /Y NewPacket.cs "../../UnityClient/Assets/Scripts/Packet"
XCOPY /Y NewPacket.cs "../../Server/Packet"

XCOPY /Y ClientPacketManager.cs "../../Client/Packet"
XCOPY /Y ClientPacketManager.cs "../../UnityClient/Assets/Scripts/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"

DEL /F /Q NewPacket.cs
DEL /F /Q ClientPacketManager.cs
DEL /F /Q ServerPacketManager.cs