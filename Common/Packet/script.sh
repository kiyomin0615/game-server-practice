#!/bin/bash

../../PacketGenerator/bin/PacketGenerator ../../PacketGenerator/PDL.xml
cp -f NewPacket.cs "../../Client/Packet"
cp -f NewPacket.cs "../../Server/Packet"
