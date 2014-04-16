﻿// VOID
//
// VOID_Tools.cs
//
// Copyright © 2014, toadicus
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
using UnityEngine;

namespace ToadicusTools
{
	public static partial class Tools
	{
		#region VESSEL_EXTENSIONS_SCIENCE
		public static CBAttributeMap.MapAttribute GetBiome(this Vessel vessel)
		{
			CBAttributeMap.MapAttribute mapAttribute;

			try
			{
				CBAttributeMap BiomeMap = vessel.mainBody.BiomeMap;

				double lat = vessel.latitude * Math.PI / 180d;
				double lon = vessel.longitude * Math.PI / 180d;

				mapAttribute = BiomeMap.GetAtt(lat, lon);

				/*
				lon -= Math.PI / 2d;

				if (lon < 0d)
				{
					lon += 2d * Math.PI;
				}

				float v = (float)(lat / Math.PI) + 0.5f;
				float u = (float)(lon / (2d * Math.PI));

				Color pixelBilinear = BiomeMap.Map.GetPixelBilinear(u, v);
				mapAttribute = BiomeMap.defaultAttribute;

				if (BiomeMap.Map != null)
				{
					if (BiomeMap.exactSearch)
					{
						for (int i = 0; i < BiomeMap.Attributes.Length; ++i)
						{
							if (pixelBilinear == BiomeMap.Attributes[i].mapColor)
							{
								mapAttribute = BiomeMap.Attributes[i];
							}
						}
					}
					else
					{
						float zero = 0;
						float num = 1 / zero;
						for (int j = 0; j < BiomeMap.Attributes.Length; ++j)
						{
							Color mapColor = BiomeMap.Attributes[j].mapColor;
							float sqrMagnitude = ((Vector4)(mapColor - pixelBilinear)).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								bool testCase = true;
								if (BiomeMap.nonExactThreshold != -1)
								{
									testCase = (sqrMagnitude < BiomeMap.nonExactThreshold);
								}
								if (testCase)
								{
									mapAttribute = BiomeMap.Attributes[j];
									num = sqrMagnitude;
								}
							}
						}
					}
				}
				*/
			}
			catch (NullReferenceException)
			{
				mapAttribute = new CBAttributeMap.MapAttribute();
				mapAttribute.name = "N/A";
			}

			return mapAttribute;
		}

		public static ExperimentSituations GetExperimentSituation(this Vessel vessel)
		{
			Vessel.Situations situation = vessel.situation;

			switch (situation)
			{
				case Vessel.Situations.PRELAUNCH:
				case Vessel.Situations.LANDED:
					return ExperimentSituations.SrfLanded;
				case Vessel.Situations.SPLASHED:
					return ExperimentSituations.SrfSplashed;
				case Vessel.Situations.FLYING:
					if (vessel.altitude < (double)vessel.mainBody.scienceValues.flyingAltitudeThreshold)
					{
						return ExperimentSituations.FlyingLow;
					}
					else
					{
						return ExperimentSituations.FlyingHigh;
					}
			}

			if (vessel.altitude < (double)vessel.mainBody.scienceValues.spaceAltitudeThreshold)
			{
				return ExperimentSituations.InSpaceLow;
			}
			else
			{
				return ExperimentSituations.InSpaceHigh;
			}
		}

		public static string HumanString(this ExperimentSituations situation)
		{
			switch (situation)
			{
				case ExperimentSituations.FlyingHigh:
					return "Upper Atmosphere";
				case ExperimentSituations.FlyingLow:
					return "Flying";
				case ExperimentSituations.SrfLanded:
					return "Surface";
				case ExperimentSituations.InSpaceLow:
					return "Near in Space";
				case ExperimentSituations.InSpaceHigh:
					return "High in Space";
				case ExperimentSituations.SrfSplashed:
					return "Splashed Down";
				default:
					return "Unknown";
			}
		}
		#endregion

