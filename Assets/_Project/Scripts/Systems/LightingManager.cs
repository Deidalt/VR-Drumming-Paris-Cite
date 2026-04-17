using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _Project.Scripts.Systems
{
    public class LightingManager : SingletonMonoBehaviour<LightingManager>
    {
        private Color originalAmbient;
        private Material originalSkybox;
        private bool originalFog;
        private Color originalFogColor;
        private float originalFogDensity;
        private float originalReflection;
        [SerializeField] private Light[] directionalLights;
        private Dictionary<Light, float> originalLightIntensities = new Dictionary<Light, float>();
        public bool isBlackoutActive = false;

        void Start()
        {
            SaveCurrentLights();
        }

        public void SaveCurrentLights()
        {
            originalLightIntensities.Clear();

            foreach (var light in directionalLights)
            {
                originalLightIntensities[light] = light.intensity;
            }
            originalAmbient = RenderSettings.ambientLight;
            originalSkybox = RenderSettings.skybox;
            originalFog = RenderSettings.fog;
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            originalReflection = RenderSettings.reflectionIntensity;
        }

        public void SetBlackout()
        {
            //No environment lighting
            isBlackoutActive = true;
            SaveCurrentLights();
            StartCoroutine(FadeToBlackout(0.5f));
        }

        private IEnumerator FadeToBlackout(float duration)
        {
            float time = 0f;

            // Stocker les intensités initiales
            Dictionary<Light, float> initialIntensities = new Dictionary<Light, float>();
            foreach (var light in directionalLights)
            {
                initialIntensities[light] = light.intensity;
            }

            Color startAmbient = RenderSettings.ambientLight;
            float startReflection = RenderSettings.reflectionIntensity;

            while (time < duration)
            {
                float t = time / duration;

                // Smooth (optionnel mais plus joli)
                t = Mathf.SmoothStep(0f, 1f, t);

                foreach (var light in directionalLights)
                {
                    light.intensity = Mathf.Lerp(initialIntensities[light], 0f, t);
                }

                RenderSettings.ambientLight = Color.Lerp(startAmbient, Color.black, t);
                RenderSettings.reflectionIntensity = Mathf.Lerp(startReflection, 0f, t);

                time += Time.deltaTime;
                yield return null;
            }

            // Assure état final propre
            foreach (var light in directionalLights)
            {
                light.intensity = 0f;
                light.enabled = false;
            }


            RenderSettings.ambientLight = Color.black;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.fog = false;
            DynamicGI.UpdateEnvironment();
        }

        private IEnumerator FadeFromBlackout(float duration)
        {
            float time = 0f;

            foreach (var light in directionalLights)
            {
                light.enabled = true;
            }

            Dictionary<Light, float> targetIntensities = new Dictionary<Light, float>();
            foreach (var light in directionalLights)
            {
                targetIntensities[light] = originalLightIntensities[light];
            }

            Color startAmbient = RenderSettings.ambientLight;
            float startReflection = RenderSettings.reflectionIntensity;

            while (time < duration)
            {
                float t = time / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                foreach (var light in directionalLights)
                {
                    light.intensity = Mathf.Lerp(0f, targetIntensities[light], t);
                }

                RenderSettings.ambientLight = Color.Lerp(startAmbient, originalAmbient, t);
                RenderSettings.reflectionIntensity = Mathf.Lerp(startReflection, originalReflection, t);

                time += Time.deltaTime;
                yield return null;
            }

            // Restore final
            RenderSettings.ambientLight = originalAmbient;
            RenderSettings.skybox = originalSkybox;
            RenderSettings.fog = originalFog;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.reflectionIntensity = originalReflection;

            DynamicGI.UpdateEnvironment();
        }

        public void RestoreBlackout()
        {
            if (!isBlackoutActive)
                return;
            isBlackoutActive = false;
            LSLMarkerStream.Send("FixationEnd");
            StartCoroutine(FadeFromBlackout(0.5f));
            /*foreach (var light in directionalLights)
            {
                light.enabled = true;
            }
            RenderSettings.ambientLight = originalAmbient;
            RenderSettings.skybox = originalSkybox;
            RenderSettings.fog = originalFog;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.reflectionIntensity = originalReflection;
            DynamicGI.UpdateEnvironment();*/
        }
    }
}