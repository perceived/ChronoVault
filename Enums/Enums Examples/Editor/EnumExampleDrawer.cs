/**
	EnumExampleDrawer:
		PropertyDrawer examples for Enums.
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-30-2017
	Date Last Modified:
		07-03-2017
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
using UnityEditor;
using UnityEngine;

namespace ChronoVault.Examples {
	
	/// <summary>
	///		v1: Specifically for EnumExample.Vehicles
	/// </summary>
	[CustomPropertyDrawer(typeof(EnumExample.Vehicles))]
	public class VehiclesDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			Color colorPrior = GUI.color;
			GUI.color = Color.cyan;

			//	Works just for Vehicles... and just barely.
			property.intValue = (int) (EnumExample.Vehicles) EditorGUI.EnumMaskField(position, label, (EnumExample.Vehicles) property.intValue);

			GUI.color = colorPrior;
		}
	}


	/// <summary>
	///		v2: Using an Attribute to work for any enum
	/// </summary>
	[CustomPropertyDrawer(typeof(EnumFlagsBrokenAttribute))]
	public class EnumFlagsBrokenDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			Color colorPrior = GUI.color;
			GUI.color = new Color32(255, 180, 0, 255);

			//	Can't use EnumMaskField again, since we don't have a Type of Enum (and int can't be converted to Enum, since it needs more information).
			//	InvalidCastException: Cannot cast from source type to destination type.
			//property.intValue = (int) (object) EditorGUI.EnumMaskField(position, label, (Enum) (object) property.intValue);

			//	Never use enumValueIndex!  We want the flagged enum to hold the combined values of its fields, not
			//	the arbitrary indices.
			//property.enumValueIndex = EditorGUI.MaskField(position, label, property.enumValueIndex, property.enumNames);

			//	Serious issue: This is still referencing by index, not intValue!  What's going on?
			//	Serious issue: Since we're using a property's value directly, this is NOT multi-object selection safe!
			property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
			
			GUI.color = colorPrior;
		}
	}


	/// <summary>
	///		v3: Oops!  That doesn't work with multi-object editing!
	/// </summary>
	[CustomPropertyDrawer(typeof(EnumFlagsBasicAttribute))]
	public class EnumFlagsBasicDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			Color colorPrior = GUI.color;
			GUI.color = new Color32(85, 255, 160, 255);

			/**	Since MaskField has no multi-object selection safe option (one that uses SerializedProperty directly as an input),
			 *	we have to wrap one ourselves. **/

			//	We need to know if we're displaying a 'Mixed' value: our selections have differing values.
			//	DO NOT confuse this with the "Mixed ..." display when a flagged enum: that means the individual selection has multiple flagged values.
			bool mixedPrior = EditorGUI.showMixedValue;
			if (property.hasMultipleDifferentValues)
				EditorGUI.showMixedValue = true;

			//	Iterate over each of the targetObjects (each selected object this Drawer will draw for) and gets its SerializedProperty.
			//	This is reversed (.Last) simply so it makes logical sense: Reversing makes the topmost selection the first in the list, as expected.
			//	We need to get the first selection and use it for drawing the inspector only
			SerializedProperty firstSelection = property.serializedObject.targetObjects.Select(selection => new SerializedObject(selection).FindProperty(property.propertyPath)).Last();

			//	Begin a 'change check', which tells us when the user selects a value.  Note that this gets called even if the user selects the value
			//	already selected.
			EditorGUI.BeginChangeCheck();

			//	Handle the field for the first selection only.  Since we're assigning only one selection's intValue to itself, this will never overwrite a value.
			firstSelection.intValue = EditorGUI.MaskField(position, label, firstSelection.intValue, property.enumNames);

			//	If the selection changed, apply it to all selections
			if (EditorGUI.EndChangeCheck())
				property.intValue = firstSelection.intValue;

			EditorGUI.showMixedValue = mixedPrior;
			GUI.color = colorPrior;


			//	Can be merged into one function for simplicity's sake.  This handles showMixedValue, proper field drawing for the top selection only,
			//	multi-object selection safe, and applying back to all selections on change.
			//EditorGUIHelper.MultiField(
			//	property,
			//	(propIter) => propIter.intValue = EditorGUI.MaskField(position, label, propIter.intValue, property.enumNames),
			//	(propIter) => property.intValue = propIter.intValue);
		}
	}
}