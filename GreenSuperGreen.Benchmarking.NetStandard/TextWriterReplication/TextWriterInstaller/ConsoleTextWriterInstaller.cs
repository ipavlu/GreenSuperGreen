namespace GreenSuperGreen.TextWriterReplication
{
	public class ConsoleTextWriterInstaller : ATextWriterInstaller
	{
		public ConsoleTextWriterInstaller(ITextWriter ancestor, ITextWriterReplicatorManager replicatorManager, DisposeAncestor disposeAncestor = DisposeAncestor.No)
			: base(ancestor, replicatorManager, System.Console.Out, disposeAncestor)
		{
		}

		protected override void Install()
		{
			base.Install();
			System.Console.SetOut(TextWriterReplicator);
		}

		protected override void Uninstall()
		{
			System.Console.SetOut(TextWriter);
			base.Uninstall();
		}

		~ConsoleTextWriterInstaller() => Dispose(false);
	}
}