		#region VESSEL_EXTENSIONS_LAT_LONG
		public static string GetLongitudeString(this Vessel vessel, string format = "F4")
		{
			string dir_long = "W";
			double v_long = vessel.longitude;

			v_long = FixDegreeDomain(v_long);

			if (v_long < -180d)
			{
				v_long += 360d;
			}
			if (v_long >= 180)
			{
				v_long -= 360d;
			}

			if (v_long > 0)
				dir_long = "E";

			return string.Format("{0}° {1}", Math.Abs(v_long).ToString(format), dir_long);
		}

		public static string GetLatitudeString(this Vessel vessel, string format = "F4")
		{
			string dir_lat = "S";
			double v_lat = vessel.latitude;
			if (v_lat > 0)
				dir_lat = "N";

			return string.Format("{0}° {1}", Math.Abs(v_lat).ToString(format), dir_lat);
		}
		#endregion

		#region VESSEL_EXTENSIONS_GENERAL
		public static double TrueAltitude(Vessel vessel)
		{
			double trueAltitude = vessel.orbit.altitude - vessel.terrainAltitude;

			// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m,
			// and water covers the whole surface at 0 m.
			if (vessel.terrainAltitude < 0 && vessel.mainBody.ocean)
			{
				trueAltitude = vessel.orbit.altitude;
			}

			return trueAltitude;
		}

		public static double Radius(this Vessel vessel)
		{
			double radius;

			radius = vessel.altitude;

			if (vessel.mainBody != null)
			{
				radius += vessel.mainBody.Radius;
			}

			return radius;
		}
		#endregion

		#region GEOMETRY_UTILS
		public static double FixAngleDomain(double Angle, bool Degrees = false)
		{
			double Extent = 2d * Math.PI;
			if (Degrees)
			{
				Extent = 360d;
			}

			Angle = Angle % (Extent);
			if (Angle < 0d)
			{
				Angle += Extent;
			}

			return Angle;
		}

		public static double FixDegreeDomain(double Angle)
		{
			return FixAngleDomain(Angle, true);
		}
		#endregion

		public static string ConvertInterval(double seconds)
		{
			string format_1 = "{0:D1}y {1:D1}d {2:D2}h {3:D2}m {4:D2}.{5:D1}s";
			string format_2 = "{0:D1}d {1:D2}h {2:D2}m {3:D2}.{4:D1}s";
			string format_3 = "{0:D2}h {1:D2}m {2:D2}.{3:D1}s";

			TimeSpan interval;

			try
			{
				interval = TimeSpan.FromSeconds(seconds);
			}
			catch (OverflowException)
			{
				return "NaN";
			}

			int years = interval.Days / 365;

			string output;
			if (years > 0)
			{
				output = string.Format(format_1,
					years,
					interval.Days - (years * 365), //  subtract years * 365 for accurate day count
					interval.Hours,
					interval.Minutes,
					interval.Seconds,
					interval.Milliseconds.ToString().Substring(0, 1));
			}
			else if (interval.Days > 0)
			{
				output = string.Format(format_2,
					interval.Days,
					interval.Hours,
					interval.Minutes,
					interval.Seconds,
					interval.Milliseconds.ToString().Substring(0, 1));
			}
			else
			{
				output = string.Format(format_3,
					interval.Hours,
					interval.Minutes,
					interval.Seconds,
					interval.Milliseconds.ToString().Substring(0, 1));
			}
			return output;
		}

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}

		//transfer angles
		public static double Nivvy_CalcTransferPhaseAngle(double r_current, double r_target, double grav_param)
		{
			double T_target = (2 * Math.PI) * Math.Sqrt(Math.Pow((r_target / 1000), 3) / (grav_param / 1000000000));
			double T_transfer = (2 * Math.PI) * Math.Sqrt(Math.Pow((((r_target / 1000) + (r_current / 1000)) / 2), 3) / (grav_param / 1000000000));
			return 360 * (0.5 - (T_transfer / (2 * T_target)));
		}

