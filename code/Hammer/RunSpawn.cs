using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
[Hammer.EntityTool( "Run Spawn", "Terry Squat Simulator", "Spawn point for running on the treadmill at the gym" )]
[Library( "tss_run_spawn" )]
public class RunSpawn : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
