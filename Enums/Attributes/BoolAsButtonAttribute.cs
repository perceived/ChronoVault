/**
	BoolAsButtonAttribute:
		Handles drawing of a bool as a toggleable button in the inspector.
		Button can call parameterless methods OnPress, OnRelease, and OnClick by assigning with a method name on the corresponding object (supports public, private, instanced, and static).
	Author(s):
		Ryan Scott Clark
	Date Created:
		04-22-2016
	Date Last Modified:
		11-03-2016
	Attach Script to:
		Nowhere; is added to enums with the [BoolAsButton] or [BoolAsButtonAttribute] attribute and drawn with the BoolAsButtonDrawer PropertyDrawer.
	Notes:
		
	Change log:
		05-29-2016:	Added ClickableDuring, for greying out the button during either Editor or Runtime.
		07-07-2016:	Moved PlaybackStates (and the isClickable property, now as an extension) to TimeUtility.
		08-11-2016:	Removed onClick.
		11-03-2016:	Removed onRelease.  Instead, if using BoolStyle.Toggleable the onPress function should use the current bool value.
	To do:
		
	Bugs:
		
**/

using System;
using UnityEngine;

namespace ChronoVault {

	public class BoolAsButtonAttribute : PropertyAttribute {

		public enum ButtonLocation {
			Label,
			Field,
			Full,
			FieldWithLabel,
		}

		/// <summary>
		///		The styles the bool can take.  Buttons release automatically OnPress, while Toggleable can hold a state of true or false.
		/// </summary>
		public enum BoolStyle {
			/// <summary>
			///		Releases automatically OnPress.
			/// </summary>
			Button,
			/// <summary>
			///		Can hold a state of true or false.
			/// </summary>
			Toggleable,
		}

		public ButtonLocation buttonLocation = ButtonLocation.Field;

		public BoolStyle boolStyle = BoolStyle.Button;

		public string labelTrue = null;

		public string labelFalse = null;

		public string tooltip;

		public string onPress = null;
		


		public BoolAsButtonAttribute(string labelTrue, string labelFalse) {

			this.labelTrue = labelTrue;
			this.labelFalse = labelFalse;
		}

		public BoolAsButtonAttribute(string label) {

			this.labelTrue = label;
			this.labelFalse = label;
		}

		public BoolAsButtonAttribute() {}
	}
}