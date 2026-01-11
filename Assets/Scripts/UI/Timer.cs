using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Timer : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI timerText;
	[Tooltip("If true, the timer counts down from Duration. If false, it counts up from 0.")]
	[SerializeField] private bool countdown = false;
	[Tooltip("Duration for countdown in seconds (ignored if countdown is false). Set 0 for infinite.)")]
	[SerializeField] private float duration = 30f;

	private bool running = false;
	private float timeValue = 0f;
	private bool started = false;

	private void Reset()
	{
		var col = GetComponent<Collider2D>();
		if (col != null)
			col.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player")) return;
		if (!started)
			StartTimer();
	}

	private void Update()
	{
		if (!running) return;

		if (countdown)
		{
			timeValue -= Time.deltaTime;
			if (timeValue <= 0f)
			{
				timeValue = 0f;
				running = false;
			}
		}
		else
		{
			timeValue += Time.deltaTime;
		}

		UpdateUI();
	}

	private void UpdateUI()
	{
		if (timerText == null) return;
		float display = countdown ? timeValue : timeValue;
		int minutes = Mathf.FloorToInt(display / 60f);
		float secondsF = display - minutes * 60;
		int seconds = Mathf.FloorToInt(secondsF);
		int hundredths = Mathf.FloorToInt((secondsF - seconds) * 100f);
		timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, hundredths);
	}

	public void StartTimer()
	{
		running = true;
		started = true;
		timeValue = countdown ? duration : 0f;
		UpdateUI();
	}

	public void StopTimer()
	{
		running = false;
	}
}
