using Godot;
using System;

public partial class HighlightSelector : Line2D
{
	[Export] private GameArea gameArea;
	
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
		if (gameArea == null)
		{
			return;
		}

		var currentTile = gameArea.getHoveredTile();
		if (currentTile != lastTile)
		{
			lastTile = currentTile;
			var tilePosition =gameArea.getGlobalFromTile(lastTile);
			Position=tilePosition;
			//GD.Print("local position " + Position + " tile position " + tilePosition + " currentTile " + currentTile);
			updateLabels();
		}
	}

	private void updateLabels()
	{
		displayPositionLabel.Text = $"({lastTile.X}, {lastTile.Y})";
		if (gameArea == null || gameArea.gameGrid == null)
			return;
		
		var cellData=gameArea.gameGrid.getGridDB(lastTile);
		if (cellData == null)
			return;
		
		displayTerrainLabel.Text = cellData.terrain.ToString();
		displayObstacleLabel.Text = cellData.obstacle.ToString()=="NULL"? "":cellData.obstacle.ToString();
	}
}
