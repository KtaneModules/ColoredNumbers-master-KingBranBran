using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentDisplay : MonoBehaviour
{
    public GameObject[] segments;
    //                                                                                                                         ""(10) Y(11)    A(12)     E(13)    S(14)    N(15)  D(16)    L(17)  u(18)  U(19)  _(20)
    private string[] segmentNumbers = { "012345", "12", "01346", "01236", "1256", "02356", "023456", "012", "0123456", "012356", "" , "12356", "012456", "03456", "02356", "246", "12346", "345", "234", "12345", "3" }; // Each number (0 - 6) represents a certain segment. List goes from numbers 0 - 9. 10 + are for a solved module.
    private string RGBcolors = "rgb";

    public void SetDisplayNumbers(int[] numbers)
    {
        var segmentsColorText = new string[7];

        if (numbers.Length != 3)
        {
            Debug.Log("Invalid amount of numbers to display!");
            return;
        }

        for (int i = 0; i < numbers.Length; i++) // Fill up segmentsColorText
        {

            int numberToBeDisplayed = numbers[i];

            for (int j = 0; j < segmentsColorText.Length; j++)
            {
                if (segmentNumbers[numberToBeDisplayed].Contains(j.ToString())) // If segmentNumbers contains a certain segment, add it to segmentsColorText with the appropriate color.
                {
                    segmentsColorText[j] += RGBcolors[i].ToString();
                }
            }
        }

        for (int i = 0; i < segmentsColorText.Length; i++)
        {
            Color color = Color.black;

            switch (segmentsColorText[i])
            {
                case "r":
                    color = Color.red;
                    break;
                case "b":
                    color = Color.blue;
                    break;
                case "g":
                    color = Color.green;
                    break;
                case "rb":
                    color = Color.magenta;
                    break;
                case "rg":
                    color = Color.yellow;
                    break;
                case "gb":
                    color = Color.cyan; 
                    break;
                case "rgb":
                    color = Color.white;
                    break;
                default:
                    color = Color.black;
                    break;
            }
            segments[i].GetComponent<Renderer>().material.color = color;
        }
    }

    public void SetColor(Color color)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].GetComponent<Renderer>().material.color = color;
        }
    }
}
