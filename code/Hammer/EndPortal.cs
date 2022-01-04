using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/terry_buff/terry_buff.vmdl" )]
[Hammer.EntityTool( "End Portal", "Terry Squat Simulator", "Spawn point for the player" )]
[Library( "tss_end_portal" )]
public class EndPortal : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
