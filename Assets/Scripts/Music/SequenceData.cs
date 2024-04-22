using FluidMidi;
using UnityEngine;

[CreateAssetMenu(fileName = "SequenceData", menuName = "ScriptableObjects/SequenceData")]
public class SequenceData : ScriptableObject
{
    [SerializeField] public StreamingAsset song;
    [SerializeField] public bool autoSoundfont = true;
    [SerializeField] public StreamingAsset soundfont;
    [SerializeField] public int startTicks;
    [SerializeField] public int startLoopTicks;
    [SerializeField] public int endTicks;
    [SerializeField][BitField] public int mutedChannelsNormal;
    [SerializeField][BitField] public int mutedChannelsSpectating;
    [SerializeField][BitField] public int mutedChannelsFrontRunning;
    [SerializeField][BitField] public int mutedChannelsLastLife;
    [SerializeField][Range(0.1f, 2.0f)] public float playbackSpeedNormal = 1.0f;
    [SerializeField][Range(0.1f, 2.0f)] public float playbackSpeedFast = 1.25f;
}
