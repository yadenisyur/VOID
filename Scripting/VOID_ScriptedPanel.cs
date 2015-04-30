﻿// VOID
//
// VOID_PanelConfig.cs
//
// Copyright © 2015, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using ToadicusTools;
using VOID;

namespace VOID_ScriptedPanels
{
	[VOID_GameModes(new Game.Modes[] {})]
	[VOID_Scenes(new GameScenes[] {})]
	public class VOID_ScriptedPanel : VOID_WindowModule, IConfigNode
	{
		public const string PANEL_KEY = "VOID_PANEL";
		public const string LINE_KEY = "PANEL_LINE";
		public const string TITLE_KEY = "Title";
		public const string SUBTITLE_KEY = "Subtitle";
		public const string POSITION_KEY = "WindowPos";
		public const string SCENES_KEY = "ValidScenes";
		public const string MODES_KEY = "ValidModes";
		public const string WIDTH_KEY = "Width";

		private static readonly List<UrlDir.UrlFile> voidScriptFiles = new List<UrlDir.UrlFile>();
		private static readonly List<VOID_ScriptedPanel> panels = new List<VOID_ScriptedPanel>();

		public static IList<VOID_ScriptedPanel> Panels
		{
			get
			{
				return panels.AsReadOnly();
			}
		}

		public static void LoadScriptedPanels()
		{
			if (GameDatabase.Instance != null && GameDatabase.Instance.IsReady())
			{
				if (voidScriptFiles.Count == 0)
				{
					foreach (var cfgFile in GameDatabase.Instance.root.AllConfigFiles)
					{
						foreach (var config in cfgFile.configs)
						{
							if (config.config.name == PANEL_KEY)
							{
								voidScriptFiles.Add(cfgFile);
								break;
							}
						}
					}
				}

				foreach (var panel in panels)
				{
					if (VOID_Data.CoreInitialized)
					{
						VOID_Data.Core.SaveConfig();
					}

					panel.StopGUI();
				}

				panels.Clear();

				foreach (UrlDir.UrlFile file in voidScriptFiles)
				{
					var configs = UrlDir.UrlConfig.CreateNodeList(file.parent, file);

					foreach (var panelConfig in configs)
					{
						if (panelConfig.config.name == PANEL_KEY)
						{
							VOID_ScriptedPanel panel = new VOID_ScriptedPanel(panelConfig.config);

							panel.SourceFileUrl = file.url;

							panels.Add(panel);
						}
					}
				}
			}
		}

		private bool showErrorPane;

		private string tooltipContents;

		private VOID_PanelLine subTitle;
		private List<VOID_PanelLine> panelLines;

		public override bool Active
		{
			get
			{
				return base.Active;
			}
			set
			{
				base.Active = value;

				if (value)
				{
					this.StartGUI();
				}
				else
				{
					this.StopGUI();
				}
			}
		}

		public IList<VOID_PanelLine> PanelLines
		{
			get
			{
				return this.panelLines.AsReadOnly();
			}
		}

		public string SourceFileUrl
		{
			get;
			private set;
		}

		public VOID_ScriptedPanel() : base()
		{
			this.panelLines = new List<VOID_PanelLine>();
			this.showErrorPane = false;
		}

		public VOID_ScriptedPanel(ConfigNode node) : this()
		{
			this.Load(node);

			this.saveKeyName = string.Format("{0}_{1}", this.saveKeyName, (this.Name + this.SourceFileUrl).ToMD5Hash());

			this.LoadConfig();

			if (this.Active)
			{
				this.StartGUI();
			}
		}

