using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private GameObject[] puzzleOrder;

    [Header("0 = black, 1 = white")]
    [Range(0, 1)]
    public float disabledGreyscale;
    private int curPuzzle;
    private void Start()
    {
        curPuzzle = 0;
        AkSoundEngine.SetState("PuzzleCount", puzzleOrder[curPuzzle].name);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            ActivateNextPuzzle();
    }
    public void ActivateNextPuzzle()
    {
        AkSoundEngine.SetState("PuzzleCount", puzzleOrder[curPuzzle].name);
        puzzleOrder[curPuzzle].GetComponent<PuzzleManager>().DeactivateHeleLortet();
        curPuzzle++;
        StartCoroutine(delayedPuzzleStart(puzzleOrder[curPuzzle]));
        puzzleOrder[curPuzzle].GetComponent<PuzzleManager>().ActivateHeleLortet();
    }
    IEnumerator delayedPuzzleStart(GameObject go)
    {
        yield return new WaitForSeconds(6f);
        go.SetActive(true);
        yield break;
    }
}
