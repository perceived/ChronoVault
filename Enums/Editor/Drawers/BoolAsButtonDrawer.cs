/**
	BoolAsButonDrawer:
		Draws a bool as a toggleable button in the Inspector.
	Author(s):
		Ryan Scott Clark
	Date Created:
		04-22-2016
	Date Last Modified:
		06-30-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		05-29-2016:	Added ClickableDuring, for greying out the button during either Editor or Runtime.
		08-09-2016:	Fixed a bug where callback methods (onPress, onRelease, onClick) couldn't modify properties of the script.
						Fix was to call serializedObject.Update.
		08-11-2016:	Removed onClick.
					Fixed a bug introduced with the 08-09-2016 update where the corresponding bool wasn't having its value change.
						With this fix, the bool can no longer have its value changed in the corresponding onPress or onRelease functions (it gets overwritten here).
		10-03-2016:	CallMethod now works for implemented interfaces and derived classes.
		10-13-2016:	Now works on buttons that are embedded (classes that aren't directly the serializedObject: ones that don't inherit from MonoBehaviour).
		11-03-2016:	Removed onRelease.  onClick now works differently:
						For a button, the bool will always have a value of false.
						For a toggle, the bool hold the current press value.  onClick gets called twice, both for press and release.
					Fixed a bug where Update (pulls changed values into serializedObject stream) was getting called, instead of ApplyModifiedProperties (assigns values to the instance).
						This allows onClick to now be able to change its own value.
		01-09-2017:	Now supports Collapsible.
		04-01-2017:	Moved CallMethod to EditorReflectionUtility, so that it can be shared with FunctionSelector.
		06-30-2017:	Added multi-object editing support.  Now calls the functions on all selections and displays mixed values.
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChronoVault {

	[CustomPropertyDrawer(typeof(BoolAsButtonAttribute))]
	public class BoolAsButtonDrawer : CustomizedPropertyDrawer {

		public BoolAsButtonAttribute baseAttribute {
			get {
				return attribute as BoolAsButtonAttribute;
			}
		}

		private bool boolValue;



		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUIHelper.GetPropertyHeight(isWrongType(property) ? 2 : 1);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			bool enabledPrior = GUI.enabled;
			EditorGUIHelper.UpdateRectData(position);

			//	Display additional label for FieldWithLabel
			if (baseAttribute.buttonLocation == BoolAsButtonAttribute.ButtonLocation.FieldWithLabel)
				EditorGUI.LabelField(EditorGUIHelper.GetLabelRect(0), label);

			//	Verify that property type is bool
			if (isWrongType(property)) {
				Rect helpRect = EditorGUIHelper.GetFieldRect(0);
				helpRect.height = EditorGUIHelper.GetPropertyHeight(2);

				EditorGUI.PrefixLabel(EditorGUIHelper.GetLineRect(0), label);
				EditorGUI.HelpBox(helpRect, "BoolAsButton Attribute can only be used on Booleans. (" + property.name + ")", MessageType.Error);
				return;
			}

			//	Layout settings, in regards to multi-object selections
			Rect displayPos = GetDisplayRect();
			bool? selection = GetSelection(property);
			GUIStyle buttonStyle = GetButtonStyle(selection);
			string labelText = GetLabelText(selection, label);

			//	Display toggle and label overlaying the toggle
			EditorGUI.BeginChangeCheck();
			boolValue = EditorGUI.Toggle(displayPos, property.boolValue, buttonStyle);
			if (EditorGUI.EndChangeCheck()) {
				if (!String.IsNullOrEmpty(baseAttribute.onPress)) {
					switch (baseAttribute.boolStyle) {
						case BoolAsButtonAttribute.BoolStyle.Button:
							foreach (SerializedProperty prop in EditorGUIHelper.Multiple(property)) {
								SerializedPropertyUtility.InvokeMethod(prop, baseAttribute.onPress);

								//	Buttons get turned off immediately
								prop.boolValue = false;
							}
							break;

						case BoolAsButtonAttribute.BoolStyle.Toggleable:
							foreach (SerializedProperty prop in EditorGUIHelper.Multiple(property)) {
								//	Update value for use in the method
								prop.boolValue = boolValue;
								prop.serializedObject.ApplyModifiedProperties();

								SerializedPropertyUtility.InvokeMethod(prop, baseAttribute.onPress);
							}
							break;
					}
				}
				else
					property.boolValue = boolValue;
			}

			//	Draw the button label on top of the button
			EditorGUI.LabelField(displayPos, new GUIContent(labelText, baseAttribute.tooltip), EditorGUIHelper.LabelCenteredStyle);

			GUI.enabled = enabledPrior;
		}



		private bool isWrongType(SerializedProperty property) {

			return property.propertyType != SerializedPropertyType.Boolean;
		}

		private Rect GetDisplayRect() {

			return baseAttribute.buttonLocation == BoolAsButtonAttribute.ButtonLocation.Label
					? EditorGUIHelper.GetLabelRect(0)
				: baseAttribute.buttonLocation == BoolAsButtonAttribute.ButtonLocation.Field || baseAttribute.buttonLocation == BoolAsButtonAttribute.ButtonLocation.FieldWithLabel
					? EditorGUIHelper.GetFieldRect(0)
					: EditorGUIHelper.GetLineRect(0);
		}

		/// <summary>
		///		Returns the boolValue property.  Options are true, false, or mixed (null).
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private bool? GetSelection(SerializedProperty property) {

			List<SerializedProperty> props = property.Multiple().ToList();
			bool? selection = props[0].boolValue;

			for (int i = 1; i < props.Count; i++) {
				if (props[i].boolValue == selection.Value)
					continue;
				else {
					selection = null;
					break;
				}
			}

			return selection;
		}

		/// <summary>
		///		Returns the button's GUIStyle for the three possible bool states: true, false, and mixed (null).
		/// </summary>
		/// <param name="selection"></param>
		/// <returns></returns>
		private GUIStyle GetButtonStyle(bool? selection) {

			//	Default is false
			GUIStyle toggleStyle = new GUIStyle(GUI.skin.button);

			if (selection.HasValue) {
				//	True
				if (selection.Value) {
					toggleStyle.normal = toggleStyle.active;
					toggleStyle.onNormal = toggleStyle.onActive;
				}
			}
			else {
				toggleStyle.normal = toggleStyle.focused;
				toggleStyle.onNormal = toggleStyle.onFocused;
			}

			return toggleStyle;
		}

		private string GetLabelText(bool? selection, GUIContent label) {

			//	Get standard non-mixed value
			if (selection.HasValue)
				return GetLabelTextNonMixed(selection.Value, label);

			//	Mixed value is a combination
			bool usesTrue = !String.IsNullOrEmpty(baseAttribute.labelTrue);
			bool usesFalse = !String.IsNullOrEmpty(baseAttribute.labelFalse);

			if (usesTrue || usesFalse)
				return GetLabelTextNonMixed(true, label) + " & " + GetLabelTextNonMixed(false, label) + " (Mixed)";
			return GetLabelTextNonMixed(true, label) + " (Mixed)";
		}

		/// <summary>
		///		Gets the label text, for retrieving true or false values only (doesn't consider mixed).
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		private string GetLabelTextNonMixed(bool selection, GUIContent label) {

			return selection
				? (!String.IsNullOrEmpty(baseAttribute.labelTrue)
					? baseAttribute.labelTrue
					: label.text)
				: (!String.IsNullOrEmpty(baseAttribute.labelFalse)
					? baseAttribute.labelFalse
					: label.text);
		}
	}
}