		public void Load(ConfigNode node)
		{
			this.Name = node.GetValue(TITLE_KEY, string.Empty);

			string positionString;

			if (node.TryGetValue(POSITION_KEY, out positionString))
			{
				Vector2 positionVector = KSPUtil.ParseVector2(positionString);

				this.WindowPos.x = positionVector.x;
				this.WindowPos.y = positionVector.y;
			}

			string widthString;

			if (node.TryGetValue(WIDTH_KEY, out widthString))
			{
				float width;

				if (float.TryParse(widthString, out width))
				{
					this.defWidth = width;
				}
			}

			string scenesString;

			if (node.TryGetValue(SCENES_KEY, out scenesString))
			{
				scenesString = scenesString.ToLower();

				if (scenesString == "all")
				{
					this.validScenes = (GameScenes[])Enum.GetValues(typeof(GameScenes));
				}

				string[] scenesArray = scenesString.Split(',');

				List<GameScenes> scenes = new List<GameScenes>();

				foreach (string sceneString in scenesArray)
				{
					GameScenes scene;

					try
					{
						scene = (GameScenes)Enum.Parse(typeof(GameScenes), sceneString.Trim(), true);
						scenes.Add(scene);
					}
					catch
					{
						Tools.PostErrorMessage(
							"{0}: Failed parsing {1}: '{2}' not a valid {3}.",
							this.Name,
							SCENES_KEY,
							sceneString,
							typeof(GameScenes).Name
						);
					}
				}

				this.validScenes = scenes.ToArray();
			}

			string modesString;

			if (node.TryGetValue(MODES_KEY, out modesString))
			{
				string[] modesArray = modesString.Split(',');

				List<Game.Modes> modes = new List<Game.Modes>();

				foreach (string modeString in modesArray)
				{
					Game.Modes mode;

					try
					{
						mode = (Game.Modes)Enum.Parse(typeof(Game.Modes), modeString.Trim(), true);
						modes.Add(mode);
					}
					catch
					{
						Tools.PostErrorMessage(
							"{0}: Failed parsing {1}: '{2}' not a valid {3}.",
							this.Name,
							SCENES_KEY,
							modeString,
							typeof(GameScenes).Name
						);
					}
				}

				this.validModes = modes.ToArray();
			}

			if (node.HasValue(SUBTITLE_KEY))
			{
				this.subTitle = new VOID_PanelLine();

				this.subTitle.LabelScript = node.GetValue(SUBTITLE_KEY);
			}

			bool hasDefinedLineOrder = false;

			if (node.HasNode(LINE_KEY))
			{
				foreach (var lineNode in node.GetNodes(LINE_KEY))
				{
					VOID_PanelLine line = new VOID_PanelLine(lineNode);

					if (line.LineNumber != ushort.MaxValue)
					{
						hasDefinedLineOrder = true;
					}

					this.panelLines.Add(line);
				}

				if (hasDefinedLineOrder)
				{
					this.panelLines.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
				}
			}
		}

		public void Save(ConfigNode node)
		{
			node.SafeSetValue(TITLE_KEY, this.Name);

			node.SafeSetValue(POSITION_KEY, string.Format("{0}, {1}", this.WindowPos.x, this.WindowPos.y));

			node.ClearNodes();

			foreach (var line in this.panelLines)
			{
				ConfigNode lineNode = node.AddNode(LINE_KEY);

				line.Save(lineNode);
			}
		}

		public override void DrawGUI()
		{
			base.DrawGUI();

			if (this.tooltipContents != null && this.tooltipContents != string.Empty)
			{
				GUIStyle tooltipStyle = VOID_Styles.currentTooltip;

				GUIContent contents = new GUIContent(this.tooltipContents);

				tooltipStyle.stretchWidth = false;
				tooltipStyle.wordWrap = true;

				Rect tooltipPos = new Rect(Event.current.mousePosition.x + 2, Event.current.mousePosition.y + 2, 0, 0);

				Vector2 tooltipSize = tooltipStyle.CalcSize(contents);

				tooltipPos.height = tooltipSize.y;
				tooltipPos.width = tooltipSize.x;

				GUI.Label(tooltipPos, contents, tooltipStyle);

				GUI.depth = 0;
			}
		}

		public override void ModuleWindow(int id)
		{
			if (this.showErrorPane)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.BeginVertical();
			}

			if (this.subTitle != null)
			{
				object subObj = this.subTitle.LabelFunction.DynamicInvoke();

				if (subObj is string)
				{
					GUILayout.Label(
						(string)subObj,
						VOID_Styles.labelCenterBold,
						GUILayout.ExpandWidth(true));
				}
				else if (subObj is GUIContent)
				{
					GUILayout.Label(
						(GUIContent)subObj,
						VOID_Styles.labelCenterBold,
						GUILayout.ExpandWidth(true));
				}

			}

			List<int> errorIndices = null;
			Dictionary<ushort, VOIDScriptRuntimeException> runtimeErrors = null;

