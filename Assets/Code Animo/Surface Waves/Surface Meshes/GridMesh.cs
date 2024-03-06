// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;
using System.Collections;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo {
	
	public class GridMesh : MonoBehaviour {
		/// <summary>
		/// Unity max num of Vertices; maximum value for a 16 bit unsigned integer
		/// </summary>
		private const int maxMeshSize = 65535;
		
		public event EventHandler meshGenerated;

		/// <summary>
		/// The number of rows of grid units of this mesh in the v direction.
		/// </summary>
		public int gridUnitCountU = 64;
		/// <summary>
		/// The number of rows of grid units of this mesh in the u direction.
		/// </summary>
		public int gridUnitCountV = 64;
		
		/// <summary>
		/// The number of rows of grid units on the u axis of a meshgroup.
		/// </summary>
		public int groupUnitCountU = 512;
		/// <summary>
		/// The number of rows of grid units on the v axis of a meshgroup
		/// </summary>
		public int groupUnitCountV = 512;

		/// <summary>
		/// The amount of gridUnits that come before this one in a group.
		/// </summary>
		public int offsetU = 0;
		/// <summary>
		/// The amount of gridUnits that come before this one in a group.
		/// </summary>
		public int offsetV = 0;	

		/// <summary>
		/// The world distance between two vertices
		/// </summary>
		public float gridUnitSizeU = 1.0f;
		/// <summary>
		/// The world distance between two vertices
		/// </summary>
		public float gridUnitSizeV = 1.0f;
		
		public string meshName = "Custom Grid Mesh";

		/// <summary>
		/// If Units normal generation should be applied.
		/// </summary>
		public bool defaultNormalGeneration = false;
		
		[HideInInspector]
		[SerializeField]
		protected Mesh generatedMesh = null;
		public GridHeightData heightData;
		
		public bool hasMesh{
			get {  return this.generatedMesh != null; }
		}
		
		protected void Reset(){
			AddMissingComponents();
			SetupEvents();
		}
		
		private void OnEnable(){
			SetupEvents();
		}
		
		private void OnDisable(){
			UnsubscribeEvents();
		}
		
		virtual protected void OnDestroy(){
			// Actually: Don't destroy meshes when they're unloaded from play mode. It should only be destroyed when it's removed in edit mode.
			// Probably better to use a gridMesh creation asset, in stead of gameObject.
//			DestroyGeneratedMesh();
		}
		
		protected void AddMissingComponents(){
			gameObject.AddComponentIfMissing<MeshFilter>();
			gameObject.AddComponentIfMissing<MeshRenderer>();
		}
		
		public void SetupEvents(){
			if (this.heightData == null) return;// Warn in inspector in stead of throwing an exception.
	//		if (heightData == null) throw new NullReferenceException("HeightData Missing");
			heightData.subscribeToHeightDataUpdated(this.HandleHeightDataUpdated);	
		}
		
		public void UnsubscribeEvents(){
			if (this.heightData == null) return;// Warn in inspector in stead of throwing an exception.
	//		if (heightData == null) throw new NullReferenceException("HeightData Missing");
			heightData.unsubscribeFromHeightDataUpdated(this.HandleHeightDataUpdated);	
		}
		
		private void HandleHeightDataUpdated(object sender, System.EventArgs e){
			GenerateGrid();
		}		
	
		/// <summary>
		/// Creates a basic plane mesh.
		/// UV coordinates determined by uvBounds
		/// y Heights based on values returned by optional yInputMethod
		/// uOffset and vOffset offset the coordinates passed to yInputMethod
		/// </summary>
		/// <exception cref='System.ArgumentOutOfRangeException'>
		/// Is thrown when the group has no height and/or width.
		/// </exception>
		/// <exception cref='UnityException'>
		/// Is thrown when the mesh is too large. (Maximum mesh index is 16 bit uint)
		/// </exception>
		public virtual void GenerateGrid(){
			Debug.LogError("Generating Grid");
			
			int uUnits = Mathf.Min(groupUnitCountU - offsetU, this.gridUnitCountU);
			int vUnits = Mathf.Min(groupUnitCountV - offsetV, this.gridUnitCountV);
			
			// Don't bother if it has no width or height:
			if (uUnits <= 0 || vUnits <= 0){
				throw new System.ArgumentOutOfRangeException("A mesh that has no width is a bit useless...");
			}
			
			// Calculate the number of vertices:
			int uCount = uUnits + 1;// One extra to close the gap with a next mesh?
			int vCount = vUnits + 1;
			
			if (uCount * vCount > maxMeshSize){
				throw new UnityException("Mesh is too large: " + uCount * vCount + " vertices. Maximum number of vertices in Unity is " + maxMeshSize);
			}
			
			int numVerts = uCount * vCount;
			
			// Create Arrays of needed size:
			Vector3[] vertList = new Vector3[numVerts];
			Vector2[] uvList = new Vector2[numVerts];
			
			Vector3[] normalList = new Vector3[numVerts];
			Vector4[] tangentList = new Vector4[numVerts];
			
			// UV coordinate distance between vertices:		
			float uStep = 1 / (float)groupUnitCountU;
			float vStep = 1 / (float)groupUnitCountV;
			float mininumU = uStep * offsetU;
			float minimumV = vStep * offsetV;
			
			for (int u = 0; u < uCount; u++){
				for (int v = 0; v < vCount; v++){
					int vertIndex = v * uCount + u;
					
					vertList[vertIndex] = CalculateVertex(u,v);
					uvList[vertIndex] = new Vector2(mininumU + u * uStep, minimumV + v * vStep);
					tangentList[vertIndex] = CalculateTangent(u,v);
				}	
			}
			
			int[] triangleList = ConstructTriangles(uCount, vCount);		
			
			normalList = CalculateNormals( uCount, vCount);
			
			Mesh customMesh = CreateMesh(vertList, uvList, triangleList, normalList, tangentList);
			if (this.defaultNormalGeneration) customMesh.RecalculateNormals();
			
			
			
			GetComponent<MeshFilter>().mesh = customMesh;
			this.generatedMesh = customMesh;
			
			OnMeshGenerated(null);
	//		setCollider();
		}
		
		public void DestroyGeneratedMesh(){
			if (generatedMesh == null) return;
			DestroyImmediate( generatedMesh );
		}	
		
		private Mesh CreateMesh(Vector3[] vertices, Vector2[] uvCoords, int[] triangles, Vector3[] normals, Vector4[] tangents){
			Mesh createdMesh;
			
			//Attempt at reusing mesh... not sure how useful this is.
			if (generatedMesh != null){
				createdMesh = generatedMesh;
				createdMesh.Clear();	
			}
			else createdMesh = new Mesh();
	//		createdMesh = new Mesh();
			
			createdMesh.vertices = vertices;
			createdMesh.uv = uvCoords;
			createdMesh.triangles = triangles;
			createdMesh.normals = normals;
			createdMesh.tangents = tangents;
			
	//		createdMesh.RecalculateBounds();// Bounds should automatically be recalculated after setting triangles.
			
			createdMesh.name = meshName;
			
			return createdMesh;
		}
		
		private Vector3 CalculateVertex(int u, int v){
			float height = GetVertexHeight(u,v);
			return new Vector3(this.gridUnitSizeU * u, height, this.gridUnitSizeV * v);
		}
		
		/**
		 * Returns the height of a vertex at the given coordinates, if there is heightdata
		 * 0 if there is no heightdata or if the coordinates are out of range
		 **/
		private float GetVertexHeight(int u, int v){
			float height = 0;
			if (heightData != null){
				u += this.offsetU;
				v += this.offsetV;
				
				// Clamp values:
				if (u < 0) u = 0;
				if (v < 0) v = 0;
				if (u > heightData.maximumU) u = heightData.maximumU;
				if (v > heightData.maximumV) v = heightData.maximumV;
				
				/*
				// Switch to 0
				if (u < 0 || v < 0 || u > heightData.maximumU || v > heightData.maximumV){
					height = 0;
				}
				else height = heightData.getGridHeight(u, v);*/
				height = heightData.getGridHeight(u, v);
			}
			return height;
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <returns>Vector4(0,0,0,0)</returns>
		private Vector4 CalculateTangent(int u, int v){
			return new Vector4(0,0,0,0);
		}
		
		private int[] ConstructTriangles(int uCount, int vCount){
			int numTris = 2 * ((uCount -1) * (vCount - 1));
			int[] triangleList = new int[3 * numTris];// 3 vertices per triangle.
			
			// Construct Triangles out of the vertices:
			int triIndex = 0;
			
			for (int u = 0; u < vCount -1; u++){
				for (int v = 0; v < uCount -1; v++){
					
					triangleList[triIndex] 		= (u * uCount) + v;
					triangleList[triIndex + 1]	= ((u + 1) * uCount) + v;
					triangleList[triIndex + 2]	= (u * uCount) + v + 1;
					
					triangleList[triIndex + 3]	= ((u + 1) * uCount) + v;
					triangleList[triIndex + 4]	= ((u + 1) * uCount) + v + 1;
					triangleList[triIndex + 5]	= (u * uCount) + v + 1;
					
					triIndex += 6;
				}
			}
			
			return triangleList;
		}
		
		private Vector3[] CalculateNormals(int vertexCountU, int vertexCountV){
			int vertexCount = vertexCountU * vertexCountV;
			Vector3[] vertexNormals = new Vector3[vertexCount];
	
			for (int u = 0; u < vertexCountU; u++){
				for (int v = 0; v < vertexCountV; v++){
					int vertIndex = v * vertexCountU + u;
					
					// calculate vertex heights:
					float local = GetVertexHeight(u, v);
					
					float topLeft = GetVertexHeight(u + 1, v + 1);
					float top = GetVertexHeight(u, v + 1);
					
					float left = GetVertexHeight(u - 1, v);
					float right = GetVertexHeight(u + 1, v);
					
					float bottom = GetVertexHeight(u, v - 1);
					float bottomRight = GetVertexHeight(u + 1, v -1);
					
					// Calculate Surface normals
					// using stripped down cross product, that assumes this is a uniformly scaled grid:
					Vector3 topLeftNormal = new Vector3(topLeft - top, 1.0f, local - top);
					Vector3 leftTopNormal = new Vector3(left - local, 1.0f, left - topLeft);
					Vector3 rightTopNormal = new Vector3(local - right, 1.0f, local - top);
					
					Vector3 leftBottomNormal = new Vector3(left - local, 1.0f, bottom - local);
					Vector3 bottomRightNormal = new Vector3(bottom - bottomRight, 1.0f, bottom - local);
					Vector3 rightBottomNormal = new Vector3(local - right, 1.0f, bottomRight - right);
					
					Vector3 vertexNormal = topLeftNormal + leftTopNormal + rightTopNormal + leftBottomNormal + bottomRightNormal + rightBottomNormal;
					vertexNormal.Normalize();
					
					vertexNormals[vertIndex] = vertexNormal;
				}
			}	
			
			return vertexNormals;
		}
		
		/// <summary>
		/// Dispatches MeshGenerated Event
		/// </summary>
		/// <param name='e'>
		/// unused.
		/// </param>
		private void OnMeshGenerated(EventArgs e){
			if (meshGenerated == null) return;
			
			meshGenerated(this, e);
		}
		
		public void AddMeshGeneratedHandler(EventHandler listener){
			meshGenerated -= listener;
			meshGenerated += listener;
		}
		
		public void RemoveMeshGeneratedHandler(EventHandler listener){
			this.meshGenerated -= listener;	
		}
		
	}
}