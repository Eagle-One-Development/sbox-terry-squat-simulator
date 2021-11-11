using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox;
using Sandbox.UI.Construct;

public class UIPanel : Panel
{
	public UIPanel()
	{
		StyleSheet.Load( "/ui/UIPanel.scss" );
	}

	public override void Tick()
	{
		base.Tick();
		if(Local.Pawn is TSSPlayer p )
		{
		 if(p.Camera is DevCamera )
			{
				SetClass( "inactive", true );
			}
			else
			{
				SetClass( "inactive", false);
			}
		}
	}
}
