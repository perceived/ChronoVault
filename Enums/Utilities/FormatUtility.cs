/**
	FormatUtility:
		Static functions for user-formatted data.
	Author(s):
		Ryan Scott Clark
	Date Created:
		12-06-2014
	Date Last Modified:
		07-01-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		07-01-2017:	Stripped version of full FormatUtility for posting with the Enum devlog.
	To do:
		
	Bugs:
		
**/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChronoVault {

	public class FormatUtility {

		/// <summary>
		///		A regex sequence to make user-friendly string displays.  Used in GetUserFriendlyText.
		/// </summary>
		public static Regex userFriendlyRegex {
			get {
				if (_userFriendlyRegex == null) {
					_userFriendlyRegex = new Regex(
					   @"  (?<=[A-Z_])(?=[A-Z][a-z_])
						|  (?<=[^A-Z_])(?=[A-Z_])
						|  (?<=[A-Za-z_])(?=[^A-Za-z_])
						", RegexOptions.IgnorePatternWhitespace);
				}
				return _userFriendlyRegex;
			}
		}
		private static Regex _userFriendlyRegex;

		/// <summary>
		///		Displays a multiplication symbol, '×' (unicode '\u00d7').
		/// </summary>
		public const string MARK_X = "×";

		/// <summary>
		///		Displays a plus symbol, '+'.
		/// </summary>
		public const string MARK_PLUS = "+";

		/// <summary>
		///		Displays a null mark, '∅' (unicode '\u2205').
		/// </summary>
		public const string MARK_NULL = "∅";

		/// <summary>
		///		Displays a checkmark, '✓' (unicode '\u2713');
		/// </summary>
		public const string MARK_CHECK = "\u2713";

		/// <summary>
		///		Displays a popup-safe version of &.  Displays a half space followed by an ampersand look-alike, ' ＆'.
		///		This is used to circumvent the special use of the standard ampersand, which doesn't display in popup fields and is for key commands in MenuItem attributes.
		/// </summary>
		public const string MARK_AMPERSAND_SAFE = " ＆";

		/// <summary>
		///		Displays a popup-safe version of /.  Displays a half space followed by a fraction separator.
		///		This is used to circumvent the special use of the standard slash, which doesn't display in popup fields.
		/// </summary>
		public const string MARK_SLASH_SAFE = "\u2044 ";



		/// <summary>
		///		Provided with a string, displays it in a user-friendly format (adds spaces where appropriate, working for numbers and acronyms).
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetUserFriendlyText(string text) {

			return String.Join(" ", userFriendlyRegex.Split(text));
		}
	}
}