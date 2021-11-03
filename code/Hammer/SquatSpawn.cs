using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
[Hammer.EntityTool( "Squat Spawn", "Terry Squat Simulator", "Spawn point for squats at the gym" )]
[Library( "tss_squat_spawn" )]
public class SquatSpawn : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
