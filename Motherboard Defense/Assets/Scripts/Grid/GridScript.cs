using UnityEngine;
using System.Collections;

// Generates a grid texture for the playing field
public class GridScript : MonoBehaviour {
	
	public int width = 25;
	public int height = 25;
	public int lineWidth = 1;
	public int squareLength = 25; // Size of the squares in the grid, including borders
	public Color gridLineColor = Color.blue; // Color of the lines
	public Color fillColor = Color.black; // Color of the filled grid
	public new MeshRenderer renderer;
	
	// Use this for initialization
	void Start () {
		renderer.materials[0].SetTexture("_MainTex", generateGridTexture(width, height, lineWidth, squareLength, gridLineColor, fillColor));
	}
	
	// Generate a simple grid texture
	public Texture2D generateGridTexture(int width, int height, int lineWidth, int squareLength, Color gridLineColor, Color fillColor) {
		int totalWidth = width * squareLength;
		int totalHeight = height * squareLength;
		Texture2D tex = new Texture2D(totalWidth, totalHeight);
		clearTex(tex, Color.black);
		
		// For each grid square
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {				
				// Get bottom left corner
				int bottom = j * squareLength;
				int left = i * squareLength;
				
				// Fill with border line color
				fill (tex, new Rect(left, bottom, squareLength, squareLength), gridLineColor);
				
				// Fill with background color, smaller by the thickness of the line
				fill(tex, new Rect(left + lineWidth, bottom + lineWidth,
					squareLength - (2 * lineWidth), squareLength - (2 * lineWidth)), fillColor);
				
				Debug.Log("Square " + i + ", " + j + " done");
			}
			
			Debug.Log("Texture finished");
		}
		
		tex.Apply();
		return tex;
	}
	
	// Fill a rectangle within a texture with a color
	private void fill(Texture2D tex, Rect fillMe, Color color) {
		for(int i = (int)fillMe.x; i < (int)fillMe.width; i++) {
			for(int j = (int)fillMe.y; j < (int)fillMe.height; j++) {
				tex.SetPixel(i, j, color);	
			}
		}
	}
	
	// Clear out a texture (set to all one color)
	private void clearTex(Texture2D tex, Color fill) {
		for(int i = 0; i < tex.width; i++) {
			for(int j = 0; j < tex.height; j++) {
				tex.SetPixel(i, j, fill);
			}	
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}
	
	
}
