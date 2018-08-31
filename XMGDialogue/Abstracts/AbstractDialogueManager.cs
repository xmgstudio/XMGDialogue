using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XMGDialogue {

	/// <summary>
	/// Abstract dialogue manager 
	/// </summary>
	public abstract class AbstractDialogueManager : MonoBehaviour {

		#region Data

		/// <summary>
		/// A list of all the dialogue contexts that this dialogue manager supports.
		/// </summary>
		[SerializeField]
		protected List<AbstractDialogueContext> dialogueContexts = null;

		#endregion

		#region Dialogue manager

		/// <summary>
		/// Initialize this dialogue manager.
		/// </summary>
		public abstract void Initialize();

		/// <summary>
		/// Find the conversation with the correct name and create a Dialogue Controller to manage that conversation in the specified abstract dialogue context.
		/// </summary>
		/// <param name="serializedConversation">The data of the conversation to start.</param>
		/// <param name="context">The dialogue context to attatch to the controller.</param>
		public abstract DialogueController StartConversation(TextAsset serializedConversation, AbstractDialogueContext context);

		#endregion
	}
}
