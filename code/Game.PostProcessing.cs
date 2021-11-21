
using Sandbox;

namespace TSS
{
	public partial class TSSGame : Game
	{

		protected Material postProcessingMat = LoadPostProcessingTexture();

		private static Material LoadPostProcessingTexture()
		{
			if ( Host.IsClient )
			{
				return Material.Load( "materials/default/post_process.vmat" );
			}

			return null;
		}

		[Event( "render.postprocess" )]
		protected void HandlePostProcessing()
		{
			if ( postProcessingMat == null ) return;

			Render.CopyFrameBuffer( false );
			Render.Material = postProcessingMat;
			Render.DrawScreenQuad();
		}
	}
}
