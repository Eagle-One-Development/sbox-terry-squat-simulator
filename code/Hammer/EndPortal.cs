using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Hammer.EditorModel( "models/terry_buff/terry_buff.vmdl" )]
[Display( Name = "End Portal", Description = "Spawn point for the player" ), Category( "Terry Squat Simulator" )]
[Library( "tss_end_portal" )]
public class EndPortal : AnimEntity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
}
