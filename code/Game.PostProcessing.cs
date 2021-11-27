using Sandbox;

namespace TSS
{

	public class VHSPostProcess : MaterialPostProcess
	{
		public VHSPostProcess() : base( "materials/default/post_process.vmat" ) { }

		public bool Enabled;
	}
}
