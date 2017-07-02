/**
	SmartEnum:
		A 'smart enum' pattern, where the value of the enumeration is a generic comparable struct (which should be immutable).
		Enum values are declared as 'public static readonly' fields in the inheriting class, and can be enumerated over using static and instance methods.
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-20-2017
	Date Last Modified:
		07-01-2017
	Attach Script to:
		
	Notes:
		Uses reflection for populating the collection.
	Change log:
		06-26-2017:	Code cleanup.
						Added GenerateCollection and moved collection genereration code out of other methods.
						Removed the private Enumerate with a Type parameter, since that was only needed for collection generation.
						Added GetListInternal for handling direct list access.
					Added Query methods: Count, Contains, Find, TryGetDeclared.
						Renamed staticReference to declaredReference.
					Added IComparable<T>, IEquatable<T>, and SmartEnum<T> and T comparison operators.
		06-29-2017:	Added isDeclared.
		06-30-2017:	Added GetNames.
		07-01-2017:	Added index methods: GetInBounds, GetIndex, TryGetByIndex, GetByIndex.
	To do:
		* Option where runtime created instances with unique values can be added to the Enumerate collection (or, more likely, a separate mixed collection).
		* EnumUtility methods:
			- static bool isValid — Determines if runtime declared values are valid
	Bugs:
		
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ChronoVault {

	/// <summary>
	///		A 'smart enum' pattern, where the value of the enumeration is a generic comparable struct (which should be immutable).
	///		Enum values are declared as 'public static readonly' fields in the inheriting class, and can be enumerated over using static and instance methods.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SmartEnum<T> : IComparable, IComparable<SmartEnum<T>>, IComparable<T>, IEquatable<SmartEnum<T>>, IEquatable<T>
		where T : struct, IComparable, IComparable<T>, IEquatable<T> {
		
		/// <summary>
		///		The value to use for the enumeration entry.
		/// </summary>
		public T value {
			get {
				return _value;
			}
		}
		[SerializeField]
		private T _value;

		/// <summary>
		///		The display name for the value.
		/// </summary>
		public string name {
			get {
				return _name;
			}
		}
		[HideInInspector, SerializeField]
		private string _name;

		/// <summary>
		///		Does this entry match a statically declared field?
		/// </summary>
		public bool isDeclared {
			get {
				return !object.ReferenceEquals(declaredReference, null);
			}
		}

		/// <summary>
		///		Stores a reference to a matching statically declared field.  This is used to get property values on comparable types.
		/// </summary>
		protected SmartEnum<T> declaredReference {
			get {
				if (!declaredReferenceChecked && object.ReferenceEquals(_declaredReference, null)) {
					//	Editor use should never cache
					if (Application.isPlaying)
						declaredReferenceChecked = true;

					foreach (SmartEnum<T> val in GetListInternal(GetType())) {
						if (val.Equals(this)) {
							_declaredReference = val;
							return _declaredReference;
						}
					}

					//	For Editor sake
					_declaredReference = null;
				}
				return _declaredReference;
			}
		}
		private SmartEnum<T> _declaredReference;
		private bool declaredReferenceChecked;



		/// <summary>
		///		Stores Enumerate values.
		///		
		///		<para> Key: Type of the SmartEnum&lt;T&gt;. </para>
		///		<para> Value: List of all statically declared fields for the SmartEnum, sorted by increasing value order. </para>
		/// </summary>
		private static Dictionary<Type, List<SmartEnum<T>>> enumerateDictionary = new Dictionary<Type, List<SmartEnum<T>>>();



		public override string ToString() {

			return name;
		}

		#region Enumerate methods (public)
		/// <summary>
		///		An enumerable collection of all statically declared fields for the SmartEnum, sorted by increasing value order.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public static IEnumerable<U> Enumerate<U>() where U : SmartEnum<T> {

			foreach (U val in GetListInternal(typeof(U)))
				yield return val as U;
		}

		/// <summary>
		///		A list of all statically declared fields for the SmartEnum, sorted by increasing value order.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public static List<U> GetList<U>() where U : SmartEnum<T> {
			
			return GetListInternal(typeof(U)).Cast<U>().ToList();
		}

		/// <summary>
		///		Starts an increasing enumeration from the value greater than this.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public IEnumerable<U> EnumerateFrom<U>() where U : SmartEnum<T> {

			bool reached = false;

			//	Case where we match a static field
			if (isDeclared) {
				foreach (U val in Enumerate<U>()) {
					if (!reached) {
						if (val.Equals(this))
							reached = true;
					}
					else
						yield return val;
				}
			}
			//	Case where we have a custom value
			else {
				foreach (U val in Enumerate<U>()) {
					if (!reached) {
						if (val > this) {
							reached = true;
							yield return val;
						}
					}
					else
						yield return val;
				}
			}
			yield break;
		}

		/// <summary>
		///		Starts an increasing enumeration from the value greater than this.
		///		NOTE: Syntactic sugar for EnumerateFrom().
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public IEnumerable<U> IncrementFrom<U>() where U : SmartEnum<T> {

			foreach (U val in EnumerateFrom<U>())
				yield return val;
		}

		/// <summary>
		///		Starts a decreasing enumeration from the value less than this.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public IEnumerable<U> DecrementFrom<U>() where U : SmartEnum<T> {
			
			bool reached = false;

			//	Case where we match a static field
			if (isDeclared) {
				foreach (U val in Enumerate<U>().Reverse()) {
					if (!reached) {
						if (val.Equals(this))
							reached = true;
					}
					else
						yield return val;
				}
			}
			//	Case where we have a custom value
			else {
				foreach (U val in Enumerate<U>().Reverse()) {
					if (!reached) {
						if (val < this) {
							reached = true;
							yield return val;
						}
					}
					else
						yield return val;
				}
			}
			yield break;
		}

		/// <summary>
		///		Returns the next largest value from this.
		///		Will be null if there are no larger elements.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public U Next<U>() where U : SmartEnum<T> {

			return EnumerateFrom<U>().FirstOrDefault();
		}

		/// <summary>
		///		Returns the next smallest value from this.
		///		Will be null if there are no smaller elements.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public U Prior<U>() where U : SmartEnum<T> {

			return DecrementFrom<U>().FirstOrDefault();
		}

		/// <summary>
		///		Retrieves an array of the names (ToString representation) of the statically declared fields in a specified enumeration.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public static string[] GetNames<U>() where U : SmartEnum<T> {

			return GetListInternal(typeof(U)).Select<SmartEnum<T>, string>(smEnum => smEnum.ToString()).ToArray();
		}
		#endregion

		#region Query methods (public)
		/// <summary>
		///		Gets the number of statically declared fields for the SmartEnum.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public static int Count<U>() where U : SmartEnum<T> {

			return GetListInternal(typeof(U)).Count();
		}

		/// <summary>
		///		Determines whether an element is one of the statically declared fields for the SmartEnum.
		///		NOTE: This checks equality of the 'value' property, not reference equality of the SmartEnum instance.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Contains<U>(U value) where U : SmartEnum<T> {

			return GetListInternal(typeof(U)).Exists(val => val.Equals(value));
		}

		/// <summary>
		///		Determines whether a value matches one of the statically declared fields' values for the SmartEnum.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Contains<U>(T value) where U : SmartEnum<T> {

			return GetListInternal(typeof(U)).Exists(val => val.value.Equals(value));
		}

		/// <summary>
		///		Searches for a statically declared field that matches the predicate.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="match"></param>
		/// <returns></returns>
		public static U Find<U>(Predicate<U> match) where U : SmartEnum<T> {

			foreach (U val in GetListInternal(typeof(U)))
				if (match(val))
					return val;
			return default(U);
		}

		/// <summary>
		///		Searches for a statically declared field that matches the predicate, by value.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="match"></param>
		/// <returns></returns>
		public static U Find<U>(Predicate<T> match) where U : SmartEnum<T> {

			return Find<U>((U val) => match(val.value));
		}

		/// <summary>
		///		Searches for a statically declared field that matches the value.
		///		NOTE: Syntactic sugar for TryGetDeclared.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static U Find<U>(U value) where U : SmartEnum<T> {

			U match;
			TryGetDeclared(value, out match);
			return match;
		}

		/// <summary>
		///		Searches for a statically declared field that matches the value.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static U Find<U>(T value) where U : SmartEnum<T> {

			return Find<U>(val => val.value.Equals(value));
		}

		/// <summary>
		///		Gets the statically declared reference that matches an instance, if one exists.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="instance"></param>
		/// <param name="declared"></param>
		/// <returns></returns>
		public static bool TryGetDeclared<U>(U instance, out U declared) where U : SmartEnum<T> {

			if (object.ReferenceEquals(instance, null) || !instance.isDeclared) {
				declared = null;
				return false;
			}

			declared = instance.declaredReference as U;
			return true;
		}
		#endregion

		#region Index methods (public)
		/// <summary>
		///		Determines if a provided index is in bounds of the statically declared types' count.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetInBounds<U>(int index) where U : SmartEnum<T> {

			return index >= 0 && index < Count<U>();
		}

		/// <summary>
		///		Gets the index that matches the instance.  If no statically declared value matches, returns -1.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static int GetIndex<U>(U instance) where U : SmartEnum<T> {

			U declared;
			if (!TryGetDeclared<U>(instance, out declared))
				return -1;
			return GetListInternal(typeof(U)).IndexOf((SmartEnum<T>) declared);
		}

		/// <summary>
		///		Attempts to return the statically declared value matching an index.
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="index"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static bool TryGetByIndex<U>(int index, out U instance) where U : SmartEnum<T> {

			if (!GetInBounds<U>(index)) {
				instance = default(U);
				return false;
			}

			instance = GetByIndex<U>(index);
			return true;
		}

		/// <summary>
		///		Returns the statically declared value matching an index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Provided index is out of range.</exception>
		/// <typeparam name="U"></typeparam>
		/// <param name="index"></param>
		/// <returns></returns>
		public static U GetByIndex<U>(int index) where U : SmartEnum<T> {

			if (!GetInBounds<U>(index))
				throw new ArgumentOutOfRangeException("index", "Provided index \"" + index + "\" is out of range.");
			return GetListInternal(typeof(U))[index] as U;
		}
		#endregion

		#region Collection methods (private)
		/// <summary>
		///		Generates a collection for the specified Type in enumerateDictionary.
		///		For internal use only—there are no Type or Dictionary exists checks.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static List<SmartEnum<T>> GenerateCollection(Type type) {

			List<SmartEnum<T>> values = new List<SmartEnum<T>>();
			foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) {
				object value = info.GetValue(null);
				if (value != null && value.GetType() == type)
					values.Add((SmartEnum<T>) value);
			}

			values.Sort();
			enumerateDictionary.Add(type, values);
			return values;
		}

		/// <summary>
		///		Directly returns the list internally used in enumerateDictionary for the specified Type.
		///		For internal use only—the actual list is returned, not a copy.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static List<SmartEnum<T>> GetListInternal(Type type) {

			List<SmartEnum<T>> values;
			if (!enumerateDictionary.TryGetValue(type, out values))
				values = GenerateCollection(type);
			return values;
		}
		#endregion

		#region Comparison operators, SmartEnum
		public static bool operator <(SmartEnum<T> left, SmartEnum<T> right) {

			return !object.ReferenceEquals(left, right) && (object.ReferenceEquals(left, null) || left.CompareTo(right) < 0);
		}

		public static bool operator >(SmartEnum<T> left, SmartEnum<T> right) {

			return !object.ReferenceEquals(left, right) && (!object.ReferenceEquals(left, null) && left.CompareTo(right) > 0);
		}

		public static bool operator <=(SmartEnum<T> left, SmartEnum<T> right) {

			return object.ReferenceEquals(left, right) || object.ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
		}

		public static bool operator >=(SmartEnum<T> left, SmartEnum<T> right) {

			return object.ReferenceEquals(left, right) || (!object.ReferenceEquals(left, null) && left.CompareTo(right) >= 0);
		}

		public static bool operator ==(SmartEnum<T> left, SmartEnum<T> right) {

			return object.ReferenceEquals(left, right) || (!object.ReferenceEquals(left, null) && left.Equals(right));
		}

		public static bool operator !=(SmartEnum<T> left, SmartEnum<T> right) {

			return !object.ReferenceEquals(left, right) && (object.ReferenceEquals(left, null) || !left.Equals(right));
		}
		#endregion

		#region Comparison operators, T
		public static bool operator <(SmartEnum<T> left, T right) {

			return object.ReferenceEquals(left, null) || left.CompareTo(right) < 0;
		}

		public static bool operator <(T left, SmartEnum<T> right) {

			return right > left;
		}

		public static bool operator >(SmartEnum<T> left, T right) {

			return !object.ReferenceEquals(left, null) && left.CompareTo(right) > 0;
		}

		public static bool operator >(T left, SmartEnum<T> right) {

			return right < left;
		}

		public static bool operator <=(SmartEnum<T> left, T right) {

			return object.ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
		}

		public static bool operator <=(T left, SmartEnum<T> right) {

			return right >= left;
		}

		public static bool operator >=(SmartEnum<T> left, T right) {

			return !object.ReferenceEquals(left, null) && left.CompareTo(right) >= 0;
		}

		public static bool operator >=(T left, SmartEnum<T> right) {

			return right <= left;
		}

		public static bool operator ==(SmartEnum<T> left, T right) {

			return !object.ReferenceEquals(left, null) && left.value.Equals(right);
		}

		public static bool operator ==(T left, SmartEnum<T> right) {

			return right == left;
		}

		public static bool operator !=(SmartEnum<T> left, T right) {

			return object.ReferenceEquals(left, null) || !left.value.Equals(right);
		}

		public static bool operator !=(T left, SmartEnum<T> right) {

			return right != left;
		}
		#endregion

		#region Comparison methods
		public int CompareTo(object obj) {

			if (obj == null)
				return 1;

			if (obj is SmartEnum<T>)
				return CompareTo((SmartEnum<T>) obj);
			if (obj is T)
				return CompareTo((T) obj);
			throw new ArgumentException("Passed obj is not a SmartEnum<T>.");
		}

		public virtual int CompareTo(SmartEnum<T> other) {

			return object.ReferenceEquals(other, null) ? 1 : value.CompareTo(other.value);
		}

		public virtual int CompareTo(T other) {

			return value.CompareTo(other);
		}

		public override bool Equals(object obj) {

			return !object.ReferenceEquals(obj, null) && ((obj is SmartEnum<T> && Equals((SmartEnum<T>) obj)) || (obj is T && Equals((T) obj)));
		}

		public bool Equals(SmartEnum<T> other) {

			return !object.ReferenceEquals(other, null) && value.Equals(other.value);
		}

		public bool Equals(T other) {

			return value.Equals(other);
		}

		public override int GetHashCode() {

			return value.GetHashCode();
		}
		#endregion



		/// <summary>
		///		Creates a SmartEnum with the provided value and name.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="name"></param>
		protected SmartEnum(T value, string name) {

			_value = value;
			_name = name;
		}

		protected SmartEnum() {}
	}
}