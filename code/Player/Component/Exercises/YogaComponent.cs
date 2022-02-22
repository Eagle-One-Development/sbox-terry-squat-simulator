using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TSS;
using TSS.UI;


namespace TSS
{
	public partial class YogaComponent : ExerciseComponent
	{
		#region Members
		/// <summary>
		/// The time since we last did a yoga pose.
		/// TODO: Review for redundancy with TimeSinceExerciseStopped
		/// </summary>
		[Net]
		public TimeSince TimeSinceYoga { get; set; }

		/// <summary>
		/// The current 'position' our player is posing in for yoga
		/// </summary>
		[Net]
		public int CurrentYogaPosition { get; set; } = -1;

		#endregion

		public override void Initialize()
		{
			ExerciseType = Exercise.Yoga;
		}



		public override void Simulate( Client client )
		{
			base.Simulate( client );
			var cam = Entity.CameraMode as TSSCamera;
			SimulateYoga( cam );
		}

		public override void Cleanup()
		{
			CurrentYogaPosition = 0;
		}

		/// <summary>
		/// Simulate the yoga exercise state
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateYoga( TSSCamera cam )
		{
			Entity.SetAnimParameter( "YogaPoses", CurrentYogaPosition );
			Entity.SetAnimParameter( "b_grounded", CurrentYogaPosition == 0 );

			if ( cam == null )
			{
				return;
			}

			if ( TimeSinceYoga > 3.05f )
			{
				TimeSinceYoga = 0;

				if ( Entity.IsClient )
				{
					// Prevent duplicate yoga qt panels appearing when alt-tabbed.
					if ( Sandbox.Entity.All.OfType<YogaQT>().Count() == 0 )
					{
						var pt = new YogaQT();
						pt.Player = Entity;
					}
				}
			}
		}



	}
}
