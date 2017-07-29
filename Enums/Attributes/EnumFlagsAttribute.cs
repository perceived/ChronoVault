/**
	EnumFlagsAttribute:
		Allows enum flags without EditorGUI code.
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-29-2015
	Date Last Modified:
		02-20-2017
	Attach Script to:
		Nowhere; is added to enums with the [EnumFlags] or [EnumFlagsAttribute] attribute and drawn with the EnumFlagsDrawer PropertyDrawer.
	Notes:
		
	Change log:
		08-24-2016:	Modified to work with EditorGUIHelper.EnumMaskPopup (supports comma-separated values, guaranteeing the value exists within the enum, bitmask for selectable entries,
						holding value by actual enum values instead of indices, and comining same value entries into one line).
		02-20-2017:	Added isSkippingNonFlags.
	To do:
		
	Bugs:
		
**/

using System;
using UnityEngine;

namespace ChronoVault {

	public class EnumFlagsAttribute : PropertyAttribute {

		/// <summary>
		///		A flagged mask for values to allow entry for.
		/// </summary>
		public int entries = ~0;

		/// <summary>
		///		If true, enum entries with a value of zero are not displayed (useful if the entry already maps to 'None').
		/// </summary>
		public bool isSkippingZero;

		/// <summary>
		///		If true, if all possible elements are selected (all elements that entries allows), 'Everything' is displayed instead of comma-separated values.
		/// </summary>
		public bool isDisplayingEverything = true;

		/// <summary>
		///		If true, non-flags are skipped (such as 1 << 0 | 1 << 1 to combine element 0 and 1).
		/// </summary>
		public bool isSkippingNonFlags = true;



		public EnumFlagsAttribute(int entries) {

			this.entries = entries;
			isDisplayingEverything = true;
		}

		public EnumFlagsAttribute() : this(~0) {}
	}
}