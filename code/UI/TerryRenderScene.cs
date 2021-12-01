using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox;
using Sandbox.UI.Construct;

namespace TSS.UI
{
	public class TerryRenderScene : Panel
	{
		/// <summary>
		/// The scene where we will put the terry
		/// </summary>
		private ScenePanel scene;

		/// <summary>
		/// The angle of our camera
		/// </summary>
		Angles CamAngles = new( 0.0f, 0.0f, 0.0f );
		/// <summary>
		/// Distance of the camera from Terry
		/// </summary>
		float CamDistance = 50;
		/// <summary>
		/// The position of the camera in world space
		/// </summary>
		Vector3 CamPos;

		/// <summary>
		/// A reference to our terry object
		/// </summary>
		public AnimSceneObject Terry;

		/// <summary>
		/// The time since the intro started
		/// </summary>
		public TimeSince TimeSinceIntroStarted;

		/// <summary>
		/// A reference to the spotlight
		/// </summary>
		public SpotLight Spot;

		public TerryRenderScene()
		{
			//Set some style properties
			Style.FlexWrap = Wrap.Wrap;
			Style.JustifyContent = Justify.Center;
			Style.AlignItems = Align.Center;
			Style.AlignContent = Align.Center;

			CreateScene();
			
		}

		/// <summary>
		/// Creates a crying terry in the middle of the screen
		/// </summary>
		public void CreateScene()
		{

			scene?.Delete();
			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				
				Terry = new AnimSceneObject( Model.Load( "models/terry/terry.vmdl" ), Transform.Zero );
				Terry.SetAnimBool( "Crying", true );
				
				

				Spot = new SpotLight((Vector3.Up * 100f) + Vector3.Forward * 20f,Color.White);
				Spot.Rotation = Rotation.LookAt( Terry.Position - Spot.Position );
				Spot.Falloff = 0f;


				
				scene = Add.ScenePanel( SceneWorld.Current, CamPos, Rotation.From( CamAngles ), 45 );
				scene.Style.Width = Length.Fraction( 1f );
				scene.Style.Height = Length.Fraction(1f);
				scene.Style.Opacity = 1f;
			}
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();
			CreateScene();
		}

		public override void Tick()
		{
			base.Tick();
			CamPos = Vector3.Up * 48f + CamAngles.Direction * -CamDistance;
			CamAngles = new( 0.0f, 180.0f, 0.0f );
			CamDistance = 300f;

			scene.CameraPosition = CamPos;
			scene.CameraRotation = Rotation.From( CamAngles );

			Spot.LightColor = Color.Black;
			if (TimeSinceIntroStarted > 3f )
			{
				float f = ((TimeSinceIntroStarted - 3f) / 5f).Clamp( 0, 1f );
				f = MathF.Pow( f, 3f );
				Spot.LightColor = new Color(1f * f,1f * f,1f * f,1f);
				
			}

			float prog = TimeSinceIntroStarted / 23.161f;

			CamDistance = MathX.LerpTo( 300f, 150f, prog );



			Terry?.Update( Time.Delta );
		}
	}
}
