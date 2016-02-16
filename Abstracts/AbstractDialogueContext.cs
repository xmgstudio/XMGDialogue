using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class AbstractDialogueContext : MonoBehaviour {

	#region Constants

	/// <summary>
	/// The regex that looks for a replacement dialogue string formatted like {string}.
	/// </summary>
	protected const string REPLACEMENT_DIALOGUE_REGEX = "(?<={).*?(?=})";

	/// <summary>
	/// The replacement tag formatter, for string formatters to write a literal curly brace you use a double curly brace 
	/// therefore three curlies in a row give you a formatted value surrounded by curly braces.
	/// </summary>
	protected const string REPLACE_TAG_FORMATTER = "{{{0}}}";

	#endregion

	#region Events & Delegates

	/// <summary>
	/// Delegate for selecting an option.
	/// </summary>
	/// <param name="optionSelected">The action key of the option that was selected.</param>
	public delegate void OptionSelectedDelegate(string optionSelected);

	/// <summary>
	/// This is the action that 
	/// </summary>
	/// <param name="actionParams">String encoding the params for the action.</param>
	public delegate void DialogueActionDelegate(string actionKey, string actionParams);
	
	/// <summary>
	/// Event that is thrown when the continue button is pressed.
	/// </summary>
	public event Action OnContinuePressed = null;

	/// <summary>
	/// Occurs when an option is selected through this context.
	/// </summary>
	public event DialogueOption.DialogueButtonDelegate OnOptionSelected = null;

	/// <summary>
	/// Occurs when a dialouge action is encountered by the line reader.
	/// </summary>
	public event DialogueActionDelegate OnDialogueActionEncountered = null;

	/// <summary>
	/// Occurs when the dialogue is displayed.
	/// </summary>
	public event Action DialogueDisplayOver = null;
	
	#endregion

	#region Accessors & Data

	/// <summary>
	/// Gets the title/ID of this context.
	/// </summary>
	public abstract string Title {
		get;
	}

	/// <summary>
	/// The camera for this dialogue context.
	/// </summary>
	[SerializeField]
	protected Camera dialogueCamera = null; 

	/// <summary>
	/// The replacement strings dictionary.
	/// </summary>
	protected Dictionary<string, string> replacementStrings = new Dictionary<string, string>();

	#endregion

	#region Methods

	/// <summary>
	/// Notifies the dialogue context that we are loading a new node and there may be new characters.
	/// </summary>
	/// <param name="conversationNode">The new conversation node that is starting.</param>
	public abstract void NewConversationNode(ConversationNode conversationNode);

	/// <summary>
	/// Handles displaying the dialogue.
	/// </summary>
	/// <param name="line">Line of dialogue to show.</param>
	public abstract void DisplayDialogue(DialogueLine line);

	/// <summary>
	/// Opens and sets up this context.
	/// </summary>
	/// <param name="initializationData">Data to use to initialize this dialogue context.</param>
	public virtual void InitializeContext(object initializationData = null) {
		this.dialogueCamera.gameObject.SetActive(true);
	}

	/// <summary>
	/// Check if this interface supports character images.
	/// </summary>
	/// <returns><c>true</c>, if images was supportsed, <c>false</c> otherwise.</returns>
	public abstract bool SupportsImages();
	
	/// <summary>
	/// Gets the number of options that this dialogue can display, -1 if it doesn't matter.
	/// </summary>
	/// <returns>The display count.</returns>
	public abstract int OptionsDisplayCount();

	/// <summary>
	/// Closes and finalizes this dialogue context.
	/// </summary>
	/// <param name="contextClosed">Callback for when the context has been closed.</param>
	public virtual void CloseContext(Action contextClosed = null) {
		this.StartCoroutine(this.CloseContextCoroutine(delegate() {
			if (contextClosed != null) {
				contextClosed();
			}
		
			this.dialogueCamera.gameObject.SetActive(false);
			this.gameObject.SetActive(false);
		}));
	}

	/// <summary>
	/// Coroutine that handles closing down this context. Moving actors off screen and doing any other cleanup.
	/// </summary>
	/// <param name="contextClosed">Callback for when this context is finished closing up.</param>
	protected abstract IEnumerator CloseContextCoroutine(Action contextClosed = null);

	#endregion

	#region Replacement Dialogue

	/// <summary>
	/// Applies the replacement dialogue.
	/// </summary>
	/// <param name="replacementKey">Replacement key.</param>
	/// <param name="replacementText">Replacement text.</param>
	public void RegisterReplacementDialogue(string replacementKey, string replacementText) {
		this.replacementStrings[replacementKey] = replacementText;
	}

	/// <summary>
	/// Remove a replacement dialogue string.
	/// </summary>
	/// <param name="replacementKey">Replacement key.</param>
	public void RemoveReplacementDialogue(string replacementKey) {
		this.replacementStrings.Remove(replacementKey);
	}

	/// <summary>
	/// Applies the replacement dialogue to a given string.
	/// </summary>
	/// <returns>The replacement dialogue.</returns>
	/// <param name="lineToCheck">Line to check.</param>
	protected string ApplyReplacementDialogue(string lineToCheck) {
		MatchCollection replacementKeys = Regex.Matches(lineToCheck, REPLACEMENT_DIALOGUE_REGEX);
		string replaceValue = string.Empty;
		for (int i = 0; i < replacementKeys.Count; i++) {
			if (replacementStrings.TryGetValue(replacementKeys[i].Value, out replaceValue)) {
				lineToCheck = lineToCheck.Replace(string.Format(REPLACE_TAG_FORMATTER, replacementKeys[i].Value), replaceValue);
			}
		}

		return lineToCheck;
	}

	#endregion

	#region Events

	/// <summary>
	/// Calls the dialogue action encountered event.
	/// </summary>
	/// <param name="actionKey">Action key.</param>
	/// <param name="actionParam">Action parameter.</param>
	protected void CallOnDialogueActionEncountered(string actionKey, string actionParam) {
		if (this.OnDialogueActionEncountered != null) {
			this.OnDialogueActionEncountered(actionKey, actionParam);
		}
	}

	/// <summary>
	/// Workaround so children can call events.
	/// </summary>
	protected void CallOnContinuePressed() {
		if (this.OnContinuePressed != null) {
			this.OnContinuePressed();
		}
	}

	/// <summary>
	/// Workaround to call DialogueDisplayOver from children of this class.
	/// </summary>
	protected void CallDialogueDisplayOver() {
		if (this.DialogueDisplayOver != null) {
			this.DialogueDisplayOver();
		}
	}

	/// <summary>
	/// Calls the option selected event if it's non-null.
	/// </summary>
	/// <param name="key">Key of the option.</param>
	/// <param name="option">Option string selected.</param>
	protected void CallOptionSelected(string key, string option) {
		if(this.OnOptionSelected != null) {
			this.OnOptionSelected(key, option);
		}
	}

	#endregion
}
