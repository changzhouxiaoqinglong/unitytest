// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace CodeAnimo{
	
	[ExecuteInEditMode]
	public class InteractiveLoader : MonoBehaviour {
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentHelp;
		#endif
		
		public event EventHandler loadingComplete;
		
		public int ElementCount{
			get { return this.loadMethodQueue.Count; }	
		}
		/// <summary>
		/// Gets a value indicating whether this <see cref="InteractiveLoader"/> is loading.
		/// </summary>
		/// <value>
		/// <c>true</c> if loading is in progress; otherwise, <c>false</c>.
		/// </value>
		public bool Loading{
			get { return m_loading; }	
		}
		
		public delegate void loadMethod(); 
		[SerializeField]
		private Queue<loadMethod> loadMethodQueue = new Queue<loadMethod>();
		
		[SerializeField][HideInInspector] private bool m_loading = false;
		[SerializeField][HideInInspector] private bool m_newMethodAdded = false;
		
		private int m_completedCount = 0;
		
		private Stopwatch updateTimer = new System.Diagnostics.Stopwatch();
		public int endUpdateTime = 8;// The maximum amount of miliseconds that are allowed for a new segment creation to start.
		public bool loadOnNewData = false;// Should loading start if new methods have been added to the list?
		
		public float CompletionFraction{
			get {
				int remaining = this.loadMethodQueue.Count;
				int total = this.m_completedCount + remaining;
				if (remaining == 0) return 1;
				else { return (float)this.m_completedCount / (float)total;}
			}
		}
		
		protected void Update () {
			if (loadOnNewData && m_newMethodAdded) StartLoading();
			
			if (m_loading) RunLoadingFrame();
		}
		
		public void EditorUpdate(){
			Update();
		}
		
		/// <summary>
		/// Adds method to the queue, if it is not null.
		/// </summary>
		/// <param name='method'>
		/// Method.
		/// </param>
		public void AddMethod(loadMethod method){
			if (method == null) return;
			this.loadMethodQueue.Enqueue(method);
			m_newMethodAdded = true;
		}
		
		public void ClearMethods(){
			this.loadMethodQueue.Clear();
		}
		
		/// <summary>
		/// Starts the loading process.
		/// Resets state.
		/// </summary>
		public void StartLoading(){
			m_newMethodAdded = false;
			if (m_loading) return;// Do not interrupt loading if it is in progress
			m_completedCount = 0;
			m_loading = true;
		}
		
		/// <summary>
		/// Stops the loading process.
		/// </summary>
		public void StopLoading(){
			m_loading = false;
		}
		
		/// <summary>
		/// Calls methods from the queue while there is time left in the frame.
		/// </summary>
		private void RunLoadingFrame(){
			this.updateTimer.Reset();
			this.updateTimer.Start();	
			
			try{
				while(updateTimer.ElapsedMilliseconds < endUpdateTime && m_loading){
					LoadElement();
				}
			}
			catch{
				StopLoading();
				throw;
			}
			finally{
				
				updateTimer.Stop();
			}
		}
		
		/// <summary>
		/// Removes a delegate from the queue and calls it.
		/// Calls onLoadingComplete if the queue is empty.
		/// Increases the count of the number of completed method calls.
		/// </summary>
		/// <exception cref='NullReferenceException'>
		/// Is thrown when the delegate is null.
		/// </exception>
		private void LoadElement(){
			if (loadMethodQueue.Count <= 0){
				OnLoadingComplete();
				return; 
			}
	
			loadMethod element = loadMethodQueue.Dequeue(); 
			if (element != null){
				element();
				m_completedCount++;
			}
			else{
				StopLoading();
				throw new NullReferenceException("One of the loading tasks is null");
			}
		}
		
		/// <summary>
		/// Called when there are no elements left in the queue.
		/// Stops loading, and sends out the loadingComplete event.
		/// </summary>
		private void OnLoadingComplete(){
			StopLoading();
			
			if (loadingComplete != null){
				loadingComplete(this, EventArgs.Empty);	
			}
		}
		
		
	}
}