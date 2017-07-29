/**
	ReflectionUtility:
		Utility methods relating to Reflection.
	Author(s):
		Ryan Scott Clark
	Date Created:
		09-13-2016
	Date Last Modified:
		07-01-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		07-01-2017:	Stripped version of full ReflectionUtility for posting with the Enum devlog.
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ChronoVault {

	public static class ReflectionUtility {

		/// <summary>
		///		Returns all implemented interfaces and parent Types for a given Type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<Type> ParentTypes(this Type type) {

			foreach (Type i in type.GetInterfaces()) {
				yield return i;
				foreach (Type t in i.ParentTypes())
					yield return t;
			}

			if (type.BaseType != null) {
				yield return type.BaseType;
				foreach (Type b in type.BaseType.ParentTypes())
					yield return b;
			}
		}

		/// <summary>
		///		A version of default that works for Type objects.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object GetDefault(Type type) {

			return type.IsValueType
				? Activator.CreateInstance(type)
				: null;
		}

		/// <summary>
		///		Attempts to get the value of a specified property.
		///		Works for public, non-public, instanced, and static properties.  Not intended for indexed properties.
		/// </summary>
		/// <param name="target">A non-null target.  For null targets (static properties), use the TryGetProperty which has Type as a parameter instead.</param>
		/// <param name="propertyName">The property name.</param>
		/// <param name="type">The type of the target.</param>
		/// <param name="value">The value of the property, if the try/get succeeds.</param>
		/// <returns>bool; whether the property exists.</returns>
		public static bool TryGetProperty(object target, string propertyName, Type type, out object value) {

			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (property == null) {
				value = null;
				return false;
			}

			MethodInfo getter = property.GetGetMethod(true);
			if (getter == null) {
				value = null;
				return false;
			}

			value = getter.Invoke(getter.IsStatic ? null : target, null);
			return true;
		}

		/// <summary>
		///		Gets the value of a specified field.
		///		Works for public, non-public, instanced, and static fields.
		/// </summary>
		/// <param name="target">A non-null target.  For null targets (static fields), use the GetField which has Type as a parameter instead.</param>
		/// <param name="fieldName">The field name.</param>
		/// <param name="type">The type of the target.</param>
		/// <returns>The value of the field.</returns>
		public static object GetField(object target, string fieldName, Type type) {

			FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			return field.GetValue(field.IsStatic ? null : target);
		}

		/// <summary>
		///		Gets the value of a member (through checking as a property first, then field second).
		///		Works for public, non-public, instanced, and static properties and fields.
		/// </summary>
		/// <param name="target">The target object.  If the field is a known static member, can be left null.</param>
		/// <param name="memberName">The member name.</param>
		/// <param name="type">The type of the target.</param>
		/// <returns>The value of the member.</returns>
		public static object GetPropertyOrField(object target, string memberName, Type type) {

			object value;
			if (TryGetProperty(target, memberName, type, out value))
				return value;
			return GetField(target, memberName, type);
		}

		/// <summary>
		///		Gets the value of a member (through checking as a property first, then field second).
		///		Works for public, non-public, instanced, and static properties and fields.
		/// </summary>
		/// <param name="target">A non-null target.  For null targets (static members), use the GetPropertyOrField which has Type as a parameter instead.</param>
		/// <param name="memberName">The member name.</param>
		/// <returns>The value of the member.</returns>
		public static object GetPropertyOrField(object target, string memberName) {

			return GetPropertyOrField(target, memberName, target.GetType());
		}

		/// <summary>
		///		Invokes a method, regardless of parameters (by passing default values).
		///		Works for public, private, internal, protected, static, instanced, and inherited methods.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="target"></param>
		/// <param name="output">The output from the Invoke.  Null if the method returns false.</param>
		/// <returns></returns>
		public static bool TryInvokeMethod(Type type, string methodName, object target, out object output) {

			MethodInfo func = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (func == null) {
				output = null;
				return false;
			}
			
			//	Assign default parameters
			ParameterInfo[] paramSignature = func.GetParameters();
			object[] paramValues = new object[paramSignature.Length];
			for (int i = 0; i < paramSignature.Length; i++)
				paramValues[i] = ReflectionUtility.GetDefault(paramSignature[i].ParameterType);

			output = func.Invoke(target, paramValues);
			return true;
		}
	}
}