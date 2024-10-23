using System;
using System.Xml;

namespace PacketGenerator
{
	class Program
	{
		static string packetContent;

		static ushort packetId;
		static string packetEnums;

		static void Main(string[] args)
		{
			string basePath = AppDomain.CurrentDomain.BaseDirectory;
			string xmlPath = Path.Combine(basePath, "../PDL.xml");

			XmlReaderSettings settings = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
			};

			if (args.Length > 0)
			{
				xmlPath = args[0];
			}

			using (XmlReader reader = XmlReader.Create(xmlPath, settings))
			{
				reader.MoveToContent();
				while (reader.Read())
				{
					if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
						ParsePacket(reader);

					// System.Console.WriteLine(reader.Name + " " + reader["name"]);
				}

				string fileText = string.Format(PacketFormat.fileFormat, packetEnums, packetContent);
				File.WriteAllText("NewPacket.cs", fileText);
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
			packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";
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
					case "byte":
					case "sbyte":
						memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
						deserializeCode += string.Format(PacketFormat.deserializeByteFormat, memberName, memberType);
						serializeCode += string.Format(PacketFormat.serializeByteFormat, memberName, memberType);
						break;
					case "bool":
					case "short":
					case "ushort":
					case "int":
					case "long":
					case "float":
					case "double":
						memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
						deserializeCode += string.Format(PacketFormat.deserializeFormat, memberName, GetDeserializeMethodName(memberType), memberType);
						serializeCode += string.Format(PacketFormat.serializeFormat, memberName, memberType);
						break;
					case "string":
						memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
						deserializeCode += string.Format(PacketFormat.deserializeStringFormat, memberName);
						serializeCode += string.Format(PacketFormat.serializeStringFormat, memberName);
						break;
					case "list":
						Tuple<string, string, string> tuple = ParsePacketContentList(reader);
						memberCode += tuple.Item1;
						deserializeCode += tuple.Item2;
						serializeCode += tuple.Item3;
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

		public static Tuple<string, string, string> ParsePacketContentList(XmlReader reader)
		{
			string listName = reader["name"];
			if (string.IsNullOrEmpty(listName))
			{
				Console.WriteLine("No List Name");
				return null;
			}

			Tuple<string, string, string> tuple = ParsePacketContent(reader);

			string memberCode = string.Format(
				PacketFormat.memberListFormat,
				ReplaceFirstCharacterWithUpperCase(listName),
				ReplaceFirstCharacterWithLowerCase(listName),
				tuple.Item1,
				tuple.Item2,
				tuple.Item3
			);
			string deserializeCode = string.Format(
				PacketFormat.deserializeListFormat,
				ReplaceFirstCharacterWithUpperCase(listName),
				ReplaceFirstCharacterWithLowerCase(listName)
			);
			string serializeCode = string.Format(
				PacketFormat.serializeListFormat,
				ReplaceFirstCharacterWithUpperCase(listName),
				ReplaceFirstCharacterWithLowerCase(listName)
			);

			return new Tuple<string, string, string>(memberCode, deserializeCode, serializeCode);
		}

		public static string GetDeserializeMethodName(string memberType)
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

		public static string ReplaceFirstCharacterWithUpperCase(string input)
		{
			if (string.IsNullOrEmpty(input))
				return "";

			return input[0].ToString().ToUpper() + input.Substring(1);
		}

		public static string ReplaceFirstCharacterWithLowerCase(string input)
		{
			if (string.IsNullOrEmpty(input))
				return "";

			return input[0].ToString().ToLower() + input.Substring(1);
		}
	}
}