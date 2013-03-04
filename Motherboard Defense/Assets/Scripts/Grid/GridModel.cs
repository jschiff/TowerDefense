using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Grid;
using System.Text;
using System;
using System.IO;

namespace AssemblyCSharp{
	// Represents the "Model" in the MVC architecture for the playing grid
	public class GridModel{
		public int width { get; private set; }
		public int length { get; private set; }
		private Dictionary<GridLocation, LinkedList<GridLocation>> adjacencyList;// Adjacency list representation of the graph
		private GridLocation[,] matrix; // 2D array representation of the grid
		
		// Create new empty gridmodel from a height and a width
		public GridModel(int width, int height) : this(new bool[width, height]) {
		}
		
		public GridModel(bool[,] model) {
			width = model.GetLength(0);
			length = model.GetLength(1);
			matrix = new GridLocation[width, length];
			
			for(int i = 0; i < width; i++) {
				for(int j = 0; j < length; j++) {
					matrix[i,j] = new GridLocation(i, j, model[i, j]);
				}
			}
			
			adjacencyList = buildAdjacencyList(matrix);
		}
		
		public GridModel(GridModel cloneMe) {
			width = cloneMe.width;
			length = cloneMe.length;
			this.matrix = new GridLocation[cloneMe.width, cloneMe.length];
			for(int i = 0; i < width; i++) {
				for(int j = 0; j < length; j++) {
					matrix[i,j] = new GridLocation(i, j, cloneMe.matrix[i, j].occupied);
				}
			}
			adjacencyList = buildAdjacencyList(this.matrix);
		}
		
		// Returns whether the grid location at the specified coordinates is occupied
		public bool isOccupied(int x, int y) {
			return matrix[x, y].occupied;
		}
		
		// Set or remove an obstacle
		public void setIsOccupied(int x, int y, bool val) {
			matrix[x, y].occupied = val;
		}
		
		// Set or remove an obstacle
		public void setIsOccupied(Coordinate c, bool val) {
			setIsOccupied(c.x, c.y, val);
		}
		
		// Set an obstacle
		public void setOccupied(int x, int y) {
			setIsOccupied(x, y, true);	
		}
		
		// Set an obstacle
		public void setOccupied(Coordinate c) {
			setIsOccupied(c, true);	
		}
		
		// Get a path to a single destination
		public LinkedList<Coordinate> getPath(Coordinate fromHere, Coordinate toHere) {
			return getPath(fromHere, new Coordinate[]{toHere});	
		}
		
		// Returns true if the source is the same as one of the values in the destination
		private bool isOneOf(GridLocation source, GridLocation[] destinations) {
			foreach(GridLocation dest in destinations) {
				if(source.Equals(dest)) {
					return true;	
				}
			}
			
			return false;	
		}
		
		private GridLocation[] validDestinations(Coordinate[] input) {
			LinkedList<GridLocation> list = new LinkedList<GridLocation>();
			
			foreach(Coordinate c in input) {
				GridLocation gl = locAt(c);
				if(!gl.occupied) {
					list.AddLast(gl);
				}
			}
			
			return (GridLocation[]) toArray<GridLocation>(list);
		}
			
		// Return the shortest path from one point on the grid to one of several destinations, or null if no path exists (Djikstra's)
		public LinkedList<Coordinate> getPath(Coordinate fromHere, Coordinate[] toHere) {
			resetJourney();
			GridLocation source = locAt(fromHere);
			GridLocation[] destinations = getAvailableLocs(locsAt(toHere));
			
			if(!isInRange(source) || source.occupied || destinations.Length == 0) {
				return null;
			}
			
			LinkedList<GridLocation> searchQueue = new LinkedList<GridLocation>();
			HashSet<GridLocation> visited = new HashSet<GridLocation>();
			searchQueue.AddLast(source);
			visited.Add(source);
			
			// If the queue runs out, there is no path!
			while(searchQueue.Count > 0) {
				GridLocation current = searchQueue.First.Value;
				searchQueue.RemoveFirst();
				LinkedList<GridLocation> neighbors = adjacencyList[current];
				
				// Check if any of the neighbors are the search target
				foreach(GridLocation neighbor in neighbors) {
					// Enqueue all of this node's children.
					if(!isOccupied(neighbor) && !visited.Contains(neighbor)) {
						visited.Add(neighbor);
						neighbor.Distance = current.Distance + 1;
						neighbor.parent = current;
						
						// Yay we made it!
						if(isOneOf(neighbor, destinations)) {
							return tracePathFrom(neighbor);
						}
						else {
							searchQueue.AddLast(neighbor);	
						}
					}
				}
			}
			
			return null;
		}
		
