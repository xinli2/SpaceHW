using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages the flyby particles effects to help the player to feel the speed of the vehicle.
    /// </summary>
    public class FlybyParticleController : MonoBehaviour 
	{

        public VehicleCamera vehicleCamera;

        [SerializeField]
		protected ParticleSystem flybyParticleSystem;
        protected ParticleSystem.MainModule flybyParticleSystemMainModule;

        [SerializeField]
        protected Color particleColor = Color.white;

        [SerializeField]
        protected bool velocityBasedSize = true;

		[SerializeField]
        protected float velocityToSizeCoefficient = 0.005f;

        [SerializeField]
        protected bool velocityBasedAlpha = true;
	
		[SerializeField]
        protected float velocityToAlphaCoefficient = 0.005f;

        [SerializeField]
        protected float frontOffset = 50;

        [SerializeField]
        protected float leadDistance = 200;

      
        protected virtual void Awake()
		{
			flybyParticleSystemMainModule = flybyParticleSystem.main;
            
            Color c = particleColor;

            if (velocityBasedAlpha)
            {
                c.a = 0;
            }

            flybyParticleSystemMainModule.startColor = c;
		}


        /// <summary>
        /// Update the flyby particle effects according to the velocity of the vehicle.
        /// </summary>
        /// <param name="vehicleVelocity">The velocity of the vehicle.</param>
        public virtual void UpdateEffect(Vehicle vehicle)
		{
            if (vehicle.CachedRigidbody == null) return;

            if (velocityBasedSize)
            {
                flybyParticleSystemMainModule.startSize = vehicle.CachedRigidbody.velocity.magnitude * velocityToSizeCoefficient;
            }

            Color c = particleColor;

            if (velocityBasedAlpha)
            {
                float alpha = vehicle.CachedRigidbody.velocity.magnitude * velocityToAlphaCoefficient;
                c.a = alpha;
            }

            flybyParticleSystemMainModule.startColor = c;

            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = flybyParticleSystem.velocityOverLifetime;
            velocityOverLifetimeModule.x = -vehicle.CachedRigidbody.velocity.x;
            velocityOverLifetimeModule.y = -vehicle.CachedRigidbody.velocity.y;
            velocityOverLifetimeModule.z = -vehicle.CachedRigidbody.velocity.z;

            if (vehicle.CachedRigidbody.velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(vehicle.CachedRigidbody.velocity.normalized, vehicle.transform.up);
                transform.position = vehicle.CachedRigidbody.transform.position + vehicle.transform.forward * frontOffset + vehicle.CachedRigidbody.velocity.normalized * leadDistance;
            }
        }


        private void Update()
        {
            if (vehicleCamera.TargetVehicle != null)
            {
                UpdateEffect(vehicleCamera.TargetVehicle);
            }
        }
    }
}
