using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Y3ADV
{
	public class RichText
	{
		public RichText(string plainText)
		{
			PlainText = plainText;
			textNode = Parse(plainText);
			if (DumpText)
			{
				Debug.LogError(plainText);
				Dump(textNode, 0);
			}
		}

		public void Dump(TextNode node, int indent)
		{
			string text = string.Empty;
			for (int i = 0; i < indent; i++)
			{
				text += "-";
			}
			foreach (TextNode textNode in node.childs)
			{
				Debug.LogError(string.Concat(new object[]
				{
					text,
					"[",
					textNode.nodeType,
					"]",
					textNode.text
				}));
				if (textNode.childs.Count != 0)
				{
					Dump(textNode, indent + 1);
				}
			}
		}

		public int Length => textNode.Length;

		public string Substring(int start, int end, bool noTag = false)
		{
			return textNode.String(end, noTag);
		}

		private TextNode Parse(string source)
		{
			TextNode textNode1 = new TextNode(NodeType.RootNode);
			TextNode textNode2 = textNode1;
			for (int i = 0; i < source.Length; i++)
			{
				if (source[i] == '}' && textNode2 != null && textNode2.nodeType != NodeType.RootNode && textNode2.nodeType != NodeType.TextNode)
				{
					textNode2.parent.Add(textNode2);
					if (textNode2.parent.nodeType == NodeType.RootNode)
					{
						if (textNode2.text != string.Empty)
						{
							TextNode node = new TextNode(textNode2.text);
							textNode2.text = string.Empty;
							textNode2.Add(node);
						}
						textNode2 = new TextNode
						{
							parent = textNode2.parent
						};
					}
					else
					{
						if (textNode2.text != string.Empty)
						{
							TextNode node2 = new TextNode(textNode2.text);
							textNode2.text = string.Empty;
							textNode2.Add(node2);
						}
						textNode2 = textNode2.parent;
					}
				}
				else
				{
					if (source[i] == '@')
					{
						TextNode textNode3 = null;
						int num = 2;
						char c = source[i + 1];
						switch (c)
						{
							case 'b':
								textNode3 = new TextNode(NodeType.BoldNode);
								break;
							case 'c':
							{
								textNode3 = new TextNode(NodeType.ColorNode);
								num = source.IndexOf("{", i + num, StringComparison.Ordinal) - i;
								if (0 < num)
								{
									string input2 = source.Substring(i + 2, num - 1);
									Match match2 = regexColor.Match(input2);
									if (match2.Success && match2.Groups.Count == 2)
									{
										textNode3.option = match2.Groups[1].Value;
									}
								}

								break;
							}
							case 'i':
								textNode3 = new TextNode(NodeType.ItalicNode);
								break;
							case 's':
							{
								textNode3 = new TextNode(NodeType.SizeNode);
								num = source.IndexOf("{", i + num, StringComparison.Ordinal) - i;
								if (0 < num)
								{
									string input = source.Substring(i + 2, num - 1);
									Match match = regexSize.Match(input);
									if (match.Success && match.Groups.Count == 2)
									{
										textNode3.option = match.Groups[1].Value;
									}
								}

								break;
							}
						}

						if (textNode3 != null && 0 < num && source[i + num] == '{')
						{
							if (textNode2.nodeType == NodeType.RootNode)
							{
								textNode3.parent = textNode2;
								textNode2 = textNode3;
							}
							else if (textNode2.nodeType == NodeType.TextNode)
							{
								textNode2.parent.Add(textNode2);
								textNode3.parent = textNode2.parent;
								textNode2 = textNode3;
							}
							else
							{
								textNode3.parent = textNode2;
								if (textNode2.text != string.Empty)
								{
									TextNode node3 = new TextNode(textNode2.text);
									textNode2.text = string.Empty;
									textNode2.Add(node3);
								}
								textNode2 = textNode3;
							}
							i += num;
							goto IL_335;
						}
					}
					if (textNode2 == textNode1)
					{
						textNode2 = new TextNode();
						textNode2.parent = textNode1;
					}
					if (i < source.Length)
					{
						TextNode textNode4 = textNode2;
						textNode4.text += source[i];
					}
				}
				IL_335:;
			}
			while (textNode2 != textNode1)
			{
				textNode2.parent.Add(textNode2);
				textNode2 = textNode2.parent;
			}
			return textNode1;
		}

		public static bool DumpText;

		public string PlainText = string.Empty;

		public TextNode textNode;

		private Regex regexColor = new("\\(([#0-9a-zA-Z]+)\\)");

		private Regex regexSize = new("\\(([0-9]+)\\)");

		public enum NodeType
		{
			None,
			RootNode,
			TextNode,
			BoldNode,
			ItalicNode,
			ColorNode,
			SizeNode
		}

		public class TextNode
		{
			public TextNode()
			{
			}

			public TextNode(string text)
			{
				this.text = text;
			}

			public TextNode(NodeType type)
			{
				nodeType = type;
				text = string.Empty;
				switch (type)
				{
				case NodeType.BoldNode:
					prefix = "<b>";
					suffix = "</b>";
					break;
				case NodeType.ItalicNode:
					prefix = "<i>";
					suffix = "</i>";
					break;
				case NodeType.ColorNode:
					prefix = "<color=%option%>";
					suffix = "</color>";
					break;
				case NodeType.SizeNode:
					prefix = "<size=%option%>";
					suffix = "</size>";
					break;
				}
			}

			public int Length => text.Length + childLength;

			private string Prefix => prefix.Replace("%option%", option);

			public void Add(TextNode node)
			{
				childLength += node.Length;
				childs.Add(node);
			}

			public string String(int end, bool noTag = false)
			{
				if (Length < end)
				{
					end = Length;
				}
				int num = 0;
				string text = string.Empty;
				if (this.text != string.Empty)
				{
					int num2 = (this.text.Length >= end) ? end : this.text.Length;
					num += num2;
					text += this.text.Substring(0, num2);
				}
				foreach (TextNode textNode in childs)
				{
					if (num + textNode.Length < end)
					{
						num += textNode.Length;
						text += textNode.String(textNode.Length, noTag);
					}
					else
					{
						int num3 = end - num;
						num += num3;
						text += textNode.String(num3, noTag);
					}
					if (end <= num)
					{
						break;
					}
				}
				if (noTag)
				{
					return text;
				}
				return Prefix + text + suffix;
			}

			public NodeType nodeType = NodeType.TextNode;
			public string text = string.Empty;
			private string prefix = string.Empty;
			private string suffix = string.Empty;
			public string option = string.Empty;
			public List<TextNode> childs = new();
			public TextNode parent;
			private int childLength;
		}
	}
}
