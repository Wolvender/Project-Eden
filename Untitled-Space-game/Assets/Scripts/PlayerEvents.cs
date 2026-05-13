using System;

public static class PlayerEvents
{
    public static event Action<int>    OnPlayerDamaged;
    public static event Action         OnPlayerDied;
    public static event Action         OnPlayerRespawned;
    public static event Action<string> OnPlayerPickedUpItem;
    public static event Action         OnPlayerLowHealth;
    public static event Action         OnPlayerFired;

    public static void TriggerDamaged(int damage)       => OnPlayerDamaged?.Invoke(damage);
    public static void TriggerDied()                    => OnPlayerDied?.Invoke();
    public static void TriggerRespawned()               => OnPlayerRespawned?.Invoke();
    public static void TriggerPickedUpItem(string item) => OnPlayerPickedUpItem?.Invoke(item);
    public static void TriggerLowHealth()               => OnPlayerLowHealth?.Invoke();
    public static void TriggerFired()                   => OnPlayerFired?.Invoke();
}
