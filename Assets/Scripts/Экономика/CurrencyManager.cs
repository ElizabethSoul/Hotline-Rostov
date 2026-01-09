using UnityEngine;
using System;

public static class CurrencyManager
{
    private static int coins;
    public static event Action<int> OnCoinsChanged;

    public static void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"[CurrencyManager] Added {amount} coins. Total: {coins}");
        OnCoinsChanged?.Invoke(coins);
    }

    public static int GetCoins()
    {
        return coins;
    }

    public static void SetCoins(int value)
    {
        coins = value;
        OnCoinsChanged?.Invoke(coins);
    }
}