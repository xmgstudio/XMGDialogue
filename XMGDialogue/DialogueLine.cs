using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XMGDialogue {

	/// <summary>
	/// This class contains a given line of dialogue
	/// </summary>
	public class DialogueLine {

		#region Constants

		/// <summary>
		/// The regex that recognizes meta info in regular strings in the dialogue system, captures things that are inside of parens. This regex is lazy and will match with the first end paren presented.
		/// </summary>
		public static readonly string META_INFO_REGEX_FORMATTER = "(?<={0}\\().*?(?=\\))";

		/// <summary>
		/// Tag for enumerating dialogue options. Written like options([[Dialogue|New Conversation Node]]).
		/// </summary>
		private const string OPTIONS_TAG = "options";

		/// <summary>
		/// Tag for action data. Written like actions([action|optionalEventTag]) or [action].
		/// </summary>
		private const string ACTION_TAG = "actions";

		/// <summary>
		/// This is the character we use to split options and actions up between their components.
		/// </summary>
		private const char PARAM_SPLIT = '|';

		/// <summary>
		/// Character that separates the name of the speaker from the body of the line.
		/// </summary>
		private const char LINE_NAME_SPLIT = ':';

		/// <summary>
		/// The regex for separating options, matches a double square brace followed by any number of spaces and then a comma before another double square brace.
		/// </summary>
		private const string OPTION_DATA_SEPARATOR_REGEX = @"(?<=\]\]\s*),(?=\s*\[\[)";

		/// <summary>
		/// Regex for separating actions.
		/// </summary>
		private const string ACTION_DATA_SEPARATOR_REGEX = @"(?<=\]\s*),(?=\s*\[)";

		/// <summary>
		/// The square bracket array for removing syntax characters from options ([[option]]) and actions ([action]).
		/// </summary>
		private static readonly char[] SQUARE_BRACKET_ARRAY = {' ','[', ']'};

		#endregion

		#region Accessors

		/// <summary>
		/// ID for the speaker of this line.
		/// </summary>
		public string SpeakerID {
			get;
			private set;
		}

		/// <summary>
		/// Gets the lin text itself.
		/// </summary>
		public string Line {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="DialogueLine"/> has buttons associated with it.
		/// </summary>
		public bool HasChoices {
			get {
				return (this.parsedOptions != null && this.parsedOptions.Count > 0);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="DialogueLine"/> has choices only.
		/// </summary>
		public bool ChoicesOnly {
			get {
				return (string.IsNullOrEmpty(this.Line) && this.HasChoices);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has actions.
		/// </summary>
		public bool HasActions {
			get {
				return (this.lineActions != null && this.lineActions.Count > 0);
			}
		}

		/// <summary>
		/// This holds a list of labels (for options) and conversation tags for this given dialogue line.
		/// </summary>
		public List<KeyValuePair<string, string>> parsedOptions {
			get;
			private set;
		}

		/// <summary>
		/// List of action and and action params for this line.
		/// </summary>
		/// <value>The line actions.</value>
		public List<KeyValuePair<string, string>> lineActions {
			get;
			private set;
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="DialogueLine"/> class.
		/// </summary>
		/// <param name="line">Line of unparsed text to use to build new dialogue lines.</param>
		public DialogueLine(string line) {
			this.parsedOptions = new List<KeyValuePair<string, string>>();
			this.lineActions = new List<KeyValuePair<string, string>>();

			// Start with getting the speaker.
			if (line.Contains(LINE_NAME_SPLIT.ToString())) {
				this.SpeakerID = line.Substring(0, line.IndexOf(LINE_NAME_SPLIT)).Trim();
				this.Line = line.Substring(line.IndexOf(LINE_NAME_SPLIT) + 1).Trim();
			} else {
				// If there wasn't a name the whole thing is a line with no character attached.
				this.Line = line;
			}

			if (this.Line.Contains(PARAM_SPLIT.ToString())) {
				// We have options, they will start after the first instance of the option split.
				int splitLocation = this.Line.IndexOf(PARAM_SPLIT);
				this.ParseMetaData(this.Line.Substring(splitLocation + 1));
				this.Line = this.Line.Substring(0, splitLocation).Trim();
			}
		}

		#endregion

		#region Options Handling

		/// <summary>
		/// Takes in an unparsed action and parses it into an actionable item.
		/// </summary>
		/// <param name="unparsedOptionString">Unparsed option string.</param>
		public void AddOptions(string unparsedOptionString) {
			// This gets a string formatted like [[Option Key|option Tag]]
			unparsedOptionString = unparsedOptionString.Trim(SQUARE_BRACKET_ARRAY);

			int splitLocation = unparsedOptionString.LastIndexOf(PARAM_SPLIT);
			this.AddOptions(unparsedOptionString.Substring(0, splitLocation).Trim(), unparsedOptionString.Substring(splitLocation + 1).Trim());
		}

		/// <summary>
		/// Directly adds an action to this dialogue line.
		/// </summary>
		/// <param name="actionKey">Option key, used as a key for displaying the action.</param>
		/// <param name="actionTag">Option tag, used to hook up actions to events via the dialogue controller.</param>
		public void AddOptions(string optionKey, string optionTag) {
			this.parsedOptions.Add(new KeyValuePair<string, string>(optionKey, optionTag));
		}

		/// <summary>
		/// Removes an action key.
		/// </summary>
		/// <param name="optionKey">Removes an option by key</param>
		public void RemoveOption(string optionKey) {
			this.parsedOptions.RemoveAll(delegate(KeyValuePair<string, string> obj) {
				return obj.Key == optionKey;
			});
		}

		#endregion

		#region Actions

		/// <summary>
		/// Adds an action to the actions list by parsing an action string.
		/// </summary>
		/// <param name="unparsedAction">Unparsed action.</param>
		private void AddAction(string unparsedAction) {
			// This method recieves a string formatted like [actionKey|ActionParams] or [actionKey] if there are no params
			unparsedAction = unparsedAction.Trim(SQUARE_BRACKET_ARRAY);

			// String now look like actionKey|ActionParams if there are params or simply actionKey is there are not.
			if (unparsedAction.Contains(PARAM_SPLIT.ToString())) {
				int splitLocation = unparsedAction.LastIndexOf(PARAM_SPLIT);
				this.AddAction(unparsedAction.Substring(0, splitLocation).Trim(), unparsedAction.Substring(splitLocation + 1).Trim());
			} else {
				this.AddAction(unparsedAction, string.Empty);
			}
		}

		/// <summary>
		/// Adds the action to the action dictionary.
		/// </summary>
		/// <param name="actionKey">Action key.</param>
		/// <param name="actionParam">Action parameter.</param>
		private void AddAction(string actionKey, string actionParam) {
			this.lineActions.Add(new KeyValuePair<string, string>(actionKey, actionParam));
		}

		/// <summary>
		/// Removes the action from the action dictionary.
		/// </summary>
		/// <param name="actionKey">Action key.</param>
		private void RemoveAction(string actionKey) {
			this.lineActions.RemoveAll(delegate(KeyValuePair<string, string> obj) {
				return obj.Key == actionKey;
			});
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Method that handles all the parsing of the per-line options.
		/// </summary>
		/// <param name="metaData">Meta data string.</param>
		private void ParseMetaData(string metadata) {
			Match actions = Regex.Match(metadata, string.Format(META_INFO_REGEX_FORMATTER, ACTION_TAG));
			if (actions.Success) {
				// Parse out the actions which will be in a format similar to Options.
				string[] actionList = Regex.Split(actions.Value, ACTION_DATA_SEPARATOR_REGEX);
				for (int i = 0; i < actionList.Length; i++) {
					this.AddAction(actionList[i]);
				}
			}
			
			Match dialogueOptions = Regex.Match(metadata, string.Format(META_INFO_REGEX_FORMATTER, OPTIONS_TAG));
			if (dialogueOptions.Success) {
				// We now have a list of comma separated values like: [[key|action]]
				string[] optionList = Regex.Split(dialogueOptions.Value, OPTION_DATA_SEPARATOR_REGEX);
				for (int i = 0; i < optionList.Length; i++) {
					this.AddOptions(optionList[i]);
				}
			}
		}

		#endregion

		#region Object
		
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="DialogueLine"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="DialogueLine"/>.</returns>
		public override string ToString() {
			object[] toStringParams = new object[] { this.SpeakerID, this.Line, this.HasChoices, this.lineActions.Count};
			return string.Format ("[DialogueLine: SpeakerID={0}, Line={1}, HasChoices={2}, HasActions={3}]", toStringParams);
		}
		
		#endregion
	}
}
