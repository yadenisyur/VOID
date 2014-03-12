//
//  VOID_Hud.cs
//
//  Author:
//       toadicus <>
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using KSP;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace VOID
{
	public class VOID_HUD : VOID_Module, IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("colorIndex")]
		protected VOID_SaveValue<int> _colorIndex = 0;

		protected List<Color> textColors = new List<Color>();

		/*
		 * Properties
		 * */
		public int ColorIndex
		{
			get
			{
				return this._colorIndex;
			}
			set
			{
				if (this._colorIndex >= this.textColors.Count - 1)
				{
					this._colorIndex = 0;
					return;
				}

				this._colorIndex = value;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_HUD() : base()
		{
			this._Name = "Heads-Up Display";

			this._Active.value = true;

			this.textColors.Add(Color.green);
			this.textColors.Add(Color.black);
			this.textColors.Add(Color.white);
			this.textColors.Add(Color.red);
			this.textColors.Add(Color.blue);
			this.textColors.Add(Color.yellow);
			this.textColors.Add(Color.gray);
			this.textColors.Add(Color.cyan);
			this.textColors.Add(Color.magenta);

			VOID_Core.Instance.LabelStyles["hud"] = new GUIStyle();
			VOID_Core.Instance.LabelStyles["hud"].normal.textColor = this.textColors [this.ColorIndex];

			Tools.PostDebugMessage ("VOID_HUD: Constructed.");
		}

		public override void DrawGUI()
		{
			StringBuilder leftHUD;
			StringBuilder rightHUD;

			GUI.skin = VOID_Core.Instance.Skin;

			if (VOID_Core.Instance.powerAvailable)
			{
				leftHUD = new StringBuilder();
				rightHUD = new StringBuilder();

				VOID_Core.Instance.LabelStyles["hud"].normal.textColor = textColors [ColorIndex];

				leftHUD.AppendFormat("Obt Alt: {0} Obt Vel: {1}",
					VOID_Data.orbitAltitude.ToSIString(),
					VOID_Data.orbitVelocity.ToSIString()
				);
				leftHUD.AppendFormat("\nAp: {0} ETA {1}",
					VOID_Data.orbitApoAlt.ToSIString(),
					VOID_Data.timeToApo.ValueUnitString()
				);
				leftHUD.AppendFormat("\nPe: {0} ETA {1}",
					VOID_Data.oribtPeriAlt.ToSIString(),
					VOID_Data.timeToPeri.ValueUnitString()
				);
				leftHUD.AppendFormat("\nInc: {0}", VOID_Data.orbitInclination.ValueUnitString("F3"));
				leftHUD.AppendFormat("\nPrimary: {0}", VOID_Data.primaryName.ValueUnitString());

				rightHUD.AppendFormat("Srf Alt: {0} Srf Vel: {1}",
					VOID_Data.trueAltitude.ToSIString(),
					VOID_Data.surfVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nVer: {0} Hor: {1}",
					VOID_Data.vertVelocity.ToSIString(),
					VOID_Data.horzVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nLat: {0} Lon: {1}",
					VOID_Data.surfLatitude.ValueUnitString(),
					VOID_Data.surfLongitude.ValueUnitString()
				);
				rightHUD.AppendFormat("\nHdg: {0}", VOID_Data.vesselHeading.ValueUnitString());
				rightHUD.AppendFormat("\nBiome: {0} Sit: {1}",
					VOID_Data.currBiome.ValueUnitString(),
					VOID_Data.expSituation.ValueUnitString()
				);

				GUI.Label (
					new Rect ((Screen.width * .2083f), 0, 300f, 70f),
					leftHUD.ToString(),
					VOID_Core.Instance.LabelStyles["hud"]);

				GUI.Label (
					new Rect ((Screen.width * .625f), 0, 300f, 90f),
					rightHUD.ToString(),
					VOID_Core.Instance.LabelStyles["hud"]);
			}
			else
			{
				VOID_Core.Instance.LabelStyles["hud"].normal.textColor = Color.red;
				GUI.Label (new Rect ((Screen.width * .2083f), 0, 300f, 70f), "-- POWER LOST --", VOID_Core.Instance.LabelStyles["hud"]);
				GUI.Label (new Rect ((Screen.width * .625f), 0, 300f, 70f), "-- POWER LOST --", VOID_Core.Instance.LabelStyles["hud"]);
			}
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button ("Change HUD color", GUILayout.ExpandWidth (false)))
			{
				++this.ColorIndex;
			}
		}
	}

	public static partial class VOID_Data
	{
		public static VOID_StrValue expSituation = new VOID_StrValue(
			"Situation",
			new Func<string> (() => VOID_Core.Instance.vessel.GetExperimentSituation().HumanString())
		);
	}
}
