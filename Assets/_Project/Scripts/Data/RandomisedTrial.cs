using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DrumRhythmGame.Data;
using UnityEngine;


namespace _Project.Scripts.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "RandomisedTrial", menuName = "Randomised Trial", order = 2)]
    public class RandomisedTrial : ScriptableObject
    {
        [SerializeField] private bool RandomiseOrderOfPhases = true; 
        public int tracksPerBlock { get; private set; } = 6;
        public bool RandomiseOrderOfTracks = true;

        [field: SerializeField] public PartnerHandPreference handPreference = PartnerHandPreference.Both;

        [SerializeField] private TrialPhase[] trailPhases;

        [field: SerializeField] public MusicSetting[] availableStrongSequences { get; private set; }
        [field: SerializeField] public MusicSetting[] availableWeakSequences { get; private set; }

        [field: Space]
        [field: SerializeField] public int trackTimeSecs { get; private set; }
        [field: Space][field: SerializeField] public MusicSetting breakObject { get; private set; }
        [field: SerializeField] public int breakTimeSecs { get; private set; }
        [field: SerializeField] public MusicSetting interferenceObject { get; private set; }
        [field: SerializeField] public int interferenceTimeSecs { get; private set; }
        [field: SerializeField] public MusicSetting recallObject { get; private set; }

        public TrialPhase[] GetTrailPhases()
        {
            if (RandomiseOrderOfPhases)
            {
                TrialPhase[] returnedArray = new TrialPhase[trailPhases.Length];
                Array.Copy(trailPhases, returnedArray, trailPhases.Length);
                for (int i = 0; i < returnedArray.Length; i++)
                {
                    TrialPhase tmp = returnedArray[i];
                    int r = UnityEngine.Random.Range(i, returnedArray.Length);
                    returnedArray[i] = returnedArray[r];
                    returnedArray[r] = tmp;
                }
                return returnedArray;
            }
            else
            {
                return trailPhases;
            }
        }
    }

    [Serializable]
    public struct TrialPhase
    {
        [field: SerializeField] public AgentSO[] availableAgents { get; private set; }
    }
}