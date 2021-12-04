using System;
using System.Collections.Generic;
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
}

[Hammer.EditorModel( "models/dev/playerstart_tint.vmdl" )]
[Hammer.EntityTool( "Player Spawn", "Terry Squat Simulator", "Spawn point for the player" )]
[Library( "tss_player_spawn" )]
public class TSSSpawn : AnimEntity
{
	[Property("spawn_type", "Spawn Type")] public SpawnType SpawnType { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
