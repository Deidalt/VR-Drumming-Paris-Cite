using UnityEngine;
using _Project.Scripts.UI;
using _Project.Scripts.Systems;

namespace _Project.Scripts.Transfer
{
    public class UIToSystems : MonoBehaviour
    {
        private BreakTimer breakTimer;


        void Awake()
        {
            breakTimer = FindFirstObjectByType<BreakTimer>();
        }

        void Update()
        {
            if (breakTimer == null)
            {
                return;
            }

            //from UI
            float currentTime = VisualTimer.CurrentTime;

            //to Systems
            breakTimer.SetTime(currentTime);
        }
    }
}