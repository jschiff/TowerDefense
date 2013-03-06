using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using AssemblyCSharp.Grid;

public class CubeSpawner : MonoBehaviour {
	private float timeSinceLastMove = 0;
	public Transform cube;
	public Transform actor;
	public Transform baseCube;
	public Transform previewCube;
	public int width = 12, height = 18; // Size of grid
	private Transform pathFinder = null;
	private GridModel model;
	private Vector3[,] gridLocations;
	private Vector3[,] baseLocations;
	private Transform previewCubeInst = null; // The preview cube being shown above the currentFocusCube
	private Transform currentFocusCube = null; // The base block e're currently pointing at.
	Transform[,] baseCubes;
	Transform[,] gridCubes;
	LinkedList<Transform> offScreenCubes = new LinkedList<Transform>();
	private LinkedList<Coordinate> path = null;
	public float spacing = 0.0f; // uniform spacing around grid cubes
	public float cubeSize = 1.0f;
	public float speed = 8f; // Number of units enemy moves per second
	private static Vector3 maxVec = Vector3.one * float.MaxValue;
	
	// Use this for initialization
	void Start () {
		generateGrid();
		loadOffScreenCubes();
		pathFinder = (Transform)Instantiate(actor, gridLocations[0, 0], Quaternion.identity);
	}
	
	// Load cubes off screen so they dont cause stutter when they are initialized
	private void loadOffScreenCubes() {
		for(int i = 0; i < (width * height); i++) {
			offScreenCubes.AddFirst((Transform)Instantiate(cube, maxVec, Quaternion.identity));
		}
		
		previewCubeInst = (Transform)Instantiate(previewCube, maxVec, Quaternion.identity);
	}
	
	private Transform spawnNewCube(Vector3 pos) {
		Transform temp = offScreenCubes.First.Value;
		temp.localPosition = pos;
		offScreenCubes.RemoveFirst();
		return temp;
	}
	
	private void destroyCube(Transform cube) {
		offScreenCubes.AddFirst(cube);
		cube.position = maxVec;
	}
	
	private void spawnPreviewCube(Vector3 pos) {
		previewCubeInst.localPosition = pos;
	}
	
	private void destroyPreviewCube() {
		previewCubeInst.position = maxVec;
	}
	
	private void generateGrid() {
		model = new GridModel(width, height);
		end = getLastRow();
		baseLocations = buildLocationArray(Vector3.down, width, height);
		gridLocations = buildLocationArray(Vector3.zero, width, height);
		baseCubes = new Transform[width, height];
		gridCubes = new Transform[width, height];
		
		populateBase(baseCubes, baseLocations);
	}
	
	private Coordinate[] getLastRow() {
		Coordinate[] ret = new Coordinate[model.width];
		
		for(int i = 0; i < model.width; i++) {
			ret[i] = new Coordinate(i, model.length - 1);
		}
		
		return ret;
	}
	
