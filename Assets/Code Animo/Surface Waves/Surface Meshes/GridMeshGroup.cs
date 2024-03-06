// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo {
	
	[Serializable]
	public class GridMeshGroup : MonoBehaviour {
		
		public delegate void SegmentEventHandler(UnityEngine.Object sender, SegmentEventData segmentData);
		public delegate void MeshGroupEventHandler(GridMeshGroup meshGroup);
		
		public event MeshGroupEventHandler groupComplete;
		public event SegmentEventHandler segmentCreated;
		
		[SerializeField] private Material m_selectedMaterial;// serializeable to make the property work in editor
		public Material selectedMaterial{
			get { return m_selectedMaterial; }
			set {
				m_selectedMaterial = value;
				ApplyMaterialToAllMeshes(value);
			}
		}
		
		public InteractiveLoader meshLoader;
		public GridHeightData heightData;
		
		public int totalGridUnitCountU = 512;
		public int totalGridUnitCountV = 512;
		public int segmentGridUnitCountU = 64;// per segment
		public int segmentGridUnitCountV = 64;
		public float totalMeshWidth = 512f;
		public float totalMeshDepth = 512f;
		
		
		public string meshNamePrefix = "Grouped GridMesh";
		
		
		
		
		[SerializeField]
		protected List<GridMesh> groupedMeshes = new List<GridMesh>();
		
		public bool isCreatingGroup{
			get { return this.creatingGroup; }		
		}
		public bool containsSegments{
			get { return this.groupedMeshes.Count > 0; }	
		}
		public int groupSize{
			get { return this.groupedMeshes.Count; }
		}
		
		// Mesh Creation session data:
		private bool creatingGroup = false;
		private int sessionTotalGridUnitCountU = 0;
		private int sessionTotalGridUnitCountV = 0;
		private int sessionSegmentGridUnitCountU = 0;
		private int sessionSegmentGridUnitCountV = 0;
		private float sessionGridUnitSizeU = 0;
		private float sessionGridUnitSizeV = 0;
		protected Vector3 sessionStartPosition;
		
		private int nextOffsetU = 0;
		private int nextOffsetV = 0;
		
		protected virtual void Reset(){
			InteractiveLoader meshLoader = gameObject.AddComponentIfMissing<InteractiveLoader>();
			if (meshLoader != null) this.meshLoader = meshLoader;
			else this.meshLoader = GetComponent<InteractiveLoader>();
		}
		
		protected void OnValidate(){
			selectedMaterial = m_selectedMaterial;// put value changed by inspector through property.
		}
		
		/// <summary>
		/// Prepares for creating the group of gridMeshes.
		/// Actual mesh creation will happen through interactive loader
		/// </summary>
		public virtual void StartCreatingGroup(){
			if (creatingGroup) return;// do not interrupt existing group creation process
			// FIXME: classes that override StartCreatingGroup might not check isCreatingGroup before making changes.
			
			this.creatingGroup = true;
			
			sessionStartPosition = this.transform.position;
			
			CodeAnimo.UnityExtensionMethods.GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = false;
			
			if (this.heightData != null){
				if (this.heightData.hasData){
					int maxWidth = Mathf.Min(this.heightData.maximumU, this.totalGridUnitCountU);
					int maxHeight = Mathf.Min(this.heightData.maximumV, this.totalGridUnitCountV);
					
					if (maxWidth < 1){
						Debug.LogWarning("Group has no width (U)", this);
						StopCreatingGroup();
						return;
					}
					if (maxHeight < 1){
						Debug.LogWarning("Group has no Height (V)", this);
						StopCreatingGroup();
						return;
					}
					
					this.totalGridUnitCountU = maxWidth;
					this.totalGridUnitCountV = maxHeight;
				}
	//			else {
	//				Debug.LogWarning("HeightData Object set, but it has no data available");	
	//			}
			}		
			
			this.sessionTotalGridUnitCountU = this.totalGridUnitCountU;
			this.sessionTotalGridUnitCountV = this.totalGridUnitCountV;
			this.sessionSegmentGridUnitCountU = this.segmentGridUnitCountU;
			this.sessionSegmentGridUnitCountV = this.segmentGridUnitCountV;
			
			this.sessionGridUnitSizeU = this.totalMeshWidth / this.totalGridUnitCountU;
			this.sessionGridUnitSizeV = this.totalMeshDepth / this.totalGridUnitCountV;
			
			this.nextOffsetU = 0;
			this.nextOffsetV = 0;
			
			int uSegmentCount = Mathf.CeilToInt(this.sessionTotalGridUnitCountU / this.sessionSegmentGridUnitCountU);
			int vSegmentCount = Mathf.CeilToInt(this.sessionTotalGridUnitCountV / this.sessionSegmentGridUnitCountV);
			int segmentCount = uSegmentCount * vSegmentCount;
			for (int i = 0; i < segmentCount; i++) {
				EnqueueSegmentCreation();
			}
			meshLoader.StartLoading();
			
	//		startListeningToApplicationUpdate();
		}
		
		protected void EnqueueSegmentCreation(){
			meshLoader.AddMethod(CreateNextSegment);
			meshLoader.AddMethod(UpdateGroupCreationLoop);
		}
	
		
		public void StopCreatingGroup(){
			CodeAnimo.UnityExtensionMethods.GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = true;
			creatingGroup = false;	
	//		stopListeningToApplicationUpdate();
		}
		
		
		public void AddGroupCompleteHandler(MeshGroupEventHandler handler){
			this.groupComplete -= handler;
			this.groupComplete += handler;
		}
		
		/// <summary>
		/// Each call to this method, during group creation, creates a new segment.
		/// </summary>
		private void CreateNextSegment(){
			//Calculate world offset:
			Vector3 worldPosition = sessionStartPosition;
			worldPosition.x += this.sessionGridUnitSizeU * this.nextOffsetU;
			worldPosition.z += this.sessionGridUnitSizeV * this.nextOffsetV;
			
			GridMesh segment = CreateSegment();
			segment.transform.position = worldPosition;
			
			SetupSegment(segment);
			
			OnSegmentCreated(new SegmentEventData(segment));
			
			if (heightData != null){		
				segment.SetupEvents();
			}
			else{
				segment.GenerateGrid();	
			}
		}
		
		/// <summary>
		/// Initializes the given segment. Called for each created segment.
		/// Override to apply additional setup.
		/// </summary>
		/// <param name='segment'>
		/// The newly created segment
		/// </param>
		protected virtual void SetupSegment(GridMesh segment){
			segment.gridUnitCountU = this.sessionSegmentGridUnitCountU;
			segment.gridUnitCountV = this.sessionSegmentGridUnitCountV;
			segment.groupUnitCountU = this.sessionTotalGridUnitCountU;
			segment.groupUnitCountV = this.sessionTotalGridUnitCountU;
			segment.gridUnitSizeU = this.sessionGridUnitSizeU;
			segment.gridUnitSizeV = this.sessionGridUnitSizeV;
			segment.heightData = this.heightData;
			
			segment.offsetU = this.nextOffsetU;
			segment.offsetV = this.nextOffsetV;
			
			
			
			segment.hideFlags = HideFlags.HideInHierarchy;
			
			segment.name = CreateSegmentName();	
			segment.transform.parent = this.transform;
			this.groupedMeshes.Add(segment);
			segment.GetComponent<Renderer>().sharedMaterial = m_selectedMaterial;
			
			segment.GenerateGrid();
		}

		
		/// <summary>
		/// Updates state to get ready for next iteration.
		/// </summary>
		private void UpdateGroupCreationLoop(){
			// Loop handling:
			this.nextOffsetU += this.sessionSegmentGridUnitCountU;
			if (this.sessionTotalGridUnitCountU - this.nextOffsetU < 1){
				// New Row:
				this.nextOffsetU = 0;
				this.nextOffsetV += this.sessionSegmentGridUnitCountV;
				
				if (this.sessionTotalGridUnitCountV - this.nextOffsetV < 1){
					// Group creation complete:
					StopCreatingGroup();
					OnGroupCreated();
				}
			}
		}
		
		/// <summary>
		/// Creates a GridMesh that can be used in the group
		/// </summary>
		/// <returns>
		/// The created segment.
		/// </returns>
		/// <param name='uOffset'>
		/// U offset in the group
		/// </param>
		/// <param name='vOffset'>
		/// V offset in the group
		/// </param>
		/// <param name='worldPosition'>
		/// World position.
		/// </param>
		/// <exception cref='NullReferenceException'>
		/// Is thrown when thereś no appropriate mesh prefab.
		/// </exception>
		protected virtual GridMesh CreateSegment(){
			GameObject segmentNode = new GameObject("incomplete grouped mesh");
			GridMesh segment = segmentNode.AddComponent<GridMesh>();
			
			return segment;
		}
		
		/**
		 * Deletes all group members and clears the list.
		 **/
		public void DeleteGroupMembers(){
			foreach (GridMesh segment in this.groupedMeshes){
				if (segment == null) continue;
				
				segment.DestroyGeneratedMesh();
				DestroyImmediate(segment.gameObject);
			}
			
			groupedMeshes.Clear();
		}
		
		/// <summary>
		/// Creates a name with number, so the position of a segment can be identified using its name.
		/// </summary>
		/// <returns>
		/// The generated segment name.
		/// </returns>
		private string CreateSegmentName(){
			int uOrder = this.nextOffsetU / this.sessionSegmentGridUnitCountU;
			int vOrder = this.nextOffsetV / this.sessionSegmentGridUnitCountV;
			return this.meshNamePrefix + " (" + uOrder + "; " + vOrder + ")";
		}
		
		/// <summary>
		/// Dispatches event for Group being successfully created.
		/// </summary>
		private void OnGroupCreated(){
			if (this.groupComplete == null) return;
			
			this.groupComplete(this);
		}
		
		private void OnSegmentCreated(SegmentEventData e){
			if (this.segmentCreated == null) return;
			
			this.segmentCreated(this, e);
		}
		
		protected void ApplyMaterialToAllMeshes(Material newMaterial){
			for (int i = 0; i < this.groupSize; i++) {
				this.groupedMeshes[i].GetComponent<Renderer>().sharedMaterial = newMaterial;
			}

		}
	
	}
	
	public class SegmentEventData : EventArgs{
		public GridMesh segment;
		
		public SegmentEventData(GridMesh segment){
			this.segment = segment;
		}
		
	}
}