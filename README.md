**Arc Reaction** is a minimal framework (more of a library) for writing RESTful web applications. It was inspired by Paul Graham's Arc programming language.

ArcReactions answer to the Arc challenge:

    using ArcReaction;
	
	public ControlPoint GetRoot(HttpContextEx context)
	{
		return new Form(c => new A("Click here", new P("You said " + c["said"])), new TextInput("said"), new Submit());
	}
