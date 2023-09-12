using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls the appearance of a projectile being fired when in different camera views.
    /// </summary>
    public class CameraViewProjectileModifier : MonoBehaviour, ICameraEntityUser
    {

        [Tooltip("The projectile weapon that fires the projectile that are being modified.")]
        [SerializeField] 
        protected ProjectileWeaponUnit projectileWeapon;

        [Tooltip("The default scale of the particles on the projectile.")]
        [SerializeField]
        protected float defaultParticleScale = 1;

        /// <summary>
        /// A container for settings for a projectile being fired in a specified camera view.
        /// </summary>
        [System.Serializable]
        public class CameraViewProjectileSettings
        {
            public CameraView cameraView;
            public float scale = 1;
        }

        [Tooltip("The projectile settings for different camera views.")]
        [SerializeField]
        protected List<CameraViewProjectileSettings> settingsList = new List<CameraViewProjectileSettings>();

        protected CameraEntity cameraEntity;

        protected float currentScale;



        protected virtual void Reset()
        {
            projectileWeapon = GetComponentInChildren<ProjectileWeaponUnit>();
        }


        protected virtual void Awake()
        {
            currentScale = defaultParticleScale;

            projectileWeapon.onProjectileLaunched.AddListener(OnProjectileLaunched);
        }


        /// <summary>
        /// Called when a camera entity follows the camera target that the line renderer is part of.
        /// </summary>
        /// <param name="cameraEntity">The camera entity following this target.</param>
        public virtual void SetCameraEntity(CameraEntity cameraEntity)
        {

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewTargetChanged);
            }

            currentScale = defaultParticleScale;

            this.cameraEntity = cameraEntity;

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
                OnCameraViewTargetChanged(cameraEntity.CurrentViewTarget);
            }
        }


        /// <summary>
        /// Called when the camera view target currently being followed by the camera changes.
        /// </summary>
        /// <param name="cameraViewTarget">The new camera view target.</param>
        protected virtual void OnCameraViewTargetChanged(CameraViewTarget cameraViewTarget)
        {
            currentScale = defaultParticleScale;

            if (cameraViewTarget == null) return;

            foreach (CameraViewProjectileSettings settings in settingsList)
            {
                if (settings.cameraView == cameraViewTarget.CameraView)
                {
                    currentScale = settings.scale;
                }
            }
        }


        protected virtual void OnProjectileLaunched(Projectile projectile)
        {
            projectile.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            LineRenderer[] lineRenderers = projectile.GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.widthMultiplier = currentScale;
            }
        }
    }
}

