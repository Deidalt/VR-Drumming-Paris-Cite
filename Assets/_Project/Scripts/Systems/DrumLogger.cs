using System;
using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Systems;
using DrumRhythmGame.Data;
using UnityEngine;
using UnityEngine.XR;

public class DrumLogger : MonoBehaviour
{
    public static DrumLogger Instance { get; private set; }
    private DateTime _ogStartTime;
    private DateTime _currentFileStartTime;
    private string _hitLogDirectory;
    private int _trailCounter = 1;
    private string _currentTrail = "FreePlay";
    private string _currentAvatar = "No Avatar";
    private PartnerHandPreference _handPreference = PartnerHandPreference.Both;
    private Queue<string> _participantHits;
    private Queue<string> _avatarHits;
    private Queue<string> _beatTimes;
    private string _drumHitHeaders = "HitCount,HitTime,Hand,Drum";
    private string _beatTimeHeaders = "BeatNumber,Time";
    private Dictionary<XRNode, string> _handMap = new Dictionary<XRNode, string> { { XRNode.LeftHand, "Left Hand" }, { XRNode.RightHand, "Right Hand" } };
    private Dictionary<InstrumentType, string> _instrumentMap = new Dictionary<InstrumentType, string> { { InstrumentType.RightMiddleTom, "Right Drum" }, { InstrumentType.LeftHighTom, "Left Drum" } };
    private Dictionary<InstrumentType, string> _avatarInstrumentMap = new Dictionary<InstrumentType, string> { { InstrumentType.LeftHighTom, "Right Drum" }, { InstrumentType.RightMiddleTom, "Left Drum" } };


    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            _participantHits = new Queue<string>();
            _avatarHits = new Queue<string>();
            _beatTimes = new Queue<string>();

            _ogStartTime = _currentFileStartTime = DateTime.Now;

            ParticipantData pptData = ParticipantData.GetPptData();
            _hitLogDirectory = $@"{Application.dataPath}/Log/{pptData.date}-ppt{pptData.pptNumber}/Drum Hit Data";
            Directory.CreateDirectory(_hitLogDirectory);
        }
    }

    public void RegisterHitEvent(ActorType whoHitDrum, InstrumentType whichDrum, XRNode whichHand)
    {
        if (whoHitDrum == ActorType.Player)
        {
            LogParticipantHit(_instrumentMap[whichDrum], _handMap[whichHand]);
        }
        else if (whoHitDrum == ActorType.Partner)
        {
            LogAvatarHit(_avatarInstrumentMap[whichDrum], _handMap[whichHand]);
        }
    }

    public void LogParticipantHit(string drum, string hand)
    {
        _participantHits.Enqueue($"{Math.Round((DateTime.Now - _currentFileStartTime).TotalMilliseconds)},{hand},{drum}");
    }
    public void LogAvatarHit(string drum, string hand)
    {
        _avatarHits.Enqueue($"{Math.Round((DateTime.Now - _currentFileStartTime).TotalMilliseconds)},{hand},{drum}");
    }

    public void LogBeatTime()
    {
        _beatTimes.Enqueue($"{Math.Round((DateTime.Now - _currentFileStartTime).TotalMilliseconds)}");
    }

    public void ChangedAvatar(string avatar, bool shouldLog = true)
    {
        if (shouldLog)
            LogCurrentTrial();
        _currentAvatar = avatar;
    }
    public void ChangedAvatar(string avatar, PartnerHandPreference handPreference, bool shouldLog = true)
    {
        if (shouldLog)
            LogCurrentTrial();
        _currentAvatar = avatar;
        _handPreference = handPreference;
    }

    public void SetCurrentTrail(string trailName)
    {
        LogCurrentTrial();
        _currentTrail = trailName;
    }

    private void LogCurrentTrial()
    {
        string trailInfo = $@"{_currentTrail} with {_currentAvatar} ({_handPreference} handed variant) started at {DateTime.Now:HH\:mm\:ss} ({_currentFileStartTime - _ogStartTime:mm\:ss} from start)";
        if (_currentTrail != "FreePlay" && _currentTrail != "SPR" && _currentTrail != "Break")
        {
            LogDataToFile(_participantHits, _drumHitHeaders, _hitLogDirectory, "Sequence Log", trailInfo);
        }
        _currentFileStartTime = DateTime.Now;
        _participantHits.Clear();
        _avatarHits.Clear();
        _beatTimes.Clear();
    }

    private void LogDataToFile(Queue<string> data, string fileHeader, string directory, string fileName, string trailInfo)
    {
        string savePath = directory + $"/{fileName}.csv";
        using (var writer = new StreamWriter(savePath, true))
        {
            // first, add headers to each column
            writer.WriteLine(trailInfo);
        }
    }

    private string MakeCurrentSaveDirectory()
    {
        string currentSaveDirectory = $@"{_hitLogDirectory}/{_trailCounter++} - {_currentTrail}";
        while (Directory.Exists(currentSaveDirectory))
        {
            currentSaveDirectory = $@"{_hitLogDirectory}/{_trailCounter++} - {_currentTrail}";
        }
        Directory.CreateDirectory(currentSaveDirectory);
        return currentSaveDirectory;
    }

    private void OnApplicationQuit()
    {
        LogCurrentTrial();
    }
}
