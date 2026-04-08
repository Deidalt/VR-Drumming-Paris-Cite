using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Systems
{
    public class WaitLoopAndHide : MonoBehaviour

    {


        private Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();

            if (anim == null)
            {
                Debug.LogError("Animator missing on " + gameObject.name);
            }
        }

        public void WaitEndOfLoop()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            StartCoroutine(HideAtEndOfLoop());
        }
        public IEnumerator HideAtEndOfLoop()
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // Attendre que normalizedTime atteigne la fin de la loop
            while (stateInfo.normalizedTime % 1 < 0.99f)
            {
                stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }

            Hide();
        }

        public void Hide()
        {
            anim.Play("CrossStopExit");
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            Show();

        }
    }
}