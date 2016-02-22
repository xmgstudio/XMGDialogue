#XMG Dialogue System

Dialogue system developed by XMG Studio. 

If you use this drop us a line or make a pull request adding your game to the list at the bottom of this readme.

There is a tutorial for setting up and writing for this system [here](http://xmgstudio.tumblr.com/post/139791670481/xmgdialoguetutorial)

##Quickstart

Uses json files created the Yarn dialogue editor found [here](https://github.com/InfiniteAmmoInc/Yarn).

### Nodes

![Nodes](http://i.imgur.com/qF4gXl8.png)

This dialogue tree translates to a file that looks like: 

![Data](http://i.imgur.com/0IqzM3L.png)

Conversation Nodes are the basic building block of Yarn data. They are made up of three parts, the Title, a list of Tags and the Body. 

A node requires a Title to be given to it and that Title must be unique within the file.

### Tags

Tags are an optional way to add meta data to a conversation node. Tags that are read by the XMGDialogue system must be formatted like:

	tag[comma, separated, list]

The tag field can be populated by multiple tag blocks which should be separated by commas eg:

	tag1[tag, data], tag2[tag2, data2]

These tags can be accessed through the ConversationNode.GetTagValues(string tag) which will return a string array containing the tags for the specified tag or an empty array if the tag can't be found.

### Body

The body of an XMGDialogue formatted Yarn script is made of separate DialogueLines. Each line is composed of three parts which are each technically optional.

#### A Line
	Name: Dialogue Line | options([[Option One|Option1]], [[Option Two|Option2]])

The three parts of a Line are:
1. The SpeakerID. This consists of the part of the name before the first colon in a line. It is accessed from DialogueLine.SpeakerID and can be used to look up display data about the speaker from your own data.
2. The line. This consists of all characters between the first colon and the meta-data separator character | (pipe).
3. Meta data. This consists of either options or actions. They are formatted in a specific way, anything after the metadata separator character | (pipe) will be passed to the parser and checked.

### Dialogue Replacement

Dialogue replacement works by looking for replacement keys inside curly braces eg: 

	I'll tell you a story for {PRICE} {CURRENCY}.

If you registered PRICE and CURRENCY as replacement dialogue using the AbstractDialogueController's RegisterReplacementDialogue method.

	controller.RegisterReplacementDialogue("PRICE", price);
	controller.RegisterReplacementDialogue("CURRENCY", (price > 1) ? "Bucks" : "Buck");

This block of code would replace the price with whatever the current value of the price variable is and would correctly pluralize it based on the value that price has.

### Options

Options are a listing of what responses a player can have to a given line of dialogue and what new ConversationNode they go to. They are formatted as such: 

	options() - This has to enclose the list of options

A single option is formmatted like:

[[OptionKey|DestinationNode]]

Both optionKey and DestinationNode are strings. Internally they are defined as a List>; and can be accessed via DialogueLine.parsedOptions. How these are displayed is up to the implemenation of the DialogueContext. Generally I have used the OptionKey as the display value for the option (ie: what is written on the button) but it could easily be a lookup key or anything else. The DestinationNode is required to match the title of another Node in the file. In Yarn this will draw an arrow pointing from the node with the option to the node with the correct title, when the event associated with an option being selected is called the DialogueController will transition to the new dialogue node.

There is one special case where you can replace DestinationNode with END (all caps) as in [[Goodbye|END]] and that will immediately end the conversation in the same way as running to the end of a dialogue tree would.

You can associate multiple options by putting a comma separated list of correctly formatted options into the options enclosure:
	options([[Option One|Option1]], [[Option Two|Option2]])

### Actions

Actions are event tags that are tied to DialogueLines. They can be tied to in game events through the DialogueController and can be executed based on the DialogueLine that is displayed. They are formatted like:

	actions() - This has to enclose the list of actions

A single action is formatted like:

	[actionKey|actionParam]

or just 

	[actionKey]

Both actionKey and actionParam are just strings, the actionParam is optional and if not specified an empty string will be passed to the delegate event handler. The actions of a line can be accessed similarly to the options through DialogueLine.lineActions which is also a List>.

This will, when the correct method in the AbstractDialogueContext is called, call any delegates assocated with the key "actionKey" and pass the string actionParam to them. Like options you can have multiple actions per line by formatting them together inside the actions enclosure as a comma separated list.

	actions([action1|params], [action2|otherParams])

##Games using this Dialogue System

### XMG Studio
[![Gastrobots](http://i.imgur.com/90TM2r7.png)](gastrobots.xmg.com) [![Giants](https://i.imgur.com/9BegGQL.png)](giants.xmg.com)