			for (int idx = 0; idx < this.panelLines.Count; idx++)
			{
				bool LineHasError = false;

				var line = this.panelLines[idx];

				if (line.LabelErrorContent != null || line.ValueErrorContent != null)
				{
					LineHasError = true;
				}

				GUILayout.BeginHorizontal();

				if (line.LabelFunction != null)
				{
					object labelObj;

					try
					{
						labelObj = line.LabelFunction.DynamicInvoke();
					}
					catch (System.Reflection.TargetInvocationException tx)
					{
						var ix = tx.InnerException;

						if (runtimeErrors == null)
						{
							runtimeErrors = new Dictionary<ushort, VOIDScriptRuntimeException>();
						}

						runtimeErrors[line.LineNumber] =
							new VOIDScriptRuntimeException(ix, VOID_PanelLine.ParsingCell.Label);

						LineHasError = true;

						labelObj = new GUIContent("Syntax Error",
							string.Format("{0}: {1}", ix.GetType().Name, ix.Message));
						
						Debug.LogException(ix);
					}

					if (labelObj is string)
					{
						GUILayout.Label((string)labelObj);
					}
					else if (labelObj is GUIContent)
					{
						GUILayout.Label((GUIContent)labelObj);
					}
					else
					{
						GUILayout.Label(labelObj.ToString());
					}
				}

				if (line.LabelFunction != null && line.ValueFunction != null)
				{
					GUILayout.FlexibleSpace();
				}

				if (line.ValueFunction != null)
				{
					object valueObj;

					try
					{
						valueObj = line.ValueFunction.DynamicInvoke();
					}
					catch (System.Reflection.TargetInvocationException tx)
					{
						var ix = tx.InnerException;

						if (runtimeErrors == null)
						{
							runtimeErrors = new Dictionary<ushort, VOIDScriptRuntimeException>();
						}

						runtimeErrors[line.LineNumber] =
							new VOIDScriptRuntimeException(ix, VOID_PanelLine.ParsingCell.Value);

						LineHasError = true;

						valueObj = new GUIContent("Syntax Error",
							string.Format("{0}: {1}", ix.GetType().Name, ix.Message));

						Debug.LogException(ix);
					}

					if (valueObj is string)
					{
						GUILayout.Label((string)valueObj);
					}
					else if (valueObj is GUIContent)
					{
						GUILayout.Label((GUIContent)valueObj);
					}
					else
					{
						GUILayout.Label(valueObj.ToString());
					}
				}

				GUILayout.EndHorizontal();

				if (LineHasError)
				{
					if (errorIndices == null)
					{
						errorIndices = new List<int>();
					}

					errorIndices.Add(idx);
				}
			}

			if (Event.current.type == EventType.Repaint)
			{
				this.tooltipContents = GUI.tooltip;
			}

			if (this.showErrorPane)
			{
				GUILayout.EndVertical();

				GUILayout.BeginVertical(this.core.Skin.box, GUILayout.MaxWidth(this.defWidth * 0.6f));

				foreach (int idx in errorIndices)
				{
					var line = this.panelLines[idx];

					if (line.LabelErrorContent != null)
					{
						GUILayout.BeginHorizontal();

						string contents = string.Format(
							"Error in label at line #{0}:\n" +
							"{1}\n" +
							"{2}",
							line.LineNumber,
							line.LabelErrorContent.text,
							line.LabelErrorContent.tooltip
						);

						GUILayout.Label(contents);

						GUILayout.EndHorizontal();
					}

					if (line.ValueErrorContent != null)
					{
						GUILayout.BeginHorizontal();

						string contents = string.Format(
							"Error in value at line #{0}:\n" +
							"{1}\n" +
							"{2}",
							line.LineNumber,
							line.ValueErrorContent.text,
							line.ValueErrorContent.tooltip
						);

						GUILayout.Label(contents);

						GUILayout.EndHorizontal();
					}

					if (runtimeErrors != null && runtimeErrors.ContainsKey(line.LineNumber))
					{
						GUILayout.BeginHorizontal();

						var error = runtimeErrors[line.LineNumber];

						string cellName;

						switch (error.Cell)
						{
							case VOID_PanelLine.ParsingCell.Label:
								cellName = "Label";
								break;
							case VOID_PanelLine.ParsingCell.Value:
								cellName = "Value";
								break;
							default:
								cellName = string.Empty;
								break;
						}

						string contents = string.Format(
							"Runtime error in {0} at line #{1}:\n" +
							"{2}\n" +
							"{3}",
							cellName,
							line.LineNumber,
							error.InnerException.GetType().FullName,
							error.Message
						);

						GUILayout.Label(contents);

						GUILayout.EndHorizontal();
					}
				}

				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}

			if (errorIndices != null)
			{
				GUIStyle buttonStyle = this.core.Skin.button;
				RectOffset padding = buttonStyle.padding;
				RectOffset border = buttonStyle.border;

				Rect errorButtonRect = new Rect(
					                      0f,
					                      0f,
					                      border.left + border.right,
					                      border.top + border.bottom
				                      );

				errorButtonRect.width = Mathf.Max(errorButtonRect.width, 16f);
				errorButtonRect.height = Mathf.Max(errorButtonRect.height, 16f);

				errorButtonRect.x = this.WindowPos.width - (errorButtonRect.width - 2f) * 2f - 8f;
				errorButtonRect.y = 2f;

				GUI.Button(errorButtonRect, this.showErrorPane ? "←" : "→", buttonStyle);

				if (Event.current.type == EventType.repaint && Input.GetMouseButtonUp(0))
				{
					if (errorButtonRect.Contains(Event.current.mousePosition))
					{
						this.showErrorPane = !this.showErrorPane;

						this.defWidth *= this.showErrorPane ? 2.5f : 0.4f;
					}
				}
			}
			else
			{
				this.showErrorPane = false;
			}

