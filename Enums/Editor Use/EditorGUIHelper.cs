/**
	EditorGUIHelper:
		Aids in the creation of Editor functionality.  This includes clean inspector GUI using EditorGUI methods, AssetDatabase functions, GUIStyles, etc.
		Lives outside of the Editor folder, so that runtime scripts can use these functions without #if UNITY_EDITOR blocks.
	Author(s):
		Ryan Scott Clark
	Date Created:
		05-07-2015
	Date Last Modified:
		07-21-2017
	Attach Script to:
		
	Notes:
		
	Change log:
		07-21-2017:	Stripped version of full EditorGUIHelper for posting with the Enum devlog.
	To do:
		
	Bugs:
		
**/

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ChronoVault {

	public static class EditorGUIHelper {

		#region Enum Mask Popup
		/// <summary>
		///		Handles a proper EnumMaskPopup field (Unity's is broken, since it handles value assignment by enum index instead of actual enum value).
		///		Uses the DescriptionAttribute for enum displays, if available.
		///		Selected value displays a comma-separated list, instead of displaying 'Mixed...'.
		///		Entries with the same value are combined onto one line.
		///		Automatically handles multi-object editing.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="entries">An optional bitmask with selectable entries.</param>
		/// <param name="isSkippingZero">If true, enum entries with a value of zero are not displayed (useful if the entry already maps to 'None').</param>
		/// <param name="isDisplayingEverything">If true, if all possible elements are selected (all elements that entries allows), 'Everything' is displayed instead of comma-separated values.</param>
		/// <param name="isSkippingNonFlags">If true, elements which don't map to a flag value aren't included.</param>
		public static void EnumMaskPopup<TEnum>(Rect position, SerializedProperty property, int entries = ~0, bool isSkippingZero = false, bool isDisplayingEverything = true, bool isSkippingNonFlags = true)
			where TEnum : struct, IConvertible {

			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("TEnum value of '" + typeof(TEnum) + "' is not an enumerated type.", "<TEnum>");

			EnumMaskPopup(EnumUtility.GetValues<TEnum>(), EnumUtility.GetDescriptions<TEnum>(), position, property, entries, isSkippingZero, isDisplayingEverything, isSkippingNonFlags);
		}

		/// <summary>
		///		Handles a proper EnumMaskPopup field, using values instead of enum indices (Unity's is broken, since it handles value assignment by enum index instead of actual enum value).
		///		Uses the DescriptionAttribute for enum displays, if available.
		///		Selected value displays a comma-separated list, instead of displaying 'Mixed...'.
		///		Entries with the same value are combined onto one line.
		///		Automatically handles multi-object editing.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="entries">An bitmask with selectable entries.</param>
		/// <param name="isSkippingZero">If true, enum entries with a value of zero are not displayed (useful if the entry already maps to 'None').</param>
		/// <param name="isDisplayingEverything">If true, if all possible elements are selected (all elements that entries allows), 'Everything' is displayed instead of comma-separated values.</param>
		/// <param name="isSkippingNonFlags">If true, elements which don't map to a flag value aren't included.</param>
		public static void EnumMaskPopup<TEnum>(Rect position, SerializedProperty property, TEnum entries, bool isSkippingZero = false, bool isDisplayingEverything = true, bool isSkippingNonFlags = true)
			where TEnum : struct, IConvertible {

			EnumMaskPopup(position, property, entries.ToInt32(default(IFormatProvider)), isSkippingZero, isDisplayingEverything, isSkippingNonFlags);
		}

		/// <summary>
		///		Handles a proper EnumMaskPopup field (Unity's is broken, since it handles value assignment by enum index instead of actual enum value).
		///		Uses the DescriptionAttribute for enum displays, if available.
		///		Selected value displays a comma-separated list, instead of displaying 'Mixed...'.
		///		Entries with the same value are combined onto one line.
		///		Automatically handles multi-object editing.
		/// </summary>
		/// <param name="enumType"></param>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="entries">An optional bitmask with selectable entries.</param>
		/// <param name="isSkippingZero">If true, enum entries with a value of zero are not displayed (useful if the entry already maps to 'None').</param>
		/// <param name="isDisplayingEverything">If true, if all possible elements are selected (all elements that entries allows), 'Everything' is displayed instead of comma-separated values.</param>
		/// <param name="isSkippingNonFlags">If true, elements which don't map to a flag value aren't included.</param>
		public static void EnumMaskPopup(Type enumType, Rect position, SerializedProperty property, int entries = ~0, bool isSkippingZero = false, bool isDisplayingEverything = true, bool isSkippingNonFlags = true) {

			if (!enumType.IsEnum)
				throw new ArgumentException("enumType value of '" + enumType + "' is not an enumerated type.", "enumType");

			EnumMaskPopup(EnumUtility.GetValues(enumType), EnumUtility.GetDescriptions(enumType), position, property, entries, isSkippingZero, isDisplayingEverything, isSkippingNonFlags);
		}

		/// <summary>
		///		Handles a proper EnumMaskPopup field (Unity's is broken, since it handles value assignment by enum index instead of actual enum value).
		///		Uses the DescriptionAttribute for enum displays, if available.
		///		Selected value displays a comma-separated list, instead of displaying 'Mixed...'.
		///		Entries with the same value are combined onto one line.
		///		Automatically handles multi-object editing.
		/// </summary>
		/// <param name="enumType"></param>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="options">Holds options for entries (mask), isSkippingZero, and isDisplayingEverything.  Uses the default values if null.</param>
		public static void EnumMaskPopup(Type enumType, Rect position, SerializedProperty property, EnumFlagsAttribute options) {

			if (!enumType.IsEnum)
				throw new ArgumentException("enumType value of '" + enumType + "' is not an enumerated type.", "enumType");

			if (options == null)
				options = new EnumFlagsAttribute();

			EnumMaskPopup(EnumUtility.GetValues(enumType), EnumUtility.GetDescriptions(enumType), position, property, options.entries, options.isSkippingZero, options.isDisplayingEverything, options.isSkippingNonFlags);
		}

		/// <summary>
		///		Internal implementation of EnumMaskPopup.
		/// </summary>
		/// <param name="values"></param>
		/// <param name="names"></param>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="entries">An optional bitmask with selectable entries.</param>
		/// <param name="isSkippingZero">If true, enum entries with a value of zero are not displayed (useful if the entry already maps to 'None').</param>
		/// <param name="isDisplayingEverything">If true, if all possible elements are selected (all elements that entries allows), 'Everything' is displayed instead of comma-separated values.</param>
		/// <param name="isSkippingNonFlags">If true, elements which don't map to a flag value aren't included.</param>
		private static void EnumMaskPopup(List<int> values, List<string> names, Rect position, SerializedProperty property, int entries = ~0, bool isSkippingZero = false, bool isDisplayingEverything = true, bool isSkippingNonFlags = true) {

			List<int> valuesCopy;
			List<string> namesCopy;
			int omitted = 0;

			//	Popup only supports 32 entries.  If exceeded, only use the first 32 and flag the field as red with an error tooltip
			if (values.Count <= 32) {
				valuesCopy = new List<int>(values);
				namesCopy = new List<string>(names);
			}
			else {
				valuesCopy = values.GetRange(0, 32);
				namesCopy = names.GetRange(0, 32);
				omitted = values.Count - valuesCopy.Count;
			}

			//	Get displayable values list, so that we don't have to work with negative values
			List<int> entriesValues = valuesCopy.Where(en => entries.Contains(en)).ToList();

			if (isSkippingZero)
				entriesValues.RemoveAll(val => val == 0);

			if (isSkippingNonFlags)
				entriesValues.RemoveAll(val => !BitmaskUtility.isPowerOfTwo(val));

			List<string> displayNames = namesCopy.Where((en, index) => entriesValues.Contains(valuesCopy[index])).ToList();

			//	Key: entry value; Value: entriesValues index
			Dictionary<int, int> existing = new Dictionary<int, int>();
			int match;
			string enumName;

			//	Combine any elements that have a duplicate value
			for (int i = 0; i < entriesValues.Count; i++) {
				if (existing.TryGetValue(entriesValues[i], out match)) {
					enumName = displayNames[i];
					displayNames.RemoveAt(i);
					entriesValues.RemoveAt(i--);

					//	Append the description.  (thin space + full-width ampersand + enumName)
					displayNames[match] += FormatUtility.MARK_AMPERSAND_SAFE + enumName;
					continue;
				}
				else {
					//	Check if we need to use a custom display name for combined-entry value, using format "entry (entry 1 & entry 2 ...)"
					if (!isSkippingNonFlags && !BitmaskUtility.isPowerOfTwo(entriesValues[i])) {
						///	First check to make sure all its values are allowed (in entries).  Otherwise, remove it
						List<int> bitmaskValues = BitmaskUtility.GetBitmaskValues(entriesValues[i], valuesCopy.Count);

						displayNames[i] +=
							" ("
							+ String.Join(
								FormatUtility.MARK_AMPERSAND_SAFE,
								//	Get all entries in the combined-entry.  Then get their display name and append
								bitmaskValues.Select<int, string>(val => displayNames[entriesValues.IndexOf(val)]).ToArray())
							+ ")";
					}

					existing.Add(entriesValues[i], i);
				}
			}

			int mask = 0;
			string displayName = String.Empty;

			if (property.hasMultipleDifferentValues)
				displayName = "—";
			else {
				//	Get selected list, removing any selected that aren't currently allowed
				List<int> selectedValues = entriesValues.Where(en => property.intValue.Contains(en)).ToList();

				//	We need to convert the entriesList (which contains potentially skipped values) into an ordered mask (with no skipped values) so that EditorGUI.MaskField can use it
				//	The reason that we iterate over entriesValues instead of selectedValues is so that we can add in duplicated entries (an enum with two entries with the same value)
				for (int i = 0; i < entriesValues.Count; i++) {
					if (selectedValues.Contains(entriesValues[i])) {
						mask |= 1 << i;

						//	Don't display combined-entry values in the label (it clutters things up)
						if (!isSkippingNonFlags && !BitmaskUtility.isPowerOfTwo(entriesValues[i]))
							continue;

						//	Add a name to display
						if (String.IsNullOrEmpty(displayName))
							displayName = displayNames[i];
						else
							displayName += ", " + displayNames[i];
					}
				}

				if (String.IsNullOrEmpty(displayName))
					displayName = "Nothing";
				else if (selectedValues.Count == entriesValues.Count && isDisplayingEverything)
					displayName = "Everything";
			}

			//	Make sure the displayName fits in the content field.  If not, reduce it.  (Subtracts pixels so that it doesn't overlap the dropdown arrow on the right)
			displayName = FitContent(displayName, position.width - 15, EditorStyles.popup, CutoffOption.ToEntry);

			int valueNew = 0;
			//	long is used for valueDiff (instead of int) because int will cause an OverflowException when attempting absolute value check for 1 << 31 (Math.Abs(int32.MinValue)).
			long valueDiff = 0;
			bool isAdded;

			//	Verify that no values outside of entries are assigned (on any of the selections)
			foreach (SerializedProperty prop in property.Multiple()) {
				foreach (int val in BitmaskUtility.GetBitmaskValues(property.intValue))
					if (!entries.Contains(val))
						prop.intValue &= ~val;
			}

			//	Append dashes to the display names for any skipped elements (due to exceeding popup display count of 32)
			displayNames.AddRange(Enumerable.Repeat<string>(null, omitted));

			//	Draw popup (without selection display)
			MultiField(
				property,
				(propIter) => valueNew = EditorGUI.MaskField(position, mask, displayNames.ToArray(), PopupWithoutLabelStyle),
				(propIter) => {
					//	'Nothing' selected
					if (valueNew == 0) {
						property.intValue = 0;
						return;
					}
					//	'Everything' selected
					if (valueNew == -1) {
						property.intValue = entries;
						return;
					}

					//	Determine what was clicked by getting the difference (bitwise difference)
					//valueDiff = valueNew ^ mask;
					valueDiff = (long) valueNew - (long) mask;
					if (valueDiff == 0)
						return;

					//	Determine if value was added or removed.
					//	(1 << 31 is a negative int value, so its isAdded will need to be flipped.  Check against the min value and its two's-complement.)
					isAdded = valueDiff > 0;
					if (valueDiff == Int32.MinValue || valueDiff == ~((long) Int32.MinValue - 1))
						isAdded = !isAdded;

					//	Convert ordered mask to real mask
					valueDiff = entriesValues[BitmaskUtility.GetMaskIndex(Math.Abs(valueDiff))];

					//	Standard, add or remove selection
					if (BitmaskUtility.isPowerOfTwo(valueDiff)) {
						if (isAdded)
							property.intValue |= (int) valueDiff;
						else
							property.intValue &= (int) ~valueDiff;
					}
					//	If not a power of two, selection is a combined-entry.  Add or remove all its parts
					else {
						if (isAdded)
							foreach (int val in BitmaskUtility.GetBitmaskValues(valueDiff, valuesCopy.Count))
								property.intValue |= val;
						else
							foreach (int val in BitmaskUtility.GetBitmaskValues(valueDiff, valuesCopy.Count))
								property.intValue &= ~val;
					}
				});

			GUIContent label = new GUIContent(displayName);
			Color colorPrior = GUI.color;
			if (omitted > 0) {
				label.tooltip = omitted + " elements were omitted, due to the length of the enum's entries (" + values.Count + ") exceeding Unity's limitation of 32 for popups.";
				GUI.color = Color.red;
			}

			//	Display label with active selection
			EditorGUI.LabelField(position, label, EditorStyles.popup);

			if (omitted > 0)
				GUI.color = colorPrior;
		}
		#endregion

		#region Element widths
		public const float LINE_HEIGHT = 16.0f;
		public const float LINE_HEIGHT_SEPARATOR = 2.0f;
		public const float ELEMENT_SEPARATOR_HORIZONTAL = 5.0f;
		public const float INDENT_PIXELS = 15.0f;

		public const float COMPARATOR_WIDTH = 13.0f;
		#endregion

		#region Property Layout methods
		public static float GetPropertyHeight(int lineCount) {

			return LINE_HEIGHT * lineCount + LINE_HEIGHT_SEPARATOR * (lineCount - 1);
		}

		public static float GetLineSpacing(int lineCount) {

			return LINE_HEIGHT * lineCount + LINE_HEIGHT_SEPARATOR * lineCount;
		}

		public static float IndentWidth {
			get {
				return EditorGUI.indentLevel * INDENT_PIXELS;
			}
		}
		#endregion

		#region Rect Layout variables and methods
		/// <summary>
		///		Stores the rect assigned from UpdateRectData.
		/// </summary>
		public static Rect LayoutRect {
			get;
			private set;
		}

		/// <summary>
		///		Updates all dimension variables for use in generation of Rects.
		///		NOTE: Must be called at the beginning of every OnGUI where RectGenerators are desired to be used!
		///		NOTE: If using nested OnGUI calls (for instance calling EditorGUI.PropertyField on a property that also will call UpdateRectData),
		///			make sure to refresh immediately afterwards by calling UpdateRectData again.
		///		Automatically corrects for EditorGUI.indentLevel.
		/// </summary>
		/// <param name="layoutRect"></param>
		public static void UpdateRectData(Rect layoutRect) {

			//	Save Rect without indent information
			LayoutRect = new Rect(
				layoutRect.x,
				layoutRect.y,
				layoutRect.width + INDENT_PIXELS,
				layoutRect.height);
		}

		/// <summary>
		///		Returns a Rect that corresponds to a label (the left part of an inspector line, where a label is generally displayed).
		/// </summary>
		/// <param name="line">The line from the start, using 0 as the first line in the GUI.</param>
		/// <returns></returns>
		public static Rect GetLabelRect(int line) {

			return new Rect(
				LayoutRect.x,
				LayoutRect.y + (LINE_HEIGHT + LINE_HEIGHT_SEPARATOR) * line,
				EditorGUIUtility.labelWidth,
				LINE_HEIGHT);
		}

		/// <summary>
		///		Returns a Rect that corresponds to a field (the right part of an inspector line, where data is generally displayed).
		/// </summary>
		/// <param name="line">The line from the start, using 0 as the first line in the GUI.</param>
		/// <returns></returns>
		public static Rect GetFieldRect(int line) {

			return new Rect(
				LayoutRect.x + EditorGUIUtility.labelWidth - IndentWidth,
				LayoutRect.y + (LINE_HEIGHT + LINE_HEIGHT_SEPARATOR) * line,
				LayoutRect.width - EditorGUIUtility.labelWidth + IndentWidth - INDENT_PIXELS,
				LINE_HEIGHT);
		}

		/// <summary>
		///		Returns a Rect that corresponds to an entire line (Label and Field).
		/// </summary>
		/// <param name="line">The line from the start, using 0 as the first line in the GUI.</param>
		/// <param name="useIndent">Determines if the indents should be used.  Should use false if the LayoutRect represents only the field's Rect, instead of the entire line.</param>
		/// <returns></returns>
		public static Rect GetLineRect(int line = 0, bool useIndent = true) {

			return new Rect(
				LayoutRect.x,
				LayoutRect.y + (LINE_HEIGHT + LINE_HEIGHT_SEPARATOR) * line,
				LayoutRect.width - (useIndent ? IndentWidth + INDENT_PIXELS : INDENT_PIXELS),
				LINE_HEIGHT);
		}

		/// <summary>
		///		Splits a Rect (corresponding to a Label) into two rects.  The caller defines which element should have a fixed width; if both are left null, will halve the Rect's width.
		///		Automatically corrects for EditorGUI.indentLevel.
		/// </summary>
		/// <param name="fullRect">The rect generated for splitting (generally from GetLabelRect).</param>
		/// <param name="rect1">Assignment of the first rect to include in the split.</param>
		/// <param name="rect2">Assignment of the second rect to include in the split.</param>
		/// <param name="firstElementWidth">If non-null, the width of the first element.</param>
		/// <param name="secondElementWidth">If non-null, the width of the second element.</param>
		/// <param name="isSpacing">If false, no horizontal spacing is placed between elements.</param>
		/// <returns></returns>
		public static void SplitLabelRect(Rect fullRect, out Rect rect1, out Rect rect2, float? firstElementWidth, float? secondElementWidth, bool isSpacing = true) {

			SplitRect(false, fullRect, out rect1, out rect2, firstElementWidth, secondElementWidth, isSpacing);
		}

		/// <summary>
		///		Splits a Rect (corresponding to a Field) into two rects.  The caller defines which element should have a fixed width; if both are left null, will halve the Rect's width.
		///		Automatically corrects for EditorGUI.indentLevel.
		/// </summary>
		/// <param name="fullRect">The rect generated for splitting (generally from GetFieldRect).</param>
		/// <param name="rect1">Assignment of the first rect to include in the split.</param>
		/// <param name="rect2">Assignment of the second rect to include in the split.</param>
		/// <param name="firstElementWidth">If non-null, the width of the first element.</param>
		/// <param name="secondElementWidth">If non-null, the width of the second element.</param>
		/// <param name="isSpacing">If false, no horizontal spacing is placed between elements.</param>
		/// <returns></returns>
		public static void SplitFieldRect(Rect fullRect, out Rect rect1, out Rect rect2, float? firstElementWidth, float? secondElementWidth, bool isSpacing = true) {

			SplitRect(true, fullRect, out rect1, out rect2, firstElementWidth, secondElementWidth, isSpacing);
		}

		/// <summary>
		///		Splits a Rect into two rects.  The caller defines which element should have a fixed width; if both are left null, will halve the Rect's width.
		///		Only one width can be declared.  If both are declared, uses firstElementWidth only.
		///		NOTE: Automatically corrects for EditorGUI.indentLevel.
		/// </summary>
		/// <param name="isField">Is SplitRect being performed on a field or label?  (Fields need additional correcting for indent level.)</param>
		/// <param name="fullRect">The rect generated for splitting (generally from GetFieldRect).</param>
		/// <param name="rect1">Assignment of the first rect to include in the split.</param>
		/// <param name="rect2">Assignment of the second rect to include in the split.</param>
		/// <param name="firstElementWidth">If non-null, the width of the first element.  NOTE: The actual rect may not have this width, due to correcting for indent levels.</param>
		/// <param name="secondElementWidth">If non-null, the width of the second element.  NOTE: The actual rect may not have this width, due to correcting for indent levels.</param>
		/// <param name="isSpacing">If false, no horizontal spacing is placed between elements.</param>
		/// <returns></returns>
		private static void SplitRect(bool isField, Rect fullRect, out Rect rect1, out Rect rect2, float? firstElementWidth, float? secondElementWidth, bool isSpacing = true) {

			if (firstElementWidth.HasValue) {
				rect1 = new Rect(
					fullRect.x,
					fullRect.y,
					firstElementWidth.Value,
					fullRect.height);
				rect2 = new Rect(
					fullRect.x + rect1.width + (isSpacing ? ELEMENT_SEPARATOR_HORIZONTAL : 0.0f),
					fullRect.y,
					fullRect.width - firstElementWidth.Value - (isSpacing ? ELEMENT_SEPARATOR_HORIZONTAL : 0.0f),
					fullRect.height);

				if (isField)
					rect1.width += IndentWidth;
			}
			else if (secondElementWidth.HasValue) {
				rect1 = new Rect(
					fullRect.x,
					fullRect.y,
					fullRect.width - secondElementWidth.Value - (isSpacing ? ELEMENT_SEPARATOR_HORIZONTAL : 0.0f),
					fullRect.height);
				rect2 = new Rect(
					fullRect.x + rect1.width + (isSpacing ? ELEMENT_SEPARATOR_HORIZONTAL : 0.0f),
					fullRect.y,
					secondElementWidth.Value,
					fullRect.height);

				if (isField) {
					rect2.x -= IndentWidth;
					rect2.width += IndentWidth;
				}
			}
			else {
				rect1 = new Rect(
					fullRect.x,
					fullRect.y,
					Mathf.FloorToInt((fullRect.width + IndentWidth) / 2.0f) - (isSpacing ? Mathf.FloorToInt(ELEMENT_SEPARATOR_HORIZONTAL / 2.0f) : 0.0f),
					fullRect.height);
				rect2 = new Rect(
					fullRect.x + Mathf.FloorToInt(fullRect.width / 2.0f) + (isSpacing ? ELEMENT_SEPARATOR_HORIZONTAL / 2.0f : 0.0f),
					fullRect.y,
					Mathf.FloorToInt((fullRect.width + IndentWidth) / 2.0f) - (isSpacing ? Mathf.CeilToInt(ELEMENT_SEPARATOR_HORIZONTAL / 2.0f) : 0.0f),
					fullRect.height);

				if (isField)
					rect2.x -= IndentWidth / 2;
			}
		}

		/// <summary>
		///		Corrects indent issues (caused due to certain EditorGUI and GUI field draw options not taking IndentLevel into consideration).
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="isLeft">Should the indent result in pushing the element left or right?</param>
		/// <returns></returns>
		public static Rect CorrectIndent(this Rect rect, bool isLeft) {

			return new Rect(
				rect.x + IndentWidth * (isLeft ? -1 : 1),
				rect.y,
				rect.width - IndentWidth * (isLeft ? -1 : 1),
				rect.height);
		}

		/// <summary>
		///		Corrects indent issues (caused due to certain EditorGUI and GUI field draw options not taking IndentLevel into consideration).
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="isLeft">Should the indent result in pushing the element left or right?</param>
		/// <returns></returns>
		public static void CorrectIndent(ref Rect rect, bool isLeft) {

			rect.x += IndentWidth * (isLeft ? -1 : 1);
			rect.width -= IndentWidth * (isLeft ? -1 : 1);
		}
		#endregion

		#region Content Fitting methods
		/// <summary>
		///		Options to handle limiting text when there isn't enough room to fit the full content.
		/// </summary>
		public enum CutoffOption {
			/// <summary>
			///		Cutoff cuts off only the content needed to fit an ellipsis.
			/// </summary>
			ToLetter,
			/// <summary>
			///		Cutoff cuts off the content needed, as whole words, to fit an ellipsis.
			/// </summary>
			ToWord,
			/// <summary>
			///		Meant to work with comma-separated entries.  Removes entire entries (and corresponding comma), displaying '... +X' with X being the number of skipped entries.
			/// </summary>
			ToEntry,
		}

		/// <summary>
		///		Reduces a string until it fits into a specified GUIStyle and width, on a single line.  Appends '…' if the provided string could not fit,
		///			with the number of entries skipped being displayed with '+X' after if using CutoffOption.ToEntry.
		///		NOTE: GUIStyle settings can break this from working.  To prevent this, the style has fixedWidth and wordWrap temporarily disabled.
		///			There may be other settings that interfere as well.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="width"></param>
		/// <param name="style"></param>
		/// <param name="cutoffOption">The option to use for determining how the content will be cutoff in order to fit.</param>
		/// <returns></returns>
		public static string FitContent(string content, float width, GUIStyle style, CutoffOption cutoffOption) {

			//	Cache and disable settings that would interfere
			bool wrapPrior = style.wordWrap;
			float fixedWidthPrior = style.fixedWidth;

			if (style.wordWrap)
				style.wordWrap = false;
			if (!Mathf.Approximately(style.fixedWidth, 0.0f))
				style.fixedWidth = 0.0f;

			float current = style.CalcSize(new GUIContent(content)).x;

			if (current > width) {
				if (cutoffOption == CutoffOption.ToLetter) {
					//	Remove characters until it fits
					while (content.Length > 0) {
						//	Get length with the ellipsis we'll be using
						current = style.CalcSize(new GUIContent(content + "…")).x;
						if (current > width)
							content = content.Substring(0, content.Length - 1);
						//	Done, add the ellipsis to the end
						else {
							content += "…";
							break;
						}
					}
				}
				else if (cutoffOption == CutoffOption.ToWord) {
					//	Split the entries
					List<string> entries = content.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
					int skipped = 1;

					while (skipped < entries.Count) {
						content = String.Join(" ", entries.GetRange(0, entries.Count - skipped).ToArray()) + "…";
						current = style.CalcSize(new GUIContent(content)).x;
						if (current <= width)
							break;

						skipped++;
					}
				}
				else {
					//	Split the entries
					List<string> entries = content.Split(new string[] { " ,", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
					int skipped = 1;

					while (skipped < entries.Count) {
						content = String.Join(",", entries.GetRange(0, entries.Count - skipped).ToArray()) + "… +" + skipped;
						current = style.CalcSize(new GUIContent(content)).x;
						if (current <= width)
							break;

						skipped++;
					}
				}
			}

			style.wordWrap = wrapPrior;
			style.fixedWidth = fixedWidthPrior;

			return content;
		}

		/// <summary>
		///		Returns re-formatted text that can be displayed in a Popup field without issues (& and / have separate meanings).
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetPopupSafeText(string text) {

			return text.Replace(@"/", FormatUtility.MARK_SLASH_SAFE).Replace("&", FormatUtility.MARK_AMPERSAND_SAFE);
		}
		#endregion

		#region Multi-Object Selection methods
		/// <summary>
		///		Handles iterating over all selected objects to safely apply SerializedProperty to, in the case of multi-object selection.
		///		NOTE: SerializedObject order is automatically reversed, since it makes more sense logically (top selection in hierarchy is first object in list).
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static IEnumerable<SerializedProperty> Multiple(this SerializedProperty property) {

			return property.serializedObject.targetObjects.Select(o => new SerializedObject(o).FindProperty(property.propertyPath)).Reverse();
		}

		/// <summary>
		///		Handles simplified handling of multi-object selections for a property.
		///		NOTE: For example use, see function body.
		/// </summary>
		/// <param name="property">The property to use for all selected objects.</param>
		/// <param name="field">The field to draw, taking the iterated object's prop field as a parameter.</param>
		/// <param name="copier">An assignment method to apply a new seleected value to the original prop.  Only called if there is a change check in the field.</param>
		public static void MultiField(SerializedProperty property, Action<SerializedProperty> field, Action<SerializedProperty> copier) {

			//	Example use:
			//EditorGUIHelper.MultiField(
			//	noteProp,
			//	(propIter) => propIter.intValue = DrawPopup(pianoPos, (Note) propIter.intValue, PopupStyle),
			//	(propIter) => noteProp.intValue = propIter.intValue);

			MultiField(property, field, copier, EditorGUI.showMixedValue || property.hasMultipleDifferentValues);
		}

		/// <summary>
		///		Handles simplified handling of multi-object selections for a property.
		/// </summary>
		/// <param name="property">The property to use for all selected objects.</param>
		/// <param name="field">The field to draw, taking the iterated object's prop field as a parameter.</param>
		/// <param name="copier">An assignment method to apply a new seleected value to the original prop.  Only called if there is a change check in the field.</param>
		/// <param name="showMixedValue">Override for property.hasMultipleDifferentValues check.</param>
		public static void MultiField(SerializedProperty property, Action<SerializedProperty> field, Action<SerializedProperty> copier, bool showMixedValue) {

			//	Example use:
			//EditorGUIHelper.MultiField(
			//	noteProp,
			//	(propIter) => propIter.intValue = DrawPopup(pianoPos, (Note) propIter.intValue, PopupStyle),
			//	(propIter) => noteProp.intValue = propIter.intValue);

			bool mixedPrior = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = showMixedValue;

			SerializedProperty prop = property.Multiple().First();
			EditorGUI.BeginChangeCheck();
			field(prop);
			if (EditorGUI.EndChangeCheck())
				copier(prop);

			EditorGUI.showMixedValue = mixedPrior;
		}
		#endregion

		#region GUIStyles
		public static GUIStyle PopupWithoutLabelStyle {
			get {
				if (_PopupWithoutLabelStyle == null) {
					_PopupWithoutLabelStyle = new GUIStyle(EditorStyles.popup);
					_PopupWithoutLabelStyle.wordWrap = false;
					_PopupWithoutLabelStyle.contentOffset = new Vector2(9999, 9999);
				}
				return _PopupWithoutLabelStyle;
			}
		}
		private static GUIStyle _PopupWithoutLabelStyle;

		public static GUIStyle LabelCenteredStyle {
			get {
				if (_LabelCenteredStyle == null) {
					_LabelCenteredStyle = new GUIStyle(GUI.skin.label);
					_LabelCenteredStyle.alignment = TextAnchor.MiddleCenter;
				}
				return _LabelCenteredStyle;
			}
		}
		private static GUIStyle _LabelCenteredStyle;
		#endregion
	}
}
#endif