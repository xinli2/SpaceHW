using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace VSX.Pooling 
{

	/// <summary>
    /// This class stores a reference to an instance of a pooled item.
    /// </summary>
	[System.Serializable]
	public class PooledObject 
	{

		protected GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		protected Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		protected float lastUsedTime;
		public float LastUsedTime { get { return lastUsedTime; } }

		public PooledObject(GameObject obj)
		{
			cachedGameObject = obj;
			cachedTransform = obj.transform;
			lastUsedTime = Time.time;
		}

		public virtual void OnUsed()
        {
			lastUsedTime = Time.time;
        }
	}


	/// <summary>
    /// This class manages a single object pool.
    /// </summary>
	public class ObjectPool : MonoBehaviour 
	{
	
		[SerializeField]
		protected GameObject prefab;
		public GameObject Prefab 
		{ 
			get { return prefab; } 
			set { prefab = value; } 
		}
	
		[SerializeField]
		protected int defaultStartingUnits = 3;
		public int DefaultStartingUnits
		{
			get { return defaultStartingUnits; }
			set { defaultStartingUnits = value; }
		}
	
		protected List<PooledObject> objectList = new List<PooledObject>();

			
		protected virtual void Start()
		{
			// Create the starting amount of objects
			if (defaultStartingUnits > 0 && prefab != null)
			{
				for (int i = 0; i < defaultStartingUnits; ++i)
				{
                    CreateNewItem();
				}
			}
		}


        /// <summary>
        /// Check if this object pool contains a particular GameObject.
        /// </summary>
        /// <param name="gameObjectToFind">The GameObject being searched for.</param>
        /// <returns>Whether the pool contains the game object.</returns>
        public virtual bool ContainsGameObject(GameObject gameObjectToFind)
        {
            return (objectList.Find(x => x.CachedGameObject == gameObjectToFind) == null);
        }
	

		/// <summary>
        /// Get a game object from the pool, with options to simultaneously set the position, rotation and parent.
        /// </summary>
        /// <param name="pos">The position where the returned item is needed.</param>
        /// <param name="rot">The rotation the returned item needs to be at.</param>
        /// <param name="parent">The transform the returned item needs to be parented to.</param>
        /// <returns>The GameObject reference to the returned item.</returns>
		public virtual GameObject Get (Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
		{
			// Search for a ready object
			for (int i = 0; i < objectList.Count; ++i)
			{
				if (!objectList[i].CachedGameObject.activeSelf)
				{
					// Prepare and deliver
					objectList[i].CachedTransform.position = pos;
					objectList[i].CachedTransform.rotation = rot;
					objectList[i].CachedGameObject.SetActive(true);

					// Set the parent
					if (parent != null)
						objectList[i].CachedTransform.SetParent(parent);

					objectList[i].OnUsed();

                    return objectList[i].CachedGameObject;

				}
			}
			
			// If an available one was not found, create one
			PooledObject newPooledObject = CreateNewItem(false);
			newPooledObject.CachedTransform.position = pos;
			newPooledObject.CachedTransform.rotation = rot;
			newPooledObject.CachedGameObject.SetActive (true);
			
			if (parent != null) 
				newPooledObject.CachedTransform.SetParent(parent);

			newPooledObject.OnUsed();

			return (newPooledObject.CachedGameObject);
		}
	

		/// <summary>
        /// Create a new item in the object pool.
        /// </summary>
        /// <param name="activate">Whether to activate the GameObject of the new item.</param>
        /// <returns>An instance of PooledObject class for the new item.</returns>
		protected virtual PooledObject CreateNewItem(bool activate = false)
		{

			GameObject newObject = Instantiate (prefab, Vector3.zero, Quaternion.identity, transform) as GameObject;

			PooledObject newPooledObject = new PooledObject(newObject);

			objectList.Add (newPooledObject); 

			newPooledObject.CachedGameObject.SetActive(activate);

			return newPooledObject;	
		}


		/// <summary>
		/// Set the number of units in this pool (will destroy excess or create the necessary number).
		/// </summary>
		/// <param name="numUnits">The number of units that this pool should have.</param>
		public virtual void SetNumber(int numUnits)
        {
			if (numUnits > objectList.Count)
            {
				int addition = numUnits - objectList.Count;
				for(int i = 0; i < addition; ++i)
                {
					CreateNewItem();
                }
            }
            else
            {
				int subtraction = objectList.Count - numUnits;
				for(int i = 0; i < subtraction; ++i)
                {
					int index = 0;
					for (int j = 0; j < objectList.Count; ++j)
					{
						if (!objectList[j].CachedGameObject.activeSelf)
                        {
							index = j;
							break;
                        }
					}

					Destroy(objectList[index].CachedGameObject);
					objectList.RemoveAt(index);
				}
            }
        }


		public virtual void DestroyItemsByTimeSinceLastUse(float timeSinceLastUse, int minQuantity = 0)
        {
			for(int i = 0; i < objectList.Count; ++i)
            {
				if (objectList.Count <= minQuantity) break;

				if (objectList[i].CachedGameObject.activeSelf) continue;

				if (Time.time - objectList[i].LastUsedTime > timeSinceLastUse)
                {
					Destroy(objectList[i].CachedGameObject);
					objectList.RemoveAt(i);
					i--;
                }
            }
        }
	}
}