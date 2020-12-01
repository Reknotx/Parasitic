
using UnityEngine;

public class UnitAudioPlayer : MonoBehaviour
{
    /// <summary> The audio source attached to this game object. </summary>
    private AudioSource _audioSource;

    /// <summary> The audio clip to be played when this unit takes damage. </summary>
    public AudioClip Damaged;

    /// <summary> The audio clip to be played when this unit dies. </summary>
    public AudioClip Death;

    /// <summary> The audio clip to be played when this unit attacks. </summary>
    public AudioClip NormalAttack;

    /// <summary> The audio clip to be played when this player uses their first ability. </summary>
    public AudioClip AbilityOne;

    /// <summary> The audio clip to be played when this player uses their second ability. </summary>
    public AudioClip AbilityTwo;

    /// <summary> The audio clip to be played when this player gains exp. </summary>
    public AudioClip ExpGain;

    /// <summary> The audio clip to be played when this unit's attack misses.</summary>
    public AudioClip AttackMissed;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    public enum AudioToPlay
    {
        Damaged,
        Death,
        NormalAttack,
        AbilityOne,
        AbilityTwo,
        ExpGain,
        AttackMissed
    }


    public void PlayAudio(AudioToPlay toPlay)
    {
        switch (toPlay)
        {
            case AudioToPlay.Damaged:
                break;

            case AudioToPlay.Death:
                break;

            case AudioToPlay.NormalAttack:
                break;

            case AudioToPlay.AbilityOne:
                break;

            case AudioToPlay.AbilityTwo:
                break;

            case AudioToPlay.ExpGain:
                break;

            case AudioToPlay.AttackMissed:
                break;

            default:
                break;
        }
    }
}
