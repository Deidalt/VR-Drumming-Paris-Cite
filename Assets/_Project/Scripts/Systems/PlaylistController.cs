using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Data;
using _Project.Scripts.Systems;
using UnityEngine;

public class PlaylistController : MonoBehaviour
{

    public static PlaylistController Instance;
    private Playlist currentPlaylist;
    private RandomisedTrial currentTrial;
    private AgentSO _currentPartner = null;
    public AgentSO ShownPartner = null;
    [SerializeField] private RecallManager recallButtons;

    private Coroutine coroutine;
    private Coroutine subCoroutine;
    private bool recalling = false;
    private bool waitingToContinue = false;
    void Start()
    {
        Instance = this;
        EventManager.AgentSelected += UpdateCurrentPartnerStored;
    }

    public void Play()
    {
        if (GameData.Instance.currentPlayType is PlayType.Playlist)
        {
            coroutine = StartCoroutine(IteratePlaylist());
        }
        else if (GameData.Instance.currentPlayType is PlayType.RandomisedTrial)
        {
            coroutine = StartCoroutine(IterateTrial());
        }
    }

    public void Reset()
    {
        Debug.Log("Resetting");
        if (coroutine != null)
        {
            Debug.Log("Coroutine");
            if (subCoroutine != null)
            {
                StopCoroutine(subCoroutine);
                EventManager.InvokeRemoveAgent();
                DrumLogger.Instance.ChangedAvatar("No Avatar", shouldLog: false);
            }
            StopCoroutine(coroutine);
            if (MusicSequence.Instance.IsPlaying)
            {
                MusicSequence.Instance.Reset();
            }
            if (recalling)
            {
                EndRecall();
                MusicSequence.Instance.Reset();
                recallButtons.gameObject.SetActive(false);
            }
            DrumLogger.Instance.SetCurrentTrail("FreePlay");
        }
        else
        {
            Debug.Log("No coroutine");
        }
    }

    private IEnumerator IteratePlaylist()
    {
        foreach (PlaylistItem item in currentPlaylist.playlistItems)
        {
            EventManager.InvokeMusicSettingChangeEvent(item.track);
            MusicSequence.Instance.Play();
            if (item.hidePartner && ShownPartner is not null)
            {
                EventManager.InvokeRemoveAgent();
                DrumLogger.Instance.ChangedAvatar("No Avatar", shouldLog: false);
            }
            else if (!item.hidePartner && ShownPartner != _currentPartner)
            {
                EventManager.InvokeAgentSelected(_currentPartner);
                if (GameData.Instance.currentPlayType == PlayType.RandomisedTrial)
                {
                    DrumLogger.Instance.ChangedAvatar(_currentPartner.name, currentTrial.handPreference, shouldLog: false);
                }
                else
                {
                    DrumLogger.Instance.ChangedAvatar(_currentPartner.name, shouldLog: false);
                }
            }
            if (item.track.name.Trim().ToLower() == "recall")
            {
                recalling = true;
                recallButtons.gameObject.SetActive(true);
                while (recalling)
                {
                    yield return null;
                }
                EventManager.InvokeTimerStopEvent();
            }
            if (item.track.name.Trim().ToLower() == "break")
            {
                yield return new WaitForSeconds(item.duration - 1);
                EventManager.InvokeAgentPrepareEvent();
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return new WaitForSeconds(item.duration);
            }
            MusicSequence.Instance.Reset();
        }
        if (currentPlaylist.playlistItems[^1].track.name.Trim().ToLower() != "recall")
        {
            Reset();
        }
        else
        {
            if (subCoroutine is not null)
            {
                StopCoroutine(subCoroutine);
            }
        }
    }

