using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.Pooling;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// This class manages the display of real vehicle and modules in the loadout menu.
    /// </summary>
	public class LoadoutDisplayManager : MonoBehaviour 
	{

		[Tooltip("The loadout manager.")]
        [SerializeField]
        protected LoadoutManager loadoutManager;

		[Tooltip("The transform that display vehicles are parented to.")]
		[SerializeField]
		protected Transform vehicleHolder;

		// The list of added display vehicles
		protected List<Vehicle> displayVehicles = new List<Vehicle>();
		public virtual List<Vehicle> DisplayVehicles { get { return displayVehicles; } }


        protected virtual void Reset()
		{
			loadoutManager = FindObjectOfType<LoadoutManager>();
			vehicleHolder = transform;
		}


		protected virtual void Awake()
        {
			loadoutManager.onDataLoad.AddListener(AddDisplayVehicles);
			loadoutManager.onLoadoutChanged.AddListener(ShowVehicle);
        }


		protected virtual void AddDisplayVehicles()
        {

			RemoveDisplayVehicles();

			if (loadoutManager.Items == null)
            {
				return;
            }

			List<LoadoutVehicleItem> vehicleItems = loadoutManager.Items.vehicles;

			foreach(LoadoutVehicleItem vehicleItem in vehicleItems)
            {
				Vehicle vehicle = GetDisplayVehicle(vehicleItem.vehiclePrefab);

				vehicle.gameObject.SetActive(false);
			}
		}


		protected virtual void RemoveModules(Vehicle vehicle)
        {
			foreach (ModuleMount moduleMount in vehicle.ModuleMounts)
			{

				List<Module> modules = moduleMount.Modules;
				moduleMount.RemoveAllModules();

				foreach (Module module in modules)
				{
					if (PoolManager.Instance != null)
                    {
						module.gameObject.SetActive(false);
                    }
                    else
                    {
						Destroy(module.gameObject);
                    }	
				}
			}
		}


		protected virtual void RemoveDisplayVehicles()
        {
			Vehicle[] displayVehiclesArray = displayVehicles.ToArray();

			foreach(Vehicle displayVehicle in displayVehiclesArray)
            {
				RemoveModules(displayVehicle);

				displayVehicles.Remove(displayVehicle);

				if (PoolManager.Instance != null)
                {
					displayVehicle.transform.SetParent(null);
					displayVehicle.gameObject.SetActive(false);
                }
                else
                {
					Destroy(displayVehicle.gameObject);
				}
			}
        }


		protected virtual Vehicle GetDisplayVehicle(Vehicle vehiclePrefab)
        {
			Vehicle vehicle;

			if (PoolManager.Instance != null)
            {
				vehicle = PoolManager.Instance.Get(vehiclePrefab.gameObject, vehicleHolder).GetComponent<Vehicle>();
            }
            else
            {
				vehicle = Instantiate(vehiclePrefab, vehicleHolder.position, vehicleHolder.rotation, vehicleHolder);
            }

			// Make rigidbody kinematic

			Rigidbody r = vehicle.GetComponent<Rigidbody>();
			if (r != null) r.isKinematic = true;

			VehicleEngines3D engines = vehicle.GetComponent<VehicleEngines3D>();
			if (engines != null)
			{
				engines.ActivateEnginesAtStart = false;
				engines.SetEngineActivation(false);
			}

			// Prevent creation of default modules

			foreach (ModuleMount moduleMount in vehicle.ModuleMounts)
			{
				moduleMount.createDefaultModulesAtStart = false;
			}

			// Add to list

			displayVehicles.Add(vehicle);

			return vehicle;
		}


		protected virtual Module GetModule(Module modulePrefab)
        {
			if (PoolManager.Instance != null)
			{
				return PoolManager.Instance.Get(modulePrefab.gameObject, vehicleHolder).GetComponent<Module>();
			}
			else
			{
				Module module = Instantiate(modulePrefab, vehicleHolder.position, vehicleHolder.rotation);
				return module;
			}
		}


        protected virtual void ShowVehicle()
		{
			for(int i = 0; i < displayVehicles.Count; ++i)
            {
				displayVehicles[i].gameObject.SetActive(false);
            }

			if (loadoutManager.Items == null) return;

			List<LoadoutVehicleItem> vehicleItems = loadoutManager.Items.vehicles;
			List<LoadoutModuleItem> moduleItems = loadoutManager.Items.modules;

			// Get the vehicle index

			int vehicleIndex = loadoutManager.WorkingSlot.selectedVehicleIndex;
			if (vehicleIndex < 0 || vehicleIndex >= vehicleItems.Count) return;

			displayVehicles[vehicleIndex].gameObject.SetActive(true);

			// Remove modules

			RemoveModules(displayVehicles[vehicleIndex]);

			// Add modules

			for(int i = 0; i < displayVehicles[vehicleIndex].ModuleMounts.Count; ++i)
			{
				if (loadoutManager.WorkingSlot.selectedModules.Count <= i) break;

				int moduleIndex = loadoutManager.WorkingSlot.selectedModules[i];

				if (moduleIndex != -1)
                {
					Module module = GetModule(moduleItems[moduleIndex].modulePrefab);
					displayVehicles[vehicleIndex].ModuleMounts[i].AddModule(module, true);
                }
                else
                {
					displayVehicles[vehicleIndex].ModuleMounts[i].UnmountAllModules();
				}
			}
		}
	}
}