		public static double Younata_DeltaVToGetToOtherBody(double mu, double r1, double r2)
		{
			/*			
            def deltaVToGetToOtherBody(mu, r1, r2):
            # mu = gravity param of common orbiting body of r1 and r2
            # (e.g. for mun to minmus, mu is kerbin's gravity param
            # r1 = initial body's orbit radius
            # r2 = target body's orbit radius
		
            # return value is km/s
            sur1 = math.sqrt(mu / r1)
            sr1r2 = math.sqrt(float(2*r2)/float(r1+r2))
            mult = sr1r2 - 1
            return sur1 * mult
            */
			double sur1, sr1r2, mult;
			sur1 = Math.Sqrt(mu / r1);
			sr1r2 = Math.Sqrt((2 * r2) / (r1 + r2));
			mult = sr1r2 - 1;
			return sur1 * mult;
		}

		public static double Younata_DeltaVToExitSOI(double mu, double r1, double r2, double v)
		{
			/*			
            def deltaVToExitSOI(mu, r1, r2, v):
            # mu = gravity param of current body
            # r1 = current orbit radius
            # r2 = SOI radius
            # v = SOI exit velocity
            foo = r2 * (v**2) - 2 * mu
            bar = r1 * foo + (2 * r2 * mu)
            r = r1*r2
            return math.sqrt(bar / r)
            */
			double foo = r2 * Math.Pow(v, 2) - 2 * mu;
			double bar = r1 * foo + (2 * r2 * mu);
			double r = r1 * r2;
			return Math.Sqrt(bar / r);
		}

		public static double Younata_TransferBurnPoint(double r, double v, double angle, double mu)
		{
			/*			
            def transferBurnPoint(r, v, angle, mu):
            # r = parking orbit radius
            # v = ejection velocity
            # angle = phase angle (from function phaseAngle())
            # mu = gravity param of current body.
            epsilon = ((v**2)/2) - (mu / r)
            h = r * v * math.sin(angle)
            e = math.sqrt(1 + ((2 * epsilon * h**2)/(mu**2)))
            theta = math.acos(1.0 / e)
            degrees = theta * (180.0 / math.pi)
            return 180 - degrees
            */
			double epsilon, h, ee, theta, degrees;
			epsilon = (Math.Pow(v, 2) / 2) - (mu / r);
			h = r * v * Math.Sin(angle);
			ee = Math.Sqrt(1 + ((2 * epsilon * Math.Pow(h, 2)) / Math.Pow(mu, 2)));
			theta = Math.Acos(1.0 / ee);
			degrees = theta * (180.0 / Math.PI);
			return 180 - degrees;
			// returns the ejection angle
		}

		public static double Adammada_CurrrentPhaseAngle(
			double body_LAN,
			double body_orbitPct,
			double origin_LAN,
			double origin_orbitPct
		)
		{
			double angle = (body_LAN / 360 + body_orbitPct) - (origin_LAN / 360 + origin_orbitPct);
			if (angle > 1)
				angle = angle - 1;
			if (angle < 0)
				angle = angle + 1;
			if (angle > 0.5)
				angle = angle - 1;
			angle = angle * 360;
			return angle;
		}

		public static double Adammada_CurrentEjectionAngle(
			double vessel_long,
			double origin_rotAngle,
			double origin_LAN,
			double origin_orbitPct
		)
		{
			//double eangle = ((FlightGlobals.ActiveVOID.vessel.longitude + orbiting.rotationAngle) - (orbiting.orbit.LAN / 360 + orbiting.orbit.orbitPercent) * 360);
			double eangle = ((vessel_long + origin_rotAngle) - (origin_LAN / 360 + origin_orbitPct) * 360);

			while (eangle < 0)
				eangle = eangle + 360;
			while (eangle > 360)
				eangle = eangle - 360;
			if (eangle < 270)
				eangle = 90 - eangle;
			else
				eangle = 450 - eangle;
			return eangle;
		}

