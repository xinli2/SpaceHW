using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] protected Tracker tracker;

        [SerializeField] protected MinimapCamera minimapCamera;

        [SerializeField] protected RectTransform mapMask;
        [SerializeField] protected RectTransform map;
        [SerializeField] protected RectTransform mapPivot;

        [SerializeField] protected Transform playerVehicle;


        
        protected virtual void Awake()
        {
            minimapCamera = FindObjectOfType<MinimapCamera>();
        }

        private void LateUpdate()
        {
            if (minimapCamera == null) return;

            float mapSize = (minimapCamera.Camera.orthographicSize / tracker.Range) * mapMask.sizeDelta.x;
            map.sizeDelta = new Vector2(mapSize, mapSize);

            Vector3 playerRelPos = (playerVehicle.position - minimapCamera.transform.position) / minimapCamera.Camera.orthographicSize;
            playerRelPos.y = 0;
            Vector3 mapRelPos = playerRelPos * (map.sizeDelta.x / 2);
            map.anchoredPosition = -1 * new Vector2(mapRelPos.x, mapRelPos.z);

            mapPivot.localRotation = Quaternion.Euler(0f, 0f, Quaternion.FromToRotation(minimapCamera.transform.up, playerVehicle.forward).eulerAngles.y);
        }

    }

}
