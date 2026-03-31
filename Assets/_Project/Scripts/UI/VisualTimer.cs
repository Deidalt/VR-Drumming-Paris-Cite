using System.Collections;
using _Project.Scripts.Systems;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    /// <summary>
    ///     This class is responsible for populating the UI timer with the elapsed time.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VisualTimer : SingletonMonoBehaviour<VisualTimer>
    {
        private static float _startTime;
        private static float _endTime;

        private static bool _isRunning;
        private static Coroutine _timerCoroutine;

        private static TextMeshProUGUI _timerText;
        public static float CurrentTime;


        #region Event Functions

        private void Start()
        {
            _timerText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            EventManager.MusicStartEvent += StartTimer;
            EventManager.MusicResetEvent += StopTimer;
            EventManager.TimerStartEvent += StartTimer;
            EventManager.TimerStopEvent += StopTimer;
        }

        private void OnDisable()
        {
            EventManager.MusicStartEvent -= StartTimer;
            EventManager.MusicResetEvent -= StopTimer;
            EventManager.TimerStartEvent -= StartTimer;
            EventManager.TimerStopEvent -= StopTimer;
        }

        #endregion

        /// <summary>
        ///     Stops the timer coroutine and resets the timer text.
        /// </summary>
        private void StopTimer()
        {
            if (_timerCoroutine is null) { return; }

            StopCoroutine(_timerCoroutine);

            _isRunning = false;
            _timerText.text = "00.00";
            _timerCoroutine = null;
        }

        private void StartTimer()
        {
            _isRunning = true;
            _startTime = Time.time;

            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private static IEnumerator TimerCoroutine()
        {
            while (_isRunning)
            {
                CurrentTime = Time.time - _startTime;
                _timerText.text = $"{Time.time - _startTime:F2}";

                yield return new WaitForEndOfFrame();
            }

            _endTime = Time.time;
        }
    }
}