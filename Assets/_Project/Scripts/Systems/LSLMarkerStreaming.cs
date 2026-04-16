using UnityEngine;
using LSL;
/// <summary>
/// Émetteur LSL pour les marqueurs expérimentaux.
/// Déposer le composant sur un GameObject de la scène de démarrage.
/// Usage depuis n'importe où :
/// LSLMarkerStream.Send("TrialStart;trial=1;condition=face");

namespace _Project.Scripts.Systems
{
    public class LSLMarkerStream : MonoBehaviour
    {
        [Header("Configuration LSL (ne pas modifier)")]
        public string streamName = "Unity_Markers_Drumming";
        public string streamID = "drumming_lsl_v1";
        private StreamOutlet outlet;
        private string[] sample = new string[1];
        private static LSLMarkerStream instance;
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            var info = new StreamInfo(
            streamName,
            "Markers",
            1,
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_string,
            streamID
            );
            outlet = new StreamOutlet(info);
            Debug.Log($"[LSL] Flux '{streamName}' prêt.");
        }
        public static void Send(string marker)
        {
            if (instance == null || instance.outlet == null) return;
            instance.sample[0] = marker;
            instance.outlet.push_sample(instance.sample);
            Debug.Log($"[LSL] {marker}");
        }
        void OnApplicationQuit()
        {
            outlet?.Close();
            outlet?.Dispose();
        }
    }
}