		// build path in reverse form destination
		private LinkedList<Coordinate> tracePathFrom(GridLocation current) {
			LinkedList<Coordinate> path = new LinkedList<Coordinate>();
			
			while(current != null) {
				path.AddFirst(current.coord);
				current = current.parent;
			}
			
			return path;	
		}
		
		// Reset the DFS.
		private void resetJourney() {
			foreach(GridLocation loc in matrix) {
				loc.resetJourney();	
			}
		}
		
		private GridLocation[] locsAt(Coordinate[] locations) {
			GridLocation[] ret = new GridLocation[locations.Length];
			
			for(int i = 0; i < ret.Length; i++) {
				ret[i] = locAt(locations[i]);
			}
			
			return ret;
		}
		
		// Get tje grid location at a particular coordinate
		private GridLocation locAt(Coordinate location) {
			return matrix[location.x, location.y];	
		}
		
		// Tests whether a grid location is in range
		private bool isInRange(GridLocation loc) {
			return isInRange(loc.coord);
		}
		
		// Tests whether a set of coordinates is in range
		private bool isInRange(Coordinate coord) {
			return !(coord.x < 0 || coord.x >= width || coord.y < 0 || coord.y >= length);
		}
		
		// Return a list of locations which are in range from the input
		private GridLocation[] getInRangeLocs(GridLocation[] input) {
			LinkedList<GridLocation> inRange = new LinkedList<GridLocation>();
			
			foreach(var loc in input) {
				if(isInRange(loc)) {
					inRange.AddLast(loc);	
				}
			}
			
			return (GridLocation[]) toArray<GridLocation>(inRange);
		}
		
		// Tests whether any of the given locations are in range
		private bool isAnyInRange(GridLocation[] locations) {
			foreach(var loc in locations) {
				if(isInRange(loc)) {
					return true;	
				}
			}
			
			return false;	
		}
		
		// Tests whether a coordinate is occupied
		public bool isOccupied(Coordinate coord) {
			return locAt(coord).occupied;	
		}
		
		private bool isOccupied(GridLocation loc) {
			return isOccupied(loc.coord);	
		}
		
		// Return a list of locations which are unoccupied from the input
		private GridLocation[] getUnoccupiedLocs(GridLocation[] input) {
			LinkedList<GridLocation> unOcc = new LinkedList<GridLocation>();
			
			foreach(var loc in input) {
				if(!isOccupied(loc)) {
					unOcc.AddLast(loc);	
				}
			}
			
			return (GridLocation[]) toArray<GridLocation>(unOcc);
		}
		
		// Tests whether ALL of the given locations are occupied
		private bool areAllOccupied(GridLocation[] locs) {
			foreach(var loc in locs) {
				if(!isOccupied(loc)) {
					return false;
				}
			}
			
			return true;	
		}
		
		// Get array of locations which are both in range and unoccupied from the input
		private GridLocation[] getAvailableLocs(GridLocation[] input) {
			GridLocation[] inRange = getInRangeLocs(input);
			return getUnoccupiedLocs(inRange);
		}
				
		// Simulate whether a path will still be possible if a particular block was occupied
		public LinkedList<Coordinate> simulate(Coordinate test, Coordinate fromC, Coordinate toC) {
			return simulate(test, fromC, new Coordinate[]{toC});
		}
		
