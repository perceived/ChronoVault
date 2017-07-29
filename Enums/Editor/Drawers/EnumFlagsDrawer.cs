/**
	EnumFlagsDrawer:
		Handles the PropertyDrawer for the [EnumFlags] attribute.
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-29-2015
	Date Last Modified:
		07-10-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		06-24-2016:	Added multi-object editing support.
		08-24-2016:	Now uses EditorGUIHelper.EnumMaskPopup and updated settings of EnumFlagsAttribute.
					Now displays an error if not used on an enum.
					Supports ReadOnlyCustom attribute.
		09-04-2016:	GUI.enabled now wraps around the label as well.
		01-09-2017:	Now supports Collapsible.
		07-10-2017:	Supports GUIContent.none label use representing the entire line, instead of just the field.
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChronoVault {

	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	public class EnumFlagsDrawer : CustomizedPropertyDrawer {

		public EnumFlagsAttribute options {
			get {
				return GetPropertyAttribute<EnumFlagsAttribute>(true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			//	Two lines tall for error message
			return EditorGUIHelper.GetPropertyHeight(fieldInfo.FieldType.IsEnum ? 1 : 2);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			EditorGUIHelper.UpdateRectData(position);

			EditorGUI.LabelField(EditorGUIHelper.GetLabelRect(0), label);

			Rect rect = GetContentRect(0, label);
			Type type = GetEnumType();

			if (!type.IsEnum) {
				Rect fieldRect = rect;
				fieldRect.height = EditorGUIHelper.GetPropertyHeight(2);

				EditorGUI.HelpBox(fieldRect, "EnumFlags attribute must be used on an enum.", MessageType.Error);
			}
			else
				EditorGUIHelper.EnumMaskPopup(type, rect, property, options);
		}

		private Type GetEnumType() {

			Type type = fieldInfo.FieldType;
			//	Get underlying type if using array/list
			if (!type.IsEnum)
				type = type.GetGenericArguments()[0];
			return type;
		}
	}
}