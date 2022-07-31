using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[EditorModel( "models/editor/playerstart.vmdl" )]
[Display( Name = "Soda Spawn", Description = "Spawn point for squats at the gym" ), Category( "Terry Squat Simulator" )]
[Library( "tss_soda_spawn" )]
public class SodaSpawn : AnimatedEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
