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
	public partial class ExerciseComponent : EntityComponent<TSSPlayer>
	{
		public virtual void Simulate( Client client ) { }
		public virtual void Initialize() { }
		public virtual void Cleanup() { }

		[Net]
		public bool Active { get; set; }

		[Net]
		public Exercise ExerciseType { get; set; }
	}
}
