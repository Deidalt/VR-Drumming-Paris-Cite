using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Data;
using _Project.Scripts.Systems;
using DrumRhythmGame.Data;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Field
{
    /// <summary>
    /// Handles the selection of different drumming partners, and also changing
    /// the behaviour of the selected partner.
    /// </summary>
    public class PartnerManager : SingletonMonoBehaviour<PartnerManager>
    {
        private static readonly string Prefix = "[<b>PartnerManager</b>]";

        #region Serialized Fields

        [SerializeField] private List<GameObject> drummingAvatars;

        [SerializeField] private Transform instantiationPositionPartnerOne;

        #endregion

        private GameObject _currentPartnerOne;
        private AgentSO _currentAgent;

        public Dictionary<(AgentSO, PartnerHandPreference), GameObject> agentObjectInstantiated;

        public PartnerBehaviourType CurrentBehaviourPartnerOne { get; private set; } = PartnerBehaviourType.None;

        private PartnerHandPreference partnerHandPreference = PartnerHandPreference.Both;

        #region Event Functions

        private void Start()
        {
            SwitchBehaviour(PartnerBehaviourType.Rhythm);
            SwitchHandPreference(PartnerHandPreference.Both);
            agentObjectInstantiated = new Dictionary<(AgentSO, PartnerHandPreference), GameObject>();
            PartnerHandPreference[] handPreferences = { PartnerHandPreference.Left, PartnerHandPreference.Right, PartnerHandPreference.Both };
            foreach (AgentSO agent in Resources.LoadAll<AgentSO>("Agents/ScriptableObjects").ToList())
            {
                foreach (var pref in handPreferences)
                {
                    partnerHandPreference = pref;
                    InstantiateAvatar(agent);
                }
            }
            _currentPartnerOne.SetActive(false);
            _currentPartnerOne = null;
            _currentAgent = null;
            partnerHandPreference = PartnerHandPreference.Both;
        }

        private void OnEnable()
        {
            EventManager.AgentSelected += InstantiateAvatar;
            EventManager.RemoveAgent += DestroyPartnerOne;
            EventManager.HandPreferenceChanged += SwitchHandPreference;
        }

        private void OnDisable()
        {
            EventManager.AgentSelected -= InstantiateAvatar;
            EventManager.RemoveAgent -= DestroyPartnerOne;
            EventManager.HandPreferenceChanged -= SwitchHandPreference;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var position = transform.position;
            Handles.DrawDottedLine(position, instantiationPositionPartnerOne.position, 4f);
        }
#endif

        #endregion

        /// <summary>
        ///     Instantiates the prefab of the selected agent.
        /// </summary>
        /// <param name="agent"></param>
        private void InstantiateAvatar(AgentSO agent)
        {

            GameObject agentPrefab;
            _currentAgent = agent;

            switch (partnerHandPreference)
            {
                case PartnerHandPreference.Left:
                    agentPrefab = agent.leftHandedVariant;
                    break;
                case PartnerHandPreference.Right:
                    agentPrefab = agent.rightHandedVariant;
                    break;
                default:
                    agentPrefab = agent.prefab;
                    break;
            }

            // If we already have a partner, and its the same one, we double clicked the same agent so remove it
            if (_currentPartnerOne != null && _currentPartnerOne == agentPrefab)
            {
                _currentPartnerOne.SetActive(false);
                // Destroy(_currentPartnerOne);
                _currentAgent = null;
                PlaylistController.Instance.ShownPartner = null;
                return;
            }

            // If we selected a different agent, destroy the old one and instantiate the new one
            if (_currentPartnerOne != null)
            {
                _currentPartnerOne.SetActive(false);
                // Destroy(_currentPartnerOne);
            }
            SaveData.Instance.avatarData.partnerOneAvatarIndex = agent.index;
            if (agentObjectInstantiated.ContainsKey((agent, partnerHandPreference)))
            {
                _currentPartnerOne = agentObjectInstantiated[(agent, partnerHandPreference)];
            }
            else
            {
                _currentPartnerOne = Instantiate(agentPrefab, instantiationPositionPartnerOne);
                agentObjectInstantiated[(agent, partnerHandPreference)] = _currentPartnerOne;
            }
            _currentPartnerOne.SetActive(CurrentBehaviourPartnerOne != PartnerBehaviourType.None);
            Debug.Log($"{Prefix} Instantiated agent <color=green>{agent.index}</color>");
            PlaylistController.Instance.ShownPartner = agent;
        }

        private void SelectAvatar(int skinIndex, int partnerIndex)
        {
            if (skinIndex < 0) skinIndex = 0;

            if (skinIndex > drummingAvatars.Count && partnerIndex == 0)
                skinIndex = 0;

            // If we already have a partner
            if (_currentPartnerOne != null)
                // _currentPartnerOne.SetActive(false);
                Destroy(_currentPartnerOne);

            // SET SKIN INDEX
            SaveData.Instance.avatarData.partnerOneAvatarIndex = skinIndex;

            // INSTANTIATE THE MODEL
            _currentPartnerOne = Instantiate(drummingAvatars[skinIndex], instantiationPositionPartnerOne);

            // GET PARTNER SCRIPT ON OBJECT
            //var partnerBehaviour = CurrentPartnerOne.GetComponentInChildren<Partner>
            // SET PARTNER AS ACTIVE
            _currentPartnerOne.SetActive(CurrentBehaviourPartnerOne != PartnerBehaviourType.None);

            Debug.Log($"{Prefix} Updated the avatar of agent: <color=green>{partnerIndex}</color>");
            PlaylistController.Instance.ShownPartner = _currentAgent;
        }

        public void DestroyPartnerOne()
        {
            if (_currentPartnerOne == null) return;
            _currentPartnerOne.GetComponentInChildren<Partner.Partner>().RevertToWaitState();
            _currentPartnerOne.GetComponentInChildren<FullBodyBipedIK>().solver.rightHandEffector.positionWeight = 0;
            _currentPartnerOne.GetComponentInChildren<FullBodyBipedIK>().solver.leftHandEffector.positionWeight = 0;
            _currentPartnerOne.GetComponentInChildren<FullBodyBipedIK>().solver.rightHandEffector.rotationWeight = 0;
            _currentPartnerOne.GetComponentInChildren<FullBodyBipedIK>().solver.leftHandEffector.rotationWeight = 0;
            _currentPartnerOne.SetActive(false);
            // Destroy(_currentPartnerOne);
            _currentPartnerOne = null;
            PlaylistController.Instance.ShownPartner = null;
        }

        public void SelectNextAvatar(int partnerIndex)
        {
            var skinIndex = SaveData.Instance.avatarData.partnerOneAvatarIndex;

            SelectAvatar((skinIndex + 1) % drummingAvatars.Count, partnerIndex);
        }

        public void SelectPreviousAvatar(int partnerIndex)
        {
            var skinIndex = SaveData.Instance.avatarData.partnerOneAvatarIndex;
            SelectAvatar((drummingAvatars.Count + skinIndex - 1) % drummingAvatars.Count, partnerIndex);
        }

        public void SwitchBehaviour(PartnerBehaviourType type)
        {
            Debug.Log($"[PartnerManager] {CurrentBehaviourPartnerOne} => {type}");

            CurrentBehaviourPartnerOne = type;
            if (_currentPartnerOne != null)
            {
                _currentPartnerOne.SetActive(CurrentBehaviourPartnerOne != PartnerBehaviourType.None);
                _currentPartnerOne.GetComponentInChildren<Partner.Partner>().SwitchType(type);
            }
        }

        public void SwitchHandPreference(PartnerHandPreference preference)
        {
            partnerHandPreference = preference;
            Debug.Log($"[PartnerManager] Switched hand preference to: {preference}");

            if (_currentAgent != null)
            {
                InstantiateAvatar(_currentAgent);
            }
        }
    }
}