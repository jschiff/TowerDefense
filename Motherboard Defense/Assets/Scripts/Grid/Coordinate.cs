using System;

namespace AssemblyCSharp.Grid
{
	public class Coordinate
	{
		public int x;
		public int y;
		
		public Coordinate(int x, int y) {
			this.x = x;
			this.y = y;
		}
		
		public override string ToString() {
			return 	"[" + x + ", " + y + "]";
		}
		
		public override bool Equals (object other){
			Coordinate otherCoord = (Coordinate) other;
			return otherCoord != null && this.x == otherCoord.x && this.y == otherCoord.y;
		}
		
		public override int GetHashCode() {
			return (x.GetHashCode() + y.GetHashCode()).GetHashCode();
		}
	}
}

