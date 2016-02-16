using MiniJSON;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Abstract dialogue manager 
/// </summary>
public abstract class AbstractDialogueManager : MonoBehaviour {

	#region Data

	/// <summary>
	/// A list of all the dialogue contexts that this dialogue manager supports.
	/// </summary>
	protected List<AbstractDialogueContext> dialogueContexts = null;
	
	/// <summary>
	/// The conversation file that is currently loaded.
	/// </summary>
	protected List<object> conversationFile = null;

	#endregion

	#region Dialogue manager

	/// <summary>
	/// Initialize this dialogue manager.
	/// </summary>
	public abstract void Initialize();

	/// <summary>
	/// Find the conversation with the correct name and create a Dialogue Controller to manage that conversation.
	/// </summary>
	/// <param name="jsonConversation">The data of the conversation to start.</param>
	/// <param name="context">The dialogue context to attatch to the controller.</param>
	public abstract DialogueController StartConversation(TextAsset jsonConversation, AbstractDialogueContext context);

	#endregion
}
