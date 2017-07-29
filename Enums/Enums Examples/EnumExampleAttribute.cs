/**
	EnumExampleAttribute:
		PropertyAttributes used for specifying PropertyDrawers for Enum fields.
	Author(s):
		Ryan Scott Clark
	Date Created:
		07-28-2017
	Date Last Modified:
		07-28-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChronoVault.Examples {

	/// <summary>
	///		v2: Using an Attribute to work for any enum
	/// </summary>
	public class EnumFlagsBrokenAttribute : PropertyAttribute {

		public EnumFlagsBrokenAttribute() {}
	}
	

	/// <summary>
	///		v3: Oops!  That doesn't work with multi-object editing!
	/// </summary>
	public class EnumFlagsBasicAttribute : PropertyAttribute {

		public EnumFlagsBasicAttribute() {}
	}
}