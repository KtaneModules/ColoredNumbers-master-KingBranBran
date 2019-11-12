using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using KModkit;
using System;

public class ColorAddition : MonoBehaviour {

    public KMBombModule module;
    public KMAudio moduleAudio;
    public KMSelectable[] buttons = new KMSelectable[3];
    public SegmentDisplay[] displays;
    public GameObject[] buttonGameObjects = new GameObject[3];
    private string[] numbers = new string[3];
    private int[] numberOrder = { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };
    private int[] correctButtonOrder = new int[3];
    private int amountOfButtonsPressed = 0;
    private bool coroutineInProgress = false;
    private bool moduleSolved = false;
    private bool buttonAnimationActive = false;
    private bool initiated = false;
    private string[] sounds = {"B", "D#", "G"};
    private static int _moduleIdCounter = 1;
    private int _moduleId;

    private int[,] solvedModuleDisplayList = { { 11, 12, 11 }, { 11, 13, 14 }, { 13, 15, 16 }, { 17, 0, 17 }, { 16, 13, 16 }, { 12, 11, 0 }, { 0, 18, 0 }, { 0, 3, 0 }, { 19, 3, 19 }, { 0, 20, 0 } };

	void Start () {
        _moduleId = _moduleIdCounter++;

        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[j].OnInteract += delegate { ButtonPressed(j); return false; };
        }

