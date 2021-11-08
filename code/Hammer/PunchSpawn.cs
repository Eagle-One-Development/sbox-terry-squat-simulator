using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
[Hammer.EntityTool( "Punch Spawn", "Terry Squat Simulator", "Spawn point for punching something at the gym" )]
[Library( "tss_punch_spawn" )]
public class PunchSpawn : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