		// Simulate whether a path will still be possible if a particular block was occupied
		public LinkedList<Coordinate> simulate(Coordinate test, Coordinate fromC, Coordinate[] toC) {
			GridModel testModel = new GridModel(this);
			testModel.setOccupied(test);
			return testModel.getPath(fromC, toC);
		}
		
		// Build adjacency list representation based on matrix representation
		private Dictionary<GridLocation, LinkedList<GridLocation>> buildAdjacencyList(GridLocation[,] matrix) {
			Dictionary<GridLocation, LinkedList<GridLocation>> ret = new Dictionary<GridLocation, LinkedList<GridLocation>>();
			
			foreach(GridLocation loc in matrix) {
				ret.Add(loc, getNeighbors(loc, matrix));
			}
			
			return ret;
		}
		
		// Get all the neighbors of a particular grid location.
		private LinkedList<GridLocation> getNeighbors(GridLocation loc, GridLocation[,] matrix) {
			LinkedList<GridLocation> ret = new LinkedList<GridLocation>();
			
			// up
			if(loc.y < length - 1) {
				ret.AddLast(matrix[loc.x, loc.y + 1]);
			}
			
			// down
			if(loc.y > 0) {
				ret.AddLast(matrix[loc.x, loc.y - 1]);	
			}
			
			// left
			if(loc.x > 0) {
				ret.AddLast(matrix[loc.x - 1, loc.y]);
			}
			
			// right
			if(loc.x < width - 1) {
				ret.AddLast(matrix[loc.x + 1, loc.y]);	
			}
			
			return ret;
		}
		
		// "Draw" as a 2d char array
		private char[,] draw() {
			char[,] drawing = new char[width, length];
			
			for(int i = 0; i < width; i++) {
				for(int j = 0; j < length; j++) {
					drawing[i,j] = matrix[i,j].occupied ? '1' : '0';
				}
			}
			
			return drawing;
		}
		
		private string getStringRepresentation(char[,] drawing) {
			StringBuilder builder = new StringBuilder();
			
			for(int j = 0; j < length; j++) {
				for(int i = 0; i < width; i++) {
					builder.Append(drawing[i,j]);			
				}
				builder.AppendLine();
			}
			
			return builder.ToString();
		}
		
		public string getStringRepresentation() {
			char[,] drawing = draw();
			return getStringRepresentation(drawing);
		}
		
		public string drawPath(LinkedList<Coordinate> path) {
			char[,] drawing = draw();
			
			// Replace Path with 'P's
			foreach(Coordinate c in path) {
				drawing[c.x, c.y] = 'P';	
			}
			
			return getStringRepresentation(drawing);	
		}
		
		// Testing
		public static string test() {
			GridModel model = new GridModel(fromString(testGrid));
			Coordinate fromC = new Coordinate(0, 0);
			Coordinate toC = new Coordinate(model.width - 1, model.length - 1);
			LinkedList<Coordinate> path = model.getPath(fromC, toC);
			
			return model.drawPath(path);
		}
		
		// generate a bool grid/matrix from a well formed string of 1s and zeros
		// This is a debug function and should have no use in the final product
		public static bool[,] fromString(string input) {
			StringReader reader = new StringReader(input);
			string line = reader.ReadLine();
			int lineCount = 0;
			int lineLength = 0;
			while(line != null) {
				lineCount++;
				lineLength = line.Length > lineLength ? line.Length : lineLength;
				line = reader.ReadLine();
			}
			
			bool[,] ret = new bool[lineLength, lineCount];
			
			// reset the stream, and now actually read in
			reader = new StringReader(input);
			for(int j = 0; j < lineCount; j++) {
				line = reader.ReadLine();
				for(int i = 0; i < line.Length; i++) {
					ret[i, j] = line[i] == '1';
				}
			}
			
			return ret;
		}
		
		private static Array toArray<T>(ICollection<T> input) {
			T[] ret = new T[input.Count];
			
			int i = 0;
			foreach(var element in input) {
				ret[i] = element;
				i++;
			}
			
			return ret;
		}

private static string testGrid = 
@"0000000000
1111101111
1111001111
1110011111
1111000001
0000000000";
	}
}