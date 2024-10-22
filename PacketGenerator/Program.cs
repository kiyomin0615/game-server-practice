﻿using System;
using System.Xml;

namespace PacketGenerator
{
	class Program
	{
		static string packetContent;

		static void Main(string[] args)
		{
			XmlReaderSettings settings = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
			};

			using (XmlReader reader = XmlReader.Create("PDL.xml", settings))
			{
				reader.MoveToContent();
				while (reader.Read())
				{
					if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
						ParsePacket(reader);

					// System.Console.WriteLine(reader.Name + " " + reader["name"]);
				}

				File.WriteAllText("NewPacket.cs", packetContent);
			}
		}

		public static void ParsePacket(XmlReader reader)
		{
			if (reader.NodeType == XmlNodeType.EndElement)
				return;

			if (reader.Name.ToLower() != "packet")
				return;

			string packetName = reader["name"];
			if (String.IsNullOrEmpty(packetName))
			{
				Console.WriteLine("No Packet Name");
				return;
			}

			Tuple<string, string, string> result = ParsePacketContent(reader);
			packetContent += string.Format(PacketFormat.packetFormat, packetName, result.Item1, result.Item2, result.Item3);
		}

		public static Tuple<string, string, string> ParsePacketContent(XmlReader reader)
		{
			string packetName = reader["name"];

			string memberCode = "";
			string deserializeCode = "";
			string serializeCode = "";

			int depth = reader.Depth + 1;

			while (reader.Read())
			{
				if (reader.Depth != depth)
					break;

				string memberName = reader["name"];
				if (String.IsNullOrEmpty(memberName))
				{
					Console.WriteLine("No Member Name");
					return null;
				}

				if (string.IsNullOrEmpty(memberCode) == false)
					memberCode += Environment.NewLine;
				if (string.IsNullOrEmpty(deserializeCode) == false)
					deserializeCode += Environment.NewLine;
				if (string.IsNullOrEmpty(serializeCode) == false)
					serializeCode += Environment.NewLine;

				string memberType = reader.Name.ToLower();
				switch (memberType)
				{
					case "bool":
					case "byte":
					case "short":
					case "ushort":
					case "int":
					case "long":
					case "float":
					case "double":
						memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
						deserializeCode += string.Format(PacketFormat.deserializeFormat, memberName, GetDerializeMethodName(memberType), memberType);
						serializeCode += string.Format(PacketFormat.serializeFormat, memberName, memberType);
						break;
					case "string":
						memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
						deserializeCode += string.Format(PacketFormat.deserializeStringFormat, memberName);
						serializeCode += string.Format(PacketFormat.serializeStringFormat, memberName);
						break;
					case "list":
						break;
					case "skill":
						break;
					default:
						break;
				}
			}

			memberCode = memberCode.Replace("\n", "\n\t");
			deserializeCode = deserializeCode.Replace("\n", "\n\t\t");
			serializeCode = serializeCode.Replace("\n", "\n\t\t");
			return new Tuple<string, string, string>(memberCode, deserializeCode, serializeCode);
		}

		public static string GetDerializeMethodName(string memberType)
		{
			switch (memberType)
			{
				case "bool":
					return "ToBoolean";
				case "short":
					return "ToInt16";
				case "ushort":
					return "ToUInt16";
				case "int":
					return "ToInt32";
				case "long":
					return "ToInt64";
				case "float":
					return "ToSingle";
				case "double":
					return "ToDouble";
				default:
					return "";
			}
		}
	}
}