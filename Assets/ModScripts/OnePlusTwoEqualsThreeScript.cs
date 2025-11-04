using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Debug;

public class OnePlusTwoEqualsThreeScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMNeedyModule Needy;

	public KMSelectable[] Buttons;

	public TextMesh MathDisplay;

	static int needyIdCounter = 1;
	int needyId;
	private bool needyDeactivated = true;

	private MathGenerator generator;
	private int answer;

	void Awake()
    {

		needyId = needyIdCounter++;

		foreach (KMSelectable button in Buttons)
			button.OnInteract += delegate () { ButtonPress(button); return false; };

		Needy.OnNeedyActivation += OnNeedyActivation;
		Needy.OnNeedyDeactivation += OnNeedyDeactivation;
		Needy.OnTimerExpired += OnTimerExpired;
    }

	
	void Start() => generator = new MathGenerator();

    void ButtonPress(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
		button.AddInteractionPunch(0.4f);

		if (needyDeactivated)
			return;


		var answerToCheck = int.Parse(button.GetComponentInChildren<TextMesh>().text);

		if (answerToCheck == answer)
			Log($"[1+2=3 #{needyId}] You answered {answerToCheck}, which is correct. Good job!");
		else
		{
			Log($"[1+2=3 #{needyId}] You answered {answerToCheck}, but everyone laughs at you because it's wrong. Strike!");
			Needy.HandleStrike();
		}

		OnNeedyDeactivation();
	}

	protected void OnNeedyActivation()
	{
		string mathProblem;

		needyDeactivated = false;

		generator.GenerateProblem(out answer, out mathProblem);

		MathDisplay.text = mathProblem;

		Log($"[1+2=3 #{needyId}] Generated solution: {generator}");
	}

	protected void OnNeedyDeactivation()
	{
		needyDeactivated = true;
		MathDisplay.text = string.Empty;
		Needy.HandlePass();
	}

	protected void OnTimerExpired()
	{
		Log($"[1+2=3 #{needyId}] The timer ran out before solving the math problem. Strike!");
		Needy.HandleStrike();
		OnNeedyDeactivation();
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press 1/2/3 [presses either 1, 2, or 3]";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		if (needyDeactivated)
		{
			yield return "sendtochaterror You cannot interact with the needy at this time!";
			yield break;
		}

		switch (split[0])
		{
			case "PRESS":
				if (split.Length == 1)
				{
					yield return "sendtochaterror Please specify which button to press!";
					yield break;
				}

				if (split.Length > 2)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				int num;

				if (!int.TryParse(split[1], out num))
				{
					yield return $"sendtochaterror {split[1]} is not a valid number!";
					yield break;
				}

				num--;

				if (!Enumerable.Range(0, 3).Contains(num))
				{
					yield return "sendtochaterror Make sure the number you want to press is 1-3 inclusive!";
					yield break;
				}

				yield return null;

				Buttons[num].OnInteract();
				yield return new WaitForSeconds(0.1f);
				break;
			default:
				yield return "sendtochaterror That is not a valid command!";
				yield break;
		}
    }

	void TwitchHandleForcedSolve() => StartCoroutine(HandleNeedyAutosolve());

	IEnumerator HandleNeedyAutosolve()
	{
		while (true)
		{
			while (needyDeactivated)
				yield return null;

			Buttons[answer - 1].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}

}





