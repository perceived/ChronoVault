/**
	EnumExample:
		Shows examples of Enum use, for use alongside Chronosapien Enum devlogs:
			http://www.chronosapien.com/blog/break-the-ice-and-enums-pt-1
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-28-2017
	Date Last Modified:
		06-28-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
namespace ChronoVault.Examples {

	public class EnumExample : MonoBehaviour {
		
		//	'Enum' is not the keyword you're looking for.  'Enum' is the back-end Type inherited by enums.
		//public Enum test {
		//	val1		= 0,
		//	val2		= 1,
		//}

		/// <summary>
		///		Standard Vehicle implementation: non-flagged (single value) enum.
		/// </summary>
		public enum Vehicle {
			Convertible	= 0,
			Hatchback	= 1,
			Compact		= 2,
			Pickup		= 3,
			BigAssTruck	= Pickup,	//	BigAssTruck holds the same value as Pickup
			SUV			= 4,
			Van			= 5,
			Minivan		= Van,		//	Minivan holds the same value as Van
		}

		#region Alternative Vehicle declarations
		/// <summary>
		///		Same as Vehicle, with values implicitly assigned (using 0-index).
		/// </summary>
		public enum Vehicle_01 {
			Convertible,
			Hatchback,
			Compact,
			Pickup,
			BigAssTruck	= Pickup,
			SUV,
			Van,
			Minivan		= Van,
		}

		/// <summary>
		///		Same as Vehicle, with the underlying type explicitly declared as int.
		/// </summary>
		public enum Vehicle_02 : int {
			Convertible,
			Hatchback,
			Compact,
			Pickup,
			BigAssTruck	= Pickup,
			SUV,
			Van,
			Minivan		= Van,
		}
		#endregion

		/// <summary>
		///		Standard Vehicles implementation: flagged (multiple value) enum.
		/// </summary>
		[Flags]
		public enum Vehicles {
			Convertible	= 1 << 0,
			[System.ComponentModel.Description("ʞɔɐqɥɔʇɐH")]
			Hatchback	= 1 << 1,
			Compact		= 1 << 2,
			Pickup		= 1 << 3,
			[System.ComponentModel.Description("Big Ass Truck")]
			BigAssTruck	= Pickup,
			SUV			= 1 << 4,
			Van			= 1 << 5,
			Minivan		= Van,
		}

		#region Alternative Vehicles declarations
		/// <summary>
		///		The "do the math yourself" version of Vehicles.
		///		Doing the math yourself isn't fun, isn't very readable, and is mistake prone.
		/// </summary>
		[Flags]
		public enum Vehicles_01 {
			Convertible	= 1,
			Hatchback	= 2,
			Compact		= 4,
			Pickup		= 8,
			BigAssTruck	= Pickup,
			SUV			= 16,
			Van			= 32,
			Minivan		= Van,
		}

		/// <summary>
		///		The hexadecimal version of Vehicles.
		///		Some prefer this because it has a pattern (1-2-4-8 then shift one digit), and hardcore programmers love that shit.
		/// </summary>
		[Flags]
		public enum Vehicles_02 {
			Convertible	= 0x01,
			Hatchback	= 0x02,
			Compact		= 0x04,
			Pickup		= 0x08,
			BigAssTruck	= Pickup,
			SUV			= 0x10,
			Van			= 0x20,
			Minivan		= Van,
		}

		//	The binary literal version of Vehicles, declared with the '0b' prefix.
		//	You may see this on the webs, but it's not valid in Unity (binary literals rely on C# 7.0, and Unity is pathetically built on a 10 year old version of C#).
		//	This clearly shows that each enum value is a bitmask: a binary value with bit-shifting being performed.
		//[Flags]
		//public enum Vehicles_03 {
		//	Convertible	= 0b000001,
		//	Hatchback	= 0b000010,
		//	Compact		= 0b000100,
		//	Pickup		= 0b001000,
		//	BigAssTruck	= Pickup,
		//	SUV			= 0b010000,
		//	Van			= 0b100000,
		//	Minivan		= Van,
		//}

		//	Don't make this mistake!  '^' is the binary XOR operator, not a power operator!
		//[Flags]
		//public enum Vehicles_04 {
		//	Convertible	= 2^0,		// This is actually 2
		//	Hatchback	= 2^1,		// This is actually 3
		//	Compact		= 2^2,		// This is actually 0
		//	Pickup		= 2^3,		// This is actually 1
		//	BigAssTruck	= Pickup,
		//	SUV			= 2^4,		// This is actually 6
		//	Van			= 2^5,		// This is actually 7
		//	Minivan		= Van,
		//}
		#endregion
		
		/// <summary>
		///		A version of Vehicles that also includes handy combined-entry values (Car and OtherCar, which each contain multiple entries' values).
		/// </summary>
		[Flags]
		public enum DetailedVehicles {
			Convertible	= 1 << 0,
			[System.ComponentModel.Description("ʞɔɐqɥɔʇɐH")]
			Hatchback	= 1 << 1,
			Compact		= 1 << 2,
			Pickup		= 1 << 3,
			[System.ComponentModel.Description("Big Ass Truck")]
			BigAssTruck	= Pickup,
			SUV			= 1 << 4,
			Van			= 1 << 5,
			Minivan		= Van,
			Car			= Convertible | Hatchback | Compact,	//	Car includes Convertible, Hatchback, and Compact (added together with boolean OR operator).
			OtherCar	= Convertible | Hatchback | Compact,	//	OtherCar matches Car
		}

		#region Other Enum declarations
		[Flags]
		public enum Emoticons {
			[System.ComponentModel.Description("( •_•)")]
			One		= 1 << 0,
			[System.ComponentModel.Description("( •_•)>⌐■-■")]
			Two		= 1 << 1,
			[System.ComponentModel.Description("(⌐■_■)")]
			Three	= 1 << 2,
			[System.ComponentModel.Description("( ͡° ͜ʖ ͡°)")]
			Four	= 1 << 3,
			[System.ComponentModel.Description("ಠᴗಠ")]
			Five	= 1 << 4,
		}

		/// <summary>
		///		Enum to illustrate maximum entry count.
		/// </summary>
		[Flags]
		public enum MaxEntries {
			a	= 1 << 0,
			b	= 1 << 1,
			c	= 1 << 2,
			d	= 1 << 3,
			e	= 1 << 4,
			f	= 1 << 5,
			g	= 1 << 6,
			h	= 1 << 7,
			i	= 1 << 8,
			j	= 1 << 9,
			k	= 1 << 10,
			l	= 1 << 11,
			m	= 1 << 12,
			n	= 1 << 13,
			o	= 1 << 14,
			p	= 1 << 15,
			q	= 1 << 16,
			r	= 1 << 17,
			s	= 1 << 18,
			t	= 1 << 19,
			u	= 1 << 20,
			v	= 1 << 21,
			w	= 1 << 22,
			x	= 1 << 23,
			y	= 1 << 24,
			z	= 1 << 25,
			aa	= 1 << 26,
			ab	= 1 << 27,
			ac	= 1 << 28,
			ad	= 1 << 29,
			//ae	= 1 << 30,	//	Commented out, so that ag (1 << 32) will display (otherwise it gets cut off due to 32 entry limit)
			af	= 1 << 31,		//	Valid, but will hold a value of -2,147,483,648. Since the leftmost bit of an int is the sign, it is forced to be negative
			ag	= 1 << 32,		//	Invalid, int can only support up to 1 << 31 (32 entries excluding 0).  This wraps around to hold the same value as 'a'.
		}

		/// <summary>
		///		Enum to illustrate what happens when entry cound is exceeded.
		/// </summary>
		public enum ExceededEntries {
			a	= 1 << 0,
			b	= 1 << 1,
			c	= 1 << 2,
			d	= 1 << 3,
			e	= 1 << 4,
			f	= 1 << 5,
			g	= 1 << 6,
			h	= 1 << 7,
			i	= 1 << 8,
			j	= 1 << 9,
			k	= 1 << 10,
			l	= 1 << 11,
			m	= 1 << 12,
			n	= 1 << 13,
			o	= 1 << 14,
			p	= 1 << 15,
			q	= 1 << 16,
			r	= 1 << 17,
			s	= 1 << 18,
			t	= 1 << 19,
			u	= 1 << 20,
			v	= 1 << 21,
			w	= 1 << 22,
			x	= 1 << 23,
			y	= 1 << 24,
			z	= 1 << 25,	//	Entry 26
			aa	= 1 << 26,	//	Won't display!  Shows up first, but is actually a larger value than z-prefixed entries, so they'll display first (cutting this entry off with the 32 entry limit)
			ab	= 1 << 27,	//	^
			ac	= 1 << 28,	//	^
			ad	= 1 << 29,	//	^
			ae	= 1 << 30,	//	^
			af	= 1 << 31,
			za	= a | z,	//	Entry 27
			zb	= b | z,
			zc	= c | z,
			zd	= d | z,
			ze	= e | z,
			zf	= f | z,	//	Entry 32 (final in popup)
			zg	= g | z,	//	Entry 33.  Won't display!  Valid according to enum declarations, but exceeds the 32 entry count limit (including combined entries) allowed with Unity popups
			zh	= h | z,	//	^
			zi	= i | z,	//	^
			zj	= j | z,	//	^
		}

		/// <summary>
		///		TestBounds has been ordered by entry value, so that it's clear which entries will work in the popup.
		///		Only 32 entries are allowed with the popup (including combined-entry values), so any entries after that will not work (due to Unity using an int value for their popup's mask).
		/// </summary>
		[Flags]
		public enum TestBounds {
			a	= 1 << 0,
			b	= 1 << 1,
			c	= 1 << 2,
			d	= 1 << 3,
			e	= 1 << 4,
			f	= 1 << 5,
			g	= 1 << 6,
			h	= 1 << 7,
			i	= 1 << 8,
			j	= 1 << 9,
			k	= 1 << 10,
			l	= 1 << 11,
			m	= 1 << 12,
			n	= 1 << 13,
			o	= 1 << 14,
			p	= 1 << 15,
			q	= 1 << 16,
			r	= 1 << 17,
			s	= 1 << 18,
			t	= 1 << 19,
			u	= 1 << 20,
			v	= 1 << 21,
			w	= 1 << 22,
			x	= 1 << 23,
			y	= 1 << 24,
			z	= 1 << 25,
			za	= a | z,
			zb	= b | z,
			zc	= c | z,
			zd	= d | z,
			ze	= e | z,
			zf	= f | z,
			zg	= g | z,	//	Won't display!  Entry 33, exceeding the bounds of the popup's bitmask.
			zh	= h | z,	//	^
			aa	= 1 << 26,	//	^
			ab	= 1 << 27,	//	^
		}
		#endregion


		[Header("Enums")]
		public Vehicle vehicle = Vehicle.Convertible;

		//	While the variable can hold multiple values, there's no way to assign them in the Inspector.
		public Vehicles vehiclesDrawer = Vehicles.SUV;

		//	Broken (value by enum index instead of enum value, no multi-object selection)
		[Header("Flagged Enums")]
		[EnumFlagsBroken]
		public Vehicles commonDrawerBroken = Vehicles.SUV;


		//	Multi-object safe, but still has issues
		[Header("Flagged Enums")]
		[EnumFlagsBasic]
		public Vehicles commonDrawerBasic = Vehicles.SUV;

		//	Fixed version of popup, which displays all entries (instead of "Everything")
		[EnumFlags(isDisplayingEverything = false)]
		public Vehicles vehiclesChronoVault = (Vehicles) ~0;

		//	Uses DetailedVehicles, but since EnumFlags skips non-flagged values by default, it will display just like Vehicles.
		[EnumFlags]
		public DetailedVehicles vehiclesDetailedSkip = DetailedVehicles.Car;

		//	Uses DetailedVehicles properly (enables non-flagged values to display).
		[EnumFlags(isSkippingNonFlags = false)]
		public DetailedVehicles vehiclesDetailed = DetailedVehicles.Car;


		[Header("EnumFlags use")]
		[EnumFlags]
		public Emoticons emoticons = Emoticons.One | Emoticons.Two | Emoticons.Three;

		//	Shows that 1 << 32 is invalid, since it wraps around and shares a value with a (1)
		[EnumFlags]
		public MaxEntries maxEntries = MaxEntries.ag;

		[EnumFlags(isSkippingNonFlags = false)]
		public ExceededEntries exceededEntries;

		[EnumFlags(isSkippingNonFlags = false)]
		public TestBounds testBounds;


		[Header("Testing")]
		[BoolAsButton(onPress = "GetTypes_Inspector"), SerializeField]
		[Tooltip("Displays type information for Vehicles.")]
		private bool getTypes;

		[BoolAsButton(onPress = "EnumCasting_Inspector"), SerializeField]
		[Tooltip("Displays casts to and from integer.")]
		private bool enumCasting;

		public string parseInput = "Compact, BigAssTruck";

		[BoolAsButton(onPress = "TestParse_Inspector"), SerializeField]
		[Tooltip("Tests Enum.Parse functionality for Vehicle and Vehicles.")]
		private bool testParse;

		[BoolAsButton(onPress = "TestValues_Inspector"), SerializeField]
		[Tooltip("Tests the values of the different Vehicles fields.")]
		private bool testValues;

		[BoolAsButton(onPress = "GetDescriptions_Inspector"), SerializeField]
		[Tooltip("Displays the descriptions for Vehicles.")]
		private bool getDescriptions;



		private void PerformOnVehicles(Action<Type> action) {

			Perform(action, typeof(Vehicle), typeof(Vehicles), typeof(DetailedVehicles));
		}

		private void Perform(Action<Type> action, params Type[] enumTypes) {

			foreach (Type type in enumTypes)
				action(type);
		}

		private void GetTypes_Inspector() {

			Debug.LogWarning("Begin of Get Types" + Environment.NewLine);

			PerformOnVehicles((type) => Debug.Log("Type: typeof(" + type.Name + ")" + Environment.NewLine
				+  "Underlying Type: " + Enum.GetUnderlyingType(typeof(Vehicles)).Name + " from Enum.GetUnderlyingType(typeof(" + type.Name + "))" + Environment.NewLine));
		}

		private void EnumCasting_Inspector() {

			Debug.LogWarning("Begin of Enum Casting" + Environment.NewLine);

			//	Explicitly cast from Vehicle's underlying Type (int)
			Vehicle vehicle = (Vehicle) 2;
			//	Prints "Compact"
			Debug.Log("Vehicle #2: " + vehicle.ToString() + Environment.NewLine);

			//	Explicitly cast to Vehicle's underlying Type (int)
			int vehicleInt = (int) Vehicle.Hatchback;
			//	Prints "1"
			Debug.Log("Vehicle.Hatchback's value: " + vehicleInt.ToString() + Environment.NewLine);


			//	Implicit cast to "None".  Implicit cast works for 0 only
			Vehicles vehiclesNone = 0;
			//	Prints "0"
			Debug.Log("vehiclesNone: " + vehiclesNone.ToString() + Environment.NewLine);

			//	Explicitly cast ~0, which means the "bitwise opposite of zero" or "all", to Vehicles
			Vehicles vehiclesAll = (Vehicles) ~0;
			//	Prints "-1", which is the enum version of "all" (due to bitwise math)
			Debug.Log("vehiclesAll: " + vehiclesAll.ToString() + Environment.NewLine);

			//	Prints true for each line, since everything is contained in vehiclesAll
			foreach (Vehicles veh in EnumUtility.GetList<Vehicles>())
				Debug.Log("vehiclesAll contains " + veh.ToString() + "? " + vehiclesAll.Contains(veh) + Environment.NewLine);
		}

		private void TestParse_Inspector() {

			Debug.LogWarning("Begin of Test Parse" + Environment.NewLine);

			PerformOnVehicles((type) => {
				Debug.Log(type.Name + " flagged parse of \"" + parseInput + "\":" + Environment.NewLine +
					"\"" + Enum.Parse(type, parseInput) + "\"");
			});
		}

		private void TestValues_Inspector() {

			Debug.LogWarning("Begin of Test Values" + Environment.NewLine);

			Debug.Log("v1: Vehicles Drawer Test" + Environment.NewLine
				+ vehiclesDrawer.ToString());

			Debug.Log("v2: Common Drawer Broken Test" + Environment.NewLine
				+ commonDrawerBroken.ToString());

			Debug.Log("v3: Common Drawer Basic Test" + Environment.NewLine
				+ commonDrawerBasic.ToString());

			Debug.Log("v4: Vehicles ChronoVault Test" + Environment.NewLine
				+ commonDrawerBasic.ToString());

			Debug.Log("Vehicles Detailed Test" + Environment.NewLine
				+ vehiclesDetailed.ToString());

			Debug.Log("Max Entries Test" + Environment.NewLine
				+ maxEntries.ToString());

			Debug.Log("Exceeded Entries Test" + Environment.NewLine
				+ exceededEntries.ToString());
		}

		private void GetDescriptions_Inspector() {

			string displayFormatter = @"<color=#ff56a0ff>{0}</color>: <color=#24aa42ff>{1}</color>, <color=#a023ffff>{2}</color>, <color=#b0b0ffff>{3}</color>";

			Perform((type) => {
				Debug.Log("Debugging '" + type.Name + "'" + Environment.NewLine
					+ String.Format(displayFormatter, "Index", "Description", "Name", "ToString by Index") + Environment.NewLine);

				List<Enum> entries = EnumUtility.GetCollection(type).ToList();
				List<string> names = Enum.GetNames(type).ToList();
				List<string> descriptions = EnumUtility.GetDescriptions(type);

				int index = 0;
				foreach (string description in descriptions) {
					Debug.Log(String.Format(displayFormatter, Convert.ToInt32(entries[index]), description, names[index], entries[index].ToString()) + Environment.NewLine);
					index++;
				}
			},
			typeof(Vehicles),
			typeof(DetailedVehicles));
		}
	}
}
#endif