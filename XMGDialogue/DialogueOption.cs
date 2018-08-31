using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace XMGDialogue {

	public class DialogueOption : MonoBehaviour {
		
		#region Events

		///<summary>
		/// Delegate for getting a response back from this dialogue option.
		/// </summary>
		/// <param name="actionKey">The key of the action.</param>
		/// <param name="action">The action associated.</param>
		public delegate void DialogueButtonDelegate(string key, string action);

		///<summary>
		/// Delegate for getting the button pressed event 
		/// </summary>
		public delegate void DialogueOptionDelegate(DialogueOption option);

		/// <summary>
		/// Called when the dialogue button is pressed.
		/// </summary>
		public event DialogueOptionDelegate OnButtonPressed = null;

		#endregion

		#region Button Data

		[SerializeField]
		protected Button optionSelector = null;

		/// <summary>
		/// The text displayed on the button.
		/// </summary>
		[SerializeField]
		protected Text buttonText = null;

		protected string optionNode = string.Empty;
		/// <summary>
		/// The action set for this dialogue option.
		/// </summary>
		public string OptionNode {
			get {
				return this.optionNode;
			}
		}

		protected string optionKey = string.Empty;
		/// <summary>
		/// The key for this dialogue option.
		/// </summary>
		public string OptionKey {
			get {
				return this.optionKey;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="DialogueOption"/> is interactable.
		/// </summary>
		public bool Interactable {
			get {
				return this.optionSelector.interactable;
			}
			set {
				this.optionSelector.interactable = value;
			}
		}

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Check serialized variables.
		/// </summary>
		protected virtual void Awake() {
			if (this.optionSelector == null) {
				Debug.LogError("Option selector is null on " + this.name);
			}

			if (this.buttonText == null) {
				Debug.LogError("Button text is null on " + this.name);
			}
		}

		#endregion

		#region Logic

		/// <summary>
		/// Sets up the button for use.
		/// </summary>
		/// <param name="key">The key for the button, this is also used as the label.</param>
		/// <param name="node">The name of the conversation node that this option should switch to.</param>
		public virtual void SetupButton(string key, string node) {
			this.buttonText.text = key;
			this.optionKey = key;
			this.optionNode = node;
			this.Interactable = true;
		}

		/// <summary>
		/// Handles the UI Button being pressed.
		/// </summary>
		public virtual void UIButtonPressed() {
			if (this.OnButtonPressed != null && this.Interactable) {
				this.OnButtonPressed(this);
			}

			this.Interactable = false;
		}

		/// <summary>
		/// Caller for the button press event so that child objects can access it.
		/// </summary>
		protected void OnButtonPressEvent() {
			if (this.OnButtonPressed != null) {
				this.OnButtonPressed(this);
			}
		}

		#endregion

	}
}
