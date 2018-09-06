using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XMGDialogue {
	/// <summary>
	/// The Dialogue controller takes a dialogue tree and displays it through a Dialogue context.
	/// </summary>
	public class DialogueController {

		#region Events & Delegates

		/// <summary>
		/// Delegate used for dialogue actions. Implementing method is responsible for parsing the dialogue action params.
		/// </summary>
		public delegate void DialogueActionDelegate(string dialogueActionParams);

		/// <summary>
		/// Occurs when a terminating dialogue is over is advanced.
		/// </summary>
		public event Action DialogueOver = null;

		/// <summary>
		/// Event occurs when the dialogue context closes.
		/// </summary>
		public event Action OnContextClose = null;

		#endregion

		#region Consts

		/// <summary>
		/// This is a special string that signals an immediate end of the conversation.
		/// </summary>
		protected const string END_CONVO_STRING = "END";

		#endregion

		#region Data

		/// <summary>
		/// Conversation nodes that are a part of the loaded dialogue.
		/// </summary>
		protected Dictionary<string, ConversationNode> conversationNodeMap = null;

		/// <summary>
		/// The dialogue context that this controller is using for display.
		/// </summary>
		protected AbstractDialogueContext context = null;

		public AbstractDialogueContext Context {
			get {
				return this.context;
			}
		}

		/// <summary>
		/// The current node that is being processed.
		/// </summary>
		protected ConversationNode currentNode = null;

		/// <summary>
		/// The event action list.
		/// </summary>
		protected Dictionary<string, DialogueActionDelegate> eventActionList = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Constrctor that will first parse a serialized Yarn file and then load the file.
		/// </summary>
		/// <param name="serializedYarnFile">Serialized file to parse into a Yarn dialogue tree.</param>
		/// <param name="context">The dialogue context to display the Yarn dialogue in.</param>
		public DialogueController(string serializedYarnFile, AbstractDialogueContext context) {
			// Set context
			this.context = context;
			this.context.OnContinuePressed += this.AdvanceLine;
			this.context.OnOptionSelected += this.HandleOptionSelected;
			this.context.OnDialogueActionEncountered += this.HandleDialogueActionEvent;

			// Initialize action list.
			this.eventActionList = new Dictionary<string, DialogueActionDelegate>();

			this.LoadConversation(serializedYarnFile);
		}

		#endregion

		#region Finalization
		
		/// <summary>
		/// Cleans up the dialogue controller before it's dereferenced.
		/// </summary>
		public void FinishDialogue(Action dialogueCleanupOver = null) {
			// Remove events.
			this.context.OnContinuePressed -= this.AdvanceLine;
			this.context.OnOptionSelected -= this.HandleOptionSelected;

			// Clean up the registered actions.
			this.eventActionList.Clear();

			// Close the dialogue context.
			this.context.CloseContext(delegate() {
				if (this.OnContextClose != null) {
					this.OnContextClose();
				}

				if (dialogueCleanupOver != null) {
					dialogueCleanupOver();
				}
			});
		}

		#endregion

		#region Dialogue Actions

		/// <summary>
		/// Registers an action to this dialogue controller that will respond to a given tag.
		/// </summary>
		/// <param name="actionTag">Action tag.</param>
		/// <param name="actionDelegate">Action delegate.</param>
		public void RegisterDialogueAction(string actionTag, DialogueActionDelegate actionDelegate) {
			if (!this.eventActionList.ContainsKey(actionTag)) {
				this.eventActionList[actionTag] = actionDelegate;
			} else {
				this.eventActionList[actionTag] += actionDelegate;
			}
		}

		/// <summary>
		/// Removes the dialogue action.
		/// </summary>
		/// <param name="actionTagToRemove">Action tag to remove.</param>
		/// <param name="delegateToRemove">Delegate to remove.</param>
		public void RemoveDialogueAction(string actionTagToRemove, DialogueActionDelegate delegateToRemove) {
			if (this.eventActionList.ContainsKey(actionTagToRemove)) {
				this.eventActionList[actionTagToRemove] -= delegateToRemove;
			}
		}

		/// <summary>
		/// Handles a dialogue action event being called.
		/// </summary>
		/// <param name="actionTag">Action tag.</param>
		/// <param name="delegateParams">Action delegate parameters.</param>
		private void HandleDialogueActionEvent(string actionTag, string delegateParams) {
			if (this.eventActionList.ContainsKey(actionTag)) {
				this.eventActionList[actionTag](delegateParams);
			}
		}

		#endregion

		#region Replacement Dialogue

		/// <summary>
		/// Registers the replacement dialogue, this is a passthrough to the currently registered AbstractDialogueContext.
		/// </summary>
		/// <param name="replacementKey">The lookup key to replace.</param>
		/// <param name="replacementText">Replacement text to use.</param>
		public void RegisterReplacementDialogue(string replacementKey, string replacementText) {
			this.context.RegisterReplacementDialogue(replacementKey, replacementText);
		}

		/// <summary>
		/// Removes the replacement key from the replacement dictionary.
		/// </summary>
		/// <param name="replacementKey">Replacement key.</param>
		public void RemoveReplacementDialogue(string replacementKey) {
			this.context.RemoveReplacementDialogue(replacementKey);
		}

		#endregion

		#region Logic

		/// <summary>
		/// Loads a conversation string into this dialogue controller.
		/// </summary>
		/// <param name="serializedYarnFile">A serialized yarn file.</param>
		public void LoadConversation(string serializedYarnFile) {
			List<object> serializedConversationNodes = MiniJSON.Json.Deserialize(serializedYarnFile) as List<object>;
			Debug.Assert(serializedConversationNodes != null, "Must have at least one conversation node in the provided data.");
			
			// Add a conversation node for each part of the dialogue tree.
			this.conversationNodeMap = new Dictionary<string, ConversationNode>(serializedConversationNodes.Count);

			for (int i = 0; i < serializedConversationNodes.Count; i++) {
				ConversationNode newNode = new ConversationNode(serializedConversationNodes[i] as Dictionary<string, object>);
				this.conversationNodeMap[newNode.Title] = newNode;
			}
		}

		/// <summary>
		/// Loads the conversation specified in the dialouge tree and automatically starts a given conversation.
		/// </summary>
		/// <param name="dialogueTree">Dialogue tree.</param>
		/// <param name="conversationNode">Conversation node.</param>
		public void LoadConversation(string serializedYarnFile, string conversationNode) {
			this.LoadConversation(serializedYarnFile);
			this.StartConversationNode(conversationNode);
		}

		/// <summary>
		/// This grabs the conversation node by title or returns null if that node does not exist.
		/// </summary>
		/// <param name="conversation">Unparsed string that represents the conversation.</param>
		public void StartConversationNode(string conversationNode) {
			if (this.conversationNodeMap.ContainsKey(conversationNode)) {
			// Grab the start node, set it's point to it's first line and then run though.
			this.currentNode = this.conversationNodeMap[conversationNode];

			this.currentNode.ResetConversation();

			this.context.NewConversationNode(this.currentNode);
			this.context.DisplayDialogue(this.currentNode.GetCurrentLine());
			} else {
				Debug.LogError("Can't find node " + conversationNode);
			}
		}

		/// <summary>
		/// Advances the next line and displays the dialogue and choices.
		/// </summary>
		private void AdvanceLine() {
			if (this.currentNode.HasNextLine()) {
				this.context.DisplayDialogue(this.currentNode.GetNextLine());
			} else {
				if (this.DialogueOver != null) {
					this.DialogueOver();
				}
			}
		}

		/// <summary>
		/// Handles an option being selected.
		/// </summary>
		/// <param name="key">Key for the action.</param>
		/// <param name="newNodeName">The new node to look up.</param>
		private void HandleOptionSelected(string key, string newNodeName) {
			if (newNodeName == END_CONVO_STRING) {
				if (this.DialogueOver != null) {
					this.DialogueOver();
				}
				return;
			}

			ConversationNode newNode = null;
			if (!this.conversationNodeMap.TryGetValue(newNodeName, out newNode)) {
				Debug.LogError(string.Format("{0} is not a conversation node name nor a special string.", newNodeName));
				return;
			} else {
				this.currentNode = newNode;
				this.context.DisplayDialogue(this.currentNode.GetCurrentLine());
			}
		}

		#endregion

		#region Object

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="DialogueController"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="DialogueController"/>.</returns>
		public override string ToString () {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine("[DialogueController] - Nodes:");
			foreach (string nodeID in this.conversationNodeMap.Keys) {
				sb.AppendLine(nodeID); 
			}

			return sb.ToString();
		}

		#endregion
	}
}
