/**
	CustomizedPropertyDrawer:
		Has additional common functionality for PropertyDrawers, especially dealing with PropertyAttributes.
		Handles support for ReadOnlyCustomAttribute.
	Author(s):
		Ryan Scott Clark
	Date Created:
		08-06-2016
	Date Last Modified:
		07-21-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		10-10-2016:	Added GetPropertyAttribute with no parameter, for handling PropertyAttribute which can't be declared with new().
					GetPropertyAttribute's parameter version now has no default value.
					Added TooltipAttribute support: hasTooltip, tooltip, and AppendTooltip.
		01-06-2017:	Now supports CollapsibleHeaderAttribute, through the use of GetPropertyHeightCollapsible and OnGUICollapsible.
		06-06-2017:	Removed the isReadOnly property and replaced it with the isReadOnly method.
						This was because the new ReadOnlyCustomAttribute boolProperty option requires a passed SerializedProperty instance.
		07-10-2017:	Added HasLabel and GetFieldRect, for handling field drawing relative to the presence of a label.
		07-21-2017:	Stripped down content for the Enums devlog.
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace ChronoVault {

	public abstract class CustomizedPropertyDrawer : PropertyDrawer {

		/// <summary>
		///		Whether TooltipAttribute is present.
		/// </summary>
		protected bool hasTooltip {
			get {
				return HasPropertyAttribute<TooltipAttribute>();
			}
		}

		/// <summary>
		///		The tooltip, if TooltipAttribute is present (empty string otherwise).
		/// </summary>
		protected string tooltip {
			get {
				return HasPropertyAttribute<TooltipAttribute>()
					? GetPropertyAttribute<TooltipAttribute>().tooltip
					: String.Empty;
			}
		}

		private Dictionary<Type, PropertyAttribute> propertyAttributes;

		private Dictionary<Type, PropertyAttribute> propertyAttributeDefaults = new Dictionary<Type, PropertyAttribute>();



		/// <summary>
		///		Determines if the label's text is empty.
		/// </summary>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool HasLabel(GUIContent label) {

			return !String.IsNullOrEmpty(label.text);
		}
		
		/// <summary>
		///		Gets a content Rect appropriate, based on the presence of a label.
		///		If no label, returns a full LineRect.  Otherwise, returns a FieldRect.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static Rect GetContentRect(int line, GUIContent label) {

			return HasLabel(label)
				? EditorGUIHelper.GetFieldRect(line)
				: EditorGUIHelper.GetLineRect(line, false);
		}

		/// <summary>
		///		Appends a tooltip (if the TooltipAttribute is used for the field).
		/// </summary>
		/// <param name="label">The label to get the text field from.</param>
		/// <param name="isOverwriting">If true, the TooltipAttribute will overwrite any present tooltip in the label.  If false, if label has a tooltip it will be used instead.</param>
		protected GUIContent AppendTooltip(GUIContent label, bool isOverwriting = false) {

			if (!isOverwriting && !String.IsNullOrEmpty(label.tooltip))
				return label;
			return new GUIContent(label.text, tooltip);
		}

		/// <summary>
		///		Determines if a specified PropertyAttribute is present on the field.
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <returns></returns>
		protected bool HasPropertyAttribute<TAttr>() where TAttr : PropertyAttribute {

			CreateAttributesDictionary();
			return propertyAttributes.ContainsKey(typeof(TAttr));
		}

		/// <summary>
		///		Returns a matching PropertyAttribute, if present on the field.
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <returns></returns>
		protected TAttr GetPropertyAttribute<TAttr>() where TAttr : PropertyAttribute {

			CreateAttributesDictionary();

			//	Return a match from the Dictionary if already present
			PropertyAttribute match;
			if (propertyAttributes.TryGetValue(typeof(TAttr), out match))
				return match as TAttr;
			return null;
		}

		/// <summary>
		///		Returns a matching PropertyAttribute, if present on the field.  If not present, will return null unless isIncludingDefault is true.
		/// </summary>
		/// <typeparam name="TAttr"></typeparam>
		/// <param name="isIncludingDefault">If true, will create and return an instance of TAttr with default field values.</param>
		/// <returns></returns>
		protected TAttr GetPropertyAttribute<TAttr>(bool isIncludingDefault) where TAttr : PropertyAttribute, new() {

			TAttr prop = GetPropertyAttribute<TAttr>();
			if (prop != null || !isIncludingDefault)
				return prop;

			//	Check if it's already present in the defaults Dictionary
			PropertyAttribute match;
			if (propertyAttributeDefaults.TryGetValue(typeof(TAttr), out match))
				return match as TAttr;

			//	Generate a default version of the attribute and add it to a secondary Dictionary (so we maintain whether the PropertyAttribute is present or not)
			TAttr defaultAttr = new TAttr();
			propertyAttributeDefaults.Add(typeof(TAttr), defaultAttr);
			return defaultAttr;
		}

		/// <summary>
		///		Internally populates a Dictionary used for storing PropertyAttributes.
		/// </summary>
		private void CreateAttributesDictionary() {

			if (propertyAttributes != null)
				return;

			propertyAttributes = new Dictionary<Type, PropertyAttribute>();

			Type attribType;
			foreach (object attrib in fieldInfo.GetCustomAttributes(false)) {
				attribType = attrib.GetType();
				if (attribType.IsSubclassOf(typeof(PropertyAttribute)) && !propertyAttributes.ContainsKey(attribType))
					propertyAttributes.Add(attribType, attrib as PropertyAttribute);
			}
		}
	}
}