using UnityEngine;

// Магазин: при взаимодействии открывает канвас вместо уничтожения объекта
public class Shop : ObjectsInteraction
{
	[Header("Shop UI")]
	[SerializeField] private GameObject shopCanvas;

	// Переопределяем поведение подбора: открываем UI
	public override void Pickup()
	{
		if (!isPlayerInRange)
			return;

		if (shopCanvas != null)
		{
			shopCanvas.SetActive(true);
			Debug.Log($"[Shop] Открыт магазин {gameObject.name}");
		}
		else
		{
			Debug.LogWarning($"[Shop] Для {gameObject.name} не назначен Shop Canvas");
		}
	}

	// Вызывается из UI, чтобы закрыть магазин
	public void CloseShop()
	{
			shopCanvas.SetActive(false);
	}
}
