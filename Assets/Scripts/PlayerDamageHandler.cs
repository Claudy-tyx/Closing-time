
using UnityEngine;

public class PlayerDamageHandler : MonoBehaviour
{
    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;

    private float invulnerableUntil;

    public bool IsInvulnerable =>
        Time.time < invulnerableUntil;

    public bool TryTakeDamage(
        int amount,
        DamageCause cause = DamageCause.Unknown)
    {
        if (amount <= 0 || IsInvulnerable)
            return false;

        RunManager manager = RunManager.Instance;

        if (manager == null ||
            manager.Data == null ||
            !manager.IsRunActive)
            return false;

        RunData data = manager.Data;

        if (data.CurrentHearts <= 0)
            return false;

        invulnerableUntil =
            Time.time + invulnerabilityDuration;

        manager.RecordDamageCause(cause);

        data.ChangeHearts(-amount);

        return true;
    }
}
