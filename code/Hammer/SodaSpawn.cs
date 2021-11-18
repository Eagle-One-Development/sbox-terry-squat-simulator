using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/editor/playerstart.vmdl" )]
[Hammer.EntityTool( "Soda Spawn", "Terry Squat Simulator", "Spawn point for squats at the gym" )]
[Library( "tss_soda_spawn" )]
public class SodaSpawn : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
