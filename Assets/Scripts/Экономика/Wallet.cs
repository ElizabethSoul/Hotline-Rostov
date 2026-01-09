using UnityEngine;

public class Wallet : ObjectsInteraction
{
	[SerializeField] private int minCoins = 5;
	[SerializeField] private int maxCoins = 10;

	public override void Pickup()
	{
		if (!isPlayerInRange)
			return;

		// Гарантируем корректный диапазон
		int min = Mathf.Max(0, minCoins);
		int max = Mathf.Max(min + 1, maxCoins);

		int amount = Random.Range(min, max + 1);

		// Начисляем монеты через менеджер валют
		CurrencyManager.AddCoins(amount);

		Debug.Log($"[Wallet] Игрок поднял кошелёк: +{amount} монет");

		// Вызов базовой логики (анимация/удаление)
		base.Pickup();
	}
}
