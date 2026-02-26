using System.Collections.Generic;
using System.Linq;
using DrumRhythmGame.Data;
using DrumRhythmGame.Field;
using RootMotion.FinalIK;
using UnityEngine;
using _Project.Scripts.Systems;
using UnityEngine.Assertions;
using System.Collections;

namespace _Project.Scripts.Field.Partner
{
    [RequireComponent(typeof(InteractionSystem))]
    [DefaultExecutionOrder(1)]
    public class Partner : MonoBehaviour
    {
        [Header("Animation setting")]
        [SerializeField] private InteractionObject crashCymbal;
        [SerializeField] private InteractionObject highTom;
        [SerializeField] private InteractionObject middleTom;
        [SerializeField] private InteractionObject snareDrum;

        private Animator avatarAnimator;
        [SerializeField] private FullBodyBipedIK fullBodyBipedIK;
        [SerializeField] private float hitDuration;
        [SerializeField] private AnimationCurve hitWeightCurve;
        [SerializeField] private float resetDuration;
        [SerializeField] private AnimationCurve resetWeightCurve;
        private AvatarHitStateMachine leftHitStateMachine;
        private AvatarHitStateMachine rightHitStateMachine;

        [SerializeField] private PartnerHandPreference partnerHandPreference;

        private Dictionary<InstrumentType, InteractionObject> _instruments;
        private Dictionary<PartnerBehaviourType, IPartnerBehaviour> _behaviours;

        private void Awake()
        {
            var interactionSystem = GetComponent<InteractionSystem>();
            avatarAnimator = GetComponent<Animator>();
            fullBodyBipedIK = GetComponent<FullBodyBipedIK>();
            Assert.IsNotNull(avatarAnimator);
            Assert.IsNotNull(fullBodyBipedIK);
            leftHitStateMachine = AvatarHitStateMachine.Create(gameObject, fullBodyBipedIK, AvatarHitStateMachine.ManagedHand.Left, hitDuration, hitWeightCurve, resetDuration, resetWeightCurve);
            rightHitStateMachine = AvatarHitStateMachine.Create(gameObject, fullBodyBipedIK, AvatarHitStateMachine.ManagedHand.Right, hitDuration, hitWeightCurve, resetDuration, resetWeightCurve);
            _instruments = new Dictionary<InstrumentType, InteractionObject>()
            {
                { InstrumentType.CrashCymbal, crashCymbal },
                { InstrumentType.LeftHighTom, highTom },
                { InstrumentType.RightMiddleTom, middleTom },
                { InstrumentType.SnareDrum, snareDrum }
            };

            _behaviours = new Dictionary<PartnerBehaviourType, IPartnerBehaviour>()
            {
                { PartnerBehaviourType.None, new NoneBehaviour() },
                { PartnerBehaviourType.Follow, new FollowerBehaviour(interactionSystem, _instruments) },
                { PartnerBehaviourType.Rhythm, new RhythmBehaviour(interactionSystem, _instruments, this) }
            };
        }

        private void OnEnable()
        {
            if (PartnerManager.Instance.CurrentBehaviourPartnerOne == PartnerBehaviourType.Rhythm)
            {
                ((RhythmBehaviour)_behaviours[PartnerManager.Instance.CurrentBehaviourPartnerOne]).Enable(partnerHandPreference);
            }
            else
            {
                _behaviours[PartnerManager.Instance.CurrentBehaviourPartnerOne].Enable();
            }

            Debug.Log($"[Partner: {name}] {PartnerManager.Instance.CurrentBehaviourPartnerOne} enabled.");

            EventManager.MusicStartEvent += StartDrumming;
            EventManager.MusicResetEvent += StopDrumming;
            EventManager.AgentPrepareEvent += StartDrumming;

            if (partnerHandPreference == PartnerHandPreference.Left)
            {
                avatarAnimator.SetLayerWeight(avatarAnimator.GetLayerIndex("Left Only"), 1);
            }
            else if (partnerHandPreference == PartnerHandPreference.Right)
            {
                avatarAnimator.SetLayerWeight(avatarAnimator.GetLayerIndex("Right Only"), 1);
            }

            if (MusicSequence.Instance.IsPlaying && MusicSequence.Instance.GetSequenceName().Trim().ToUpper() != "BREAK")
            {
                StartDrumming();
            }
        }

        private void OnDisable()
        {
            _behaviours[PartnerManager.Instance.CurrentBehaviourPartnerOne].Disable();

            Debug.Log($"[Partner: {name}] {PartnerManager.Instance.CurrentBehaviourPartnerOne} disabled.");

            EventManager.MusicStartEvent -= StartDrumming;
            EventManager.MusicResetEvent -= StopDrumming;
            EventManager.AgentPrepareEvent -= StartDrumming;
        }

        public void SwitchType(PartnerBehaviourType type)
        {
            foreach (var behaviour in _behaviours.Values.Where(b => b.Enabled))
            {
                behaviour.Disable();
            }

            _behaviours[type].Enable();
        }

        public void StartDrumming()
        {
            StartCoroutine(TemporarilyDisableDrumHitbox());
            avatarAnimator?.SetBool("Drumming", true);
        }

        public void StopDrumming()
        {
            if (highTom is not null)
            {
                highTom.gameObject.GetComponent<MeshCollider>().enabled = false;
            }
            if (middleTom is not null)
            {
                middleTom.gameObject.GetComponent<Collider>().enabled = false;
            }
            avatarAnimator?.SetBool("Drumming", false);
        }

        private IEnumerator TemporarilyDisableDrumHitbox(float duration = 0.25f)
        {
            if (highTom is not null)
            {
                highTom.gameObject.GetComponent<MeshCollider>().enabled = false;
            }
            if (middleTom is not null)
            {
                middleTom.gameObject.GetComponent<Collider>().enabled = false;
            }
            yield return new WaitForSeconds(duration);
            if (highTom is not null)
            {
                highTom.gameObject.GetComponent<MeshCollider>().enabled = true;
            }
            if (middleTom is not null)
            {
                middleTom.gameObject.GetComponent<Collider>().enabled = true;
            }

        }

        public void DrumHit(FullBodyBipedEffector hittingHand, InstrumentType instrument)
        {
            if (hittingHand == FullBodyBipedEffector.RightHand)
            {
                rightHitStateMachine.TriggerHit();
            }
            else
            {
                leftHitStateMachine.TriggerHit();
            }
        }
    }
}