    private IEnumerator IterateTrial()
    {
        TrialPhase[] trailPhases = currentTrial.GetTrailPhases();
        PlaylistItem shownBreakItem = new PlaylistItem(currentTrial.breakObject, currentTrial.breakTimeSecs, false);
        PlaylistItem hiddenBreakItem = new PlaylistItem(currentTrial.breakObject, currentTrial.breakTimeSecs, true);
        PlaylistItem inteferenceItem = new PlaylistItem(currentTrial.interferenceObject, currentTrial.interferenceTimeSecs, true);
        PlaylistItem recallItem = new PlaylistItem(currentTrial.recallObject, 0, true);

        Queue<int> strongTrackQueue = RandomTrackOrder(currentTrial.availableStrongSequences.Length);
        Queue<int> weakTrackQueue = RandomTrackOrder(currentTrial.availableWeakSequences.Length);

        EventManager.InvokeHandPreferenceChanged(currentTrial.handPreference);

        foreach (TrialPhase phase in trailPhases)
        {
            Queue<int> agentQueue;
            if (phase.availableAgents.Length == 1)
            {
                // when there is only 1 agent available
                agentQueue = new Queue<int>(new int[currentTrial.tracksPerBlock]);
            }
            else
            {
                // when 2 agents need to be alternated between
                agentQueue = RandomisedEvenBinaryQueue(currentTrial.tracksPerBlock);
            }
            Queue<int> strongOrWeakQueue = RandomisedEvenBinaryQueue(currentTrial.tracksPerBlock);
            for (int i = 0; i < currentTrial.tracksPerBlock; i++)
            {
                PlaylistItem currentTrack;
                if (strongOrWeakQueue.Dequeue() == 0)
                {
                    currentTrack = new PlaylistItem(
                        currentTrial.availableStrongSequences[strongTrackQueue.Dequeue()],
                        currentTrial.trackTimeSecs,
                        false
                    );
                }
                else
                {
                    currentTrack = new PlaylistItem(
                        currentTrial.availableWeakSequences[weakTrackQueue.Dequeue()],
                        currentTrial.trackTimeSecs,
                        false
                    );
                }

                Debug.Log("Track playing " + currentTrack.track.name);

                UpdateCurrentPartnerStored(phase.availableAgents[agentQueue.Dequeue()]);

                currentPlaylist = Playlist.CreatePlaylist(
                    new PlaylistItem[] {
                    shownBreakItem,
                    currentTrack,
                    hiddenBreakItem,
                    inteferenceItem,
                    hiddenBreakItem,
                    recallItem
                    }
                );

                yield return subCoroutine = StartCoroutine(IteratePlaylist());
            }
            waitingToContinue = true;
            recallButtons.gameObject.SetActive(true);
            EventManager.InvokeTimerStopEvent();
            EventManager.InvokeTimerStartEvent();
            DrumLogger.Instance.SetCurrentTrail("FreePlay");

            while (waitingToContinue)
            {
                yield return null;
            }
        }
        Reset();
    }

    private Queue<int> RandomisedEvenBinaryQueue(int length)
    {
        Queue<int> binaryQueue = new Queue<int>(length);
        int[] binaryDigitUsageCount = new int[2];
        for (int i = 0; i < length; i++)
        {
            int randomBinaryDigit = UnityEngine.Random.Range(0, 2);
            if (!currentTrial.RandomiseOrderOfTracks)
                randomBinaryDigit = 0; // put all tracks in Strong queue when randomise is off
            if (binaryDigitUsageCount[randomBinaryDigit] >= length / 2)
            {
                binaryQueue.Enqueue(1 - randomBinaryDigit);
            }
            else
            {
                binaryQueue.Enqueue(randomBinaryDigit);
                binaryDigitUsageCount[randomBinaryDigit]++;
            }
        }
        return binaryQueue;
    }

    private Queue<int> RandomTrackOrder(int availableTrackCount)
    {
        Queue<int> trackIndexQueue = new Queue<int>(availableTrackCount);
        List<int> availableTracks = new List<int>(availableTrackCount);
        availableTracks.AddRange(Enumerable.Range(0, availableTrackCount));
        for (int i = 0; i < availableTrackCount; i++)
        {
            int randomTrackIndex = Random.Range(0, availableTracks.Count);
            if (!currentTrial.RandomiseOrderOfTracks)
                randomTrackIndex = 0;
            trackIndexQueue.Enqueue(availableTracks[randomTrackIndex]);
            availableTracks.RemoveAt(randomTrackIndex);
        }
        return trackIndexQueue;
    }

    public void SetCurrentPlaylist(Playlist playlist)
    {
        currentPlaylist = playlist;
    }

    public void SetRandomisedTrial(RandomisedTrial trial)
    {
        currentTrial = trial;
    }

    private void UpdateCurrentPartnerStored(AgentSO agent)
    {
        _currentPartner = agent;
    }

    public bool IsRecalling()
    {
        return recalling;
    }

    public void EndRecall()
    {
        recalling = false;
    }

    public void ContinueToNextTrialPhase()
    {
        waitingToContinue = false;
    }

}
