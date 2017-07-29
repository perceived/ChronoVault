/**
	CastTo:
		Handles casting of generic Types in a way other than boxing.  Is faster than boxing, and has other benefits: see the link.
	Author(s):
		Ryan Scott Clark
	Date Created:
		06-12-2017
	Date Last Modified:
		07-17-2017
	Attach Script to:
		
	Notes:
		From Stack Overflow answer: https://stackoverflow.com/questions/1189144/c-sharp-non-boxing-conversion-of-generic-enum-to-int
	Change log:
		07-17-2017:	No longer uses Get if in WebGL (since Reflection.Emit is not allowed).
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace ChronoVault {

	/// <summary>
	///		Class to cast to type <see cref="T"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class CastTo<T> {

		private static class Cache<S> {

			public static readonly Func<S, T> caster = Get();

			private static Func<S, T> Get() {

				ParameterExpression p = Expression.Parameter(typeof(S), String.Empty);
				UnaryExpression c = Expression.ConvertChecked(p, typeof(T));
				return Expression.Lambda<Func<S, T>>(c, p).Compile();
			}
		}



		/// <summary>
		///		Casts <see cref="S"/> to <see cref="T"/>.
		///		This does not cause boxing for value types.
		/// </summary>
		/// <typeparam name="S">Source type to cast from.</typeparam>
		/// <param name="s"></param>
		/// <returns></returns>
		public static T From<S>(S s) {
			
			#if UNITY_WEBGL
			return (T) (object) s;
			#else
			return Cache<S>.caster(s);
			#endif
		}
	}
}