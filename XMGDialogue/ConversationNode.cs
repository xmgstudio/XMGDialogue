using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XMGDialogue {

	/// <summary>
	/// This class represents a single conversation node, corresponding to a single block of data in Yarn.
	/// </summary>
	public class ConversationNode {
		
		#region Constants

		/// <summary>
		/// This matches any comma that is preceded and followed by [[ ]]
		/// </summary>
		private const string OPTIONS_SPLIT_REGEX = "(?<=\\]\\]),(?=\\[\\[)";

		/// <summary>
		/// This matches any close square brace, any number of white spaces, a comma and then the start of another tag.
		/// </summary>
		private const string TAG_SPLIT_REGEX = @"(?<=\])\s*,\s*(?=\w+\[)";

		/// <summary>
		/// This regex extracts a key from a properly formatted tag block formatted like key[list, of, tags]
		/// </summary>
		private const string TAG_KEY_REGEX = @"\w+(?=\[)";

		/// <summary>
		/// This regex extracts the values as a comma separated list from a properly formatted tag block.
		/// </summary>
		private const string TAG_VALUE_REGEX = @"(?<=\w+\[).*(?=\])";

		/// <summary>
		/// This matches a comma surrounded by any number of spaces so it splits and trims a list of comma separate values if used with Regex.Split.
		/// </summary>
		private const string CSL_SPLIT_REGEX = @"\s*,\s*";

		/// <summary>
		/// The json tag for the title information.
		/// </summary>
		private const string YARN_TITLE_TAG = "title";

		/// <summary>
		/// The json tag for the tag information.
		/// </summary>
		private const string YARN_TAGS_TAG = "tags";

		/// <summary>
		/// The json tag for the body information.
		/// </summary>
		private const string YARN_BODY_TAG = "body";

		/// <summary>
		/// The options opening tag.
		/// </summary>
		private const string OPTION_OPENING_TAG = "[[";
		
		#endregion

		#region Data and Accessors

		/// <summary>
		/// Gets the title of this conversation node.
		/// </summary>
		public string Title {
			get;
			private set;
		}

		/// <summary>
		/// A mapping of tag Keys to the values in that tag. Tags are structured like 
		/// </summary>
		private Dictionary<string, string[]> tagMap = new Dictionary<string, string[]>();

		private List<DialogueLine> dialogue = null;
		/// <summary>
		/// List of the dialogue lines.
		/// </summary>
		public List<DialogueLine> Dialogue {
			get {
				return this.dialogue;
			}
		}

		/// <summary>
		/// The line of dialogue that this conversation node is pointing to.
		/// </summary>
		private int dialoguePointer = 0;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a conversation node with the parsed data 
		/// </summary>
		/// <param name="parsedData">Parsed data.</param>
		public ConversationNode(Dictionary<string, object> parsedData) {
			this.Title = this.GetStringForKey(parsedData, YARN_TITLE_TAG);
			this.ParseTags(this.GetStringForKey(parsedData, YARN_TAGS_TAG));
			this.ParseScript(this.GetStringForKey(parsedData, YARN_BODY_TAG));

			// Set this conversation to it's default state.
			this.ResetConversation();
		}

		#endregion

		#region Conversation Logic

		/// <summary>
		/// Function for parsing the script information out of the body tag.
		/// </summary>
		/// <param name="bodyTagData">Script tags from the body.</param>
		private void ParseScript(string bodyTagData) {
			string[] bodyStrings = bodyTagData.Split('\n');
			this.dialogue = new List<DialogueLine>(bodyStrings.Length);
			for (int i = 0; i < bodyStrings.Length; i++) {
				if (bodyStrings[i].StartsWith(OPTION_OPENING_TAG)) {
					// This is a choice block and doesn't have dialogue options. We need to slot it into the preceding dialogue line chunk.
					string[] optionsList = Regex.Split(bodyStrings[i], OPTIONS_SPLIT_REGEX);
					if (i > 0) {
						for (int optionIndex = 0; i < optionsList.Length; optionIndex++) {
							this.dialogue[this.dialogue.Count - 1].AddOptions(optionsList[optionIndex]);
						}
					} else {
						Debug.LogError("Cannot have options without a preceding line of dialogue.");
					}
				} else {
					if (!string.IsNullOrEmpty(bodyStrings[i])) {
						this.dialogue.Add(new DialogueLine(bodyStrings[i]));
					} else {
						Debug.Log(string.Format("Empty line on line {0} of {1}; skipping.", i, this.Title));
					}
				}
			}
		}

		/// <summary>
		/// Resets the conversation pointer.
		/// </summary>
		public void ResetConversation() {
			this.dialoguePointer = 0;
		}

		/// <summary>
		/// Advances the point and gets the next line in the dialogue, returns null if the pointer is beyond the end of the dialogue.
		/// </summary>
		/// <returns>The line.</returns>
		public DialogueLine GetNextLine() {
			this.dialoguePointer++;
			if (this.dialoguePointer < this.dialogue.Count) {
				return this.dialogue[this.dialoguePointer];
			} else {
				return null;
			}
		}
		/// <summary>
		/// Sets the current line pointer to a specific value.
		/// </summary>
		/// <param name="lineValue">The new value for the dialogue pointer.</param>
		public void SetLine(int lineValue) {
			this.dialoguePointer = lineValue;
		}

		/// <summary>
		/// Gets the current line in this conversation line without advancing the conversation.
		/// </summary>
		/// <returns>The current line.</returns>
		public DialogueLine GetCurrentLine() {
			return this.dialogue[this.dialoguePointer];
		}

		/// <summary>
		/// Determines whether this instance has a next line.
		/// </summary>
		/// <returns><c>true</c> if this instance has a next line; otherwise, <c>false</c>.</returns>
		public bool HasNextLine() {
			return this.dialoguePointer < (this.dialogue.Count - 1);
		}

		/// <summary>
		/// Determines whether this instance has a specific line or not.
		/// </summary>
		/// <param name="value">The specific line to check.</param>
		/// <returns><c>true</c> if this instance has the specific line; otherwise, <c>false</c>.</returns>
		public bool HasLine(int value) {
			return value <= (this.dialogue.Count - 1);
		}

		#endregion

		#region Tags

		/// <summary>
		/// Parse the tag string out into the meta-information dictionary.
		/// </summary>
		/// <param name="tagString">Tag string.</param>
		private void ParseTags(string tagString) {
			string[] tagItems = Regex.Split(tagString, TAG_SPLIT_REGEX);
			
			for (int i = 0; i < tagItems.Length; i++) {
				Match keyMatch = Regex.Match(tagItems[i], TAG_KEY_REGEX);
				Match valueMatch = Regex.Match(tagItems[i], TAG_VALUE_REGEX);
				
				if (keyMatch.Success && valueMatch.Success) {
					string[] tagValues = Regex.Split(valueMatch.Value, CSL_SPLIT_REGEX);
					this.tagMap.Add(keyMatch.Value, tagValues);
				}
			}
		}

		/// <summary>
		/// Gets a list of values for a given tag.
		/// </summary>
		/// <returns>The tag values or an empty array if there are no values.</returns>
		/// <param name="tag">Tag to get values for.</param>
		public string[] GetTagValues(string tag) {
			if (tagMap.ContainsKey(tag)) {
				return tagMap[tag];
			} else {
				Debug.LogError(string.Format("No tag key matching {0} found in conversation node {1}", tag, this.Title));
				return new string[0];
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets a string from a dictionary by key. If the dictionary does not contain that key return an empty string.
		/// </summary>
		/// <returns>The string for the given key or null.</returns>
		/// <param name="dict">Dictionary to search.</param>
		/// <param name="key">Key to find.</param>
		private string GetStringForKey(Dictionary<string, object> dict, string key) {
			if (dict.ContainsKey(key)) {
				return dict[key] as string;
			} else {
				return string.Empty;
			}
		}

		#endregion

	}
}
