START ../../PacketGenerator/bin/PacketGenerator ../../PacketGenerator/PDL.xml
XCOPY /Y NewPacket.cs "../../Client/Packet"
XCOPY /Y NewPacket.cs "../../Server/Packet"