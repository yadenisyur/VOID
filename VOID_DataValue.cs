//
//  VOID_DataValue.cs
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
using System;
using UnityEngine;

namespace VOID
{
	public interface IVOID_DataValue
	{
		void Refresh();
	}

	public class VOID_DataValue<T> : IVOID_DataValue
	{
		/*
		 * Static Members
		 * */
		public static implicit operator T(VOID_DataValue<T> v)
		{
			return (T)v.Value;
		}

		/*
		 * Instance Members
		 * */
		/*
		* Fields
		 * */
		protected T cache;
		protected Func<T> ValueFunc;

		/* 
		 * Properties
		 * */
		public string Label { get; protected set; }
		public string Units { get; protected set; }

		public T Value {
			get {
				return (T)this.cache;
			}
		}

		public string ValueUnitString {
			get {
				return this.Value.ToString() + this.Units;
			}
		}


		/*
		 * Methods
		 * */
		public VOID_DataValue(string Label, Func<T> ValueFunc, string Units = "")
		{
			this.Label = Label;
			this.Units = Units;
			this.ValueFunc = ValueFunc;
			this.cache = this.ValueFunc.Invoke ();
		}

		public void Refresh()
		{
			this.cache = this.ValueFunc.Invoke ();
		}

		public T GetFreshValue()
		{
			this.Refresh ();
			return (T)this.cache;
		}

		public virtual void DoGUIHorizontal()
		{
			GUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
			GUILayout.Label (this.Label + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (this.ValueUnitString, GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal ();
		}

		public override string ToString()
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (),
				this.Units
			);
		}
	}

	internal interface IVOID_NumericValue
	{
		string ToString(string format);
		string ToSIString(int digits, int MinMagnitude, int MaxMagnitude);
	}

	public abstract class VOID_NumValue<T> : VOID_DataValue<T>, IVOID_NumericValue
	{
		public VOID_NumValue(string Label, Func<T> ValueFunc, string Units = "") : base(Label, ValueFunc, Units) {}

		public abstract string ToString(string Format);
		public abstract string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue);

		public ushort DoGUIHorizontal(ushort digits)
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + ":", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			GUILayout.Label(
				this.ToSIString(digits),
				GUILayout.ExpandWidth(false)
			);
			if (GUILayout.Button ("P")) {
				digits = (ushort)((digits + 3) % 15);
			}
			GUILayout.EndHorizontal();

			return digits;
		}
		public override void DoGUIHorizontal ()
		{
			this.DoGUIHorizontal (3);
		}
	}

	public class VOID_DoubleValue : VOID_NumValue<double>, IVOID_NumericValue
	{
		public VOID_DoubleValue(string Label, Func<double> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}

		public override string ToString(string format)
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (format),
				this.Units
			);
		}

		public override string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue)
		{
			return string.Format (
				"{0}{1}",
				Tools.MuMech_ToSI (this.Value, digits, MinMagnitude, MaxMagnitude),
				this.Units
			);
		}
	}
	public class VOID_FloatValue : VOID_NumValue<float>, IVOID_NumericValue
	{
		public VOID_FloatValue(string Label, Func<float> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}

		public override string ToString(string format)
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (format),
				this.Units
			);
		}

		public override string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue)
		{
			return string.Format (
				"{0}{1}",
				Tools.MuMech_ToSI ((double)this.Value, digits, MinMagnitude, MaxMagnitude),
				this.Units
			);
		}
	}
	public class VOID_IntValue : VOID_NumValue<int>, IVOID_NumericValue
	{
		public VOID_IntValue(string Label, Func<int> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}

		public override string ToString(string format)
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (format),
				this.Units
			);
		}

		public override string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue)
		{
			return string.Format (
				"{0}{1}",
				Tools.MuMech_ToSI ((double)this.Value, digits, MinMagnitude, MaxMagnitude),
				this.Units
			);
		}
	}


	public class VOID_StrValue : VOID_DataValue<string>
	{
		public VOID_StrValue(string Label, Func<string> ValueFunc, string Units = "") : base(Label, ValueFunc, Units) {}
	}
}

