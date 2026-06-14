using Godot;
using System;

public partial class HighlightSelector : Line2D
{
	[Export] private Battle _battle;
	
	private Vector2I lastTile = new Vector2I(0, 0);

	private Label displayPositionLabel;
	private Label displayTerrainLabel;
	private Label displayObstacleLabel;

	public override void _Ready()
	{
		displayPositionLabel = GetNode<Label>("%DisplayPositionLabel");
		displayTerrainLabel = GetNode<Label>("%DisplayTerrainLabel");
		displayObstacleLabel = GetNode<Label>("%DisplayObstacleLabel");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_battle == null || _battle.gameArea ==null)
		{
			return;
		}

		var currentTile = _battle.gameArea.getHoveredTile();
		if (currentTile != lastTile)
		{
			lastTile = currentTile;
			var tilePosition =_battle.gameArea.getGlobalFromTile(lastTile);
			Position=tilePosition;
			//GD.Print("local position " + Position + " tile position " + tilePosition + " currentTile " + currentTile);
			updateLabels();
		}
	}

	private void updateLabels()
	{
		displayPositionLabel.Text = $"({lastTile.X}, {lastTile.Y})";
		if (_battle == null || _battle.gameArea == null || _battle.gameArea.gameGrid == null)
			return;
		
		var cellData=_battle.GetGridData(lastTile);
		if (cellData == null)
			return;
		
		displayTerrainLabel.Text = cellData.terrain.ToString();
		displayObstacleLabel.Text = cellData.obstacle.ToString()=="NULL"? "":cellData.obstacle.ToString();
	}
}
