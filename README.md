**Arc Reaction** is a minimal framework (more of a library) for writing RESTful web applications. It was inspired by Paul Graham's Arc programming language.

It aims to be the ideal library for web developers that understand HTTP, HTML, and C# extremely well.

ArcReaction's answer to the [Arc challenge](http://paulgraham.com/arcchallenge.html).

    using ArcReaction;
	
	public sealed class AppRouter : Router
	{
		public override AppState GetRoot(HttpContextEx context)
		{
			return new Form(c => 
				new A("Click here", new P("You said " + c["said"])), 
				new TextInput("said"), 
				new Submit());
		}
	}