        // Pick the numbers. Cannot do Random.Range(0, 1000) because single or double digit numbers can be generated and will break code. 
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = "" + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10);
        }

        DebugLog("RED: " + numbers[0]); DebugLog("GREEN: " + numbers[1]); DebugLog("BLUE: " + numbers[2]);

        correctButtonOrder = FindSolution();
        
        DebugLog("CORRECT BUTTON ORDER: " + correctButtonOrder[0] + " " + correctButtonOrder[1] + " " + correctButtonOrder[2]);

        moduleAudio.PlaySoundAtTransform("StartUp", module.transform);

        module.OnActivate += Init;
	}

    private void Init()
    {
        StartCoroutine(SetDisplayNumbersGlitchy());
        initiated = true;
    }

    private void SetDisplayNumbers()
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            var digitsToBeDisplayed = new[] { int.Parse(numbers[0][i].ToString()), int.Parse(numbers[1][i].ToString()), int.Parse(numbers[2][i].ToString()) }; // Get a certain digit from each number.
            displays[i].SetDisplayNumbers(digitsToBeDisplayed);
        }
    }

    void ButtonPressed(int buttonNumber)
    {
        StartCoroutine(ButtonAnimation(buttonGameObjects[buttonNumber]));
        buttons[buttonNumber].AddInteractionPunch(.5f);
        moduleAudio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, buttons[buttonNumber].transform);

        if (amountOfButtonsPressed >= correctButtonOrder.Length)
        {
            return;
        }

        

        if (buttonNumber + 1 == correctButtonOrder[amountOfButtonsPressed]) // buttonNumber originally starts at 0, so +1
        {            
            amountOfButtonsPressed++;
            moduleAudio.PlaySoundAtTransform(sounds[amountOfButtonsPressed - 1], module.transform);

            StartCoroutine(ClearCorrectNumbers(displays[buttonNumber]));
            if (amountOfButtonsPressed > 2)
            {
                module.HandlePass();
                ModuleSolved();
            }
        }
        else
        {
            moduleAudio.PlaySoundAtTransform(sounds[amountOfButtonsPressed], module.transform);

            module.HandleStrike();
            ResetModule();
        }
    }

    private void ModuleSolved()
    {
        moduleSolved = true;
        StartCoroutine(SolvedModuleDisplay());
    }

    private void ResetModule()
    {
        amountOfButtonsPressed = 0;

        // Display the numbers again.
        StartCoroutine(SetDisplayNumbersGlitchy());
    }

    void SetAllDisplayOneColor(Color color)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetColor(color);
        }
    }

    int[] FindSolution()
    {
        int[] solution = new int[3];
        string solutionNumbers;
        int[] intNumbers = { int.Parse(numbers[0]), int.Parse(numbers[1]), int.Parse(numbers[2]) };
        int sum = intNumbers[0] + intNumbers[1] + intNumbers[2];
        int buttonsFound = 0;

        DebugLog("THE SUM OF THE NUMBERS IS " + sum);

        if (sum < 100)
        {
            solutionNumbers = sum < 10 ? "00" + sum : "0" + sum;
        }
        else
        {
            var sumString = sum.ToString();
            solutionNumbers = sumString.Substring(sumString.Length - 3);
        }
        
        for (int i = 0; i < numberOrder.Length && buttonsFound < 3; i++)
        {
            for (int b = 2; b > -1; b--)
            {
                if (numberOrder[i].ToString() == solutionNumbers[b].ToString())
                {                  
                    solution[buttonsFound] = b + 1;
                    buttonsFound++;
                }
            }
        }

        return solution;
    }

    void SetSpecificDisplayOneColor(SegmentDisplay display, Color color)
    {
        display.SetColor(color);
    }

    private IEnumerator ButtonAnimation(GameObject button)
    {
        yield return new WaitUntil(() => !buttonAnimationActive);

        buttonAnimationActive = true;

         for (int i = 0; i < 10; i++) {
            button.transform.localPosition = new Vector3(0, button.transform.localPosition.y - .01f, 0);
            yield return new WaitForSeconds(.01f);
        }
        
        for (int i = 0; i < 10; i++) {
            button.transform.localPosition = new Vector3(0, button.transform.localPosition.y + .01f, 0);
            yield return new WaitForSeconds(.01f);
        }

        buttonAnimationActive = false;
    }

    private IEnumerator ClearCorrectNumbers(SegmentDisplay display)
    {
        yield return new WaitUntil(() => !coroutineInProgress);

        SetSpecificDisplayOneColor(display, Color.black);
        yield return new WaitForSeconds(.2f);
        SetSpecificDisplayOneColor(display, Color.green);
        yield return new WaitForSeconds(.1f);
        SetSpecificDisplayOneColor(display, Color.black);

        coroutineInProgress = false;
    }

    private IEnumerator SetDisplayNumbersGlitchy()
    {
        yield return new WaitUntil(() => !coroutineInProgress);

        coroutineInProgress = true;

        SetAllDisplayOneColor(Color.black);
        yield return new WaitForSeconds(1f);
        SetDisplayNumbers();
        yield return new WaitForSeconds(.2f);
        SetAllDisplayOneColor(Color.black);
        yield return new WaitForSeconds(.1f);
        SetDisplayNumbers();

        coroutineInProgress = false;
    }

    private IEnumerator SolvedModuleDisplay()
    {
        yield return new WaitUntil(() => !coroutineInProgress);

        yield return new WaitForSeconds(1f);
        moduleAudio.PlaySoundAtTransform("Solve", module.transform);
        var randomDisplayNum = Random.Range(0, 10); // Amount of end emojis i have

        DebugLog("EMOJI #" + randomDisplayNum);

        for (int i = 0;  i < displays.Length; i++)
        {
            displays[i].SetDisplayNumbers(new[] { 10, solvedModuleDisplayList[randomDisplayNum, i], 10 }); // Red and Blue is 10(Blank) becuase I only want green.
        }

        yield return new WaitForSeconds(.2f);
        SetAllDisplayOneColor(Color.black);
        yield return new WaitForSeconds(.1f);

        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].SetDisplayNumbers(new[] { 10, solvedModuleDisplayList[randomDisplayNum, i], 10 }); // Red and Blue is 10(Blank) becuase I only want green.
        }

        coroutineInProgress = false;
    }

    private void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[Color Addition #{0}] {1}", _moduleId, logData);
    }

    private string TwitchHelpMessage = @"Use !press 1 2 3 the press the buttons from left to right!";
    IEnumerator ProcessTwitchCommand(string command)
    {
        var parts = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 4 && parts[0] == "press" && parts.Skip(1).All(part => (part.Length == 1) && "123".Contains(part))) {

            yield return null;
            string[] buttonNumbers = parts.Skip(1).ToArray();

            int[] buttonsToBePressed = {-1, -1, -1};

            for (int i = 0; i < buttonNumbers.Length; i++)
            {
                int.TryParse(buttonNumbers[i], out buttonsToBePressed[i]);
            }

            for (int i = 0; i < 3; i++) {
                ButtonPressed(buttonsToBePressed[i] - 1);
                yield return new WaitForSeconds(.4f);
            }
        }
    }
}
