/**
	SerializedPropertyUtility:
		Reflection methods specifically dealing with managing SerializedProperty values as direct objects, intended for Editor-use only.
	Author(s):
		Ryan Scott Clark
	Date Created:
		04-01-2017
	Date Last Modified:
		07-13-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		04-01-2017:	Moved in GetParent from EditorGUIHelper.
					Moved in CallMethod from BoolAsButtonDrawer.
		07-01-2017:	Added GetTarget, SetTargetValue.
					Renamed to SerializedPropertyUtility from EditorReflectionUtility.
					Renamed CallMethod to InvokeMethod, added an output return, and re-ordered its parameters in order to match ReflectionUtility's standard.
					Moved CallMethod(type, string, object) and MethodExists to ReflectionUtility.
		07-05-2017:	Renamed GetTarget to GetValue.  Renamed SetTargetValue to SetValue.
					Made all methods work properly for multi-object selection and as elements in arrays.
						Added GetIndex, GetIndices, isArrayElement.
					Added a placeholder for ForceUpdate (it's not yet working).
					Made all methods extension methods.
		07-10-2017:	Moved SerializedPropertyList in from EditorGUIHelper.
					Added Cast<T>, GetCaster, GetArrayCollection.
		07-13-2017:	Fixed GetArrayCollection to work with multi-object selection with miscounted array sizes.
					Added GetPropertyTarget, GetPropertyType.
					Fixed InvokeMethod to work for non-MonoBehaviour instances.
	To do:
		
	Bugs:
		
**/

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ChronoVault {

	public static class SerializedPropertyUtility {

		/// <summary>
		///		Gets the proper parent object for a property.  (Used in the case with nested properties, where we need to use the property as a target for Reflection.)
		///			<para> Modified version of answer from http://answers.unity3d.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html. </para>
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetParent(this SerializedProperty property) {

			string path = property.propertyPath.Replace(".Array.data[", "[");
			string[] elements = path.Split('.');
			object obj = property.serializedObject.targetObject;

			foreach (string element in elements.Take(elements.Length - 1)) {
				if (element.Contains("[")) {
					string elementName = element.Substring(0, element.IndexOf("["));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

					IEnumerator enumerator = (ReflectionUtility.GetPropertyOrField(obj, elementName) as IEnumerable).GetEnumerator();
					while (index-- >= 0)
						enumerator.MoveNext();
					obj = enumerator.Current;
				}
				else
					obj = ReflectionUtility.GetPropertyOrField(obj, element);
			}

			return obj;
		}

		/// <summary>
		///		Calls a method with the provided name, regardless of parameters.
		///		Works for public, private, internal, protected, static, instanced, and inherited methods.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="methodName"></param>
		/// <returns>Method output.</returns>
		public static object InvokeMethod(this SerializedProperty property, string methodName) {

			object output;

			//	Attempt directly (for non-MonoBehaviour instances)
			try {
				if (ReflectionUtility.TryInvokeMethod(property.GetPropertyType(), methodName, property.GetPropertyTarget(), out output))
					return output;
			}
			catch {}

			object target = GetParent(property);

			//	Attempt to call on exact Type first
			if (!ReflectionUtility.TryInvokeMethod(target.GetType(), methodName, target, out output)) {
				bool success = false;

				//	Iterate over implemented interfaces and derived classes
				foreach (Type type in target.GetType().ParentTypes()) {
					Debug.Log(type.Name);
					if (ReflectionUtility.TryInvokeMethod(type, methodName, target, out output)) {
						success = true;
						break;
					}
				}

				if (!success) {
					Debug.LogWarning("The provided function \"" + methodName + "\" does not exist." + Environment.NewLine);
					return output;
				}
			}

			//	Pull update from any changes performed in the method call
			property.serializedObject.Update();
			return output;
		}

		/// <summary>
		///		Returns the Type for a SerializedProperty.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static Type GetPropertyType(this SerializedProperty property) {

			return property.serializedObject.targetObject.GetType().GetField(property.propertyPath).FieldType;
		}

		/// <summary>
		///		Gets the target object for the property.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetPropertyTarget(this SerializedProperty property) {

			object parent = GetParent(property);
			return ReflectionUtility.GetField(parent, property.propertyPath, parent.GetType());
		}
	}
}
#endif