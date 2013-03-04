using System;

namespace AssemblyCSharp.Grid
{
	public class GridLocation
	{
		public Coordinate coord;
		public bool occupied = false;
		public bool visited = false;
		public GridLocation parent = null;
		private int distance;
		
		public int x { get { return coord.x; } set { coord.x = value; } } 		
		public int y { get { return coord.y; } set { coord.y = value; } } 
		
		public int Distance { 
			get { return distance; } 
			set { distance = distance > value ? value : distance; }
		}
		
		public GridLocation(int x, int y, bool occupied) {
			this.coord = new Coordinate(x, y);
			this.occupied = occupied;
		}
		
		// Resets the Grid location for pathfinding
		public void resetJourney() {
			visited = false;
			parent = null;
		}
	}
}

