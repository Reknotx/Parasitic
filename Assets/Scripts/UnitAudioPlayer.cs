
using UnityEngine;

public class UnitAudioPlayer : MonoBehaviour
{
    /// <summary> The audio source attached to this game object. </summary>
    private AudioSource _audioSource;

    /// <summary> The audio clip to be played when this unit takes damage. </summary>
    [SerializeField] private AudioClip Damaged;

    /// <summary> The audio clip to be played when this unit dies. </summary>
    [SerializeField] private AudioClip Death;

    /// <summary> The audio clip to be played when this unit attacks. </summary>
    [SerializeField] private AudioClip NormalAttack;

    /// <summary> The audio clip to be played when this player uses their first ability. </summary>
    [SerializeField] private AudioClip AbilityOne;

    /// <summary> The audio clip to be played when this player uses their second ability. </summary>
    [SerializeField] private AudioClip AbilityTwo;

    /// <summary> The audio clip to be played when this player gains exp. </summary>
    [SerializeField] private AudioClip ExpGain;

    /// <summary> The audio clip to be played when this unit's attack misses.</summary>
    [SerializeField] private AudioClip AttackMissed;

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
                if (Damaged != null)
                    _audioSource.clip = Damaged;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.Death:
                if (Death != null)
                    _audioSource.clip = Death;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.NormalAttack:
                if (NormalAttack != null)
                    _audioSource.clip = NormalAttack;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.AbilityOne:
                if (AbilityOne != null)
                    _audioSource.clip = AbilityOne;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.AbilityTwo:
                if (AbilityTwo != null)
                    _audioSource.clip = AbilityTwo;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.ExpGain:
                if (ExpGain != null)
                    _audioSource.clip = ExpGain;
                else
                    _audioSource.clip = null;
                break;

            case AudioToPlay.AttackMissed:
                if (AttackMissed != null)
                    _audioSource.clip = AttackMissed;
                else
                    _audioSource.clip = null;
                break;

            default:
                break;
        }

        if (_audioSource.clip != null)
        {
            _audioSource.Play();
        }
    }

    public AudioSource GetAudioSource()
    {
        if (_audioSource != null) return _audioSource;
        return null;
    }
}