		public static double mrenigma03_calcphase(Vessel vessel, CelestialBody target)   //calculates phase angle between the current body and target body
		{
			Vector3d vecthis = new Vector3d();
			Vector3d vectarget = new Vector3d();
			vectarget = target.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());

			if ((vessel.mainBody.name == "Sun") || (vessel.mainBody.referenceBody.referenceBody.name == "Sun"))
			{
				vecthis = vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());
			}
			else
			{
				vecthis = vessel.mainBody.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());
			}

			vecthis = Vector3d.Project(new Vector3d(vecthis.x, 0, vecthis.z), vecthis);
			vectarget = Vector3d.Project(new Vector3d(vectarget.x, 0, vectarget.z), vectarget);

			Vector3d prograde = new Vector3d();
			prograde = Quaternion.AngleAxis(90, Vector3d.forward) * vecthis;

			double phase = Vector3d.Angle(vecthis, vectarget);

			if (Vector3d.Angle(prograde, vectarget) > 90)
				phase = 360 - phase;

			return (phase + 360) % 360;
		}

		public static double adjustCurrPhaseAngle(double transfer_angle, double curr_phase)
		{
			if (transfer_angle < 0)
			{
				if (curr_phase > 0)
					return (-1 * (360 - curr_phase));
				else if (curr_phase < 0)
					return curr_phase;
			}
			else if (transfer_angle > 0)
			{
				if (curr_phase > 0)
					return curr_phase;
				else if (curr_phase < 0)
					return (360 + curr_phase);
			}
			return curr_phase;
		}

		public static double adjust_current_ejection_angle(double curr_ejection)
		{
			//curr_ejection WILL need to be adjusted once for all transfers as it returns values ranging -180 to 180
			// need 0-360 instead
			//
			// ie i have -17 in the screenshot
			// need it to show 343
			//
			// do this
			//
			// if < 0, add curr to 360  // 360 + (-17) = 343
			// else its good as it is

			if (curr_ejection < 0)
				return 360 + curr_ejection;
			else
				return curr_ejection;

		}

		public static double adjust_transfer_ejection_angle(double trans_ejection, double trans_phase)
		{
			// if transfer_phase_angle < 0 its a lower transfer
			//180 + curr_ejection
			// else if transfer_phase_angle > 0 its good as it is

			if (trans_phase < 0)
				return 180 + trans_ejection;
			else
				return trans_ejection;

		}

		public static void display_transfer_angles_SUN2PLANET(CelestialBody body, Vessel vessel)
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(
				Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + Tools.Nivvy_CalcTransferPhaseAngle(
					vessel.orbit.semiMajorAxis,
					body.orbit.semiMajorAxis,
					vessel.mainBody.gravParameter
				).ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label(
				(Tools.Younata_DeltaVToGetToOtherBody(
					(vessel.mainBody.gravParameter / 1000000000),
					(vessel.orbit.semiMajorAxis / 1000),
					(body.orbit.semiMajorAxis / 1000)
				) * 1000).ToString("F2") + "m/s",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();
		}

		public static void display_transfer_angles_PLANET2PLANET(CelestialBody body, Vessel vessel)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody(
				(vessel.mainBody.referenceBody.gravParameter / 1000000000),
				(vessel.mainBody.orbit.semiMajorAxis / 1000),
				(body.orbit.semiMajorAxis / 1000)
			);
			double dv2 = Tools.Younata_DeltaVToExitSOI(
				(vessel.mainBody.gravParameter / 1000000000),
				(vessel.orbit.semiMajorAxis / 1000),
				(vessel.mainBody.sphereOfInfluence / 1000),
				Math.Abs(dv1)
			);

			double trans_ejection_angle = Tools.Younata_TransferBurnPoint(
				(vessel.orbit.semiMajorAxis / 1000),
				dv2,
				(Math.PI / 2.0),
				(vessel.mainBody.gravParameter / 1000000000)
			);
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(
				FlightGlobals.ActiveVessel.longitude,
				FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent
			);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(
				vessel.mainBody.orbit.semiMajorAxis,
				body.orbit.semiMajorAxis,
				vessel.mainBody.referenceBody.gravParameter
			) % 360;
			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(
				body.orbit.LAN,
				body.orbit.orbitPercent,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent
			);

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(
				adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Ejection angle (curr/trans):");
			GUILayout.Label(
				adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		public static void display_transfer_angles_PLANET2MOON(CelestialBody body, Vessel vessel)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody(
				(vessel.mainBody.gravParameter / 1000000000),
				(vessel.orbit.semiMajorAxis / 1000),
				(body.orbit.semiMajorAxis / 1000)
			);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(
				vessel.orbit.semiMajorAxis,
				body.orbit.semiMajorAxis,
				vessel.mainBody.gravParameter
			);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(
				Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv1 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		public static void display_transfer_angles_MOON2MOON(CelestialBody body, Vessel vessel)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody(
				(vessel.mainBody.referenceBody.gravParameter / 1000000000),
				(vessel.mainBody.orbit.semiMajorAxis / 1000),
				(body.orbit.semiMajorAxis / 1000)
			);
			double dv2 = Tools.Younata_DeltaVToExitSOI(
				(vessel.mainBody.gravParameter / 1000000000),
				(vessel.orbit.semiMajorAxis / 1000),
				(vessel.mainBody.sphereOfInfluence / 1000),
				Math.Abs(dv1)
			);
			double trans_ejection_angle = Tools.Younata_TransferBurnPoint(
				(vessel.orbit.semiMajorAxis / 1000),
				dv2,
				(Math.PI / 2.0),
				(vessel.mainBody.gravParameter / 1000000000)
			);

			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(
				body.orbit.LAN,
				body.orbit.orbitPercent,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent
			);
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(
				FlightGlobals.ActiveVessel.longitude,
				FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN,
				FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent
			);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(
				vessel.mainBody.orbit.semiMajorAxis,
				body.orbit.semiMajorAxis,
				vessel.mainBody.referenceBody.gravParameter
			) % 360;

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
			//double adj_ejection_angle = adjustCurrEjectionAngle(trans_phase_angle, curr_ejection_angle);

			//new stuff
			//
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);
			//
			//

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(
				adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Ejection angle (curr/trans):");
			GUILayout.Label(
				adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°",
				GUILayout.ExpandWidth(false)
			);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		public static string get_heading_text(double heading)
		{
			if (heading > 348.75 || heading <= 11.25)
				return "N";
			else if (heading > 11.25 && heading <= 33.75)
				return "NNE";
			else if (heading > 33.75 && heading <= 56.25)
				return "NE";
			else if (heading > 56.25 && heading <= 78.75)
				return "ENE";
			else if (heading > 78.75 && heading <= 101.25)
				return "E";
			else if (heading > 101.25 && heading <= 123.75)
				return "ESE";
			else if (heading > 123.75 && heading <= 146.25)
				return "SE";
			else if (heading > 146.25 && heading <= 168.75)
				return "SSE";
			else if (heading > 168.75 && heading <= 191.25)
				return "S";
			else if (heading > 191.25 && heading <= 213.75)
				return "SSW";
			else if (heading > 213.75 && heading <= 236.25)
				return "SW";
			else if (heading > 236.25 && heading <= 258.75)
				return "WSW";
			else if (heading > 258.75 && heading <= 281.25)
				return "W";
			else if (heading > 281.25 && heading <= 303.75)
				return "WNW";
			else if (heading > 303.75 && heading <= 326.25)
				return "NW";
			else if (heading > 326.25 && heading <= 348.75)
				return "NNW";
			else
				return "";
		}
	}
}

