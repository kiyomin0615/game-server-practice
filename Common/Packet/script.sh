#!/bin/bash

../../PacketGenerator/bin/PacketGenerator ../../PacketGenerator/PDL.xml

cp -f NewPacket.cs "../../Client/Packet"
cp -f NewPacket.cs "../../UnityClient/Assets/Scripts/Packet"
cp -f NewPacket.cs "../../Server/Packet"

cp -f ClientPacketManager.cs "../../Client/Packet"
cp -f ClientPacketManager.cs "../../UnityClient/Assets/Scripts/Packet"
cp -f ServerPacketManager.cs "../../Server/Packet"

rm -f NewPacket.cs
rm -f ClientPacketManager.cs
rm -f ServerPacketManager.cs