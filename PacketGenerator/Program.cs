using System;
using System.Xml;

namespace PacketGenerator
{
	class Program
	{
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
					if (reader.Depth == 1 && reader.NodeType == XmlNodeType.EndElement)
						ParsePacket(reader);

					// System.Console.WriteLine(reader.Name + " " + reader["name"]);
				}
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

			ParsePacketContent(reader);
		}

		public static void ParsePacketContent(XmlReader reader)
		{
			string packetName = reader["name"];

			int depth = reader.Depth + 1;

			while (reader.Read())
			{
				if (reader.Depth != depth)
					break;

				string memberName = reader["name"];
				if (String.IsNullOrEmpty(memberName))
				{
					Console.WriteLine("No Member Name");
					return;
				}

				string memberType = reader.Name.ToLower();
				switch (memberType)
				{
					case "bool":
						break;
					case "byte":
						break;
					case "short":
						break;
					case "ushort":
						break;
					case "int":
						break;
					case "long":
						break;
					case "float":
						break;
					case "double":
						break;
					case "string":
						break;
					case "list":
						break;
					case "skill":
						break;
					default:
						break;
				}
			}
		}
	}
}