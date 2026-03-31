using System.Collections;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Systems
{
    public class BreakTimer : MonoBehaviour
    {
        public static BreakTimer Instance;
        private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI researcherTimer;
        private Coroutine coroutine;
        // Start is called before the first frame update
        void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
            Instance = this;
        }

        public void Show()
        {
            text.enabled = true;
            coroutine = StartCoroutine(UpdateTimer());
        }

        public void Hide()
        {
            text.enabled = false;
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private IEnumerator UpdateTimer()
        {
            while (true)
            {
                text.text = researcherTimer.text.Substring(0, 3);
                yield return null;
            }
        }

    }
}