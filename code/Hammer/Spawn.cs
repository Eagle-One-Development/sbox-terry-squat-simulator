using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public enum SpawnType
{
	Squat,
	Run,
	Punch,
	Yoga,
	Heaven,
	Void,
	Nature,
	NatureExercise
}

[Hammer.EditorModel( "models/dev/playerstart_tint.vmdl" )]
[Display( Name = "Player Spawn", Description = "Spawn point for the player" ), Category( "Terry Squat Simulator" )]
[Library( "tss_player_spawn" )]
public class TSSSpawn : AnimEntity
{
	[Property( "spawn_type", "Spawn Type" )] public SpawnType SpawnType { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
