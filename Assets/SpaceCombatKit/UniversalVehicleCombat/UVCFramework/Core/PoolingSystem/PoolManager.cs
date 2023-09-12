using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.Pooling 
{

	/// <summary>
    /// This class is a singleton that manages all of the object pools in the scene. Through this singleton, the game
    /// can get a pooled item, using either the prefab reference or the resource name. Items are returned to the pool 
    /// by deactivating the GameObject.
    /// </summary>
	[DefaultExecutionOrder(-50)]
	public class PoolManager : MonoBehaviour 
	{
	
		// Singleton reference
		protected static PoolManager instance = null;
		public static PoolManager Instance
		{
			get { return instance; }
		}

		// List of all the object pools being managed through this manager
		protected List<ObjectPool> objectPools = new List<ObjectPool>();


		[Tooltip("The default number of units that a pool should start off with when created.")]
		[SerializeField]
		protected int defaultStartingUnits = 3;

		[Tooltip("Whether to destroy pooled items that have spent a specified amount of time since last being used.")]
		[SerializeField]
		protected bool destroyItemsByTimeSinceLastUse = false;

		[Tooltip("The maximum time since last use that an item can have before it is destroyed.")]
		[SerializeField]
		protected float maxTimeSinceLastUse = 20;

		[Tooltip("The minimum quantity to leave in the pool when destroying unused items.")]
		[SerializeField]
		protected int minQuantity = 0;

       
		protected virtual void Awake()
		{

			// Set up the static singleton reference
			if (instance == null)
			{ 
				instance = this;
			}
			else
			{
				Destroy(gameObject);
			}

			// Find all the pools already in the scene
			ObjectPool[] objectPoolsInScene = GameObject.FindObjectsOfType<ObjectPool>();
			foreach (ObjectPool pool in objectPoolsInScene)
			{
                objectPools.Add(pool);
			}
		}


        /// <summary>
        /// Get an item from the pool, using the prefab's GameObject reference.
        /// </summary>
        /// <param name="prefab">The prefab of the pooled item.</param>
        /// <param name="pos">The position the returned item is needed at.</param>
        /// <param name="rot">The rotation the returned item needs to be at.</param>
        /// <param name="parent">The parent for the returned item.</param>
        /// <returns>The GameObject reference of the returned item.</returns>
        public virtual GameObject Get(GameObject prefab, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
		{

			// Look for an existing pool
			for (int i = 0; i < objectPools.Count; ++i)
			{
				if (objectPools[i].Prefab.name == prefab.name)
				{
					return (objectPools[i].Get(pos, rot, parent));
				}
			}
				
			// Get from new pool
			ObjectPool newPool = GetObjectPool(prefab);
			return (newPool.Get(pos, rot, parent));

		}


		public virtual GameObject Get(GameObject prefab, Transform parent)
		{

			// Look for an existing pool
			for (int i = 0; i < objectPools.Count; ++i)
			{
				if (objectPools[i].Prefab.name == prefab.name)
				{
					return (objectPools[i].Get(parent.position, parent.rotation, parent));
				}
			}

			// Get from new pool
			ObjectPool newPool = GetObjectPool(prefab);
			return (newPool.Get(parent.position, parent.rotation, parent));

		}


		/// <summary>
		/// Get an item from the pool, using the resource name of the prefab.
		/// </summary>
		/// <param name="prefabResourceName">The resource name of the prefab.</param>
		/// <param name="pos">The position the returned item is needed at.</param>
		/// <param name="rot">The rotation the returned item needs to be at.</param>
		/// <param name="parent">The parent for the returned item.</param>
		/// <returns>The GameObject reference of the returned item.</returns>
		public virtual GameObject Get(string prefabResourceName, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
		{

			// Look for an existing pool
			for (int i = 0; i < objectPools.Count; ++i)
			{
				if (objectPools[i].Prefab.name == prefabResourceName)
				{
					return (objectPools[i].Get(pos, rot, parent));
				}
			}

			// Try to load prefab from resources
			GameObject prefab = (GameObject)Resources.Load(prefabResourceName);
			if (prefab == null)
			{
				Debug.LogError("No prefab with name " + prefabResourceName + " found in Resources folder, cannot create pool!");
				return null;
			}

			// Get from new pool
			ObjectPool newPool = GetObjectPool(prefab);
			return (newPool.Get(pos, rot, parent));
		}


		/// <summary>
        /// Get an object pool reference for a needed item.
        /// </summary>
        /// <param name="prefab">The needed item's prefab GameObject reference.</param>
        /// <returns>The ObjectPool for the needed item.</returns>
		public virtual ObjectPool GetObjectPool(GameObject prefab)
		{

			// Check if there is already a pool for this object
			for (int i = 0; i < objectPools.Count; ++i)
			{
				if (objectPools[i].Prefab == prefab)
				{
					return objectPools[i];
				}
			}
		
			// Create a pool for the prefab, and to keep the scene neat, parent it to the manager
			GameObject newPoolGameObject = new GameObject (prefab.name + "Pool");
			newPoolGameObject.transform.SetParent(transform);
	
			// Set up the pool
			ObjectPool objectPool = newPoolGameObject.AddComponent<ObjectPool>();
			objectPool.Prefab = prefab;
			objectPool.DefaultStartingUnits = defaultStartingUnits;
			
			// Add the object pool to the list
			objectPools.Add (objectPool);
	
			return objectPool;
		}


		/// <summary>
		/// Destroy an object pool for a specified prefab.
		/// </summary>
		/// <param name="prefab">The prefab.</param>
		public virtual void DestroyObjectPool(GameObject prefab)
        {
			for (int i = 0; i < objectPools.Count; ++i)
			{
				if (objectPools[i].Prefab == prefab)
				{
					Destroy(objectPools[i]);
					objectPools.RemoveAt(i);
					break;
				}
			}
		}


		/// <summary>
		/// Set the number of units that the pool should have for a prefab (will create object pool if it doesn't exist, and destroy excess or create the necessary number).
		/// </summary>
		/// <param name="prefab">The prefab to set the number of units for.</param>
		/// <param name="numUnits">The new number of pooled units to have available.</param>
		public virtual void SetObjectPoolItemNumber(GameObject prefab, int numUnits)
		{
			ObjectPool pool = GetObjectPool(prefab);
			pool.SetNumber(numUnits);
		}

		public virtual void DestroyItemsByTimeSinceLastUse(float timeSinceLastUse, int minQuantity = 0)
		{
			for (int i = 0; i < objectPools.Count; ++i)
			{
				objectPools[i].DestroyItemsByTimeSinceLastUse(timeSinceLastUse, minQuantity);
			}
		}

		protected virtual void Update()
        {
			if (destroyItemsByTimeSinceLastUse)
            {
				DestroyItemsByTimeSinceLastUse(maxTimeSinceLastUse, minQuantity);
            }
        }
	}
}