	// Fully populate the board.
	private void populateBase(Transform[,] baseCubes, Vector3[,] locations) {
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				baseCubes[i,j] = (Transform)Instantiate(baseCube, transform.TransformPoint(locations[i, j]), Quaternion.identity);
			}
		}
	}
	
	// Build array of locations.  These are the centers of the cubes in the grid
	private Vector3[,] buildLocationArray(Vector3 center, int width, int height) {
		Vector3[,] ret = new Vector3[width, height];
		
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				float x = i * (cubeSize + spacing) - ((width / 2) * (cubeSize + spacing));
				float z = j * (cubeSize + spacing) - ((height / 2) * (cubeSize + spacing));
				ret[i, j] = new Vector3(x, 0, z) + center;
			}
		}
		
		return ret;
	}
	
	// width and length are the width and length of the grid model
	private Coordinate getCoordinateFromWorldPosition(Vector3 pos, Vector3 baseCenter, int width, int length) {
		// This is the same calculation used for generating the location, solved for i
		int i = (int)Mathf.Round((pos.x + ((width / 2) * (cubeSize + spacing))) /  (cubeSize + spacing));
		int j = (int)Mathf.Round((pos.z + ((length / 2) * (cubeSize + spacing))) /  (cubeSize + spacing));
		
		return new Coordinate(i, j);
	}
	
	/*
	private void generateMaze() {
		model = new GridModel(GridModel.fromString(testGrid));
		int blocksLong = model.width;
		int blocksDeep = model.length;
		gridLocations = new Vector3[model.width, model.length];
		float blockSize = 1;
		
		for(int i = 0; i < blocksLong; i++) {
			for(int k = 0; k < blocksDeep; k++) {
				float x = i * (blockSize + spacing) - ((blocksLong / 2) * (blockSize + spacing));
				float z = k * (blockSize + spacing) - ((blocksDeep / 2) * (blockSize + spacing));
				
				gridLocations[i, k] = new Vector3(x, 0, z);
				
				if(model.isOccupied(i, k)) {
					Transform newCube = (Transform)Instantiate(cube, transform.TransformPoint(gridLocations[i, k]), Quaternion.identity);
				}
			}
		}
	}
	*/
	
	// Update is called once per frame
	void Update () {
		Transform focusBlock = mouseHit();
		
		if(Input.GetMouseButtonDown(0)) {
			placeBlock(focusBlock);	
		}
		
		if(Input.GetMouseButtonDown(1)) {
			removeBlock(focusBlock);	
		}
		
		if(focusBlock != currentFocusCube) {
			destroyPreviewCube();
			
			currentFocusCube = focusBlock;
			
			if(focusBlock != null) {
				drawPreviewBlock(currentFocusCube);
			}
		}
		
		updateEnemyPosition();
	}
	
	private void removeBlock(Transform baseBlock) {
		if(baseBlock == null) {
			return;	
		}
		
		Coordinate coord = getCoordinateFromWorldPosition(baseBlock.position, Vector3.down, width, height);
		if(model.isOccupied(coord)) {
			model.setIsOccupied(coord, false);
			destroyCube(gridCubes[coord.x, coord.y]);
			gridCubes[coord.x, coord.y] = null;
			path = null;
			node = null;
			spawnPreviewCube(gridLocations[coord.x, coord.y]);
		}
	}
	
	private void placeBlock(Transform baseBlock) {
		if(baseBlock == null) {
			return;	
		}
		
		Coordinate coord = getCoordinateFromWorldPosition(baseBlock.position, Vector3.down, width, height);
		
		if(model.isOccupied(coord) || enemyTarget.Equals(coord)) {
			return;	// May as well do this before bothering to simulate for performance
		}
		
		// Simulate for the starting position
		LinkedList<Coordinate> simPath = model.simulate(coord, start, end);
		if(simPath != null) {
			gridCubes[coord.x, coord.y] = spawnNewCube(gridLocations[coord.x, coord.y]);
			model.setOccupied(coord);
			path = null;
			node = null;
			destroyPreviewCube();
		}
		
		// Simulate for each enemy on the field already
	}
	
	private void drawPreviewBlock(Transform baseBlock) {
		Coordinate coord = getCoordinateFromWorldPosition(baseBlock.position, Vector3.down, width, height);
		if(!model.isOccupied(coord) && !model.isOccupied(enemyTarget)) {
			spawnPreviewCube(gridLocations[coord.x, coord.y]);
		}
	}
	
	// Return the block that the mouse is pointing at
	private Transform mouseHit() {
		Ray mouseRay = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);

		RaycastHit hitInfo;
		if(Physics.Raycast(mouseRay, out hitInfo)) {
			return hitInfo.transform;
		}
		
		return null;
	}
	
	// Enemy AI
	private void updateEnemyPosition() {
		float travelDistance = Time.deltaTime * speed;
		Vector3 targetLocation = gridLocations[enemyTarget.x, enemyTarget.y];
		Vector3 currentLocation = pathFinder.position;
		
		// Simulate movement.
		while (travelDistance > 0) {
			Vector3 diff = targetLocation - currentLocation;
			float diffMag = diff.magnitude;
			
			if(travelDistance >= diffMag) {
				currentLocation = targetLocation;
				enemyLocation = enemyTarget;
				enemyTarget = getNextCoordinate();
				if (enemyTarget == start){
					currentLocation = gridLocations[start.x, start.y];
				}
				targetLocation = gridLocations[enemyTarget.x, enemyTarget.y];
				Debug.Log(enemyTarget);			
				travelDistance -= diffMag;
			}
			else {
				currentLocation += (diff.normalized * travelDistance);
				travelDistance = 0;
			}
			
		}
		
		pathFinder.position = currentLocation;
	}
	
	private Coordinate getNextCoordinate() {
		if(path == null || node == null) {
			Coordinate fromC = enemyTarget;
			Coordinate[] toC = end;
			if (contains(toC, fromC)) {
				return start;
			}
			
			path = model.getPath(fromC, toC);
			node = path.First.Next;
		}
				
		var temp = node;
		node = node.Next;
		return temp.Value;
	}
	
	private bool contains(Coordinate[] arr, Coordinate c) {
		foreach(var el in arr) {
			if(c.Equals(el)) {
				return true;	
			}
		}
		
		return false;
	}
	
	public Coordinate start = new Coordinate(4, 0);
	public Coordinate[] end;
	private Coordinate enemyTarget = new Coordinate(4, 0);
	private Coordinate enemyLocation = new Coordinate(4, 0);
	private LinkedListNode<Coordinate> node;
	private static string testGrid = 
@"0000001111
1111101111
1000000001
1010010111
1011000001
0000000000
1011110111
0000110101
1110000000";
}