			base.ModuleWindow(id);
		}
	}

	public class VOID_PanelLine : IConfigNode
	{
		public const string LABEL_KEY = "Label";
		public const string VALUE_KEY = "Value";
		public const string LINENO_KEY = "LineNumber";

		private string labelScript;
		private string valueScript;

		public ushort LineNumber;

		public Delegate LabelFunction
		{
			get;
			private set;
		}

		public Delegate ValueFunction
		{
			get;
			private set;
		}

		public string LabelScript
		{
			get
			{
				return this.labelScript;
			}
			set
			{
				this.LabelFunction = this.parseFunctionScript(value, ParsingCell.Label);

				this.labelScript = value;
			}
		}

		public string ValueScript
		{
			get
			{
				return this.valueScript;
			}
			set
			{
				this.ValueFunction = this.parseFunctionScript(value, ParsingCell.Value);

				this.valueScript = value;
			}
		}

		public GUIContent LabelErrorContent
		{
			get;
			private set;
		}

		public GUIContent ValueErrorContent
		{
			get;
			private set;
		}

		public VOID_PanelLine()
		{
			this.LineNumber = ushort.MaxValue;
			this.labelScript = string.Empty;
			this.valueScript = string.Empty;
		}

		public VOID_PanelLine(ConfigNode node) : this()
		{
			this.Load(node);
		}

		public void Load(ConfigNode node)
		{
			string labelScript;
			string valueScript;

			if (node.TryGetValue(LABEL_KEY, out labelScript))
			{
				this.LabelScript = labelScript;
			}

			if (node.TryGetValue(VALUE_KEY, out valueScript))
			{
				this.ValueScript = valueScript;
			}

			string lineNo;
			ushort lineNumber;

			if (node.TryGetValue(LINENO_KEY, out lineNo))
			{
				if (ushort.TryParse(lineNo, out lineNumber))
				{
					this.LineNumber = lineNumber;
				}
			}
		}

		public void Save(ConfigNode node)
		{
			if (node.HasValue(LABEL_KEY))
			{
				node.SetValue(LABEL_KEY, this.LabelScript);
			}
			else
			{
				node.AddValue(LABEL_KEY, this.LabelScript);
			}

			if (node.HasValue(VALUE_KEY))
			{
				node.SetValue(VALUE_KEY, this.ValueScript);
			}
			else
			{
				node.AddValue(VALUE_KEY, this.ValueScript);
			}

			if (node.HasValue(LINENO_KEY))
			{
				node.SetValue(LINENO_KEY, this.LineNumber.ToString());
			}
			else
			{
				node.AddValue(LINENO_KEY, this.LineNumber.ToString());
			}
		}

		private Delegate parseFunctionScript(string script, ParsingCell cell)
		{
			LambdaExpression scriptExpression = null;

			try
			{
				ScriptParser parser = new ScriptParser(script);

				scriptExpression = parser.Parse();

				Tools.PostDebugMessage(this, "Parsed expression '{0}' from '{1}'.", scriptExpression, script);

				return scriptExpression.Compile();
			}
			catch (VOIDScriptSyntaxException sx)
			{
				return this.getSyntaxErrorContent("Syntax Error", sx.Message, cell);
			}
			catch (InvalidOperationException iox)
			{
				return this.getSyntaxErrorContent("Syntax Error",
					string.Format("{0}: {1}", iox.GetType().Name, iox.Message), cell);
			}
			catch (FormatException fx)
			{
				return this.getSyntaxErrorContent("Syntax Error",
					string.Format("{0}: {1}", fx.GetType().Name, fx.Message), cell);
			}
			catch (VOIDScriptParserException px)
			{
				return this.getSyntaxErrorContent("Parser Error", px.Message + " Please report!", cell);
			}
			catch (Exception ex)
			{
				Tools.PostErrorMessage(
					"Compiler error processing VOIDScript line '{0}'.  Please report!\n{1}: {2}\n{3}",
					script, ex.GetType().Name, ex.Message, ex.StackTrace);
				return this.getSyntaxErrorContent("Compiler Error", ex.GetType().Name + ": " + ex.Message, cell);
			}
		}

		private Func<GUIContent> getSyntaxErrorContent(string message, string tooltip, ParsingCell cell)
		{
			switch (cell)
			{
				case ParsingCell.Label:
					if (this.LabelErrorContent == null)
					{
						this.LabelErrorContent = new GUIContent(message);
					}

					this.LabelErrorContent.tooltip = tooltip;

					return () => this.LabelErrorContent;
				case ParsingCell.Value:
					if (this.ValueErrorContent == null)
					{
						this.ValueErrorContent = new GUIContent(message);
					}
					this.ValueErrorContent.tooltip = tooltip;

					return () => this.ValueErrorContent;
				default:
					return () => new GUIContent(message);
			}
		}

		public enum ParsingCell
		{
			Label,
			Value
		}
	}
}

